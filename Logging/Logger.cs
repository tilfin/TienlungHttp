using System;
using System.IO;

namespace TienlungHttp.Logging
{
	/// <summary>
	/// ロガー
	/// </summary>
	public class Logger
	{
		public static string LogDirPath = null;

		private const string ACCESS_LOG = "access.log";
		private const string ERROR_LOG = "error.log";

		private static Logger instance;
		private ILogWriter accesslogWriter;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		private Logger() {
			if (LogDirPath != null) {
				accesslogWriter = new FileLogWriter(Path.Combine(LogDirPath, ACCESS_LOG));
			} else {
				accesslogWriter = new ConsoleLogWriter();
			}
		}

		/// <summary>
		/// デストラクタ
		/// </summary>
		~Logger() {
			if (accesslogWriter != null) {
				accesslogWriter.Close();
			}
		}

		/// <summary>
		/// インスタンス
		/// </summary>
		public static Logger Instance {
			get {
				if (instance == null) {
					instance = new Logger();
				}
				return instance;
			}
		}

		/// <summary>
		/// 書き出す
		/// </summary>
		/// <param name="request">リクエスト</param>
		/// <param name="response">リスボンス</param>
		public void WriteLine(HttpRequest request, HttpResponse response) {
			accesslogWriter.WriteLine("{0} - - [{1}] \"{2}\" {3} {4}", 
				request.EndPointInfo, DateTime.Now.ToString("dd/MMM/yyyy:hh:mm:ss zz00"),
				request.Request,
				Convert.ToString((int)response.StatusCode),
				response.ContentLength.ToString());
			accesslogWriter.Flush();
		}
	}
}
