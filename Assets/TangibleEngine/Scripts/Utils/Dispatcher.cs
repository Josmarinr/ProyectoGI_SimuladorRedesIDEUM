using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace TE {
  public class Dispatcher : MonoBehaviour {

    public event Action<Exception> ExceptionThrown;

    private int? _threadDispatchId;
    private Queue<Action> _dispatchQueue = new Queue<Action>();
    
    public void Dispatch(Action a) {
      if (a == null) return;
      var i = Thread.CurrentThread.ManagedThreadId;
      if (i == _threadDispatchId) {
        a();
      }
      else {
        lock (_dispatchQueue) {
          _dispatchQueue.Enqueue(a);
        }
      }
    }

    private void DoDispatching() {
      lock (_dispatchQueue) {
        var originalCount = _dispatchQueue.Count;
        while (originalCount>0 && _dispatchQueue.Count>0) {
          try {
            var a = _dispatchQueue.Dequeue();
            originalCount--;
            if (a != null) {
              a();
            }
          }
          catch (Exception e) {
            if (ExceptionThrown != null) ExceptionThrown.Invoke(e);
          }
        }
      }
    }

    void Awake() {
      _threadDispatchId = Thread.CurrentThread.ManagedThreadId;
    }

    void Update() {
      DoDispatching();
    }
  }
}