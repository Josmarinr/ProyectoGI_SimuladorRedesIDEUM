using System;

namespace TE {
  public class EngineConfiguration {

    public const float EpsilonAmt = float.Epsilon*2;

    public class RecoveryConfig {
      public int Timeout;             // Amount of time (in ms) the engine will allow a tangible to recover
      public float TangibleDistance;  // The distance threshold used to recover a tangible
      public float PointDistance;     // The distance threshold used to recover a point

      public bool IsEquivalentTo(RecoveryConfig c) {
        return Timeout == c.Timeout
               && Math.Abs(TangibleDistance - c.TangibleDistance) < EpsilonAmt
               && Math.Abs(PointDistance - c.PointDistance) < EpsilonAmt;
      }
    }

    public class SmoothingConfig {
      public int PositionHistory; // Number of data points to keep for running average of tangible position
      public int RotationHistory; // Number od data points to keep for running average of tangible rotation

      public bool IsEquivalentTo(SmoothingConfig c) {
        return PositionHistory == c.PositionHistory
               && RotationHistory == c.RotationHistory;
      }
    }
    
    public string Name;                 // human readible name of this configuration
    public float SensitivityMultiplier; // The flexibility multiplier of the engine to recognize any pattern.
    public RecoveryConfig Recovery;     // Settings pertaining to the recovery of tangibles and points
    public SmoothingConfig Smoothing;   // Settings pertaining to the smoothing of tangible properties (e.g. position, rotation)
    public int Pending3PointMatchDelay; // Delay for adding new matches with 3 points while search for matches higher point patterns



    public bool IsEquivalentTo(EngineConfiguration c) {
      return string.Equals(Name, c.Name, StringComparison.InvariantCultureIgnoreCase)
             && Math.Abs(SensitivityMultiplier - c.SensitivityMultiplier) < EpsilonAmt
             && Pending3PointMatchDelay == c.Pending3PointMatchDelay
             && Recovery.IsEquivalentTo(c.Recovery)
             && Smoothing.IsEquivalentTo(c.Smoothing);
    }
  }
}