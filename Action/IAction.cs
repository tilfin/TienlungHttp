using System;

namespace TienlungHttp
{
	/// <summary>
	/// �A�N�V�����C���^�[�t�F�C�X
	/// Java�ł�Servlet�Ɠ��l�̂���
	/// </summary>
	public interface IAction
	{
		/// <summary>
		/// �A�N�V�����̎��s���e���`����B
		/// �����ŏ�������Ȃ�������O�́A
		/// HttpService �� InternalServerError �Ƃ��ď��������B
		/// </summary>
		/// <param name="request">���N�G�X�g</param>
		/// <param name="response">���X�|���X</param>
		void Execute(HttpRequest request, HttpResponse response);
	}
}
