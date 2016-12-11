using System;
using System.Collections.Generic;

namespace TienlungHttp
{
	/// <summary>
	/// HTTP ���N�G�X�g
	/// </summary>
	public class HttpRequest : IDisposable
	{
		private IDictionary<string, string> arguments;
		private System.Collections.IDictionary headers;
		private int maxRequests;
		private string requestLine;

		/// <summary>
		/// �R���X�g���N�^
		/// </summary>
		public HttpRequest() {
			headers = new System.Collections.Hashtable();
		}

		internal string EndPointInfo;

		/// <summary>
		/// ���N�G�X�g��1�s��
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
		/// ���\�b�h
		/// </summary>
		public string Method { get; private set; }

		/// <summary>
		/// ���N�G�X�g�̃p�X
		/// ?�ȍ~�̈����͊܂܂Ȃ��B
		/// </summary>
		public string Path { get; private set; }

		/// <summary>
		/// ���N�G�X�g����
		/// �L�[���p�����[�^��
		/// </summary>
		public IDictionary<string, string> Arguments {
            get {
                return arguments;
            }
        }

		/// <summary>
		/// HTTP �o�[�W����
		/// </summary>
		public string HttpVersion { get; private set; }

		/// <summary>
		/// Keep Alive ���ǂ��������B�ݒ肷��B
		/// </summary>
		public bool KeepAlive { get; set; }

        /// <summary>
        /// Connection �l
        /// </summary>
        public string Connection { get; set; } = null;

        /// <summary>
        /// MaxKeepAliveRequests �l
        /// </summary>
        public int MaxKeepAliveRequests {
			get { return maxRequests;  }
			set { maxRequests = value; }
		}

		/// <summary>
		/// �R���e���g�̒���
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
        /// �w�b�_�̃}�b�v
        /// </summary>
		public System.Collections.IDictionary Headers {
            get { return headers; }
        }

        /// <summary>
        /// QUERY_STRING
        /// </summary>
        public string QueryString { get; private set; }

		/// <summary>
		/// POST ���ꂽ�R���e���g�X�g���[����Ԃ��B
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

		#region IDisposable �����o

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
