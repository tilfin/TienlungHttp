using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TienlungHttp
{
	/// <summary>
	/// Http �T�[�r�X
	/// </summary>
	public class HttpService
	{
		protected HttpServer server;
		private IDictionary<string, IAction> actionmap;
		private IAction fileGet;

		/// <summary>
		/// �R���X�g���N�^
		/// �t�@�C���T�[�o�Ƃ��Ă̂ݓ���
		/// </summary>
		/// <param name="port">�|�[�g</param>
		/// <param name="documentRootPath">�h�L�������g���[�g</param>
		public HttpService(int port, string documentRootPath) {
			server = new HttpServer(port);
			fileGet = new FileGetAction(documentRootPath);
			server.Action = new ResponseAction(OnFileGetAction);
		}

		/// <summary>
		/// �R���X�g���N�^
		/// �A�v���P�[�V�����T�[�o�Ƃ��Ă̂ݓ���
		/// </summary>
		/// <param name="port">�|�[�g</param>
		/// <param name="actionmap">�A�N�V�����}�b�v</param>
        public HttpService(int port, IDictionary<string, IAction> actionmap) {
			server = new HttpServer(port);
			this.actionmap = actionmap;
			server.Action = new ResponseAction(OnApplicationAction);
		}

		/// <summary>
		/// �R���X�g���N�^
		/// �n�C�u���b�h�T�[�o�Ƃ��ē���
		/// </summary>
		/// <param name="port">�|�[�g</param>
		/// <param name="documentRootPath">�h�L�������g���[�g</param>
		/// <param name="actionmap">�A�N�V�����}�b�v</param>
        public HttpService(int port, string documentRootPath, IDictionary<string, IAction> actionmap) {
			server = new HttpServer(port);
			this.actionmap = actionmap;
			fileGet = new FileGetAction(documentRootPath);
			server.Action = new ResponseAction(OnHybridAction);
		}

		/// <summary>
		/// �f�X�g���N�^
		/// </summary>
		~HttpService() {
			if (server.IsRunning) {
				this.Stop();
			}
		}

		/// <summary>
		/// �T�[�r�X�J�n
		/// </summary>
		public void Start() {
			server.Start();
		}

		/// <summary>
		/// �T�[�r�X��~
		/// </summary>
		public void Stop() {
			server.Stop();
		}

		/// <summary>
		/// �t�@�C���T�[�o�p�A�N�V����
		/// </summary>
		/// <param name="httpreq">���N�G�X�g</param>
		/// <param name="httpres">���X�|���X</param>
		protected void OnFileGetAction(HttpRequest httpreq, HttpResponse httpres) {
			try {
				fileGet.Execute(httpreq, httpres);
			} catch (Exception ex) {
				httpres.StatusCode = StatusCode.InternalServerError;
				ReponseErrorScreen(httpres, ex);
			}
		}

		/// <summary>
		/// �A�v���P�[�V�����T�[�o�p�A�N�V����
		/// </summary>
		/// <param name="httpreq">���N�G�X�g</param>
		/// <param name="httpres">���X�|���X</param>
		protected void OnApplicationAction(HttpRequest httpreq, HttpResponse httpres) {
			// Action Mapping ����K�؂ȃA�N�V�����ɑ���
			if (actionmap.ContainsKey(httpreq.Path)) {
				try {
					actionmap[httpreq.Path].Execute(httpreq, httpres);
				} catch (Exception ex) {
					httpres.StatusCode = StatusCode.InternalServerError;
					ReponseErrorScreen(httpres, ex);
				}
				return;
			}

			// ActionMap �� �o�^���Ȃ���Η��p�ł��Ȃ��T�[�r�X
			httpres.StatusCode = StatusCode.ServiceUnavailable;
			ReponseErrorScreen(httpres, null);
		}

		/// <summary>
		/// �n�C�u���b�h�T�[�o�p�A�N�V����
		/// </summary>
		/// <param name="httpreq">���N�G�X�g</param>
		/// <param name="httpres">���X�|���X</param>
		protected void OnHybridAction(HttpRequest httpreq, HttpResponse httpres) {
			try {
				// Action Mapping ����K�؂ȃA�N�V�����ɑ���
				// �Ȃ���΁A�t�@�C�����擾
                if (actionmap.ContainsKey(httpreq.Path)) {
					actionmap[httpreq.Path].Execute(httpreq, httpres);
				} else {
					fileGet.Execute(httpreq, httpres);
				}
			} catch (Exception ex) {
				System.Diagnostics.Debug.WriteLine(ex.StackTrace);

				httpres.StatusCode = StatusCode.InternalServerError;
				ReponseErrorScreen(httpres, ex);
			}
		}

		internal static void ReponseErrorScreen(HttpResponse httpres, Exception ex) {
			var sb = new StringBuilder();

			sb.Append("<html><head><title>");
			sb.Append((int)httpres.StatusCode);
			sb.Append(" ");
			sb.Append(httpres.Message);
			sb.Append("</title></head>");
			sb.Append("<body><h1>");
			sb.Append((int)httpres.StatusCode);
			sb.Append(" ");
			sb.Append(httpres.Message);
			sb.Append("</h1><hr>");

			if (ex != null) {
				sb.Append("<pre>");
				sb.Append(ex.Message);
				sb.Append("\r\n");
				sb.Append(ex.StackTrace);
				sb.Append("</pre>");
			}

			sb.Append("</body></html>");

			byte[] data = Encoding.UTF8.GetBytes(sb.ToString());
			httpres.ContentType = "text/html;charset=utf8";
			httpres.ContentLength = data.LongLength;

			try {
				Stream stream = httpres.GetOutputStream();
				stream.Write(data, 0, data.Length);
			} catch (ObjectDisposedException) {
				// NetworkStream ���j�����ꂽ���Ƃɂ��G���[���������
			}
		}
	}
}
