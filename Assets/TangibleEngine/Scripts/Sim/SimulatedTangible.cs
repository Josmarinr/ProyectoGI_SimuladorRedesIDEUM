using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TE.Sim {
  public class SimulatedTangible : MonoBehaviour {

    public static int tangibleId;

    public SimulatedTangibleRotationHandle rotateHandle;


    public Color SelectedButtonColor, UnselectedButtonColor, SelectedTextColor, UnselectedTextColor;

    public Graphic OnButtonBg, OffButtonBg, OnLabel, OffLabel;


    public Text PatternText;

    private RectTransform _rectTransform;
    private Vector2 _offset;

    private EngineProfile _profile;
    private Tangible _tangible;
    private SimulatorView _simView;

    private List<int> _patternIds;
    private Dictionary<int, Pattern> _patternMap = new Dictionary<int, Pattern>();
    private int _patternIndex;

    void Start() {
      _rectTransform = GetComponent<RectTransform>();
      OnButtonBg.color = UnselectedButtonColor;
      OnLabel.color = UnselectedTextColor;
      OffButtonBg.color = SelectedButtonColor;
      OffLabel.color = SelectedTextColor;
    }

    public void Init(SimulatorView simView, EngineProfile profile) {
      _simView = simView;
      _profile = profile;
      _patternIds = new List<int>();
      foreach (var p in _profile.Patterns) {
        _patternMap[p.PatternId] = p;
        _patternIds.Add(p.PatternId);
      }
      _patternIds.Sort();
      _patternIndex = 0;
      UpdatePatternVisualProperties();
    }

    private void UpdatePatternVisualProperties() {
      PatternText.text = _patternIds[_patternIndex]+"";
    }

    void Update() {

      if (_profile != null) {
        var id = _patternIds[_patternIndex];
        var p = _patternMap[id];
        //135-96
        var m = Mathf.Max(135, p.Radius);
        _rectTransform.sizeDelta = Vector2.one * m * 2;
        rotateHandle.Distance = m - 39;
      }

      if (_tangible == null || _profile == null) return;
      _tangible.X = _rectTransform.anchoredPosition.x;
      _tangible.Y = _rectTransform.anchoredPosition.y;
      _tangible.R = rotateHandle.R * Mathf.Deg2Rad;
    }

    private void RemoveThis() {
      if (_tangible != null) {
        DeactivateTangible();
      }
      if (_simView != null) {
        _simView.RemoveSimulatedTangible(this);
      }
      Destroy(gameObject);
    }

    public Tangible GetTangible() {
      if (_rectTransform == null) {
        _rectTransform = GetComponent<RectTransform>();
      }

      var id = _patternIds[_patternIndex];
      var tangible = new Tangible {
        Id = tangibleId++,
        PatternId = id,
        Pos = {
          x = _rectTransform.anchoredPosition.x,
          y = _rectTransform.anchoredPosition.y
        },
        R = rotateHandle.R * Mathf.Deg2Rad,
        PointerIds = new[] {1, 2, 3}
      };
      return tangible;
    }

    public void PatternDecrement() {
      if (_tangible != null) return;
      _patternIndex--;
      if (_patternIndex < 0) {
        _patternIndex = _patternIds.Count - 1;
      }
      UpdatePatternVisualProperties();
    }

    public void PatternIncrement() {
      if (_tangible != null) return;
      _patternIndex++;
      if (_patternIndex >= _patternIds.Count) {
        _patternIndex = 0;
      }
      UpdatePatternVisualProperties();
    }

    public void RemovePattern() {
      RemoveThis();
    }

    public void ActivateTangible() {
      //Debug.Log("Activate Tangible");
      _tangible = GetTangible();
      _simView.AddTangible(_tangible);
      OnButtonBg.color = SelectedButtonColor;
      OnLabel.color = SelectedTextColor;
      OffButtonBg.color = UnselectedButtonColor;
      OffLabel.color = UnselectedTextColor;
    }

    public void DeactivateTangible() {
      //Debug.Log("Deactivate Tangible");
      if (_tangible == null) return;
      _simView.RemoveTangible(_tangible);
      _tangible = null;
      OnButtonBg.color = UnselectedButtonColor;
      OnLabel.color = UnselectedTextColor;
      OffButtonBg.color = SelectedButtonColor;
      OffLabel.color = SelectedTextColor;
    }
  }
}