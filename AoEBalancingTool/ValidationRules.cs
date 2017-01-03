using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace AoEBalancingTool
{
	public class ShortTypeRule : ValidationRule
	{
		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			// Check whether value is of type short or at least a string that is convertible to short
			short val;
			if(value == null || value is short || value is string && short.TryParse((string)value, out val))
				return new ValidationResult(true, null);
			return new ValidationResult(false, "no short type");
		}
	}
}