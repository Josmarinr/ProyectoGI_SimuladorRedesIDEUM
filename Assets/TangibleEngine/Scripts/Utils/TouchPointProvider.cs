using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TE {
  public class TouchPointProvider : MonoBehaviour, ITouchPointProvider {

    public event Action<ICollection<Pointer>> TouchPointsUpdated;

    [NonSerialized]
    private Dictionary<int, Pointer> _touchIdToPointerMap = new Dictionary<int, Pointer>();

    [NonSerialized]
    private List<int> _touchIdsToRemove = new List<int>();

    [NonSerialized]
    private int _minIdValue;

    void Update() {
      //var touches = Input.touches;
      var touches = TouchScript.TouchManager.Instance.Pointers;
      foreach (var t in touches) {
        int rawId = t.Id;
        Pointer v;
        if (_touchIdToPointerMap.TryGetValue(rawId, out v)) {
          v.X = t.Position.x;
          v.Y = t.Position.y;
        }
        else {
          v = new Pointer {
            Id = rawId,
            X = t.Position.x,
            Y = t.Position.y
          };
          _touchIdToPointerMap[rawId] = v;
        }
      }

      foreach (var id in _touchIdToPointerMap.Keys) {
        if (touches.All(p => p.Id != id)) {
          _touchIdsToRemove.Add(id);
        }
      }

      foreach (var id in _touchIdsToRemove) {
        _touchIdToPointerMap.Remove(id);
      }

      _touchIdsToRemove.Clear();

      if (TouchPointsUpdated != null) {
        TouchPointsUpdated(_touchIdToPointerMap.Values);
      }
    }
  }
}