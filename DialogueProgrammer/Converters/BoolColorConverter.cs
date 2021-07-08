using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace DialogueProgrammer.Converters
{
    public class BoolColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolVal = (bool)value;
            var parameterColor = (string[])parameter;

            return (Brush)typeof(Brushes).GetProperty(parameterColor[boolVal ? 1 : 0]).GetValue(null, null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
