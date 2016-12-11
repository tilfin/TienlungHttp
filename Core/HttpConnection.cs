using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace TienlungHttp
{
	public delegate void RequestReceivedEventHandler(HttpConnection link, HttpRequest httpreq);
	public delegate void ResponseAction(HttpRequest httpreq, HttpResponse httpres);

	/// <summary>
	/// HTTP �R�l�N�V����
	/// </summary>
	public class HttpConnection : IDisposable
	{
		/// <summary>
		/// �����A�N�V�����̃f���Q�[�g
		/// </summary>
		public ResponseAction Action;

		/// <summary>
		/// ���X�i�[�X���b�h
		/// </summary>
		internal Thread ListenerThread;
		
		protected bool disposed = false;

		/// <summary>
		/// �l�b�g���[�N�X�g���[��������Ɏ󂯎��ő�f�[�^�T�C�Y
		/// </summary>
		private const int MAX_FRAGMENT_SIZE = 5120;
		
		private Socket socket;
		private NetworkStream networkStream;
		private int restSerialReqCount = HttpServer.MaxKeepAliveRequests;
		private Pipeline pipeline;
		private HttpRequest curhttpreq = null;
		private MultiPartContentStream mpcs;
		private bool flagReadCompleted = false;
		private bool usePipeline = false;
        private bool isClosing = false;

		/// <summary>
		/// �ڑ��������Ƃ��ɋN����C�x���g
		/// </summary>
		public event EventHandler Closed;

		/// <summary>
		/// �R���X�g���N�^
		/// </summary>
		public HttpConnection(Socket socket) {
			this.socket = socket;
			this.networkStream = new NetworkStream(socket);

			mpcs = new MultiPartContentStream();
		}

		#region Disposing

		void IDisposable.Dispose () {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void Dispose (bool disposing) {
			if (!disposed) {
				disposed = true;
				if (disposing) {
					// release managed resources
				}

				// release unmanaged resources
				NetworkStream n = networkStream;
				networkStream = n;
				if (n != null) {
					n.Close();
				}

				Socket s = socket;
				socket = null;
				if (s != null) {
					s.Close();
				}
			}
		}
		
		~HttpConnection () {
			Dispose(false);
		}

		#endregion

		public EndPoint RemoteEndPoint {
			get { return socket.RemoteEndPoint; }
		}

		/// <summary>
		/// Listen �p�̃X���b�h�𗧂��グ�A
		/// �K�v�Ȃ�΃p�C�v���C�������ł���悤�ɂ���B
		/// </summary>
		public void Activate(bool usePipeline){
			var threadStart = new ThreadStart(Listening);
			ListenerThread = new Thread(threadStart);
			ListenerThread.Name = socket.RemoteEndPoint.ToString();

			this.usePipeline = usePipeline;
			if (usePipeline) {
				pipeline = new Pipeline(ListenerThread.Name, new ThreadStart(Responsing));
				pipeline.Start();
			}
		}

		private void Listening() {
			var buffer = new byte[MAX_FRAGMENT_SIZE];

			try {
				int readSize;
				do {
					readSize = networkStream.Read(buffer, 0, MAX_FRAGMENT_SIZE);
					if (readSize == 0) break;

					mpcs.Append(buffer, readSize);

				} while (ReceivedData());

				if (usePipeline) {
					flagReadCompleted = true;
				} else {
					Close();
				}

			} catch (IOException) {
#if DEBUG
				Console.WriteLine(ListenerThread.Name + " - Keep Alive Time out.");
#endif
				// �ؒf
				Close();
			} catch (NullReferenceException ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
		}

        private bool isKeepAlive = false;

		protected bool ReceivedData() {
			while (mpcs.CanRead) {
				if (ReadRequestHeaders(ref curhttpreq)) {

					// �p���I�ڑ��������߂�
					bool flagContinue = false;
					if (curhttpreq.Headers.Contains("Connection")) {
						curhttpreq.Connection = (string)curhttpreq.Headers["Connection"];
						curhttpreq.Headers.Remove("Connection");
						if (curhttpreq.Connection.ToLower().Equals("keep-alive")) {
							curhttpreq.MaxKeepAliveRequests = restSerialReqCount--;
							curhttpreq.KeepAlive = true;
							flagContinue = true;
                            isKeepAlive = true;
						} else if (curhttpreq.Connection.ToLower().Equals("close")) {
							curhttpreq.MaxKeepAliveRequests = restSerialReqCount--;
							curhttpreq.KeepAlive = false;
							flagContinue = false;
						}
                    } else if (isKeepAlive) {
                        // ���� Keep-Alive �ɃZ�b�g����Ă���Ƃ�
                        curhttpreq.MaxKeepAliveRequests = restSerialReqCount--;
                        curhttpreq.KeepAlive = true;
                        flagContinue = true;
					} else {
						// GET * HTTP 1.1 �Ȃ�f�t�H���g�p��
						flagContinue = curhttpreq.Method.Equals(HttpMethod.GET) && "HTTP/1.1".Equals(curhttpreq.HttpVersion);
					}

					if (curhttpreq.Method.Equals(HttpMethod.GET)) {
						if (usePipeline) {
							// �p�C�v���C���̃L���[�ɉ�������
							pipeline.PutRequest(curhttpreq);
						} else {
							OnRequestReceived(curhttpreq);
						}

					} else if (curhttpreq.Method.Equals(HttpMethod.POST)) {
						// �R���e���g��ǂݍ���
						ReadPostContent(curhttpreq);

						if (usePipeline) {
							// �p�C�v���C���̃L���[�ɉ�������
							pipeline.PutRequest(curhttpreq);
						} else {
							OnRequestReceived(curhttpreq);
						}
					}

					curhttpreq = null;
					if (!flagContinue) return false;
				} else {
					break;
				}
			}

			return true;
		}

		/// <summary>
		/// ���N�G�X�g�w�b�_��ǂݎ��
		/// </summary>
		/// <param name="sr">�X�g���[�����[�_�[</param>
		/// <param name="hr">�������� httpreq ��Ԃ�</param>
		/// <returns>���N�G�X�g�w�b�_�̓ǂݍ��݂������������Ԃ�</returns>
		private bool ReadRequestHeaders(ref HttpRequest httpreq) {
			if (httpreq == null) {
				// ���N�G�X�g
				string str = mpcs.ReadLineASCII();
				if (str == null) return false;

				httpreq = new HttpRequest();
				httpreq.EndPointInfo = this.socket.RemoteEndPoint.ToString();
				httpreq.Request = str;
			}
			
			// �w�b�_
			string line;
			while ((line = mpcs.ReadLineASCII()) != null) {
				if (line.Length == 0) {
					// ��s�Ȃ̂Ńw�b�_�I���
					return true;
				}

				string[] strs = line.Split(':');
				if (strs.Length == 2) {
					httpreq.Headers.Add(strs[0].Trim(), strs[1].Trim());
				}
			}

			return false;
		}

		/// <summary>
		/// POST�R���e���g��ǂݍ���Ń��N�G�X�g�ɃZ�b�g����
		/// </summary>
		/// <param name="httpreq">���N�G�X�g</param>
		private void ReadPostContent(HttpRequest httpreq) {
			int restMemSize = mpcs.data.Length - mpcs.currentPosition;

			var fragment = new byte[MAX_FRAGMENT_SIZE];
			
			var contentStream = new MemoryStream();
			contentStream.Write(mpcs.data, mpcs.currentPosition, restMemSize);
			mpcs.Clear();

			long restContentSize = httpreq.ContentLength - restMemSize;
			if (restContentSize < 0) restContentSize = restMemSize;

			while (restContentSize > 0) {
				int readSize;
				if (restContentSize >= MAX_FRAGMENT_SIZE) {
					readSize = networkStream.Read(fragment, 0, MAX_FRAGMENT_SIZE);
				} else {
					readSize = networkStream.Read(fragment, 0, (int)restContentSize);
				}

				if (readSize == 0) break;
				
				contentStream.Write(fragment, 0, readSize);
				restContentSize -= readSize;
			}

			contentStream.Seek(0, SeekOrigin.Begin);
            httpreq.ContentStream = contentStream;
		}


		private void Responsing() {
			while (pipeline != null && pipeline.IsAlive) {
				object obj = pipeline.TakeRequest();
				if (obj == null) break;

				OnRequestReceived((HttpRequest)obj);

				if (flagReadCompleted && pipeline.Count == 0) break;
			}
			
			// �ؒf
			Close();
		}

		protected void OnRequestReceived(HttpRequest httpreq) {
			var httpres = new HttpResponse(httpreq);

			httpres.SetWriteStream(networkStream);
			Action(httpreq, httpres);

			try {
				if (socket != null) {
					TienlungHttp.Logging.Logger.Instance.WriteLine(httpreq, httpres);
				}
			} catch (Exception ex) {
				Console.WriteLine(ex.StackTrace);
			}

			httpres.Close();
		}

		protected bool canWriteStream {
			get { return networkStream.CanWrite; }
		}
		

		/// <summary>
		/// �X�g���[����S�ĕ���
		/// </summary>
		public void Close(){
            if (isClosing) return;
            isClosing = true;

			try {
				if (pipeline != null) {
					pipeline.Close();
					pipeline = null;
				}

				if (networkStream != null) networkStream.Close();

                if (socket != null) {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
			} catch (SocketException ex){
				Console.WriteLine(ex.Message);
			} catch (Exception ex){
				Console.WriteLine(ex.Message);
			} finally {
				networkStream = null;
				socket = null;
			}

			if (Closed != null) {
				Closed(this, EventArgs.Empty);
			}
		}
	}
}
