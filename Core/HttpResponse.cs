using System;
using System.IO;

namespace TienlungHttp
{
	/// <summary>
	/// HTTP レスポンス
	/// </summary>
	public class HttpResponse
	{
		private const string CONTENT_TYPE = "Content-Type";
		private const string CONTENT_LENGTH = "Content-Length";
		private const string CONTENT_ENCODING = "Content-Encoding";
        
		private bool hasWrittenHeader = false;
		private long contentLength = -1;
        
		private System.Collections.IDictionary headers = new System.Collections.Hashtable();
		private Stream writeStream;
		
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public HttpResponse() {}
		public HttpResponse(HttpRequest httpreq) {
			headers.Add("Server", HttpServer.SERVER);

			if (httpreq.Connection != null) {
				headers.Add("Keep-Alive", "timeout=" + HttpServer.KeepAliveTimeOut.ToString()
					+ ", max=" + httpreq.MaxKeepAliveRequests.ToString());

				if (httpreq.Connection.ToLower().Equals("keep-alive")) {
					headers.Add("Connection", "Keep-Alive");
				} else if (httpreq.Connection.ToLower().Equals("close")) {
					headers.Add("Connection", "close");
				}
			}

			HttpVersion = httpreq.HttpVersion;
		}

		public HttpResponse(string accept) {
			var accepts = accept.Split(' ');
            HttpVersion = accepts[0];
			StatusCode = (StatusCode)Convert.ToInt32(accepts[1]);
		}

		/// <summary>
		/// 応答コード
		/// </summary>
		public StatusCode StatusCode { get; set; } = StatusCode.OK;

        /// <summary>
        /// 応答メッセージ
        /// </summary>
        public string Message {
			get { return StatusCode.ToString().Replace('_', ' ');  }
		}

		/// <summary>
		/// HTTP バージョン
		/// </summary>
		public string HttpVersion { get; set; }

		/// <summary>
		/// レスポンスの1行目
		/// </summary>
		public string Response {
			get {
				return HttpVersion + " " + ((int)StatusCode).ToString() + " " + this.Message;
			}
		}

		/// <summary>
		/// Content-Encoding
		/// </summary>
		public string ContentEncoding {
			set {
                headers.Add(CONTENT_ENCODING, value);
			}
			get {
				if (headers.Contains(CONTENT_ENCODING)) {
					return (string)headers[CONTENT_ENCODING];
				} else {
					return null;
				}
			}
		}

		/// <summary>
		/// Content-Type
		/// </summary>
		public string ContentType {
			set {
    			headers.Add(CONTENT_TYPE, value);
            }
            get {
                if (headers.Contains(CONTENT_TYPE)) {
                    return (string)headers[CONTENT_TYPE];
                } else {
                    return null;
                }
            }
        }

		/// <summary>
		/// Content-Length
		/// </summary>
		public long ContentLength {
			set {
				contentLength = value;
				if (headers.Contains(CONTENT_LENGTH)) {
					headers[CONTENT_LENGTH] = contentLength;
				} else {
					headers.Add(CONTENT_LENGTH, contentLength);
				}
			}
			get {
				return contentLength;
			}
		}

		public System.Collections.IDictionary Headers {
			get { return headers; }
		}

		public void SetHeader(string name, string value) {
			headers.Add(name, value);
		}

        public Stream GetOutputStreamForCGI() {
            WriteHeader(false);

			if ("gzip".Equals(this.ContentEncoding)) {
				return new System.IO.Compression.GZipStream(writeStream, System.IO.Compression.CompressionMode.Compress);
			} else {
				return writeStream;
			}
        }
		
		public Stream GetOutputStream() {
            WriteHeader(true);

			if ("gzip".Equals(this.ContentEncoding)) {
				return new System.IO.Compression.GZipStream(writeStream, System.IO.Compression.CompressionMode.Compress);
			} else {
				return writeStream;
			}
		}
		
		internal void SetWriteStream(Stream stream) {
			this.writeStream = stream;
		}

		/// <summary>
		/// フラッシュしないように
		/// </summary>
		private void WriteHeader(bool lastCrlf) {
			byte[] data;
			using (var ms = new MemoryStream()) {
				var sw = new StreamWriter(ms);
				sw.NewLine = "\r\n";
				sw.WriteLine(this.Response);
				foreach (System.Collections.DictionaryEntry entry in headers) {
					sw.WriteLine("{0}: {1}", entry.Key, entry.Value);
				}
				if (lastCrlf) sw.WriteLine();
				sw.Flush();
				data = ms.ToArray();
			}

			writeStream.Write(data, 0, data.Length);

			hasWrittenHeader = true;
		}

        /// <summary>
        /// 閉じる
        /// </summary>
		public void Close() {
			try {
				if (!hasWrittenHeader) {
					if (!headers.Contains(CONTENT_LENGTH)) {
						this.ContentLength = 0;
					}
					WriteHeader(true);
				}
			} catch (ObjectDisposedException) {
			}
			headers.Clear();
		}
	}
}
