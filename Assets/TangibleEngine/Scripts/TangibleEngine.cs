using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ideum.Logging;
using TE.Utils;
using TE.Sim;
using UnityEngine;

namespace TE {
  /// <summary>
  /// ---- Overview ---
  /// Tangible engine enables users to take advantage of specially engineered physical objects that can be
  /// recognized by Ideum touch-enabled products. The objects, called tangibles, are made of conductive
  /// material that mimics touches on the surface of a p-cap display. On the software end, the Tangible
  /// Engine is a windows service that runs in the background, awaiting the connection of client apps that
  /// require tangible recognition capabilities. A service model was chosen to enable Ideum to create and
  /// deploy fixes and new features without requiring clients to rebuild their own apps. The unity bindings
  /// use a TCP socket to communicate with a locally running service.
  ///
  /// 
  /// --- Setting up the Engine ---
  /// 
  /// This class is designed to operate as a singleton, so the primary way of accessing it should be through
  /// the call TangibleEngine.Instance. If the engine is not initialized or added to the scene, it will be
  /// when that property is invoked for the first time. On the contrary, if this property getter is invoked
  /// and there is a TangibleEngine MonoBehaviour attached to an active GameObject instance in the scene,
  /// it will be used as the singleton instance.
  ///
  /// There is also a prefab, which can be found at  TangibleEngine/Resources/TE.TangibleEngine. Putting
  /// this in your scene enables you to reference the TangibleEngine component directly, as well as giving
  /// you the ability to change some of the configuration options. There are two sets of properties, one for
  /// the editor <see cref="EditorProperties"/>, and one for runtime <see cref="RuntimeProperties"/>. Changing
  /// these properties allows one to enable or disable logging as well as switching between the service or
  /// simulator modes of operation.
  ///
  /// By default, the editor mode will utilize the simulator (which does not require the TE service to run).
  /// The intent of this mode is to provide developers with a method of testing the TE API without requiring
  /// a touch screen, a trained profile for that screen, and a set of tangibles.
  ///
  /// The runtime mode will default to using the TE service which requires a supported Ideum touch screen, a
  /// profile, and a set of tangibles. This can be changed easily by modifying the value on the TangibleEngine
  /// component, either in the scene or by updating the prefab directly.
  ///
  /// 
  /// --- Interfacing with the Engine ---
  /// 
  /// There are two methods of getting data from Tangible Engine: Events and Interfaces. The decision to
  /// implement both types of subscription models was to accomidate different styles of coding. For example,
  ///
  /// <code>
  /// public class EventDemo : MonoBehaviour {
  ///   void Start() {
  ///     TangibleEngine.OnTangibleAdded += HandleOnTangibleAdded;
  ///     TangibleEngine.OnTangibleUpdated += HandleOnTangibleUpdated;
  ///     TangibleEngine.OnTangibleRemoved += HandleOnTangibleRemoved;
  ///   }
  ///
  ///   void OnDestroy() {
  ///     TangibleEngine.OnTangibleAdded -= HandleOnTangibleAdded;
  ///     TangibleEngine.OnTangibleUpdated -= HandleOnTangibleUpdated;
  ///     TangibleEngine.OnTangibleRemoved -= HandleOnTangibleRemoved;
  ///   }
  /// 
  ///   private void HandleOnTangibleAdded(Tangible obj) {   }
  ///   
  ///   private void HandleOnTangibleUpdated(Tangible obj) {   }
  ///   
  ///   private void HandleOnTangibleRemoved(Tangible obj) {   }      
  /// }
  /// </code>
  ///
  /// is effectively the same as:
  ///
  /// <code>
  /// public class InterfaceDemo : MonoBehaviour, IOnTangibleAdded, IOnTangibleUpdated, IOnTangibleRemoved {
  ///   void Start() {
  ///     TangibleEngine.Subscribe(this);
  ///   }
  ///
  ///   void OnDestroy() {
  ///     TangibleEngine.Unsubscribe(this);
  ///   }
  ///   
  ///   public void OnTangibleAdded(Tangible t) {   }
  ///   
  ///   public void OnTangibleUpdated(Tangible t) {   }
  ///   
  ///   public void OnTangibleRemoved(Tangible t) {   }
  /// }
  /// </code>
  ///
  /// There are also other events and interface equivalents pertaining to the state of the engine:
  ///
  /// <see cref="OnEngineConnected"/>
  /// <see cref="OnEngineDisconnected"/>
  /// <see cref="OnEngineFailedToConnect"/>
  /// <see cref="OnEngineFailedToConnect"/>
  ///
  /// See the example scene and associated scripts for a sample use case of Tangible Engine 2.
  /// </summary>
  public sealed class TangibleEngine : MonoBehaviour {

