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

		#region ILogWriter ƒƒ“ƒo

		public void WriteLine(string format, params object[] args) {
		}

		public void Flush() {
		}

		public void Close() {
		}

		#endregion

		#region IDisposable ƒƒ“ƒo

		public void Dispose() {
		}

		#endregion
	}
}
