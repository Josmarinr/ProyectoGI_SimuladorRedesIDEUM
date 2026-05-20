using DG.Tweening;
using Shared;
using UnityEngine;
using UnityEngine.UI;

namespace Example3 {
  public class InfoPopOut : MonoBehaviour {

    private const float ContentMaskMaskSizeX = 695f;
    private const float ContentMaskSizeY = 306f;

    public Transform Origin;
    public Collider2D Collider;
    public RectTransform ContentMask;
    public Graphic Thumbnail;
    public CanvasGroupVisibility CanvasGroupVisibility;

    private Sequence _popSequence;


    void Start() {
      _popSequence?.Kill();
      _popSequence = CreatePopClose();
    }

    void OnTriggerEnter2D(Collider2D collider) {
      _popSequence?.Kill();
      _popSequence = CreatePopOpen();
    }

    void OnTriggerExit2D(Collider2D collider) {
      _popSequence?.Kill();
      _popSequence = CreatePopClose();
    }

    private Sequence CreatePopOpen() {
      var s = DOTween.Sequence();
      s.Insert(0f,transform.DOScale(1f, 0.2f).SetEase(Ease.InOutCubic));
      s.Insert(0f,ContentMask.DOSizeDelta(new Vector2(ContentMaskMaskSizeX, ContentMaskSizeY), 0.8f).SetEase(Ease.InOutQuint));
      
      return s;
    }

    private Sequence CreatePopClose() {
      var s = DOTween.Sequence();
      s.Insert(0, ContentMask.DOSizeDelta(new Vector2(0, ContentMaskSizeY), 0.4f).SetEase(Ease.InOutQuint));
      s.Insert(0, transform.DOScale(0.8f, 0.2f).SetEase(Ease.InOutCubic));
      return s;
    }
  }
}