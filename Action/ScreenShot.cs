using System;
using System.Runtime.InteropServices;

namespace Tienlung.Net.Http
{
	/// <summary>
	/// ScreenShot
	/// </summary>
	public class ScreenShot {
		private const int HORZRES = 8; 
		private const int VERTRES = 10; 
		private const int SRCCOPY = 13369376; 
		private const int SRCINVERT = 6684742; 
		private const int USE_SCREEN_WIDTH = -1; 
		private const int USE_SCREEN_HEIGHT = -1; 
		private const short GW_HWNDNEXT = 2; 
		private const short GW_CHILD = 5; 
		private const string TAGET_CLASS_HTMLVIEW = "Shell Embedding"; 
		
		private struct RECT { 
			public Int32 Left; 
			public Int32 Top; 
			public Int32 Right; 
			public Int32 Bottom; 
		} 

		[DllImport("gdi32", EntryPoint="CreateDCA")] 
		private static extern IntPtr CreateDC(string lpDriverName, string lpDeviceName, string lpOutput, string lpInitData); 
		[DllImport("GDI32")] 
		private static extern IntPtr CreateCompatibleDC(IntPtr hDC); 
		[DllImport("GDI32")] 
		private static extern int DeleteDC(IntPtr hDC); 
		[DllImport("user32", EntryPoint="GetWindowDC")] 
		private static extern IntPtr GetWindowDC(IntPtr hwnd); 
		[DllImport("user32", EntryPoint="ReleaseDC")] 
		private static extern IntPtr ReleaseDC(IntPtr hwnd, IntPtr hdc); 
		[DllImport("gdi32", EntryPoint="GetDeviceCaps")] 
		private static extern int GetDeviceCaps(IntPtr hdc, int nIndex); 
		[DllImport("GDI32")] 
		private static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight); 
		[DllImport("GDI32")] 
		private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject); 
		[DllImport("GDI32")] 
		private static extern int DeleteObject(IntPtr hObj); 
		[DllImport("GDI32")] 
		private static extern int BitBlt(IntPtr hDestDC, int X, int Y, int nWidth, int nHeight, IntPtr hSrcDC, int SrcX, int SrcY, int Rop); 
		[DllImport("user32", EntryPoint="GetForegroundWindow")] 
		private static extern IntPtr GetForegroundWindow(); 
		[DllImport("user32", EntryPoint="IsWindow")] 
		private static extern int IsWindow(IntPtr hwnd); 
		[DllImport("user32.dll")] 
		private static extern Int32 GetWindowRect(IntPtr hWnd, ref RECT lpRect); 
		[DllImport("user32")] 
		private static extern IntPtr GetWindow(IntPtr hwnd, int wCmd); 
		[DllImport("user32", EntryPoint="GetClassNameW")] 
		private static extern int GetClassName(IntPtr hwnd, [MarshalAs(UnmanagedType.LPTStr)] System.Text.StringBuilder lpClassName, int nMaxCount);


		public static System.Drawing.Bitmap GrabScreen() { 
			return GrabScreen(0, 0, USE_SCREEN_WIDTH, USE_SCREEN_HEIGHT); 
		} 

		public static System.Drawing.Bitmap GrabScreen(System.Drawing.Rectangle Rect) { 
			return GrabScreen(Rect.X, Rect.Y, Rect.Width, Rect.Height); 
		} 

		public static System.Drawing.Bitmap GrabScreen(System.Drawing.Point Location, System.Drawing.Size Size) { 
			return GrabScreen(Location.X, Location.Y, Size.Width, Size.Height); 
		} 

		public static System.Drawing.Bitmap GrabScreen(int X, int Y, int Width, int Height) { 
			IntPtr hDesktopDC; 
			IntPtr hOffscreenDC; 
			IntPtr hBitmap; 
			IntPtr hOldBmp; 
			System.Drawing.Bitmap MyBitmap = null; 
			
			hDesktopDC = CreateDC("DISPLAY", String.Empty, String.Empty, String.Empty);

			if (!(IntPtr.Zero.Equals(hDesktopDC))) { 
				if (Width == USE_SCREEN_WIDTH) { 
					Width = GetDeviceCaps(hDesktopDC, HORZRES); 
				} 
				if (Height == USE_SCREEN_HEIGHT) { 
					Height = GetDeviceCaps(hDesktopDC, VERTRES); 
				} 
				hOffscreenDC = CreateCompatibleDC(hDesktopDC); 
				if (!(IntPtr.Zero.Equals(hOffscreenDC))) { 
					hBitmap = CreateCompatibleBitmap(hDesktopDC, Width, Height); 
					if (!(IntPtr.Zero.Equals(hBitmap))) { 
						hOldBmp = SelectObject(hOffscreenDC, hBitmap); 
						BitBlt(hOffscreenDC, 0, 0, Width, Height, hDesktopDC, X, Y, SRCCOPY); 
						MyBitmap = System.Drawing.Bitmap.FromHbitmap(hBitmap); 
						DeleteObject(SelectObject(hOffscreenDC, hOldBmp)); 
					} 
					DeleteDC(hOffscreenDC); 
				} 
				DeleteDC(hDesktopDC); 
			} 
			return MyBitmap; 
		} 

		public static System.Drawing.Bitmap GrabActiveWindow() { 
			return GrabWindow(GetForegroundWindow()); 
		} 

		public static System.Drawing.Bitmap GrabWindow(IntPtr hWnd) { 
			IntPtr hWindowDC; 
			IntPtr hOffscreenDC; 
			RECT rec = new RECT(); 
			int nWidth; 
			int nHeight; 
			IntPtr hBitmap; 
			IntPtr hOldBmp; 
			System.Drawing.Bitmap MyBitmap = null; 

			if (!IntPtr.Zero.Equals(hWnd) && IsWindow(hWnd) != 0) { 
				hWindowDC = GetWindowDC(hWnd); 
				if (!IntPtr.Zero.Equals(hWindowDC)) { 
					if (GetWindowRect(hWnd, ref rec) != 0){
						nWidth = rec.Right - rec.Left; 
						nHeight = rec.Bottom - rec.Top; 
						hOffscreenDC = CreateCompatibleDC(hWindowDC); 
						if (!IntPtr.Zero.Equals(hOffscreenDC)) { 
							hBitmap = CreateCompatibleBitmap(hWindowDC, nWidth, nHeight); 
							if (!IntPtr.Zero.Equals(hBitmap)) { 
								hOldBmp = SelectObject(hOffscreenDC, hBitmap); 
								BitBlt(hOffscreenDC, 0, 0, nWidth, nHeight, hWindowDC, 0, 0, SRCCOPY); 
								MyBitmap = System.Drawing.Bitmap.FromHbitmap(hBitmap); 
								DeleteObject(SelectObject(hOffscreenDC, hOldBmp)); 
							} 
							DeleteDC(hOffscreenDC); 
						} 
					} 
					ReleaseDC(hWnd, hWindowDC); 
				} 
			} 
			return MyBitmap; 
		} 

		public static IntPtr FindChildHwnd(IntPtr parenthwnd, string targetClass) { 
			IntPtr childhwnd; 
			System.Text.StringBuilder className = new System.Text.StringBuilder(256);
			childhwnd = GetWindow(parenthwnd, GW_CHILD); 

			do {
				GetClassName(childhwnd, className, 256); 
				if (className.ToString().StartsWith(targetClass)) { 
					return childhwnd; 
				} 
				childhwnd = GetWindow(childhwnd, GW_HWNDNEXT); 
			} while (!IntPtr.Zero.Equals(childhwnd)); 
			
			return IntPtr.Zero;
		}
	}
}
