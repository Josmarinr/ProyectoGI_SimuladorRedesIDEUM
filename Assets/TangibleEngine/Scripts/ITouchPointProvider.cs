using System;
using System.Collections.Generic;

namespace TE {
  public interface ITouchPointProvider {
    event Action<ICollection<Pointer>> TouchPointsUpdated;
  }
}