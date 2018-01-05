// Copyright 2015 Renaud Paquay All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace mtsuite.shared.Utils {
  public static class FormatHelpers {
    public static string FormatElapsedTime(TimeSpan elapsed) {
      var totalSeconds = elapsed.TotalSeconds;
      if (totalSeconds < 60) {
        return string.Format("{0:n2}s", totalSeconds);
      } else if (totalSeconds < 60*60) {
        return string.Format("{0:00}m{1:00.00}s", elapsed.Minutes, (elapsed.Seconds * 1000 + elapsed.Milliseconds) / 1000.0);
      } else if (totalSeconds < 24*60*60) {
        return string.Format("{0}h{1:00}m{2:00.00}s", elapsed.Hours, elapsed.Minutes, (elapsed.Seconds * 1000 + elapsed.Milliseconds) / 1000.0);
      } else {
        return elapsed.ToString();
      }
    }
  }
}