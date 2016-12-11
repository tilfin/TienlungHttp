using System;

namespace TienlungHttp
{
	/// <summary>
	/// アクションインターフェイス
	/// JavaでのServletと同様のもの
	/// </summary>
	public interface IAction
	{
		/// <summary>
		/// アクションの実行内容を定義する。
		/// ここで処理されなかった例外は、
		/// HttpService で InternalServerError として処理される。
		/// </summary>
		/// <param name="request">リクエスト</param>
		/// <param name="response">レスポンス</param>
		void Execute(HttpRequest request, HttpResponse response);
	}
}
