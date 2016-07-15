namespace Measurements.Extensions
{
#if NETFX_CORE
  using WindowsPreview.Kinect;
#else
  using Microsoft.Kinect;
#endif
  using System;

  public static class CameraSpacePointExtensions
  {
    public static double DistanceTo(this CameraSpacePoint pt1, CameraSpacePoint pt2)
    {
      return (
        Math.Sqrt(
          Math.Pow(pt1.X - pt2.X, 2) +
          Math.Pow(pt1.Y - pt2.Y, 2) +
          Math.Pow(pt1.Z - pt2.Z, 2)));
    }
    public static CameraSpacePoint MidPointWith(this CameraSpacePoint pt1,
      CameraSpacePoint pt2)
    {
      return (
        new CameraSpacePoint()
        {
          X = (pt1.X + pt2.X) / 2.0f,
          Y = (pt1.Y + pt2.Y) / 2.0f,
          Z = (pt1.Z + pt2.Z) / 2.0f
        }
      );
    }
  }
}
