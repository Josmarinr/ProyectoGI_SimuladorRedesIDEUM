namespace TE.Comm {
  public interface ISender {
    void Send(Msg m);
    void Close();
    string Destination {
      get;
    }
  }
}