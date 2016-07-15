namespace BodyFrameReaders
{
#if NETFX_CORE
  using WindowsPreview.Kinect;
#else
  using Microsoft.Kinect;
#endif
  using System.Collections.Generic;
  public class TrackedBodyFrameEventArgs : MultipleFrameReferenceEventArgs
  {
    internal TrackedBodyFrameEventArgs()
    {
    }
    internal void ClearBodyFrameData()
    {
      this.AddedBodyIds = null;
      this.RemovedBodyIds = null;
      this.TrackedBodies = null;
    }
    internal void PopulateBodyFrameData(
      IReadOnlyDictionary<ulong, Body> currentlyTrackedBodies,
      IReadOnlyList<ulong> addedBodies,
      IReadOnlyList<ulong> removedBodies)
    {
      this.TrackedBodies = currentlyTrackedBodies;
      this.AddedBodyIds = addedBodies;
      this.RemovedBodyIds = removedBodies;
    }
    public IReadOnlyList<ulong> AddedBodyIds { get; internal set; }
    public IReadOnlyList<ulong> RemovedBodyIds { get; internal set; }
    public IReadOnlyDictionary<ulong, Body> TrackedBodies { get; internal set; }
  }
}