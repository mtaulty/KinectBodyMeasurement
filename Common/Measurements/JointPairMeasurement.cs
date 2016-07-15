namespace Measurements
{
#if NETFX_CORE
  using WindowsPreview.Kinect;
#else
  using Microsoft.Kinect;
#endif
  using System;
  using Measurements.Extensions;

  public class JointPairMeasurement : JointPair
  {
    public JointPairMeasurement(string name,
      JointType start, JointType end) : base(name,start,end)
    {

    }
    public virtual bool CanMeasure(Body body)
    {
      return (body.HasTrackedJoints(this.Start, this.End));
    }
    public virtual void Reset()
    {
      base.Value = 0.0d;
    }
    public virtual void Measure(Body body)
    {
      base.Value = this.MeasureDistance(body);
    }
    protected virtual double MeasureDistance(Body body)
    {
      if (!this.CanMeasure(body))
      {
        throw new InvalidOperationException();
      }
      return (
        body.Joints[this.Start].Position.DistanceTo(
        body.Joints[this.End].Position));
    }
  }
}
