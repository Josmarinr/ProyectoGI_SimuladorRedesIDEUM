using Newtonsoft.Json;
using UnityEngine;

namespace TE {
  public class Point {

    //Position of the tangible
    [JsonIgnore]
    public Vector2 Pos;

    public Point() {

    }

    public Point(Point p) {
      Pos.x = p.X;
      Pos.y = p.Y;
    }

    public Point(Vector2 p) {
      Pos = p;
    }

    //X coordinate shortcut
    public float X {
      get { return Pos.x; }
      set { Pos.x = value; }
    }

    //Y coordinate shortcut
    public float Y {
      get { return Pos.y; }
      set { Pos.y = value; }
    }
    
  }
}