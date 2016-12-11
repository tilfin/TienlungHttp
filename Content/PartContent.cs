using System;

namespace TienlungHttp
{
	/// <summary>
	/// MultiPartコンテンツの一部分
	/// </summary>
	public class PartContent
	{
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="headers">ヘッダ</param>
        /// <param name="contentStream">コンテントストリーム</param>
		internal PartContent(System.Collections.IDictionary headers, System.IO.MemoryStream contentStream) {
            Headers = headers;
            ContentStream = contentStream;
		}

        /// <summary>
        /// ヘッダ
        /// </summary>
		public System.Collections.IDictionary Headers { get; private set; }

        /// <summary>
        /// コンテントストリーム
        /// </summary>
		public System.IO.MemoryStream ContentStream { get; private set; }
    }
}
