using System;

namespace TienlungHttp.Logging
{
	/// <summary>
	/// LogWriter Interface
	/// </summary>
	public interface ILogWriter : IDisposable
	{
		void WriteLine(string format, params object[] args);
		void Flush();
		void Close();
	}
}
