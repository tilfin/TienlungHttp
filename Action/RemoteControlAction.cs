using System;
using System.IO;
using System.Collections;

namespace Tienlung.Net.Http
{
	/// <summary>
	/// リモートコントロール
	/// </summary>
	public class RemoteControlAction : Action {

		public RemoteControlAction() {
		}

		#region Action メンバ

		public void Execute(HttpRequest request, HttpResponse response) {
			IDictionary argmap = request.Arguments;
			int x = 0;
			int y = 0;

			if (argmap.Contains("x")) {
				x = Convert.ToInt32((string)argmap["x"]);
			}
			if (argmap.Contains("y")) {
				y = Convert.ToInt32((string)argmap["y"]);
			}
			if (argmap["op"].Equals("mouse")) {
				string cmd = (string)argmap["cmd"];
				if (cmd.Equals("move")) {
					MouseControl.Move(x, y);
				} else if (cmd.Equals("click")) {
					MouseControl.Click(x, y);
				} else if (cmd.Equals("dblclick")) {
					MouseControl.DblClick(x, y);
				} else if (cmd.Equals("drag")) {
					MouseControl.Drag(x, y);
				}
			}

			System.Drawing.Image img = ScreenShot.GrabScreen();
			// Content-Typeの取得
			response.ContentEncoding = "gzip";
			response.ContentType = "image/bmp";

			//response.SetHeader("Pragma", "no-cache");
			response.SetHeader("Last-Modified", DateTime.Now.ToString("r"));

			MemoryStream ms = new MemoryStream();
			img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
			ms.Seek(0, SeekOrigin.Begin);

			response.Code = ResponseCode.OK;
			response.Message = "OK";
			response.SetContent(ms);
		}

		#endregion
	}
}
