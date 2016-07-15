namespace Measurements.Extensions
{
  #if NETFX_CORE
  using WindowsPreview.Kinect;
  #else
  using Microsoft.Kinect;
  #endif
  using System.Linq;

  public static class BodyExtensions
  {
    public static bool HasTrackedJoints(this Body body, params JointType[] jointTypes)
    {
      return (jointTypes.All(
        jt => body.Joints[jt].TrackingState == TrackingState.Tracked));
    }
  }
}