    #region Engine Handling
    /// <summary>
    /// Called when the Tangible Engine has connected to the service.
    /// </summary>
    public static event Action OnEngineConnected {
      add { Instance._OnEventEngineConnected += value; }
      remove { Instance._OnEventEngineConnected -= value; }
    }

    /// <summary>
    /// Called when the Tangible Engine has failed to connect to the service.
    /// </summary>
    public static event Action OnEngineFailedToConnect {
      add { Instance._OnEventEngineFailedToConnect += value; }
      remove { Instance._OnEventEngineFailedToConnect -= value; }
    }

    /// <summary>
    /// Called when the Tangible Engine has disconnected from the service.
    /// </summary>
    public static event Action OnEngineDisconnected {
      add { Instance._OnEventEngineDisconnected += value; }
      remove { Instance._OnEventEngineDisconnected -= value; }
    }

    /// <summary>
    /// Called when the tangible engine's patterns have been updated
    /// </summary>
    public static event Action<List<Pattern>> OnPatternsUpdated {
      add { Instance._OnEventPatternsUpdated += value; }
      remove { Instance._OnEventPatternsUpdated -= value; }
    }

    /// <summary>
    /// Called when a new tangible has been added
    /// </summary>
    public static event Action<Tangible> OnTangibleAdded {
      add { Instance._OnEventTangibleAdded += value; }
      remove { Instance._OnEventTangibleAdded -= value; }
    }

    /// <summary>
    /// Called when a tangible has been removed
    /// </summary>
    public static event Action<Tangible> OnTangibleRemoved {
      add { Instance._OnEventTangibleRemoved += value; }
      remove { Instance._OnEventTangibleRemoved -= value; }
    }

    /// <summary>
    /// Called when a tangible has been updated
    /// </summary>
    public static event Action<Tangible> OnTangibleUpdated {
      add { Instance._OnEventTangibleUpdated += value; }
      remove { Instance._OnEventTangibleUpdated -= value; }
    }

    /// <summary>
    /// True when the Tangible Engine is connected to the service
    /// </summary>
    public bool IsConnected { get; private set; }

    /// <summary>
    /// True when the Tangible Engine is initialized
    /// </summary>
    public bool IsEngineInit { get; private set; }

    /// <summary>
    /// The collection of patterns
    /// </summary>
    public List<Pattern> Patterns {
      get { return _patterns; }
    }

    /// <summary>
    /// The collection of active tangibles
    /// </summary>
    public List<Tangible> Tangibles {
      get { return _tangibles; }
    }

    public TangibleEnginePropertiesData EditorProperties = new TangibleEnginePropertiesData() {
      LogLevels = Log.Levels.Bad,
      Mode = TangibleEnginePropertiesData.Modes.Simulator
    };

    public TangibleEnginePropertiesData RuntimeProperties = new TangibleEnginePropertiesData() {
      LogLevels = Log.Levels.None,
      Mode = TangibleEnginePropertiesData.Modes.Service
    };

    private event Action _OnEventEngineConnected;
    private event Action _OnEventEngineFailedToConnect;
    private event Action _OnEventEngineDisconnected;
    private event Action<List<Pattern>> _OnEventPatternsUpdated;
    private event Action<Tangible> _OnEventTangibleAdded;
    private event Action<Tangible> _OnEventTangibleRemoved;
    private event Action<Tangible> _OnEventTangibleUpdated;

