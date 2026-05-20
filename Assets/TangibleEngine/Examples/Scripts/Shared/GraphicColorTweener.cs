using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Shared {
  public class GraphicColorTweener : MonoBehaviour {

    public Color A, B;

    private Tween _toTween;

    private void To(Color endValue, float duration, float delay) {
      _toTween?.Kill();
      var g = GetComponent<Graphic>();
      if (g == null) return;
      _toTween = g.DOColor(endValue, duration).SetDelay(delay);
    }

    public void ToA(float duration, float delay = 0f) {
      To(A, duration, delay);
    }

    public void ToB(float duration, float delay = 0f) {
      To(B, duration, delay);
    }
  }
}