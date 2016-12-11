using System;
using System.IO;

namespace TienlungHttp
{
	/// <summary>
	/// MultiPartContent の概要の説明です。
	/// </summary>
	public class MultiPartContentReader : MultiPartContentStream
	{
		private readonly byte MINUS = (byte)'-';

		protected readonly char[] FIELD_DELIMITER = new char[] {':'};
		protected System.Collections.Hashtable headers;

		private string boundary = null;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="contentStream"></param>
		public MultiPartContentReader(MemoryStream contentStream) : base (contentStream) {
		}
		
		public PartContent GetPart() {
			return new PartContent(ReadHeader(), ReadContent());
		}
		
		/// <summary>
		/// パートをロードする
		/// </summary>
		private System.Collections.IDictionary ReadHeader() {
			string itemstr = String.Empty;
			string headline;

			var headers = new System.Collections.Hashtable();

			while ((headline = ReadLineASCII()) != null) {
				if (boundary == null && headline.StartsWith("--")) {
					boundary = headline.Substring(2);
					continue;
				}

				if (headline.Length == 0) {
					addHeader(headers, itemstr);
					break;
				}

				if (headline[0] == ' ' || headline[0] == '\t') {
					itemstr += headline;
				} else {
					addHeader(headers, itemstr);
					itemstr = headline;
				}
			}

			return headers;
		}

		/// <summary>
		/// フィールド追加
		/// </summary>
		/// <param name="str"></param>
		private void addHeader(System.Collections.Hashtable headers, string str) {
			if (str.Length == 0) return;

			string[] strs = str.Split(FIELD_DELIMITER, 2);

			if (strs.Length == 2) {
				string fieldName = strs[0].Trim();
				string fieldValue = strs[1].Trim();
				
				if (headers.Contains(fieldName)) {
					headers[fieldName] = ((string)headers[fieldName]) + fieldValue;
				} else {
					headers.Add(fieldName, fieldValue);
				}
			}
		}

		private System.IO.MemoryStream ReadContent() {
			const int BUFFER_SIZE = 4096;
			var buffer = new byte[BUFFER_SIZE];
			var content = new MemoryStream();

			int readSize;
			int writeSize;
			int offset = 0;
			bool notCompleted = true;
			
			while (notCompleted && (readSize = ReadBytes(buffer, offset, BUFFER_SIZE - offset)) > 0) {
				readSize += offset;
				int checkSize = readSize - boundary.Length;
				writeSize = checkSize;
				for (int i = 1; i < checkSize; i++) {
					if (buffer[i-1] == MINUS && buffer[i] == MINUS) {
						string line = System.Text.Encoding.ASCII.GetString(buffer, i+1, boundary.Length);
						if (line.Equals(boundary)) {
							// 終了
							writeSize = i - 1;
							if (i > 3 && buffer[i-3] == (byte)'\r' && buffer[i-2] == (byte)'\n') {
								// CRLF
								writeSize -= 2;
							}
							notCompleted = false;
							break;
						}
					}
				}

				content.Write(buffer, 0, writeSize);

				offset = readSize - writeSize;
				if (offset > 0) {
					Array.Copy(buffer, writeSize, buffer, 0, offset);
				}
			}

			content.Seek(0, SeekOrigin.Begin);

			return content;
		}
	}
}
