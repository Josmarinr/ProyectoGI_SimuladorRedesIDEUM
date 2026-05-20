using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TE.Sim {
  public class SimulatedTangibleRotationHandle : MonoBehaviour, IPointerDownHandler, IDragHandler {

    [NonSerialized]
    public float R;

    private RectTransform _rect;

    private float _distance;

    public float Distance {
      get { return _distance; }
      set {
        _distance = value;
        var rr = R * Mathf.Deg2Rad;
        _rect.localPosition = new Vector3(Distance * Mathf.Cos(rr), Distance * Mathf.Sin(rr), 0);
      }
    }


    void Start() {
      _rect = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData) {
      
    }

    public void OnDrag(PointerEventData eventData) {
      Vector2 p;
      float last_r = Mathf.Atan2(_rect.localPosition.y, _rect.localPosition.x);
      RectTransformUtility.ScreenPointToLocalPointInRectangle(transform.parent.GetComponent<RectTransform>(), eventData.position, null, out p);
      var new_r = Mathf.Atan2(p.y, p.x);
      float diff = new_r - last_r;
      diff *= Mathf.Rad2Deg;
      if (diff < -180f) { diff += 360f; }
      if (diff > 180f) { diff -= 360f; }
      R += diff;
      _rect.localPosition = new Vector3(Distance * Mathf.Cos(new_r), Distance * Mathf.Sin(new_r), 0);
      _rect.localEulerAngles = new Vector3(0, 0, R + 90);
    }
  }
}