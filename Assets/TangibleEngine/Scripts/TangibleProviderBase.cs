using System;
using System.Collections.Generic;

namespace TE {
  // Used by TangibleProviderService

  public abstract class TangibleProviderBase : ITangibleProvider {
    
    public event Action Connected;
    public event Action Disconnected;
    public event Action FailedToConnect;
    public event Action<List<Tangible>> TangiblesUpdated;
    public event Action<List<Pattern>> PatternsUpdated;

    public abstract void SetPatterns(List<Pattern> patterns);
    public abstract void RequestPatterns();
    public abstract void UpdatePointers(ICollection<Pointer> pointers);
    public abstract void StartProvider();

    public abstract void Dispose();

    protected virtual void OnConnected() {
      var handler = Connected;
      if (handler != null) handler();
    }

    protected virtual void OnDisconnected() {
      var handler = Disconnected;
      if (handler != null) handler();
    }

    protected virtual void OnFailedToConnect() {
      var handler = FailedToConnect;
      if (handler != null) handler();
    }

    protected virtual void OnTangiblesUpdated(List<Tangible> obj) {
      var handler = TangiblesUpdated;
      if (handler != null) handler(obj);
    }

    protected virtual void OnPatternsUpdated(List<Pattern> obj) {
      var handler = PatternsUpdated;
      if (handler != null) handler(obj);
    }
  }
}