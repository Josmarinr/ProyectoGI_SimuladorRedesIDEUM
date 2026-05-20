using UnityEngine;
using UnityEngine.EventSystems;

namespace TE.Utils {
  public class ScreenDraggable : MonoBehaviour, IPointerDownHandler, IDragHandler {

    public RectTransform TransformTarget;
    private Vector2 _offset;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void OnPointerDown(PointerEventData e) {
      _offset = TransformTarget.anchoredPosition - e.position;
    }

    public void OnDrag(PointerEventData e) {
      TransformTarget.anchoredPosition = e.position + _offset;
    }
  }
}
