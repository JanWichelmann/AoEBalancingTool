using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AoEBalancingTool
{
	internal static class ExtensionMethods
	{
		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool DeleteObject([In] IntPtr hObject);

		/// <summary>
		/// Converts the bitmap into an image source that is usable by WPF.
		/// </summary>
		/// <param name="b">The bitmap t be converted.</param>
		/// <returns></returns>
		public static ImageSource ToImageSource(this Bitmap b)
		{
			IntPtr bH = b.GetHbitmap();
			try
			{
				return Imaging.CreateBitmapSourceFromHBitmap(bH, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
			}
			finally
			{
				DeleteObject(bH);
			}
		}
	}
}