using System;
using System.Collections.Generic;

namespace TienlungHttp
{
	/// <summary>
	/// HTTP リクエスト
	/// </summary>
	public class HttpRequest : IDisposable
	{
		private IDictionary<string, string> arguments;
		private System.Collections.IDictionary headers;
		private int maxRequests;
		private string requestLine;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public HttpRequest() {
			headers = new System.Collections.Hashtable();
		}

		internal string EndPointInfo;

		/// <summary>
		/// リクエストの1行目
		/// </summary>
		public string Request {
			set {
				requestLine = value;

				var strs = requestLine.Trim().Split(' ');
				Method = strs[0];
				
				if (strs.Length > 1) {
                    ParsePath(strs[1]);
				}

				if (strs.Length > 2) {
					HttpVersion = strs[2];
				}
			}
			get { return requestLine; }
		}

		/// <summary>
		/// メソッド
		/// </summary>
		public string Method { get; private set; }

		/// <summary>
		/// リクエストのパス
		/// ?以降の引数は含まない。
		/// </summary>
		public string Path { get; private set; }

		/// <summary>
		/// リクエスト引数
		/// キーがパラメータ名
		/// </summary>
		public IDictionary<string, string> Arguments {
            get {
                return arguments;
            }
        }

		/// <summary>
		/// HTTP バージョン
		/// </summary>
		public string HttpVersion { get; private set; }

		/// <summary>
		/// Keep Alive かどうか示す。設定する。
		/// </summary>
		public bool KeepAlive { get; set; }

        /// <summary>
        /// Connection 値
        /// </summary>
        public string Connection { get; set; } = null;

        /// <summary>
        /// MaxKeepAliveRequests 値
        /// </summary>
        public int MaxKeepAliveRequests {
			get { return maxRequests;  }
			set { maxRequests = value; }
		}

		/// <summary>
		/// コンテントの長さ
		/// </summary>
		public long ContentLength {
			get {
				if (headers.Contains("Content-Length")) {
					return Convert.ToInt64(headers["Content-Length"]);
				} else {
					return -1;
				}
			}
		}

        /// <summary>
        /// ヘッダのマップ
        /// </summary>
		public System.Collections.IDictionary Headers {
            get { return headers; }
        }

        /// <summary>
        /// QUERY_STRING
        /// </summary>
        public string QueryString { get; private set; }

		/// <summary>
		/// POST されたコンテントストリームを返す。
		/// </summary>
		/// <returns></returns>
		public System.IO.Stream ContentStream { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void AddHeader(string name, string value) {
            headers.Add(name, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetHeaderValue(string name) {
            return headers?[name] as string;
        }

		#region IDisposable メンバ

		public void Dispose() {
            QueryString = null;
			headers.Clear();
			if (ContentStream != null)
				ContentStream.Close();
		}

        #endregion

        private void ParsePath(string uri) {
            var hatenaPos = uri.IndexOf('?');
            if (hatenaPos < 0) {
                Path = uri;
                return;
            }
            Path = uri.Substring(0, hatenaPos);

            var qs = uri.Substring(hatenaPos + 1);
            QueryString = qs;
            var args = qs.Split('&');

            arguments = new Dictionary<string, string>();
            for (int i = 0; i < args.Length; i++) {
                string[] entry = args[i].Split('=');
                if (entry.Length == 2) {
                    arguments.Add(entry[0], entry[1]);
                }
            }
        }
    }
}
