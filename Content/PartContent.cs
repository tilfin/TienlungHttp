using System;

namespace TienlungHttp
{
	/// <summary>
	/// MultiPart�R���e���c�̈ꕔ��
	/// </summary>
	public class PartContent
	{
        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="headers">�w�b�_</param>
        /// <param name="contentStream">�R���e���g�X�g���[��</param>
		internal PartContent(System.Collections.IDictionary headers, System.IO.MemoryStream contentStream) {
            Headers = headers;
            ContentStream = contentStream;
		}

        /// <summary>
        /// �w�b�_
        /// </summary>
		public System.Collections.IDictionary Headers { get; private set; }

        /// <summary>
        /// �R���e���g�X�g���[��
        /// </summary>
		public System.IO.MemoryStream ContentStream { get; private set; }
    }
}
