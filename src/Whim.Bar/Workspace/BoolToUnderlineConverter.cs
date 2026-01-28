using System;
using Microsoft.UI.Xaml.Data;
using Windows.UI.Text;

namespace Whim.Bar;

/// <summary>
/// Converts a boolean value to a TextDecorations value.
/// Returns Underline when true, None when false.
/// </summary>
internal class BoolToUnderlineConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, string language)
	{
		if (value is bool hasWindows && hasWindows)
		{
			return TextDecorations.Underline;
		}
		return TextDecorations.None;
	}

	public object ConvertBack(object value, Type targetType, object parameter, string language)
	{
		throw new NotImplementedException();
	}
}
