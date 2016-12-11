using System;
using System.IO;

namespace TienlungHttp
{
	/// <summary>
	/// �R���e���g�X�g���[��
	/// </summary>
	public class MultiPartContentStream : IDisposable
	{
		private readonly byte CR = (byte)'\r';
		private readonly byte LF = (byte)'\n';

		internal byte[] data = null;
		internal int currentPosition = 0;

		/// <summary>
		/// �R���X�g���N�^
		/// </summary>
		/// <param name="contentStream">�f�[�^��MemoryStream</param>
		public MultiPartContentStream(MemoryStream contentStream) {
			data = contentStream.GetBuffer();
		}

		public MultiPartContentStream() {
		}

		public void Append(byte[] bytes, int length) {
			if (data == null || data.Length == currentPosition) {
				if (bytes.Length != length) {
					data = new byte[length];
					Array.Copy(bytes, 0, data, 0, length);
				} else {
					data = bytes;
				}
			} else {
				byte[] newData = new byte[bytes.Length + data.Length - currentPosition];

				Array.Copy(data, currentPosition, newData, 0, data.Length - currentPosition);
				Array.Copy(bytes, 0, newData, data.Length - currentPosition, bytes.Length);

				data = newData;
			}
			currentPosition = 0;
		}

		public void Append(byte[] bytes) {
			Append(bytes, bytes.Length);
		}

		public void Append(MemoryStream stream) {
			Append(stream.GetBuffer());
		}

		public void Clear() {
			data = null;
			currentPosition = 0;
		}

		public bool CanRead {
			get { return (data != null && currentPosition < data.Length); }
		}

		public byte[] ReadBytesLine() {
			int start = currentPosition;
			int length = ReadLineLength();
			if (length == -1) return null;

			byte[] readData = new byte[length];
			Array.Copy(data, start, readData, 0, length);

			return readData;
		}

		public int ReadBytes(byte[] buffer, int maxlength) {
			return ReadBytes(buffer, 0, maxlength);
		}

		public int ReadBytes(byte[] buffer, int offset, int maxlength) {
			int readsize;
			maxlength -= offset;
			if (currentPosition + maxlength <= data.Length) {
				readsize = maxlength;
			} else {
				readsize = data.Length - currentPosition;
			}

			Array.Copy(data, currentPosition, buffer, offset, readsize);
			currentPosition += readsize;
			return readsize;
		}

		public string ReadLineASCII() {
			int start = currentPosition;
			int length = ReadLineLength();
			if (length == -1) return null;

			return System.Text.Encoding.ASCII.GetString(data, start, length);
		}

		/// <summary>
		/// �ʒu���w�肵���T�C�Y�����߂��B
		/// </summary>
		/// <param name="offset"></param>
		public void SeekBack(int offset) {
			currentPosition -= offset;
		}
		
		/// <summary>
		/// CRLF�����s�R�[�h�Ƃ��Ă�����܂܂Ȃ��I���̈ʒu��Ԃ��A
		/// ���݈ʒu�����̍s�܂Ői�߂�B
		/// </summary>
		/// <returns>�s�̃o�C�g��̒���</returns>
		protected int ReadLineLength() {
			int startPos = currentPosition;
			int endPos = currentPosition + 1;

			while (endPos < data.Length) {
				if (data[endPos - 1] == CR && data[endPos] == LF) {
					currentPosition = endPos + 1;
					return endPos - startPos - 1;
				}
				endPos++;
			}

			return -1;
		}

		#region IDisposable �����o

		public void Dispose() {
			data = null;
		}

		#endregion
	}
}
