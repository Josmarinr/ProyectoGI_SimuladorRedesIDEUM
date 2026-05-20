using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TE {
  public class EngineProfile {

    public event Action PropertyChanged;


    // Read tangible json config, return true if it worked
    public static bool TryDeserialize(string s, out EngineProfile p) {
      var status = false;
      try {
        p = JsonConvert.DeserializeObject<EngineProfile>(s);
        status = true;
      }
      catch (JsonException) {
        p = null;
        status = false;
      }

      return status;
    }

    // Return the default profile for engine 
    public static EngineProfile DefaultProfile() {
      return new EngineProfile {
        Config = new EngineConfiguration {
          Name = "New Tangible Profile",
          Recovery = new EngineConfiguration.RecoveryConfig {
            PointDistance = 50f,
            TangibleDistance = 50f,
            Timeout = 200
          },
          Pending3PointMatchDelay = 0,
          SensitivityMultiplier = 1,
          Smoothing = new EngineConfiguration.SmoothingConfig {
            PositionHistory = 1,
            RotationHistory = 10
          }
        },
        Patterns = new List<Pattern>()
      };
    }


    public EngineConfiguration Config;  // configuration information for this profile
    public List<Pattern> Patterns;      // Collection of patterns

    public void DoPropertyChanged() {
      if (PropertyChanged != null) PropertyChanged.Invoke();
    }
  }
}