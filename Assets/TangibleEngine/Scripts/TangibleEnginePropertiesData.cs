using System;

namespace TE {
  [Serializable]
  public class TangibleEnginePropertiesData {

    public enum Modes {
      Simulator,
      Service
    }

    public Modes Mode = Modes.Service;

    public Ideum.Logging.Log.Levels LogLevels;

    public int PortNumber = 4949;
  }
}