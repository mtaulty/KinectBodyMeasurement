namespace BodyFrameReaders
{
  using System;

  public class TrackedBodyFrameReader : IDisposable
  {
    public event EventHandler<TrackedBodyFrameEventArgs> FrameArrived;

    internal TrackedBodyFrameReader(TrackedBodyFrameSource source)
    {
      this.source = source;
    }
    internal void FireFrameArrived(TrackedBodyFrameEventArgs args)
    {
      var handlers = this.FrameArrived;

      if (this.FrameArrived != null)
      {
        this.FrameArrived(this, args);
      }
    }
    ~TrackedBodyFrameReader()
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
      if (disposing && (this.source != null))
      {
        this.source.RemoveDisposedTrackedBodyFrameReader(this);
      }
    }
    TrackedBodyFrameSource source;
  }
}