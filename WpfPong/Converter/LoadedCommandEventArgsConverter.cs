using GalaSoft.MvvmLight.Command;
using System.Windows;

namespace WpfPong.Converter
{
    public class LoadedCommandEventArgsConverter : IEventArgsConverter
    {
        public object Convert(object value, object parameter)
        {
            return (FrameworkElement)parameter;
        }
    }
}
