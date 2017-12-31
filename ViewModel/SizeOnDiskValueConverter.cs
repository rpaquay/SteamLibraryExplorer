using System;
using System.Globalization;
using System.Windows.Data;

namespace SteamLibraryExplorer.ViewModel {
  class SizeOnDiskValueConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var fileCount = (long)value;
      return MainView.HumanReadableDiskSize(fileCount);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return null;
    }
  }
}