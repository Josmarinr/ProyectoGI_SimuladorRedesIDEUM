namespace TE {
  public interface IOnTangibleRemoved {
    /// <summary>
    /// Called when a tangible has been removed / picked up from table
    /// </summary>
    /// <param name="t">The soon to be deleted tangible instance</param>
    void OnTangibleRemoved(Tangible t);
  }
}