using System;
using System.Collections;
using System.Threading;

namespace TienlungHttp
{
	/// <summary>
	/// �p�C�v���C��
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
		/// �R���X�g���N�^
		/// </summary>
		/// <param name="work">�X���b�h���s���p�C�v���C������</param>
		public Pipeline(string connectionName, ThreadStart work) {
			requestQueue = new Queue();
			
			signal = new ManualResetEvent(false);

			worker = new Thread(work);
			worker.Name = connectionName + " - worker";
		}

		/// <summary>
		/// �p�C�v���C���X���b�h���J�n����B
		/// </summary>
        public void Start() {
			worker.Start();
#if DEBUG
            Console.WriteLine(worker.Name + " - Start");
#endif
		}

		/// <summary>
		/// �L���[�ɓ����Ă��郊�N�G�X�g��
		/// </summary>
		public int Count {
			get { return requestQueue.Count; }
		}

		/// <summary>
		/// �L���[�ɓ����
		/// </summary>
		/// <param name="obj">�����A�C�e��</param>
        public void PutRequest(object obj) {
			lock (requestQueue) {
				requestQueue.Enqueue(obj);
			}

			signal.Set();
		}

		/// <summary>
		/// �L���[������o��
		/// </summary>
		/// <returns>���o�����A�C�e��</returns>
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
		/// ����
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
		/// ���N�G�X�g��S�ēǂݍ��񂾂Ƃ��ɂ��̃��\�b�h���Ăяo�����ƁB
		/// </summary>
		public void Complete() {
#if DEBUG
			Console.WriteLine(worker.Name + " - Complete");
#endif
			hasCompleted = true;
		}

		/// <summary>
		/// ���쒆������
		/// </summary>
		public bool IsAlive {
			get { return isAlive; }
		}
	}
}
