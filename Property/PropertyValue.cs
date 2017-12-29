using System;
using System.Security.AccessControl;

namespace SteamLibraryExplorer
{
  public class PropertyValue<T> {
    private T _value;

    public PropertyValue() {
    }

    public PropertyValue(T value) {
      _value = value;
    }

    public T Value {
      get { return _value; }
      set {
        if (Object.Equals(_value, value)) {
          return;
        }
        var oldValue = _value;
        _value = value;
        OnValueChanged(new ValueChangedArg<T> {
          OldValue = oldValue,
          NewValue = _value,
        });
      }
    }

    public event EventHandler<ValueChangedArg<T>> ValueChanged;

    protected virtual void OnValueChanged(ValueChangedArg<T> e) {
      ValueChanged?.Invoke(this, e);
    }
  }
}