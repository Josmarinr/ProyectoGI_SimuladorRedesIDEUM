using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Ideum.Logging;
using TE.Comm;

namespace TE {
  // Creates, manages, and destroys TCP connection
  // Reads and sends messages to and from server

  // Used by TangibleEngine on start
  internal class TangibleProviderService : TangibleProviderBase, IReceiver {

    public static uint MessageId = 0;

    public int PortNumber = 4949;

    private ConnectionManager _connectionManager;
    private List<Tangible> _tangibles;
    private List<Pattern> _patterns;

    public override void StartProvider() {
      CreateConnection();
    }


    public override void SetPatterns(List<Pattern> patterns) {
      Msg newMessage = new Msg(Msg.MsgTypes.Patterns) { Patterns = patterns.ToArray<Pattern>() };
      Send(newMessage);
    }

    public override void RequestPatterns() {
      Msg newMessage = new Msg(Msg.MsgTypes.Patterns);
      Send(newMessage);
    }

    public override void UpdatePointers(ICollection<Pointer> pointers) {
      if (pointers == null) {
        return;
      }

      Msg newMessage = new Msg(Msg.MsgTypes.Update) { Pointers = pointers.ToArray() };
      Send(newMessage);
    }

    public override void Dispose() {
      CloseConnection();
    }

    #region Connection Methods
    private void CloseConnection() {
      if (_connectionManager != null && _connectionManager.Connection != null) {
        _connectionManager.Connection.Close();
        OnDisconnected();
      }
      _connectionManager = null;
    }

    private bool CreateConnection() {
      TCPConnection connection;
      if (TCPConnection.TryOpen(new IPEndPoint(IPAddress.Loopback, PortNumber), out connection)) {
        _connectionManager = new ConnectionManager(connection, this, OnGeneralException, OnParseError, ConnectionClosed);
        OnConnected();
        return true;
      }
      else {
        OnFailedToConnect();
        return false;
      }
    }

    private void ConnectionHeartbeatCallback(object state) {
      if (_connectionManager != null && !_connectionManager.Connection.IsClosed()) {
        Send(new Msg { Type = Msg.MsgTypes.None });
      }
      else {
        Log.Warn("TE2: Ping failed. Reconnecting success: " + CreateConnection());
      }
    }

    private void OnGeneralException(ConnectionManager c, Exception e) {
      Log.Error("TE2: unhandled exception - " + e);
      if (!c.Connection.IsClosed()) {
        CloseConnection();
      }
      else {
        CreateConnection();
      }
    }

    private void OnParseError(ConnectionManager c, Msg.DecodeResult decodeResult, string s) {
      Log.Error("TE2: Parse Error " + decodeResult + "- " + s);
    }

    private void ConnectionClosed(ConnectionManager c) {
      Log.Trace("TE2: Connection closed");
    }

    public void Send(Msg msg) {
      if (_connectionManager != null && !_connectionManager.Connection.IsClosed()) {
        msg.Id = MessageId;
        MessageId++;
        _connectionManager.Send(msg);
      }
      else {
        Log.Error("TE2: Failed to send message, not connected.");
      }
    }

    public void Receive(Msg message) {
      if (message.Status == false) {
        ParseServerError(message);
        return;
      }
      ParseServerResponse(message);
    }
    #endregion

    #region Server Response
    private void ParseServerError(Msg message) {
      Log.Error(message.Type + " failed. Error: " + message.DebugText);
    }

    private void ParseServerResponse(Msg message) {
      switch (message.Type) {
        case Msg.MsgTypes.Update:
          _tangibles = message.Tangibles.ToList();
          OnTangiblesUpdated(_tangibles);
          break;
        case Msg.MsgTypes.Patterns:
          _patterns = message.Patterns.ToList();
          OnPatternsUpdated(_patterns);
          break;
      }
    }
    #endregion
  }
}