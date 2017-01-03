using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AoEBalancingTool
{
	/// <summary>
	/// Converts a given boolean value into a text decoration property. This is used to highlight modified fields in the main window.
	/// </summary>
	public class BoolToTextDecorationConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// Return style depending on value
			return value != null && (bool)value ? TextDecorations.Underline : null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// Do nothing
			return Binding.DoNothing;
		}
	}

	/// <summary>
	/// Performs a null check: Returns true, if the given object is not null.
	/// </summary>
	public class NullCheckToBoolConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// Return whether value does not equal null
			return value != null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// Do nothing
			return Binding.DoNothing;
		}
	}
}