    private TangibleEnginePropertiesData _properties;
    private ITangibleProvider _engine;
    private ITouchPointProvider _touchProvider;
    private Dispatcher _dispatch;
    private readonly List<Pattern> _patterns = new List<Pattern>();
    private readonly Dictionary<int,Pattern> _patternsMap = new Dictionary<int, Pattern>();
    private readonly List<Tangible> _tangibles = new List<Tangible>();
    private readonly Dictionary<int, Tangible> _tangiblesMap = new Dictionary<int, Tangible>();
    private readonly List<Tangible> _tangibleAddedBuffer = new List<Tangible>();
    private readonly List<Tangible> _tangibleRemovedBuffer = new List<Tangible>();
    private readonly List<Tangible> _tangibleUpdatedBuffer = new List<Tangible>();
    private readonly List<Tangible> _tangibleBuffer = new List<Tangible>();
    private readonly OnTangibleAddedPublisher _onTangibleAdded = new OnTangibleAddedPublisher();
    private readonly OnTangibleRemovedPublisher _onTangibleRemoved = new OnTangibleRemovedPublisher();
    private readonly OnTangibleUpdatedPublisher _onTangibleUpdated = new OnTangibleUpdatedPublisher();
    private readonly OnEnginePatternsChangedPublisher _onEnginePatternsChanged = new OnEnginePatternsChangedPublisher();
    private readonly OnEngineConnectedPublisher _onEngineConnected = new OnEngineConnectedPublisher();
    private readonly OnEngineDisconnectedPublisher _onEngineDisconnected = new OnEngineDisconnectedPublisher();
    private readonly OnEngineFailedToConnectPublisher _onEngineFailedToConnect = new OnEngineFailedToConnectPublisher();

    /// <summary>
    /// Subscribes to tangible engine events
    /// </summary>
    public static void Subscribe(object o) {
      var i = Instance;
      i._onTangibleAdded.Subscribe(o as IOnTangibleAdded);
      i._onTangibleRemoved.Subscribe(o as IOnTangibleRemoved);
      i._onTangibleUpdated.Subscribe(o as IOnTangibleUpdated);
      i._onEnginePatternsChanged.Subscribe(o as IOnEnginePatternsChanged);
      i._onEngineConnected.Subscribe(o as IOnEngineConnected);
      i._onEngineFailedToConnect.Subscribe(o as IOnEngineFailedToConnect);
      i._onEngineDisconnected.Subscribe(o as IOnEngineDisconnected);

      var ec = o as IOnEngineConnected;
      if (ec != null && i.IsConnected) {
        ec.OnEngineConnected();
      }
    }

    /// <summary>
    /// Unsubscribes from tangible engine events
    /// </summary>
    public static void Unsubscribe(object o) {
      var i = Instance;
      i._onTangibleAdded.Unsubscribe(o as IOnTangibleAdded);
      i._onTangibleRemoved.Unsubscribe(o as IOnTangibleRemoved);
      i._onTangibleUpdated.Unsubscribe(o as IOnTangibleUpdated);
      i._onEnginePatternsChanged.Unsubscribe(o as IOnEnginePatternsChanged);
      i._onEngineConnected.Unsubscribe(o as IOnEngineConnected);
      i._onEngineFailedToConnect.Unsubscribe(o as IOnEngineFailedToConnect);
      i._onEngineDisconnected.Unsubscribe(o as IOnEngineDisconnected);
    }

