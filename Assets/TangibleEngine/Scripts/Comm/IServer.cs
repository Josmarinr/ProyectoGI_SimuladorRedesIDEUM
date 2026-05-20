using System;
using System.Net.Sockets;

namespace TE.Comm {
  public interface IServer : IDisposable {
    void Stop();
    void Start();
    void SetProtocol(ProtocolType protocol);
  }
}
