using System;
using System.IO;
using System.Net.Sockets;

namespace TienlungHttp
{
	/// <summary>
	/// HTTP �N���C�A���g�i�s�g�p�j
	/// </summary>
	public class HttpClient
	{
		private Socket socket;
		private NetworkStream networkStream;
		private HttpResponse curhttpres = null;
		private MultiPartContentStream mpcs;

		private string connection;
		private MemoryStream resContent;
		private string host;
		private string contentType;
		private string contentLength;
		private bool isPost = false;
		private int timeout;

		/// <summary>
		/// �R���X�g���N�^
		/// </summary>
		public HttpClient() {
			mpcs = new MultiPartContentStream();
		}

        /// <summary>
		/// �l�b�g���[�N�X�g���[��������Ɏ󂯎��ő�f�[�^�T�C�Y
		/// </summary>
		public int MaxFragmentSize { get; set; }

        /// <summary>
        /// �^�C���A�E�g
        /// </summary>
        public int TimeOut {
			set { timeout = value; }
		}

		/// <summary>
		/// �L�[�v�A���C�u
		/// </summary>
		public bool KeepAlive {
			set {
				if (value) {
					connection = "Keep-Alive";
				} else {
					connection = "Close";
				}
			}
		}

		/// <summary>
		/// �R���e���g�^�C�v
		/// </summary>
		public string ContentType {
			set { contentType = "Content-Type: " + value; }
		}

		/// <summary>
		/// �R���e���g��
		/// </summary>
		public long ContentLength {
			set { contentLength = "Content-Length: " + value.ToString(); }
		}

		public bool Open(string server, int port) {
			this.host = "Host: " + server;

			//�z�X�g������IP�A�h���X���擾
			var hostadd = System.Net.Dns.GetHostEntry(server).AddressList[0];

			//IPEndPoint���擾
			var ephost = new System.Net.IPEndPoint(hostadd, port);

			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, timeout);
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout);
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, MaxFragmentSize);
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, MaxFragmentSize);

			socket.Connect(ephost);
			if (socket.Connected) {
				networkStream = new NetworkStream(socket);
				return true;
			} else {
				return false;
			}
		}

		public void GET(string path) {
			isPost = false;

			string message = "GET " + path + " HTTP/1.1";
			message += ("\r\nHost: " + host);
			message += ("\r\nConnection: " + connection);
			message += "\r\n\r\n";

			Send(System.Text.Encoding.ASCII.GetBytes(message));
		}

		public void Post(string path, string contentType, byte[] postData) {
			isPost = true;

			MultiPartForm.Post(this, "POST " + path + " HTTP/1.1", postData);
		}

		public System.IO.MemoryStream GetResponse() {
			getResponse();
			return resContent;
		}
		
		public void Close() {
			try {
				if (networkStream != null) networkStream.Close();
				if (socket != null) socket.Close();
			} catch (Exception) {}
		}

		internal void SendHeaders(string request) {
			string message = request;
			message += ("\r\nHost: " + host);
			message += ("\r\nConnection: " + connection);
			message += ("\r\nContent-Type: " + contentType);
			message += ("\r\nContent-Length: " + contentLength);
			message += "\r\n\r\n";

			Send(System.Text.Encoding.ASCII.GetBytes(message));
		}

		internal void Send(byte[] data) {
			networkStream.Write(data, 0, data.Length);
		}

		private void getResponse() {
			var buffer = new byte[MaxFragmentSize];

			try {
				int readSize;
				do {
					readSize = networkStream.Read(buffer, 0, MaxFragmentSize);
					if (readSize == 0) break;

					mpcs.Append(buffer, readSize);
				} while (ReceivedData());

				if (((string)curhttpres.Headers["Connection"]).Equals("Close")) {
					Close();
				}
			} catch (System.IO.IOException) {
				// �ؒf
				Close();
			} catch (NullReferenceException) {}
		}

		protected bool ReceivedData() {
			while (mpcs.CanRead) {
				if (ReadResponseHeaders(ref curhttpres)) {
					if (isPost) {
						resContent = ReadContent(curhttpres);
					}
					return false;
				} else {
					break;
				}
			}
			return true;
		}

		/// <summary>
		/// ���N�G�X�g�w�b�_��ǂݎ��
		/// </summary>
		/// <param name="hr">�������� httpreq ��Ԃ�</param>
		/// <returns>���N�G�X�g�w�b�_�̓ǂݍ��݂������������Ԃ�</returns>
		private bool ReadResponseHeaders(ref HttpResponse httpres) {
			if (httpres == null) {
				// ���N�G�X�g
				string str = mpcs.ReadLineASCII();
				if (str == null) return false;

				httpres = new HttpResponse(str);
			}
			
			// �w�b�_
			string line;
			while ((line = mpcs.ReadLineASCII()) != null) {
				if (line.Length == 0) {
					// ��s�Ȃ̂Ńw�b�_�I���
					return true;
				}

				var strs = line.Split(':');
				if (strs.Length == 2) {
					httpres.Headers.Add(strs[0].Trim(), strs[1].Trim());
				}
			}

			return false;
		}
		
		/// <summary>
		/// POST�R���e���g��ǂݍ���Ń��N�G�X�g�ɃZ�b�g����
		/// </summary>
		/// <param name="httpreq">���N�G�X�g</param>
		private System.IO.MemoryStream ReadContent(HttpResponse httpres) {
			var restMemSize = mpcs.data.Length - mpcs.currentPosition;
			var fragment = new byte[MaxFragmentSize];
			
			var contentStream = new MemoryStream();
			contentStream.Write(mpcs.data, mpcs.currentPosition, restMemSize);
			mpcs.Clear();

            int restContentSize = (int)(httpres.ContentLength - restMemSize);
			if (restContentSize < 0) restContentSize = restMemSize;

			while (restContentSize > 0) {
                int readSize = (restContentSize > MaxFragmentSize) ? MaxFragmentSize : restContentSize;
                readSize = networkStream.Read(fragment, 0, readSize);
				if (readSize == 0) break;
				
				contentStream.Write(fragment, 0, readSize);
				restContentSize -= readSize;
			}

			contentStream.Seek(0, SeekOrigin.Begin);

			return contentStream;
		}
	}
}
