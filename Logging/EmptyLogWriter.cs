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

		#region ILogWriter メンバ

		public void WriteLine(string format, params object[] args) {
		}

		public void Flush() {
		}

		public void Close() {
		}

		#endregion

		#region IDisposable メンバ

		public void Dispose() {
		}

		#endregion
	}
}
