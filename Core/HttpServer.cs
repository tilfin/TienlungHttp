using System;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections.Generic;

namespace TienlungHttp
{
	/// <summary>
	/// 接続発生に関するイベントハンドラ
	/// </summary>
	public delegate void ConnectEventHandler(HttpConnection connection);

	/// <summary>
	/// HTTP サーバ
	/// </summary>
	public class HttpServer : IDisposable
    {
        /// <summary>
        /// サーバを表す文字列
        /// </summary>
		public static readonly string SERVER = "Tienlung Http Server/0.9.0 (Windows .NET Framework 2.0)";

		public static int TimeOut = 300000;           // 300s
		public static int KeepAliveTimeOut = 30000;   //  30s
		public static int MaxKeepAliveRequests = 100;

		private bool isRunning = false;
		private bool disposed = false;
		private bool usePipelineConnection = false;

		private Thread serverThread;
		private ManualResetEvent allDone;
		private const int MAX_SOCKETS = 50;
        private List<HttpConnection> connections;
		private int port = 80;

		/// <summary>
		/// 接続が発生したとき走るイベント
		/// </summary>
		public event ConnectEventHandler Connected;

		/// <summary>
		/// 接続が発生したとき走るイベント
		/// </summary>
		public event ConnectEventHandler Disconnected;

		/// <summary>
		/// アクション
		/// </summary>
		internal ResponseAction Action;

		public HttpServer(int port) {
			this.port = port;
			connections = new List<HttpConnection>(MAX_SOCKETS);
		}

		#region Disposing

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose(bool disposing) {
			if (!disposed) {
				disposed = true;
				if (disposing) {
					// release managed resources
					serverThread = null;
					connections = null;
				}

				// release unmanaged resources
				if (allDone != null) {
					allDone.Close();
					allDone = null;
				}
			}
		}
		
		~HttpServer() {
			Dispose(false);
		}

		#endregion

		/// <summary>
		/// サーバが起動中か示す
		/// </summary>
		public bool IsRunning {
			get { return isRunning; }
		}

		/// <summary>
		/// パイプライン処理を使うかどうか示すまたは設定する。
		/// </summary>
		public bool UsePipelineConnection {
			get { return usePipelineConnection;  }
			set { usePipelineConnection = value; }
		}

		/// <summary>
		/// サーバを開始する
		/// </summary>
		public void Start() {
			allDone = new ManualResetEvent(false);

			isRunning = true;

			serverThread = new Thread(new ThreadStart(Accepting));
			serverThread.Name = "Http Server";
			serverThread.IsBackground = true;
			serverThread.Start();
		}

		/// <summary>
		/// サーバを停止する
		/// </summary>
		public void Stop() {
			isRunning = false;
			if (allDone != null && !allDone.SafeWaitHandle.IsClosed)
				allDone.Set();

			while (connections.Count > 0){
				HttpConnection connection = connections[0];
				connection.Close();
			}
		}

	#region "Accept"

		private void Accepting(){
			var localEndPoint = new IPEndPoint(IPAddress.Any, port);

			using (var listener = new Socket(AddressFamily.InterNetwork,
				SocketType.Stream, ProtocolType.Tcp)){
			
				try {
					listener.Bind(localEndPoint);
					listener.Listen(MAX_SOCKETS);

					Console.WriteLine("start http Listening on " + port.ToString());

					while (isRunning) {
						allDone.Reset();

						listener.BeginAccept(new AsyncCallback(AcceptCallback),	listener);

						allDone.WaitOne();
					}
				} catch (Exception ex) {
					Console.WriteLine(ex.Message);
				} finally {
					if (listener != null) listener.Close();
				}
			}
		}

		private void AcceptCallback(IAsyncResult ar) {
			if (!isRunning) return;

			allDone.Set();

			Socket socket;
			try {
				Socket listener = (Socket)ar.AsyncState;
				socket = listener.EndAccept(ar);
			} catch (Exception ex) {
				Console.WriteLine(ex.Message);
				return;
			}

			// タイムアウトセット
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, TimeOut);
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, KeepAliveTimeOut);
		
			OnConnected(socket);
		}

		private void OnConnected(Socket socket) {
			var connection = new HttpConnection(socket);
			connection.Closed += new EventHandler(Connection_Closed);

			if (connections.Capacity <= connections.Count) {
				// スレッド数が最大のため接続を閉じる
				connection.Close();
				return;
			}

			connection.Activate(usePipelineConnection);

			if (Connected != null) {
				Connected(connection);
			}

			connection.Action += new ResponseAction(this.Action);

			lock (connections) {
				connections.Add(connection);
			}

			connection.ListenerThread.Start();
		}

		#endregion

		private void Connection_Closed(object sender, EventArgs e) {
			var connection = sender as HttpConnection;

			if (Disconnected != null) {
				Disconnected(connection);
			}

			if (connections.Contains(connection)) {
				lock (connections) {
					connections.Remove(connection);
				}
			}
		}
	}
}
