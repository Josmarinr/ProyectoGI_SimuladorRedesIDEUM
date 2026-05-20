using System.Collections.Generic;
using Ideum.Logging;
using Newtonsoft.Json;
using UnityEngine;

namespace TE.Sim {
  public class TangibleProviderSimulator : TangibleProviderBase {

    public static TangibleProviderSimulator Instance;

    private readonly List<Tangible> _tangibles = new List<Tangible>();
    private SimulatorView _simulatorView;
    private EngineProfile _profile;

    public TangibleProviderSimulator() {
      Instance = this;
    }

    public override void StartProvider() {

      var p = Resources.Load("TE.Profile") as TextAsset;
      if (p != null) {
        var s = p.text;
        if (!string.IsNullOrEmpty(s)) {
          _profile = JsonConvert.DeserializeObject<EngineProfile>(s);
          Log.Out("TE2: Parsed patterns file");
        }
      }

      if (_profile == null) {
        _profile = EngineProfile.DefaultProfile();
      }

      var sim = Object.FindObjectOfType<SimulatorView>();
      if (sim == null)
      {
        var o = Resources.Load("TE.Sim.SimulatorView");
        if (o == null)
        {
          Log.Error("TE2: Could not find TE.SimulatorView prefab. The simulator will not function.");
          OnFailedToConnect();
        }
        else
        {
          var inst = Object.Instantiate(o);
          var go = inst as GameObject;
          if (go == null)
          {
            OnFailedToConnect();
          }
          else
          {
            _simulatorView = go.GetComponent<SimulatorView>();
            if (_simulatorView == null)
            {
              OnFailedToConnect();
            }
            else
            {
              _simulatorView.Init(this, _tangibles, _profile);
              OnConnected();
            }
          }
        }
      }
      else
      {
        _simulatorView = sim;
        _simulatorView.Init(this, _tangibles, _profile);
        OnConnected();
      }
    }

    public void InvokeUpdate() {
      OnTangiblesUpdated(_tangibles);
    }

    public override void UpdatePointers(ICollection<Pointer> pointers) {
      OnTangiblesUpdated(_tangibles);
    }

    public override void Dispose() {
      OnDisconnected();
    }

    public override void SetPatterns(List<Pattern> patterns) {
      OnPatternsUpdated(_profile.Patterns);
    }

    public override void RequestPatterns() {
      OnPatternsUpdated(_profile.Patterns);
    }

  }
}
