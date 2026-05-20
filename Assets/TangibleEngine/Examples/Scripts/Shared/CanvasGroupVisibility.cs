using DG.Tweening;
using UnityEngine;

namespace Shared {

  /// <summary>
  /// Utility behavior to show and hide a canvas group by fading in or out over time.
  /// </summary>
  public class CanvasGroupVisibility : MonoBehaviour {

    public enum States {
      Unknown,
      Showing,
      Shown,
      Hiding,
      Hidden
    }

    private Tween _fadeTween;

    private States _state = States.Unknown;

    /// <summary>
    /// Helper function to get/add a CanvasGroup component for this MonoBehaviour.
    /// </summary>
    /// <returns>The CanvasGroup component</returns>
    private CanvasGroup GetCanvasGroup() {
      var c = GetComponent<CanvasGroup>();
      if (c == null) {
        c = gameObject.AddComponent<CanvasGroup>();
      }
      return c;
    }

    /// <summary>
    /// Utility function to stop any current tween and start a new tween to reach the specified end value
    /// </summary>
    /// <param name="endValue">the target value to tween to</param>
    /// <param name="duration">length of the tween in seconds</param>
    /// <param name="delay">amount of time in seconds to wait before starting the tween.</param>
    /// <param name="endState">the state to be in after the tween completes</param>
    private void Fade(float endValue, float duration, float delay, States endState) {
      var canvasGroup = GetCanvasGroup();
      StopFading();
      if (duration <= 0 && delay <= 0) {
        canvasGroup.alpha = endValue;
        _state = endState;
        return;
      }
      _fadeTween = canvasGroup.DOFade(endValue, duration).SetDelay(delay).OnComplete(() => { _state = endState; });
    }

    /// <summary>
    /// Shows the CanvasGroup by increasing the opacity of the CanvasGroup to one over the specified duration and after the specified delay.
    /// </summary>
    /// <param name="duration">length of the tween in seconds</param>
    /// <param name="delay">amount of time in seconds to wait before starting the tween.</param>
    public void FadeIn(float duration = 0.5f, float delay = 0f) {
      if (_state == States.Showing || _state == States.Shown) {
        return;
      }

      _state = States.Showing;
      Fade(1f, duration, delay, States.Shown);
    }

    /// <summary>
    /// Hides the CanvasGroup by reducing the opacity of the CanvasGroup to zero over the specified duration and after the specified delay.
    /// </summary>
    /// <param name="duration">length of the tween in seconds</param>
    /// <param name="delay">amount of time in seconds to wait before starting the tween.</param>
    public void FadeOut(float duration = 0.5f, float delay = 0f) {
      if (_state == States.Hiding || _state == States.Hidden) {
        return;
      }

      _state = States.Hiding;
      Fade(0f, duration, delay, States.Hidden);
    }


    /// <summary>
    /// Stops any active fade tween
    /// </summary>
    public void StopFading() {
      _fadeTween?.Kill();
      _fadeTween = null;
      _state = States.Unknown;
    }

  }

}