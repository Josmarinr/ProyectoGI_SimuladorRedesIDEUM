using UnityEngine;

namespace Example2 {

  /// <summary>
  /// Helper class to store a set of euler angles. Used in conjunction with colliders to re-orient floating UI's to the correct angle
  /// </summary>
  public class RotationInformation : MonoBehaviour {
    public Vector3 EulerAngles;
  }
} 