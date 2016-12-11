using System;
using System.IO;

namespace TienlungHttp.Logging
{
	/// <summary>
	/// ���K�[
	/// </summary>
	public class Logger
	{
		public static string LogDirPath = null;

		private const string ACCESS_LOG = "access.log";
		private const string ERROR_LOG = "error.log";

		private static Logger instance;
		private ILogWriter accesslogWriter;

		/// <summary>
		/// �R���X�g���N�^
		/// </summary>
		private Logger() {
			if (LogDirPath != null) {
				accesslogWriter = new FileLogWriter(Path.Combine(LogDirPath, ACCESS_LOG));
			} else {
				accesslogWriter = new ConsoleLogWriter();
			}
		}

		/// <summary>
		/// �f�X�g���N�^
		/// </summary>
		~Logger() {
			if (accesslogWriter != null) {
				accesslogWriter.Close();
			}
		}

		/// <summary>
		/// �C���X�^���X
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
		/// �����o��
		/// </summary>
		/// <param name="request">���N�G�X�g</param>
		/// <param name="response">���X�{���X</param>
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
