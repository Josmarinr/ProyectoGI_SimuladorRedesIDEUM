using System.Collections.Generic;

namespace TE.Utils {

  public class OnTangibleAddedPublisher : GenericDataPublisher<IOnTangibleAdded, Tangible> {
    protected override void PublishItem(IOnTangibleAdded i, Tangible data) {
      i.OnTangibleAdded(data);
    }
  }

  public class OnTangibleRemovedPublisher : GenericDataPublisher<IOnTangibleRemoved, Tangible> {
    protected override void PublishItem(IOnTangibleRemoved i, Tangible data) {
      i.OnTangibleRemoved(data);
    }
  }

  public class OnTangibleUpdatedPublisher : GenericDataPublisher<IOnTangibleUpdated, Tangible> {
    protected override void PublishItem(IOnTangibleUpdated i, Tangible data) {
      i.OnTangibleUpdated(data);
    }
  }

  public class OnEnginePatternsChangedPublisher : GenericDataPublisher<IOnEnginePatternsChanged, List<Pattern>> {
    protected override void PublishItem(IOnEnginePatternsChanged i, List<Pattern> data) {
      i.OnEnginePatternsChanged(data);
    }
  }

  public class OnEngineConnectedPublisher : GenericPublisher<IOnEngineConnected> {
    protected override void PublishItem(IOnEngineConnected i) {
      i.OnEngineConnected();
    }
  }

  public class OnEngineFailedToConnectPublisher : GenericPublisher<IOnEngineFailedToConnect> {
    protected override void PublishItem(IOnEngineFailedToConnect i) {
      i.OnEngineFailedToConnect();
    }
  }

  public class OnEngineDisconnectedPublisher : GenericPublisher<IOnEngineDisconnected> {
    protected override void PublishItem(IOnEngineDisconnected i) {
      i.OnEngineDisconnected();
    }
  }
}