    private void UpdateTangiblesState() {
      if (!IsEngineInit) return;
      if (!IsConnected) return;
      lock (_tangibleBuffer) {
        foreach (var newTangible in _tangibleBuffer) {
          Tangible t;
          if (_tangiblesMap.TryGetValue(newTangible.Id, out t)) {
            t.X = newTangible.X;
            t.Y = newTangible.Y;
            t.R = newTangible.R;
            _tangibleUpdatedBuffer.Add(t);
          }
          else {
            t = newTangible;
            if (!_patternsMap.TryGetValue(t.PatternId, out t.Pattern)) {
              Log.Error("Could not find matching pattern for tangible with pattern id: " + t.PatternId);
            }
            _tangibleAddedBuffer.Add(t);
          }
        }

        foreach (var curTangible in _tangiblesMap) {
          if (_tangibleBuffer.All(p => p.Id != curTangible.Key)) {
            _tangibleRemovedBuffer.Add(curTangible.Value);
          }
        }
        _tangibleBuffer.Clear();
      }

      if (_tangibleAddedBuffer.Count > 0) {
        foreach (var tangible in _tangibleAddedBuffer) {
          _tangibles.Add(tangible);
          _tangiblesMap[tangible.Id] = tangible;
          _onTangibleAdded.Publish(tangible);
          if (_OnEventTangibleAdded != null) {
            _OnEventTangibleAdded(tangible);
          }
        }
        _tangibleAddedBuffer.Clear();
      }

      if (_tangibleUpdatedBuffer.Count > 0) {
        foreach (var tangible in _tangibleUpdatedBuffer) {
          _tangiblesMap[tangible.Id] = tangible;
          _onTangibleUpdated.Publish(tangible);
          if (_OnEventTangibleUpdated != null) {
            _OnEventTangibleUpdated(tangible);
          }
        }
        _tangibleUpdatedBuffer.Clear();
      }

      if (_tangibleRemovedBuffer.Count > 0) {
        foreach (var tangible in _tangibleRemovedBuffer) {
          _tangibles.Remove(tangible);
          _tangiblesMap.Remove(tangible.Id);
          _onTangibleRemoved.Publish(tangible);
          if (_OnEventTangibleRemoved != null) {
            _OnEventTangibleRemoved(tangible);
          }
        }
        _tangibleRemovedBuffer.Clear();
      }

    }

