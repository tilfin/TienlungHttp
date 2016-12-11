using System;
using System.Collections.Specialized;
using System.IO;
using System.Diagnostics;

namespace TienlungHttp
{
	/// <summary>
	/// ファイルを取得するアクション
	/// </summary>
	public class FileGetAction : IAction
	{
		/// <summary>
		/// MIME/TYPE マップ
		/// </summary>
        public static StringDictionary contentTypeMap = new StringDictionary();

        /// <summary>
        /// DirectoryIndex
        /// </summary>
		private static string[] DirectoryIndex = new string[] { "index.html", "index.htm", "default.html", "index.php" };
        
		/// <summary>
		/// 静的コンストラクタ
		/// </summary>
        static FileGetAction() {
            LoadMimeTypes();
        }

		private static void LoadMimeTypes() {
			using (var sr = new StreamReader("mime.types")) {
				string line;
				while (sr.Peek() >= 0) {
					line = sr.ReadLine();
					if (line.Length > 0 && !line.StartsWith("#")) {
						string[] words = line.Split(new char[] { ' ', '\t' } );
						for (int i = 1; i < words.Length; i++) {
							string s = words[i].Trim();
							if (s.Length > 0) {
								if (!contentTypeMap.ContainsKey(s)) {
									contentTypeMap.Add(s, words[0]);
								} else {
                                    Console.WriteLine("duplicate definition of \"{0}\"", s);
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="docRoot">ドキュメントルート</param>
		public FileGetAction(string docRoot) {
            DocumentRoot = docRoot;
		}

        /// <summary>
        /// ドキュメントルート
        /// </summary>
        public string DocumentRoot { get; private set; }

		#region Action メンバ

		public void Execute(HttpRequest request, HttpResponse response) {
			string localPath = DocumentRoot + request.Path.Replace('/', Path.DirectorySeparatorChar);

			if (Directory.Exists(localPath)) {
                if (OutputDirectoryIndex(request, response, localPath)) return;
			}
            
			int writtenSize = 0;
			try {
				var fi = new FileInfo(localPath);
				if (!fi.Exists) {
					response.StatusCode = StatusCode.NotFound;
					HttpService.ReponseErrorScreen(response, null);
					return;
				}

                if (".php".Equals(fi.Extension)) {
                    // 拡張子 PHP 
                    Process myProcess = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo();

                    startInfo.FileName = "php-cgi.exe";
                    startInfo.Arguments = fi.FullName + " " + request.QueryString.Replace('&', ' ');
                    startInfo.RedirectStandardOutput = true;

                    // 環境変数を使うのでfalseを設定する
                    startInfo.UseShellExecute = false;

                    myProcess.StartInfo = startInfo;
                    myProcess.Start();
                    string strbuf = myProcess.StandardOutput.ReadToEnd();
                    myProcess.WaitForExit();

                    // ヘッダ出力はコンテント長に含めない
                    int buflen = strbuf.Length - strbuf.IndexOf("\r\n\r\n") - 4;
                    byte[] buf = System.Text.UTF8Encoding.UTF8.GetBytes(strbuf);

                    response.ContentLength = buflen;
                    response.StatusCode = StatusCode.OK;

                    var outstream2 = response.GetOutputStreamForCGI();
                    outstream2.Write(buf, 0, buf.Length);
                    outstream2.Flush();

                    return;
                }

				// Content-Typeの取得
				var ext = fi.Extension.Substring(1);
				if (contentTypeMap.ContainsKey(ext)) {
					response.ContentType = (string)contentTypeMap[ext];
				}

				response.ContentLength = fi.Length;
				response.SetHeader("Last-Modified", fi.LastWriteTimeUtc.ToString("r"));
				response.StatusCode = StatusCode.OK;

                var outstream = response.GetOutputStream();
				using (var fs = fi.OpenRead()) {
					byte[] buffer = new byte[8192];
					int size;
					while ((size = fs.Read(buffer, 0, buffer.Length)) > 0) {
						outstream.Write(buffer, 0, size);
						writtenSize += size;
					}
				}
			} catch (IOException ex) {
				System.Diagnostics.Debug.WriteLine("FileGetAction - IOException");
				System.Diagnostics.Debug.WriteLine(ex.Message);
				System.Diagnostics.Debug.WriteLine("ContentLength: " + response.ContentLength.ToString());
				System.Diagnostics.Debug.WriteLine("WrittenContentLength: " + writtenSize.ToString());
				response.StatusCode = StatusCode.Forbidden;

				try {
					HttpService.ReponseErrorScreen(response, ex);
				} catch (Exception ex2) {
					Console.WriteLine(ex2.Message + " - " + request.Request);
				}
			}
		}

        private bool OutputDirectoryIndex(HttpRequest request, HttpResponse response, string localPath) {
            // / を省略していたら 301 
            if (!request.Path.EndsWith("/")) {
                response.StatusCode = StatusCode.MovedPermanentlyLocation;
                response.SetHeader("Location", request.Path + "/");
                return true;
            }

            // DirectoryIndex を確認
            if (!SetDirectoryIndex(ref localPath)) {
                // セットできなかったので、ディレクトリのファイル一覧を生成して返す。
                response.ContentType = "text/html;charset=utf8";
                response.StatusCode = StatusCode.OK;
                var ms = CreateDirectoryEntriesHtml(request.Path, localPath);
                response.ContentLength = ms.Length;
                ms.Seek(0, SeekOrigin.Begin);

                Stream stream = response.GetOutputStream();
                byte[] buffer = new byte[8192];
                int size;
                while ((size = ms.Read(buffer, 0, 8192)) > 0) {
                    stream.Write(buffer, 0, size);
                }
                ms.Close();
                return true;
            }

            return false;
        }

        /// <summary>
        /// ディレクトリインデックスをセットする。
        /// </summary>
        /// <param name="localPath">ローカルのファイルパス</param>
        /// <returns>見つかったかどうかを返す</returns>
        private bool SetDirectoryIndex(ref string localPath) {
            for (int i = 0; i < DirectoryIndex.Length; i++) {
                string filePath = Path.Combine(localPath, DirectoryIndex[i]);
                if (File.Exists(filePath)) {
                    localPath = filePath;
                    return true;
                }
            }
            return false;
        }

        private MemoryStream CreateDirectoryEntriesHtml(string reqPath, string dirPath) {
			var sb = new System.Text.StringBuilder();
			
			sb.Append("<html><head><title>");
			sb.Append(reqPath);
			sb.Append("</title></head><body>");
			
			var dirInfo = new DirectoryInfo(dirPath);

			sb.Append("<h1>Index of ");
			sb.Append(reqPath);
			sb.Append("</h1>");
			sb.Append("<table width=\"600\">");
			sb.Append("<tr><th>Name</th><th>Size</th><th>LastWriteTime</th>");

			foreach (var di in dirInfo.GetDirectories()) {
				sb.Append("<tr><td>");
				sb.Append("<a href=\"");
				sb.Append(di.Name);
				sb.Append("/\">");
				sb.Append(di.Name);
				sb.Append("/</a> ");
				sb.Append("</td><td>");
				sb.Append("Directory");
				sb.Append("</td><td>");
				sb.Append(di.LastWriteTime.ToString());
				sb.Append("</td></tr>");
			}

			foreach (FileInfo fi in dirInfo.GetFiles()) {
				sb.Append("<tr><td>");
				sb.Append("<a href=\"");
				sb.Append(fi.Name);
				sb.Append("\">");
				sb.Append(fi.Name);
				sb.Append("</a> ");
				sb.Append("</td><td>");
				sb.Append(fi.Length.ToString());
				sb.Append("</td><td>");
				sb.Append(fi.LastWriteTime.ToString());
				sb.Append("</td></tr>");
			}
			
			sb.Append("</table>");
			sb.Append("<p>");
			sb.Append(HttpServer.SERVER);
			sb.Append("</p>");
			sb.Append("</body></html>");

			return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(sb.ToString()));
		}

		#endregion
	}
}
