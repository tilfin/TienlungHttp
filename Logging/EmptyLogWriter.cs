using System;

namespace TienlungHttp.Logging
{
	/// <summary>
	/// Log output nothing
	/// </summary>
	public class EmptyLogWriter : ILogWriter
	{
		public EmptyLogWriter() {
		}

		#region ILogWriter �����o

		public void WriteLine(string format, params object[] args) {
		}

		public void Flush() {
		}

		public void Close() {
		}

		#endregion

		#region IDisposable �����o

		public void Dispose() {
		}

		#endregion
	}
}
