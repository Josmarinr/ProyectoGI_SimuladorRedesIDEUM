using DG.Tweening;
using Newtonsoft.Json;
using TE;
using TMPro;
using UnityEngine;

namespace Example2 {
  /// <summary>
  /// Class to handle the behavior of the Floating UIs that are coupled to tangibles
  /// </summary>
  public class FloatingUi : MonoBehaviour {
    
    /// <summary>
    /// Reference to the text used to show the pattern name
    /// </summary>
    public TextMeshProUGUI TitleText;
    
    /// <summary>
    /// Reference to the text used to display the JSON version of the pattern information
    /// </summary>
    public TextMeshProUGUI DescText;

    /// <summary>
    /// Reference to the tween used to rotate this object to align with the closest edge of the touch table
    /// </summary>
    private Tween _rotationTween;

    /// <summary>
    /// Updates the textual information to match the provided pattern
    /// </summary>
    /// <param name="pattern"></param>
    public void SetInfo(Pattern pattern) {
      TitleText.text = pattern.Name;
      DescText.text = JsonConvert.SerializeObject(pattern, Formatting.Indented);
    }

    /// <summary>
    /// Updates the position of this object to match that of the tangible
    /// </summary>
    /// <param name="t">The tangible to follow</param>
    /// <param name="c">The camera - used to transform between screen space and world space coordinates</param>
    public void UpdateTransform(Tangible t, Camera c) {
      var worldPos = c.ScreenToWorldPoint(t.Pos);
      //Only use the x and y components of the world position because we are in a 2D canvas.
      transform.position = new Vector3(worldPos.x, worldPos.y, 0);
    }

    void OnTriggerEnter2D(Collider2D collider) {
      //retrieve the rotation information associated with the zone this object is currently in
      var rotInfo = collider.GetComponent<RotationInformation>();
      if (rotInfo == null) return;
      _rotationTween?.Kill();
      //rotate to the euler angles specified by the rotation information component
      _rotationTween = transform.DOLocalRotate(rotInfo.EulerAngles, 0.5f, RotateMode.Fast);
    }

  }
}