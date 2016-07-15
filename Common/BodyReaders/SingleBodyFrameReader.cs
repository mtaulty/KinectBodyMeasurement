namespace BodyFrameReaders
{
  using System;

  public class SingleBodyFrameReader : IDisposable
  {
    public event EventHandler<SingleBodyFrameEventArgs> FrameArrived;
    public event EventHandler TrackingIdLost;

    internal SingleBodyFrameReader(TrackedBodyFrameSource source)
    {
      this.source = source;
    }
    public ulong TrackingId 
    { 
      get; 
      internal set; 
    }
    ~SingleBodyFrameReader()
    {
      this.Dispose(false);
    }
    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize(this);
    }
    void Dispose(bool disposing)
    {
      if (disposing)
      {
        this.source.RemoveDisposedSingleBodyFrameReader(this);
      }
    }
    internal void FireFrameArrived(SingleBodyFrameEventArgs args)
    {
      var handlers = this.FrameArrived;

      if (handlers != null)
      {
        handlers(this, args);
      }
    }
    internal void FireTrackingIdLost()
    {
      var handlers = this.TrackingIdLost;

      if (handlers != null)
      {
        handlers(this, EventArgs.Empty);
      }
    }
    TrackedBodyFrameSource source;
  }
}