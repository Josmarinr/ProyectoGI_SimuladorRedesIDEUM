using System.Collections.Generic;
using Ideum.Logging;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TE.Sim {
  public class SimulatorView : MonoBehaviour {
    
    private TangibleProviderSimulator _sim;
    private List<Tangible> _tangibles;
    private readonly List<SimulatedTangible> _sims = new List<SimulatedTangible>();
    private EngineProfile _profile;
    private Object _simTangiblePrefab;


    public void Init(TangibleProviderSimulator sim, List<Tangible> tangiblesList, EngineProfile profile) {
      _sim = sim;
      _tangibles = tangiblesList;
      _profile = profile;
      var es = FindObjectOfType<EventSystem>();
      if (es == null) {
        gameObject.AddComponent<EventSystem>();
      }
      var inputMod = FindObjectOfType<BaseInputModule>();
      if (inputMod == null) {
        gameObject.AddComponent<StandaloneInputModule>();
      }
    }


    public void AddTangible(Tangible t) {
      if (_tangibles != null && !_tangibles.Contains(t)) {
        _tangibles.Add(t);
        _sim.InvokeUpdate();
      }
    }

    public void RemoveTangible(Tangible t) {
      if (_tangibles != null && _tangibles.Contains(t)) {
        _tangibles.Remove(t);
        if (_tangibles.Count <= 0) {
          _sim.InvokeUpdate();
        }
      }
      
    }

    public void CreateSimulatedTangible() {
      Log.Out("TE2: Creating Simulated Tangible.");
      if (_simTangiblePrefab == null) {
        _simTangiblePrefab = Resources.Load("TE.Sim.SimulatedTangible");
        
      }
      if (_simTangiblePrefab == null) {
        Log.Error("TE2: Can not find SimulatedTangible prefab.");
      }
      else {
        var inst = Instantiate(_simTangiblePrefab) as GameObject;
        if (inst != null) {
          inst.transform.SetParent(transform, false);
          var sim = inst.GetComponent<SimulatedTangible>();
          _sims.Add(sim);
          sim.Init(this, _profile);
        }
        else {
          Log.Error("TE2: Could not instantiate SimulatedTangible prefab");
        }
      }
    }

    public void RemoveSimulatedTangible(SimulatedTangible tangible) {
      if (_sims.Contains(tangible)) {
        _sims.Remove(tangible);
      }
    }

    void Update() {
      if (_sim == null || _tangibles==null || _profile==null) return;
      if (_tangibles.Count > 0) {
        _sim.InvokeUpdate();
      }
    }
  }
}