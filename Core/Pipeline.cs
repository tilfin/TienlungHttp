using System;
using System.Collections;
using System.Threading;

namespace TienlungHttp
{
	/// <summary>
	/// パイプライン
	/// </summary>
	public class Pipeline
	{
		private ManualResetEvent signal;
		private Queue requestQueue;
		private Thread worker;
		private bool isAlive = true;
		private bool hasCompleted = false;
        private bool isDisposed = false;

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="work">スレッドが行うパイプライン処理</param>
		public Pipeline(string connectionName, ThreadStart work) {
			requestQueue = new Queue();
			
			signal = new ManualResetEvent(false);

			worker = new Thread(work);
			worker.Name = connectionName + " - worker";
		}

		/// <summary>
		/// パイプラインスレッドを開始する。
		/// </summary>
        public void Start() {
			worker.Start();
#if DEBUG
            Console.WriteLine(worker.Name + " - Start");
#endif
		}

		/// <summary>
		/// キューに入っているリクエスト数
		/// </summary>
		public int Count {
			get { return requestQueue.Count; }
		}

		/// <summary>
		/// キューに入れる
		/// </summary>
		/// <param name="obj">入れるアイテム</param>
        public void PutRequest(object obj) {
			lock (requestQueue) {
				requestQueue.Enqueue(obj);
			}

			signal.Set();
		}

		/// <summary>
		/// キューから取り出す
		/// </summary>
		/// <returns>取り出したアイテム</returns>
        public object TakeRequest() {
			if (requestQueue.Count > 0) {
				return dequeue();
			} else if (hasCompleted) {
				//return null;
			}

			signal.Reset();
			signal.WaitOne();
			
			if (!isAlive) return null;

			return dequeue();
		}

		private object dequeue() {
			try {
				lock (requestQueue) {
					return requestQueue.Dequeue();
				}
			} catch (InvalidOperationException) {
#if DEBUG
				Console.WriteLine(worker.Name + " - dequeue() throws InvalidOperationException");
#endif
				return null;
			}
		}

		/// <summary>
		/// 閉じる
		/// </summary>
		public void Close() {
            if (isDisposed)
                throw new ObjectDisposedException("pipeline has disposed.");
#if DEBUG
			Console.WriteLine(worker.Name + " - Close");
#endif
			isAlive = false;
			signal.Set();
            signal.Close();

			requestQueue.Clear();

            isDisposed = true;
		}

		/// <summary>
		/// リクエストを全て読み込んだときにこのメソッドを呼び出すこと。
		/// </summary>
		public void Complete() {
#if DEBUG
			Console.WriteLine(worker.Name + " - Complete");
#endif
			hasCompleted = true;
		}

		/// <summary>
		/// 動作中か示す
		/// </summary>
		public bool IsAlive {
			get { return isAlive; }
		}
	}
}
