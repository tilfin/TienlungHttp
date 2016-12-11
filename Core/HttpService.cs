using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TienlungHttp
{
	/// <summary>
	/// Http サービス
	/// </summary>
	public class HttpService
	{
		protected HttpServer server;
		private IDictionary<string, IAction> actionmap;
		private IAction fileGet;

		/// <summary>
		/// コンストラクタ
		/// ファイルサーバとしてのみ動作
		/// </summary>
		/// <param name="port">ポート</param>
		/// <param name="documentRootPath">ドキュメントルート</param>
		public HttpService(int port, string documentRootPath) {
			server = new HttpServer(port);
			fileGet = new FileGetAction(documentRootPath);
			server.Action = new ResponseAction(OnFileGetAction);
		}

		/// <summary>
		/// コンストラクタ
		/// アプリケーションサーバとしてのみ動作
		/// </summary>
		/// <param name="port">ポート</param>
		/// <param name="actionmap">アクションマップ</param>
        public HttpService(int port, IDictionary<string, IAction> actionmap) {
			server = new HttpServer(port);
			this.actionmap = actionmap;
			server.Action = new ResponseAction(OnApplicationAction);
		}

		/// <summary>
		/// コンストラクタ
		/// ハイブリッドサーバとして動作
		/// </summary>
		/// <param name="port">ポート</param>
		/// <param name="documentRootPath">ドキュメントルート</param>
		/// <param name="actionmap">アクションマップ</param>
        public HttpService(int port, string documentRootPath, IDictionary<string, IAction> actionmap) {
			server = new HttpServer(port);
			this.actionmap = actionmap;
			fileGet = new FileGetAction(documentRootPath);
			server.Action = new ResponseAction(OnHybridAction);
		}

		/// <summary>
		/// デストラクタ
		/// </summary>
		~HttpService() {
			if (server.IsRunning) {
				this.Stop();
			}
		}

		/// <summary>
		/// サービス開始
		/// </summary>
		public void Start() {
			server.Start();
		}

		/// <summary>
		/// サービス停止
		/// </summary>
		public void Stop() {
			server.Stop();
		}

		/// <summary>
		/// ファイルサーバ用アクション
		/// </summary>
		/// <param name="httpreq">リクエスト</param>
		/// <param name="httpres">レスポンス</param>
		protected void OnFileGetAction(HttpRequest httpreq, HttpResponse httpres) {
			try {
				fileGet.Execute(httpreq, httpres);
			} catch (Exception ex) {
				httpres.StatusCode = StatusCode.InternalServerError;
				ReponseErrorScreen(httpres, ex);
			}
		}

		/// <summary>
		/// アプリケーションサーバ用アクション
		/// </summary>
		/// <param name="httpreq">リクエスト</param>
		/// <param name="httpres">レスポンス</param>
		protected void OnApplicationAction(HttpRequest httpreq, HttpResponse httpres) {
			// Action Mapping から適切なアクションに託す
			if (actionmap.ContainsKey(httpreq.Path)) {
				try {
					actionmap[httpreq.Path].Execute(httpreq, httpres);
				} catch (Exception ex) {
					httpres.StatusCode = StatusCode.InternalServerError;
					ReponseErrorScreen(httpres, ex);
				}
				return;
			}

			// ActionMap に 登録がなければ利用できないサービス
			httpres.StatusCode = StatusCode.ServiceUnavailable;
			ReponseErrorScreen(httpres, null);
		}

		/// <summary>
		/// ハイブリッドサーバ用アクション
		/// </summary>
		/// <param name="httpreq">リクエスト</param>
		/// <param name="httpres">レスポンス</param>
		protected void OnHybridAction(HttpRequest httpreq, HttpResponse httpres) {
			try {
				// Action Mapping から適切なアクションに託す
				// なければ、ファイルを取得
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
				// NetworkStream が破棄されたことによるエラーを回避する
			}
		}
	}
}
