using UnityEngine;
using UnityEngine.UI;

namespace TE.Examples {
  /// <summary>
  /// Class used to hold references to unity engine components. Instances of this class are used by the DemoController.cs
  /// </summary>
  public class ExampleTangibleData {

    public Transform Transform;
    public Text Text;
    public GameObject GameObject;

    public ExampleTangibleData(GameObject o) {
      GameObject = o;
      Transform = o.transform;
      Text = o.GetComponentInChildren<Text>();
    }

    public void DoShow() {
      GameObject.SetActive(true);
    }

    public void DoHide() {
      GameObject.SetActive(false);
    }

    public void Update(Tangible t, Vector3 offset) {

      if (Transform != null) {
        Transform.localPosition = new Vector3(t.X, t.Y, 0) + offset;
        Transform.localEulerAngles = new Vector3(0, 0, t.R * Mathf.Rad2Deg);
      }

      if (Text != null) {
        Text.text = string.Format("Tangible {0}\nPattern {1}\nPosition {2:000.0}, {3:000.0}\nRotation {4:000.0}", t.Id, t.PatternId, t.X, t.Y, Mathf.Rad2Deg * t.R);
      }
    }
  }
}