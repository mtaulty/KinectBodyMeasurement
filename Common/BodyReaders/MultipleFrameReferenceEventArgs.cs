namespace BodyFrameReaders
{
#if NETFX_CORE
  using WindowsPreview.Kinect;
#else
  using Microsoft.Kinect;
#endif
  using System;

  public class MultipleFrameReferenceEventArgs : EventArgs
  {
    internal MultipleFrameReferenceEventArgs()
    {
    }
    internal void PopulateAdditionalFrameData(MultiSourceFrame frame)
    {
      this.BodyFrameReference = frame.BodyFrameReference;
      this.BodyIndexFrameReference = frame.BodyIndexFrameReference;
      this.ColorFrameReference = frame.ColorFrameReference;
      this.DepthFrameReference = frame.DepthFrameReference;
      this.InfraredFrameReference = frame.InfraredFrameReference;
      this.LongExposureInfraredFrameReference = frame.LongExposureInfraredFrameReference;
    }
    public TrackedBodyFrameSource Source { get; internal set; }
    public BodyFrameReference BodyFrameReference { get; internal set; }
    public BodyIndexFrameReference BodyIndexFrameReference { get; internal set; }
    public ColorFrameReference ColorFrameReference { get; internal set; }
    public DepthFrameReference DepthFrameReference { get; internal set; }
    public InfraredFrameReference InfraredFrameReference { get; internal set; }
    public LongExposureInfraredFrameReference LongExposureInfraredFrameReference { get; internal set; }
  }
}
