using System.Collections.Generic;
using UnityEngine;

namespace TE.Examples {
  /// <summary>
  /// A simple example of using tangible engine
  /// </summary>
  public class DemoController : MonoBehaviour, IOnTangibleAdded, IOnTangibleRemoved, IOnTangibleUpdated, IOnEnginePatternsChanged {

    public RectTransform CanvasRForm;
    public Transform ChildRoot;
    public GameObject ExampleTangiblePrefab;


    private Queue<ExampleTangibleData> _pool = new Queue<ExampleTangibleData>();

    private Dictionary<int,ExampleTangibleData> _tangibleMap = new Dictionary<int, ExampleTangibleData>();

    private Vector3 _offset;

    /// <summary>
    /// Gets a reference to or create an ExampleTangibleData instance that can be used.
    /// </summary>
    /// <param name="id">The tangible id to reference this instance</param>
    /// <returns></returns>
    private ExampleTangibleData GetExampleTangible(int id) {
      ExampleTangibleData e;
      if (!_tangibleMap.TryGetValue(id, out e)) {
        if (_pool.Count <= 0) {
          var o = Instantiate(ExampleTangiblePrefab, ChildRoot, false);
          e = new ExampleTangibleData(o);
          _tangibleMap[id] = e;
          return e;
        }
        else {
          e = _pool.Dequeue();
          _tangibleMap[id] = e;
        }
      }
      e.DoShow();
      return e;
    }

    /// <summary>
    /// Removes an ExampleTangibleData instance from the _tangibleMap, returns the instance to the _pool, and then hides the instance.
    /// </summary>
    /// <param name="id"></param>
    private void ReturnExampleTangible(int id) {
      ExampleTangibleData e;
      if (_tangibleMap.TryGetValue(id, out e)) {
        _tangibleMap.Remove(id);
        e.DoHide();
        _pool.Enqueue(e);
      }
    }

    void Start() {
      var r = CanvasRForm.rect;
      _offset = new Vector3(r.width, r.height) * -0.5f;
      TangibleEngine.Subscribe(this);
    }

    public void OnEnginePatternsChanged(List<Pattern> patterns) {
      Debug.Log("Patterns Updated");
    }

    public void OnTangibleAdded(Tangible t) {
      Debug.Log("Tangible added: "+t.Id);
      var e = GetExampleTangible(t.Id);
      e.Update(t, _offset);
    }

    public void OnTangibleRemoved(Tangible t) {
      Debug.Log("Tangible removed: "+t.Id);
      ReturnExampleTangible(t.Id);
    }

    public void OnTangibleUpdated(Tangible t) {
      ExampleTangibleData e;
      if (_tangibleMap.TryGetValue(t.Id, out e)) {
        e.Update(t, _offset);
      }
    }
    
  }
}