using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace SteamLibraryExplorer.Utils {
  /// <summary>
  /// Queue for callbacks to be run on the Dispatcher threads.
  /// Callbacks can be queued from any thread, and are deduplicated using a Key string.
  /// </summary>
  public class ThrottledDispatcher {
    private readonly object _callbacksLock = new object();
    private readonly DispatcherTimer _timer = new DispatcherTimer();
    private IDictionary<string, Action> _callbacks = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase);

    public void Start(TimeSpan period) {
      _timer.Interval = period;
      _timer.Tick += TimerOnTick;
      _timer.Start();
    }

    public void Enqeue(string key, Action callback) {
      lock (_callbacksLock) {
        _callbacks[key] = callback;
      }
    }

    private void TimerOnTick(object o, EventArgs eventArgs) {
      IDictionary<string, Action> callbacks;
      lock (_callbacksLock) {
        if (_callbacks.Count == 0) {
          return;
        }

        callbacks = _callbacks;
        _callbacks = new Dictionary<string, Action>();
      }

      foreach (var callback in callbacks.Values) {
        callback();
      }
    }
  }
}
