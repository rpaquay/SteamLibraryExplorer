using System;
using System.Globalization;
using System.Windows.Data;

namespace SteamLibraryExplorer.WpfUtils {
  /// <summary>
  /// An implementation of <see cref="IValueConverter"/> that returns the human readable string
  /// representation of a <see cref="Int64"/> value representing a file disk on disk (e.g. "10 MB").
  /// </summary>
  public class SizeOnDiskValueConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value == null) {
        return "";
      }
      var fileCount = (long)value;
      return MainView.HumanReadableDiskSize(fileCount);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return null;
    }
  }
}