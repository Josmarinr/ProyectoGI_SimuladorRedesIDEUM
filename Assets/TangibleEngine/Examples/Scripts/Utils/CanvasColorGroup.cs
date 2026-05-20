using UnityEngine;
using UnityEngine.UI;

namespace Ideum.Examples {

  /// <summary>
  /// Changes the graphic color of all child elements to match the Color property of this class
  /// </summary>
  [ExecuteInEditMode]
  public class CanvasColorGroup : MonoBehaviour {

    public Color Color = Color.white;

    private Graphic[] _childGraphics;

    void OnEnabled() {
      UpdateGraphicsList();
    }

    void OnTransformChildrenChanged() {
      UpdateGraphicsList();
    }

    void UpdateGraphicsList() {
      _childGraphics = GetComponentsInChildren<Graphic>();
    }

    void Update() {
      if (_childGraphics == null) {
        UpdateGraphicsList();
      }

      foreach (var gfx in _childGraphics) {
        gfx.color = Color;
      }
    }
  }

}