    private void InitEngine() {
      if (IsEngineInit) return;
      IsEngineInit = true;


      if (_properties == null) {
#if UNITY_EDITOR
        _properties = EditorProperties;
        LogSettings.EditorFormatting = true;
#else
        _properties = RuntimeProperties;
        LogSettings.EditorFormatting = false;
#endif
      }

      LogSettings.EnabledLevels = _properties.LogLevels;

      switch (_properties.Mode) {
        case TangibleEnginePropertiesData.Modes.Simulator:
          Log.Out("TE2: Initializing Simulator...");
          _engine = new TangibleProviderSimulator();
          break;
        case TangibleEnginePropertiesData.Modes.Service:
          Log.Out("TE2: Initializing Service Connection...");
          _engine = new TangibleProviderService();
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      if (_engine == null) {
        Log.Error("TE2: Failed to find or create ITangibleProvider instance. Defaulting to Service.");
        _engine = new TangibleProviderService() {
          PortNumber = _properties.PortNumber
        };
      }

      _engine.Connected += HandleEngineConnectedEvent;
      _engine.Disconnected += HandleEngineDisconnectEvent;
      _engine.FailedToConnect += HandleEngineFailedToConnectEvent;
      _engine.TangiblesUpdated += HandleEngineTangiblesUpdated;
      _engine.PatternsUpdated += HandleEnginePatternsUpdated;
      

      if (_touchProvider == null) {
        _touchProvider = gameObject.GetComponent<ITouchPointProvider>();
      }
      if (_touchProvider == null) {
        _touchProvider = gameObject.AddComponent<TouchPointProvider>();
      }

      _touchProvider.TouchPointsUpdated += HandleTouchPointsUpdated;

      if (_dispatch == null) {
        _dispatch = gameObject.GetComponent<Dispatcher>();
      }
      if (_dispatch == null) {
        _dispatch = gameObject.AddComponent<Dispatcher>();
      }

      _engine.StartProvider();
    }

    private void DeinitEngine() {
      if (!IsEngineInit) return;
      IsEngineInit = false;
      if (_engine != null) {
        _engine.Dispose();
      }

      if (_touchProvider != null) {
        _touchProvider.TouchPointsUpdated -= HandleTouchPointsUpdated;
      }
    }

    private void HandleEngineTangiblesUpdated(ICollection<Tangible> tangibles) {
      if (tangibles == null) return;
      lock (_tangibleBuffer) {
        foreach (var t in tangibles) {
          _tangibleBuffer.Add(t);
        }
      }
    }

    private void HandleEnginePatternsUpdated(ICollection<Pattern> patterns) {
      _dispatch.Dispatch(() => {
        _patterns.Clear();
        _patterns.AddRange(patterns);
        _patternsMap.Clear();
        foreach (var p in _patterns) {
          _patternsMap[p.PatternId] = p;
        }
        _onEnginePatternsChanged.Publish(_patterns);

        if (_OnEventPatternsUpdated != null) {
          _OnEventPatternsUpdated(_patterns);
        }
      });
    }

    private void HandleEngineConnectedEvent() {
      IsConnected = true;
      _engine.RequestPatterns();
      Log.Out("TE2: Connected");
      _onEngineConnected.Publish();
      if (_OnEventEngineConnected != null) _OnEventEngineConnected();
    }

    private void HandleEngineFailedToConnectEvent() {
      IsConnected = false;
      Log.Failure("TE2: Failed to connect");
      _onEngineFailedToConnect.Publish();
      if (_OnEventEngineFailedToConnect != null) _OnEventEngineFailedToConnect();
    }

    private void HandleEngineDisconnectEvent() {
      IsConnected = false;
      Log.Out("TE2: Disconnected");
      _onEngineDisconnected.Publish();
      _OnEventEngineDisconnected?.Invoke();
      if (_OnEventEngineFailedToConnect != null) _OnEventEngineFailedToConnect();
    }

    private void HandleTouchPointsUpdated(ICollection<Pointer> pointers) {
      if (_engine != null) {
        _engine.UpdatePointers(pointers);
      }
    }

    /// <summary>
    /// Provides an array of Touch instances that are being used by tangibles.
    /// </summary>
    public TouchScript.Pointers.Pointer[] AssociatedTouchPoints {
      get {
        var i = _tangibles.SelectMany(p => p.PointerIds);
        return TouchScript.TouchManager.Instance.Pointers.Where(p => i.Contains(p.Id)).ToArray();
      }
    }

    /// <summary>
    /// Provides an array of Touch instances that are not being used by tangibles.
    /// </summary>
    public TouchScript.Pointers.Pointer[] UnassociatedTouchPoints {
      get {
        var i = _tangibles.SelectMany(p => p.PointerIds);
        return TouchScript.TouchManager.Instance.Pointers.Where(p => !i.Contains(p.Id)).ToArray();
      }
    }

    #endregion


    #region MonoBehaviour Messages

    void Awake() {
      InsureSingleton(this);
    }

    void Start() {
      InitEngine();
    }

    private void OnEnable() {
      InitEngine();
    }

    private void OnDisable() {
      DeinitEngine();
    }

    private void Update() {
      UpdateTangiblesState();
    }

    private void OnDestroy() {
      DeinitEngine();
    }

    private void OnApplicationQuit() {
      DeinitEngine();
      Thread.Sleep(500); // give system resources time to release before stopping the unity player.
    }

    #endregion


    #region Singleton

    private static TangibleEngine _instance;

    /// <summary>
    /// Retreives the existing TangibleEngine interface or creates  
    /// </summary>
    public static TangibleEngine Instance {
      get { return _instance ?? (_instance = GetOrCreateInstance()); }
    }

    /// <summary>
    /// Searches the scene heirarchy for an instance of the TangibleEngine mono behavior. If none is found, a new GameObject is created and the TangibleEngine MonoBehaviour is added.
    /// </summary>
    /// <returns>'The' instance of the TangibleEngine</returns>
    private static TangibleEngine GetOrCreateInstance() {
      if (_instance != null) return _instance;
      //look for an active instance in the scene.
      var te = FindObjectOfType<TangibleEngine>();
      if (te == null) {
        //look for any TangibleEngine object that is currently loaded (active or inactive).
        var tes = Resources.FindObjectsOfTypeAll<TangibleEngine>();
        if (tes.Length > 0) {
          te = Instantiate(tes[0]);
        }
        if (tes.Length > 1) {
          Log.Warn("TE2: There are multiple instances of the TangibleEngine found in resources. The first one is being instantiated.");
        }
      }
      if (te == null) {
        var go = new GameObject("TE.TangibleEngine");
        te = go.AddComponent<TangibleEngine>();
      }

      if (!te.gameObject.activeInHierarchy) {
        Log.Warn("TE2: The found TangibleEngine instance is not active. Attempting to activate the object.");
        te.gameObject.SetActive(true);
      }

      return te;
    }

    private static void InsureSingleton(TangibleEngine te) {
      if (_instance == null) {
        _instance = te;
      }

      if (_instance != te) {
        Log.Error("TE2: Duplicate singleton instance. Removing this instance.");
        Destroy(te.gameObject);
      }
    }

    #endregion

  }
}
