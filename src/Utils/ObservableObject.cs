using System.ComponentModel;

namespace SteamLibraryExplorer.Utils {
  public abstract class ObservableObject : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;

    protected void RaisePropertyChangedEvent(string propertyName) {
      var handler = PropertyChanged;
      if (handler != null)
        handler(this, new PropertyChangedEventArgs(propertyName));
    }


    protected void UpdateProperty<T>(ref T field, T value, string fieldName) {
      if (!Equals(field, value)) {
        field = value;
        RaisePropertyChangedEvent(fieldName);
      }
    }
  }
}