﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace SteamLibraryExplorer.WpfUtils {
  class FileCountValueConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      if (value == null) {
        return "";
      }
      var fileCount = (long)value;
      return MainView.HumanReadableFileCount(fileCount);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return null;
    }
  }
}
