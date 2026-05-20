namespace TE {
  public class Pattern {

    /// <summary>
    /// The unique ID of this pattern 
    /// </summary>
    public int PatternId;

    /// <summary>
    /// The human-readable name of this pattern;
    /// </summary>
    public string Name;

    /// <summary>
    /// The minimum number of points required to keep this tangible alive.
    /// </summary>
    public int KeepAlivePoints;

    public Point Offset = new Point();

    public Point[] Points;

    public float Radius;
  }
}