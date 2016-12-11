using System;

namespace TienlungHttp
{
	/// <summary>
	/// HTTP �X�e�[�^�X�R�[�h
	/// </summary>
	public enum StatusCode : int
    {
		/// <summary>
		/// �������p�����Ă��܂��B�����̃��N�G�X�g�𑗐M���Ă��������B
		/// </summary>
		Continue = 100,

		/// <summary>
		/// Upgrade �w�b�_�Ŏw�肵���v���g�R���ɕύX���čėv�����Ă��������B
		/// </summary>
		SwitchingProtocols = 101,

		/// <summary>
		/// �������܂����B
		/// </summary>
		OK = 200,

		/// <summary>
		/// �w�b�_�Ŏw�肵���ꏊ�ɐV�����R���e���c���쐬����܂����B
		/// </summary>
		CreatedLocation = 201,

		/// <summary>
		/// �v���͎󗝂���܂����B�����������͊������Ă��܂���B
		/// </summary>
		Accepted = 202,

		/// <summary>
		/// �����w�b�_�̓I���W�i���T�[�o�[���Ԃ������̂Ƃ͈قȂ�܂����A�����͐����ł��B
		/// </summary>
		NonAuthoritativeInformation = 203,

		/// <summary>
		/// �R���e���c�͂���܂��񂪁A�����͐������܂����B
		/// </summary>
		NoContent = 204,

		/// <summary>
		/// �v�����󗝂����̂ŁA���݂̃R���e���c�i��ʁj��j�����Ă��������B
		/// </summary>
		ResetContent = 205,

		/// <summary>
		/// �R���e���c���ꕔ�̂ݕԋp���܂��B
		/// </summary>
		PartialContent = 206,

		/// <summary>
		/// �R���e���c������@�ɂ��ĕ����̑I����������܂��B
		/// </summary>
		MultipleChoices = 300,

		/// <summary>
		/// �w�b�_�Ŏw�肳�ꂽ�ʂ̏ꏊ�Ɉړ����܂����B
		/// </summary>
		MovedPermanentlyLocation = 301,

		/// <summary>
		/// �w�b�_�Ŏw�肳�ꂽ�ʂ̏ꏊ�Ɍ�����܂����B����������Ă��������B
		/// </summary>
		FoundLocation = 302,

		/// <summary>
		/// �w�b�_�Ŏw�肳�ꂽ���̏ꏊ�����Ă��������B
		/// </summary>
		SeeOtherLocation = 303,

		/// <summary>
		/// �X�V����Ă��܂���BIf-Modified-Since �w�b�_��p�����ꍇ�ɕԋp����܂��B
		/// </summary>
		NotModified = 304,

		/// <summary>
		/// �w�b�_�Ŏw�肵���v���L�V���g�p���Ă��������B
		/// </summary>
		UseProxyLocation = 305,

		/// <summary>
		/// �ʂ̏ꏊ�Ɉꎞ�I�Ɉړ����Ă��܂��B
		/// </summary>
		TemporaryRedirect = 307,

		/// <summary>
		/// �v�����s���ł��B
		/// </summary>
		BadRequest = 400,

		/// <summary>
		/// �F�؂���Ă��܂���B
		/// </summary>
		Unauthorized = 401,

		/// <summary>
		/// �x�������K�v�ł��B
		/// </summary>
		PaymentRequired = 402,

		/// <summary>
		/// �A�N�Z�X���F�߂��Ă��܂���B
		/// </summary>
		Forbidden = 403,

		/// <summary>
		/// ������܂���B
		/// </summary>
		NotFound = 404,

		/// <summary>
		/// �w�肵�����\�b�h�̓T�|�[�g����Ă��܂���B
		/// </summary>
		MethodNotAllowed = 405,

		/// <summary>
		/// ������Ă��܂���B
		/// </summary>
		NotAcceptable = 406,

		/// <summary>
		/// �v���L�V�F�؂��K�v�ł��B
		/// </summary>
		ProxyAuthenticationRequired = 407,

		/// <summary>
		/// ���N�G�X�g���^�C���A�E�g���܂����B
		/// </summary>
		RequestTimeout = 408,

		/// <summary>
		/// ���N�G�X�g���R���t���N�g�i�ՓˁE�����j���܂����B
		/// </summary>
		Conflict = 409,

		/// <summary>
		/// �v�����ꂽ�R���e���c�͖����Ȃ��Ă��܂��܂����B
		/// </summary>
		Gone = 410,

		/// <summary>
		/// �w�b�_��t�����ėv�����Ă��������B
		/// </summary>
		LengthRequiredContentLength = 411,

		/// <summary>
		/// �w�b�_�Ŏw�肳�ꂽ�����ɍ��v���܂���ł����B
		/// </summary>
		PreconditionFailedIf = 412,

		/// <summary>
		/// �v�����ꂽ�G���e�B�e�B���傫�����܂��B
		/// </summary>
		RequestEntityTooLarge = 413,

		/// <summary>
		/// �v�����ꂽ URI ���������܂��B
		/// </summary>
		RequestURITooLong = 414,

		/// <summary>
		/// �T�|�[�g����Ă��Ȃ����f�B�A�^�C�v�ł��B
		/// </summary>
		UnsupportedMediaType = 415,

		/// <summary>
		/// �v�����ꂽ�����W���s���ł��B
		/// </summary>
		RequestedRangeNotSatisfiable = 416,

		/// <summary>
		/// �w�b�_�Ŏw�肳�ꂽ�g���v���͎��s���܂����B
		/// </summary>
		ExpectationFailedExpect = 417,

		/// <summary>
		/// �T�[�o�[�ŗ\�����Ȃ��G���[���������܂����B
		/// </summary>
		InternalServerError = 500,

		/// <summary>
		/// ��������Ă��܂���B
		/// </summary>
		NotImplemented = 501,

		/// <summary>
		/// �Q�[�g�E�F�C���s���ł��B
		/// </summary>
		BadGateway = 502,

		/// <summary>
		/// �T�[�r�X�͗��p�\�ł͂���܂���B
		/// </summary>
		ServiceUnavailable = 503,

		/// <summary>
		/// �Q�[�g�E�F�C���^�C���A�E�g���܂����B
		/// </summary>
		GatewayTimeout = 504,

		/// <summary>
		/// ����HTTP�o�[�W�����̓T�|�[�g����Ă��܂���B
		/// </summary>
		HTTPVersionNotSupported = 505
	}

}
