using System;
using System.IO;
using System.Net;
using System.Text;

namespace TienlungHttp
{
	/// <summary>
	/// MultiPart Form
	/// </summary>
	public class MultiPartForm {
		private const string MULTIPART_FORMDATA = "multipart/form-data";
		private const string BOUNDARY_FIELD = "boundary=";

		public static void Post(HttpWebRequest webreq, string contenttype, byte[] data) {
			if (contenttype == null || contenttype.Length == 0) {
				contenttype = "application/octet-stream";
			}

			string boundary = "----------" + DateTime.Now.Ticks.ToString("x");

			webreq.ContentType = MULTIPART_FORMDATA + "; " + BOUNDARY_FIELD + boundary;
			
			// Build up the post message header
			StringBuilder sb = new StringBuilder();
			sb.Append("--");
			sb.Append(boundary);
			sb.Append("\r\nContent-Disposition: form-data");
			sb.Append("\r\nContent-Type: ");
			sb.Append(contenttype);
			sb.Append("\r\n\r\n");

			string postHeader = sb.ToString();
			byte[] postHeadData = Encoding.ASCII.GetBytes(postHeader);
			byte[] boundaryData = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

			long length = postHeadData.Length + data.Length + boundaryData.Length;
			webreq.ContentLength = length;

			using (Stream reqStream = webreq.GetRequestStream()) {
				reqStream.Write(postHeadData, 0, postHeadData.Length);
				reqStream.Write(data, 0, data.Length);
				reqStream.Write(boundaryData, 0, boundaryData.Length);
			}
		}

		public static void Post(HttpClient client, string request, byte[] data) {
			string boundary = "----------" + DateTime.Now.Ticks.ToString("x");
			
			StringBuilder sb = new StringBuilder();
			sb.Append("--");
			sb.Append(boundary);
			sb.Append("\r\nContent-Disposition: form-data");
			sb.Append("\r\nContent-Type: application/octet-stream");
			sb.Append("\r\n\r\n");

			byte[] postHeadData = Encoding.ASCII.GetBytes(sb.ToString());
			byte[] boundaryData = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

			client.ContentType = MULTIPART_FORMDATA + "; " + BOUNDARY_FIELD + boundary;
			client.ContentLength = postHeadData.Length + data.Length + boundaryData.Length;

			client.SendHeaders(request);
			client.Send(postHeadData);
			client.Send(data);
			client.Send(boundaryData);
		}

		public static string GetBoundary(string contentType) {
			if (!contentType.StartsWith(MULTIPART_FORMDATA))
				return null;
			
			int pos = contentType.IndexOf(BOUNDARY_FIELD);
			if (pos < 0) return null;

			pos += BOUNDARY_FIELD.Length;
			return contentType.Substring(pos);
		}

	}
}
