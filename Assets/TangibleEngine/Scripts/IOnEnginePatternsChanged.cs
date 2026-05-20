using System.Collections.Generic;

namespace TE {
  public interface IOnEnginePatternsChanged {
    /// <summary>
    /// Called when the set of active patterns has been changed 
    /// </summary>
    /// <param name="patterns">The collection of new patterns currently active</param>
    void OnEnginePatternsChanged(List<Pattern> patterns);
  }
}