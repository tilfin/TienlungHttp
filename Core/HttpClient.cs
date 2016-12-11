using System;
using System.IO;
using System.Net.Sockets;

namespace TienlungHttp
{
	/// <summary>
	/// HTTP クライアント（不使用）
	/// </summary>
	public class HttpClient
	{
		private Socket socket;
		private NetworkStream networkStream;
		private HttpResponse curhttpres = null;
		private MultiPartContentStream mpcs;

		private string connection;
		private MemoryStream resContent;
		private string host;
		private string contentType;
		private string contentLength;
		private bool isPost = false;
		private int timeout;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public HttpClient() {
			mpcs = new MultiPartContentStream();
		}

        /// <summary>
		/// ネットワークストリームから一回に受け取る最大データサイズ
		/// </summary>
		public int MaxFragmentSize { get; set; }

        /// <summary>
        /// タイムアウト
        /// </summary>
        public int TimeOut {
			set { timeout = value; }
		}

		/// <summary>
		/// キープアライブ
		/// </summary>
		public bool KeepAlive {
			set {
				if (value) {
					connection = "Keep-Alive";
				} else {
					connection = "Close";
				}
			}
		}

		/// <summary>
		/// コンテントタイプ
		/// </summary>
		public string ContentType {
			set { contentType = "Content-Type: " + value; }
		}

		/// <summary>
		/// コンテント長
		/// </summary>
		public long ContentLength {
			set { contentLength = "Content-Length: " + value.ToString(); }
		}

		public bool Open(string server, int port) {
			this.host = "Host: " + server;

			//ホスト名からIPアドレスを取得
			var hostadd = System.Net.Dns.GetHostEntry(server).AddressList[0];

			//IPEndPointを取得
			var ephost = new System.Net.IPEndPoint(hostadd, port);

			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, timeout);
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout);
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, MaxFragmentSize);
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, MaxFragmentSize);

			socket.Connect(ephost);
			if (socket.Connected) {
				networkStream = new NetworkStream(socket);
				return true;
			} else {
				return false;
			}
		}

		public void GET(string path) {
			isPost = false;

			string message = "GET " + path + " HTTP/1.1";
			message += ("\r\nHost: " + host);
			message += ("\r\nConnection: " + connection);
			message += "\r\n\r\n";

			Send(System.Text.Encoding.ASCII.GetBytes(message));
		}

		public void Post(string path, string contentType, byte[] postData) {
			isPost = true;

			MultiPartForm.Post(this, "POST " + path + " HTTP/1.1", postData);
		}

		public System.IO.MemoryStream GetResponse() {
			getResponse();
			return resContent;
		}
		
		public void Close() {
			try {
				if (networkStream != null) networkStream.Close();
				if (socket != null) socket.Close();
			} catch (Exception) {}
		}

		internal void SendHeaders(string request) {
			string message = request;
			message += ("\r\nHost: " + host);
			message += ("\r\nConnection: " + connection);
			message += ("\r\nContent-Type: " + contentType);
			message += ("\r\nContent-Length: " + contentLength);
			message += "\r\n\r\n";

			Send(System.Text.Encoding.ASCII.GetBytes(message));
		}

		internal void Send(byte[] data) {
			networkStream.Write(data, 0, data.Length);
		}

		private void getResponse() {
			var buffer = new byte[MaxFragmentSize];

			try {
				int readSize;
				do {
					readSize = networkStream.Read(buffer, 0, MaxFragmentSize);
					if (readSize == 0) break;

					mpcs.Append(buffer, readSize);
				} while (ReceivedData());

				if (((string)curhttpres.Headers["Connection"]).Equals("Close")) {
					Close();
				}
			} catch (System.IO.IOException) {
				// 切断
				Close();
			} catch (NullReferenceException) {}
		}

		protected bool ReceivedData() {
			while (mpcs.CanRead) {
				if (ReadResponseHeaders(ref curhttpres)) {
					if (isPost) {
						resContent = ReadContent(curhttpres);
					}
					return false;
				} else {
					break;
				}
			}
			return true;
		}

		/// <summary>
		/// リクエストヘッダを読み取る
		/// </summary>
		/// <param name="hr">生成した httpreq を返す</param>
		/// <returns>リクエストヘッダの読み込みが完了したか返す</returns>
		private bool ReadResponseHeaders(ref HttpResponse httpres) {
			if (httpres == null) {
				// リクエスト
				string str = mpcs.ReadLineASCII();
				if (str == null) return false;

				httpres = new HttpResponse(str);
			}
			
			// ヘッダ
			string line;
			while ((line = mpcs.ReadLineASCII()) != null) {
				if (line.Length == 0) {
					// 空行なのでヘッダ終わり
					return true;
				}

				var strs = line.Split(':');
				if (strs.Length == 2) {
					httpres.Headers.Add(strs[0].Trim(), strs[1].Trim());
				}
			}

			return false;
		}
		
		/// <summary>
		/// POSTコンテントを読み込んでリクエストにセットする
		/// </summary>
		/// <param name="httpreq">リクエスト</param>
		private System.IO.MemoryStream ReadContent(HttpResponse httpres) {
			var restMemSize = mpcs.data.Length - mpcs.currentPosition;
			var fragment = new byte[MaxFragmentSize];
			
			var contentStream = new MemoryStream();
			contentStream.Write(mpcs.data, mpcs.currentPosition, restMemSize);
			mpcs.Clear();

            int restContentSize = (int)(httpres.ContentLength - restMemSize);
			if (restContentSize < 0) restContentSize = restMemSize;

			while (restContentSize > 0) {
                int readSize = (restContentSize > MaxFragmentSize) ? MaxFragmentSize : restContentSize;
                readSize = networkStream.Read(fragment, 0, readSize);
				if (readSize == 0) break;
				
				contentStream.Write(fragment, 0, readSize);
				restContentSize -= readSize;
			}

			contentStream.Seek(0, SeekOrigin.Begin);

			return contentStream;
		}
	}
}
