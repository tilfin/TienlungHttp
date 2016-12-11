using System;

namespace TienlungHttp.Logging
{
    /// <summary>
    /// 
    /// </summary>
    class ConsoleLogWriter : ILogWriter
    {
        public ConsoleLogWriter() {
        }

        #region ILogWriter メンバ

        public void WriteLine(string format, params object[] args) {
            Console.WriteLine(format, args);
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
