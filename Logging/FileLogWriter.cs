using System;
using System.IO;

namespace TienlungHttp.Logging
{
	/// <summary>
	/// ファイルにログを書き出します。
	/// </summary>
	public class FileLogWriter : ILogWriter
	{
		private StreamWriter sw;

		public FileLogWriter(string fileName) {
			sw = new StreamWriter(fileName, true);
		}

		#region ILogWriter メンバ

		public void WriteLine(string format, params object[] args) {
			sw.WriteLine(format, args);
		}

		public void Flush() {
			sw.Flush();
		}

		public void Close() {
            try {
                sw.Close();
            } catch (ObjectDisposedException) { }
		}

		#endregion

		#region IDisposable メンバ

		public void Dispose() {
			sw.Close();
		}

		#endregion
	}
}
