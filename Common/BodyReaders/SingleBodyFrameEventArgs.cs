namespace BodyFrameReaders
{
#if NETFX_CORE
  using WindowsPreview.Kinect;
#else
  using Microsoft.Kinect;
#endif

  public class SingleBodyFrameEventArgs : MultipleFrameReferenceEventArgs
  {
    internal SingleBodyFrameEventArgs ()
	  {
	  }
    public Body Body { get; internal set; }
    public int BodyIndex { get; internal set; }
  }
}