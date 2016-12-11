using System;
using System.IO;
using System.Drawing;

namespace Tienlung.Net.Http
{
	/// <summary>
	/// ScreenShotAction ÇÃäTóvÇÃê‡ñæÇ≈Ç∑ÅB
	/// </summary>
	public class ScreenShotAction : Action
	{
		public ScreenShotAction(){
		}
		
		#region Action ÉÅÉìÉo

		public void Execute(HttpRequest request, HttpResponse response) {
			Image img = ScreenShot.GrabScreen();

			string tempPngFile = Path.GetTempFileName();
			img.Save(tempPngFile, System.Drawing.Imaging.ImageFormat.Png);

		
			try {
				FileInfo fi = new FileInfo(tempPngFile);
				if (!fi.Exists) {
					response.Code = ResponseCode.NotFound;
					response.Message = "Not Found";
					return;
				}

				// Content-TypeÇÃéÊìæ
				if (FileGetAction.contentTypeMap.ContainsKey("png")) {
					response.ContentType = (string)FileGetAction.contentTypeMap["png"];
				}

				response.SetHeader("Last-Modified", fi.LastWriteTimeUtc.ToString("r"));

				FileStream fs = fi.OpenRead();

				response.Code = ResponseCode.OK;
				response.Message = "OK";
				response.SetContent(fs);

			} catch (IOException) {
				response.Code = ResponseCode.Forbidden;
				response.Message = "Forbidden";
			}
		}

		#endregion
	}
}
