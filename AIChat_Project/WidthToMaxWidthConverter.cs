using System;
using System.Globalization;
using System.Windows.Data;

namespace AIChat_Project
{
    public class WidthToMaxWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double actualWidth)
            {
                // 윈도우 너비의 80%를 MaxWidth로 설정
                return actualWidth * 0.8;
            }
            return 400; // 기본값
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
