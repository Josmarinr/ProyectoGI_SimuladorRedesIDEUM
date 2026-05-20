using System;
using System.Diagnostics;
using System.Text;
using Ideum.Logging;

namespace TE.Comm {
  /// <summary>
  /// Handles translation between Msg instances from connection recieving raw bytes.
  /// </summary>
  public class ConnectionManager : ISender {

    public IComm Connection { get; private set; }
    public string Destination { get; set; }

    private IReceiver _onMsgRecieved;
    private Action<ConnectionManager, byte[]> _onMsgBytesRecieved;
    private Action<ConnectionManager, Msg.DecodeResult, string> _onParseError;
    private object _parserLock = new object();
    private ByteMessageParser _parser;
    private Action<ConnectionManager> _connectionClosed;
    private Action<ConnectionManager, Exception> _generalExceptionAction;

    /// <summary>
    /// Creates a Connection Translator.
    /// </summary>
    /// <param name="connection">The Connection instance to use</param>
    /// <param name="onMsgRecieved">Called when a Msg instance is created from incoming byte arrays</param>
    /// <param name="generalExceptionAction">Called when an exception is thrown</param>
    /// <param name="onParseError">Called when a Msg cannot be parsed</param>
    /// <param name="connectionClosed">Called when the Connection closes</param>
    /// <param name="byteMsgRecieved">Called when bytes are received</param>
    public ConnectionManager(IComm connection, IReceiver onMsgRecieved, Action<ConnectionManager, Exception> generalExceptionAction, Action<ConnectionManager, Msg.DecodeResult, string> onParseError = null, Action<ConnectionManager> connectionClosed = null, Action<ConnectionManager, byte[]> byteMsgRecieved = null) {
      if (connection == null) {
        throw new ArgumentNullException();
      }
      Connection = connection;
      Destination = connection.Socket.RemoteEndPoint.ToString();
      _parser = new ByteMessageParser(PayloadCallback, ExceptionCallback);
      _onMsgRecieved = onMsgRecieved;
      _generalExceptionAction = generalExceptionAction;
      _onParseError = onParseError;
      _onMsgBytesRecieved = byteMsgRecieved;
      _connectionClosed = connectionClosed;
      Connection.Bind(ConnectionRecieve, OnConnectionClosed);
    }

    private void ExceptionCallback(Exception obj) {
      if (_generalExceptionAction != null) {
        _generalExceptionAction(this, obj);
      }
    }

    private void OnConnectionClosed() {
      if (_connectionClosed == null) return;
      _connectionClosed(this);
    }

    /// <summary>
    /// Translate and queue a Msg instance to send
    /// </summary>
    /// <param name="msg"></param>
    public void Send(Msg msg) {
      var s = msg.Serialize();
      var bytes = Encoding.UTF8.GetBytes(s);
      Connection.Send(ByteMessageParser.CreateMessage(bytes));
    }

    public void Close() {
      if(Connection != null) {
        Connection.Close();
      }
    }

    private void ConnectionRecieve(byte[] raw) {
      //NOTE: prevent parser state from getting corrupted by multiple threads accessing the data.
      lock (_parserLock) {
        _parser.Consume(raw);
      }
    }


    private void PayloadCallback(byte[] bytes) {
      if (bytes == null) {
        Log.Trace("TE2: No bytes are written");
        _onMsgRecieved.Receive(null);
        return;
      }
      if (Connection == null || Connection.Socket == null) {
        Log.Failure("TE2: Connection or Socket is null for payload callback");
        return;
      }

      var rawMsg = Encoding.UTF8.GetString(bytes);
      //UnityEngine.Debug.Log("RECIEVED RAW<" + Connection.Socket.LocalEndPoint + ">:" + rawMsg);
      if (_onMsgBytesRecieved != null) {
        _onMsgBytesRecieved(this, bytes);
      }

      var msg = new Msg();
      var result = msg.TryDeserialize(bytes, out msg);
      if (result == Msg.DecodeResult.Decoded) {
        _onMsgRecieved.Receive(msg);
      } else {
        Trace.WriteLine("PARSE FAIL<" + Connection.Socket.LocalEndPoint + ">:" + rawMsg);
        _onParseError(this, result, rawMsg);
      }
    }
  }
}