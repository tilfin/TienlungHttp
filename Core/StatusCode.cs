using System;

namespace TienlungHttp
{
	/// <summary>
	/// HTTP ステータスコード
	/// </summary>
	public enum StatusCode : int
    {
		/// <summary>
		/// 処理を継続しています。続きのリクエストを送信してください。
		/// </summary>
		Continue = 100,

		/// <summary>
		/// Upgrade ヘッダで指定したプロトコルに変更して再要求してください。
		/// </summary>
		SwitchingProtocols = 101,

		/// <summary>
		/// 成功しました。
		/// </summary>
		OK = 200,

		/// <summary>
		/// ヘッダで指定した場所に新しいコンテンツが作成されました。
		/// </summary>
		CreatedLocation = 201,

		/// <summary>
		/// 要求は受理されました。ただし処理は完了していません。
		/// </summary>
		Accepted = 202,

		/// <summary>
		/// 応答ヘッダはオリジナルサーバーが返したものとは異なりますが、処理は成功です。
		/// </summary>
		NonAuthoritativeInformation = 203,

		/// <summary>
		/// コンテンツはありませんが、処理は成功しました。
		/// </summary>
		NoContent = 204,

		/// <summary>
		/// 要求を受理したので、現在のコンテンツ（画面）を破棄してください。
		/// </summary>
		ResetContent = 205,

		/// <summary>
		/// コンテンツを一部のみ返却します。
		/// </summary>
		PartialContent = 206,

		/// <summary>
		/// コンテンツ入手方法について複数の選択肢があります。
		/// </summary>
		MultipleChoices = 300,

		/// <summary>
		/// ヘッダで指定された別の場所に移動しました。
		/// </summary>
		MovedPermanentlyLocation = 301,

		/// <summary>
		/// ヘッダで指定された別の場所に見つかりました。そちらを見てください。
		/// </summary>
		FoundLocation = 302,

		/// <summary>
		/// ヘッダで指定された他の場所を見てください。
		/// </summary>
		SeeOtherLocation = 303,

		/// <summary>
		/// 更新されていません。If-Modified-Since ヘッダを用いた場合に返却されます。
		/// </summary>
		NotModified = 304,

		/// <summary>
		/// ヘッダで指定したプロキシを使用してください。
		/// </summary>
		UseProxyLocation = 305,

		/// <summary>
		/// 別の場所に一時的に移動しています。
		/// </summary>
		TemporaryRedirect = 307,

		/// <summary>
		/// 要求が不正です。
		/// </summary>
		BadRequest = 400,

		/// <summary>
		/// 認証されていません。
		/// </summary>
		Unauthorized = 401,

		/// <summary>
		/// 支払いが必要です。
		/// </summary>
		PaymentRequired = 402,

		/// <summary>
		/// アクセスが認められていません。
		/// </summary>
		Forbidden = 403,

		/// <summary>
		/// 見つかりません。
		/// </summary>
		NotFound = 404,

		/// <summary>
		/// 指定したメソッドはサポートされていません。
		/// </summary>
		MethodNotAllowed = 405,

		/// <summary>
		/// 許可されていません。
		/// </summary>
		NotAcceptable = 406,

		/// <summary>
		/// プロキシ認証が必要です。
		/// </summary>
		ProxyAuthenticationRequired = 407,

		/// <summary>
		/// リクエストがタイムアウトしました。
		/// </summary>
		RequestTimeout = 408,

		/// <summary>
		/// リクエストがコンフリクト（衝突・矛盾）しました。
		/// </summary>
		Conflict = 409,

		/// <summary>
		/// 要求されたコンテンツは無くなってしまいました。
		/// </summary>
		Gone = 410,

		/// <summary>
		/// ヘッダを付加して要求してください。
		/// </summary>
		LengthRequiredContentLength = 411,

		/// <summary>
		/// ヘッダで指定された条件に合致しませんでした。
		/// </summary>
		PreconditionFailedIf = 412,

		/// <summary>
		/// 要求されたエンティティが大きすぎます。
		/// </summary>
		RequestEntityTooLarge = 413,

		/// <summary>
		/// 要求された URI が長すぎます。
		/// </summary>
		RequestURITooLong = 414,

		/// <summary>
		/// サポートされていないメディアタイプです。
		/// </summary>
		UnsupportedMediaType = 415,

		/// <summary>
		/// 要求されたレンジが不正です。
		/// </summary>
		RequestedRangeNotSatisfiable = 416,

		/// <summary>
		/// ヘッダで指定された拡張要求は失敗しました。
		/// </summary>
		ExpectationFailedExpect = 417,

		/// <summary>
		/// サーバーで予期しないエラーが発生しました。
		/// </summary>
		InternalServerError = 500,

		/// <summary>
		/// 実装されていません。
		/// </summary>
		NotImplemented = 501,

		/// <summary>
		/// ゲートウェイが不正です。
		/// </summary>
		BadGateway = 502,

		/// <summary>
		/// サービスは利用可能ではありません。
		/// </summary>
		ServiceUnavailable = 503,

		/// <summary>
		/// ゲートウェイがタイムアウトしました。
		/// </summary>
		GatewayTimeout = 504,

		/// <summary>
		/// このHTTPバージョンはサポートされていません。
		/// </summary>
		HTTPVersionNotSupported = 505
	}

}
