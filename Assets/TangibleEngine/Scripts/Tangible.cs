using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace TE {

  /// <summary>
  /// The tangible class represents an instance of a tangible which has been added, updated, or removed.
  /// 
  /// </summary>
  public class Tangible : Point {

    private static string _defaultPatternNameValue = null;
    private static float? _defaultPatternRadiusValue = null;
    private static int? _defaultPatternKeepAlivePointsValue = null;
    private static Point[] _defaultPointsValue = null;
    private static Point _defaultOffsetValue = new Point();

    /// <summary>
    /// The instance-id of this tangible
    /// </summary>
    public int Id;

    /// <summary>
    /// The pattern id of this tangible instance
    /// </summary>
    public int PatternId;

    /// <summary>
    /// The relative rotation of this tangible, in radians.
    /// </summary>
    public float R;

    /// <summary>
    /// the pointer id's that are used by this tangible.
    /// </summary>
    public int[] PointerIds;

    [JsonIgnore]
    public Pattern Pattern;

    /// <summary>
    /// The name of the Pattern associated with this tangible. Returns null if no pattern is assigned.
    /// </summary>
    public string PatternName {
      get { return Pattern == null ? _defaultPatternNameValue : Pattern.Name; }
    }

    /// <summary>
    /// The number of points used to keep instances of this pattern alive. Returns null if no pattern is assigned.
    /// </summary>
    public int? KeepAlivePoints {
      get { return Pattern == null ? _defaultPatternKeepAlivePointsValue : Pattern.KeepAlivePoints; }
    }

    /// <summary>
    /// The offset of the tangible's center point from the centroid of the touch points that were used to define the tangible. Returns (0,0) if no pattern is assigned.
    /// </summary>
    public Point Offset {
      get { return Pattern==null ? _defaultOffsetValue : Pattern.Offset; }
    }

    /// <summary>
    /// Array of Point instances containing the relative offsets of each potential touch point used to define the tangible. Returns null if no pattern is assigned.
    /// </summary>
    public Point[] PatternPoints {
      get { return Pattern == null ? _defaultPointsValue : Pattern.Points; }
    }

    /// <summary>
    /// The radius of this tangible as defined by its pattern. Returns null if no pattern is assigned.
    /// </summary>
    public float? Radius {
      get { return Pattern == null ? _defaultPatternRadiusValue : Pattern.Radius; }
    }

    /// <summary>
    /// An array containing active touch points associated with this tangible. This structure will be empty while in simulator mode.
    /// </summary>
    public TouchScript.Pointers.Pointer[] AssociatedTouchPoints {
      get { return TouchScript.TouchManager.Instance.Pointers.Where(p => PointerIds.Contains(p.Id)).ToArray(); }
    }

    public Tangible() {

    }

    public Tangible(Tangible t) : base(t) {
      Id = t.Id;
      PatternId = t.PatternId;
      R = t.R;
      PointerIds = new int[t.PointerIds.Length];
      Pattern = t.Pattern;
      System.Array.Copy(t.PointerIds, PointerIds, t.PointerIds.Length);
    }
    
  }
}