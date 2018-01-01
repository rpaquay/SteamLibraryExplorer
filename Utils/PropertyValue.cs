using System;
using JetBrains.Annotations;

namespace SteamLibraryExplorer.Utils
{
  public class PropertyValue<T> {
    [CanBeNull]
    private T _value;

    public PropertyValue() {
    }

    public PropertyValue([CanBeNull]T value) {
      _value = value;
    }

    [CanBeNull]
    public T Value {
      get { return _value; }
      set {
        if (Equals(_value, value)) {
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

    protected virtual void OnValueChanged([NotNull]ValueChangedArg<T> e) {
      ValueChanged?.Invoke(this, e);
    }
  }
}