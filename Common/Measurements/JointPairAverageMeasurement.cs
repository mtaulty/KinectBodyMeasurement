namespace Measurements
{
#if NETFX_CORE
  using WindowsPreview.Kinect;
#else
  using Microsoft.Kinect;
#endif
  using System;
  using Measurements.Extensions;

  public class JointPairAverageMeasurement : JointPairMeasurement
  {
    public JointPairAverageMeasurement(string name,
      JointType start, JointType end) : base(name,start,end)
    {
    }
    public override void Reset()
    {
      base.Reset();
      this.SampleCount = 0;
    }
    public override void Measure(Body body)
    {
      base.Value += this.MeasureDistance(body);
      this.SampleCount++;
    }
    public override double Value
    {
      get
      {
        return (base.Value / this.SampleCount);
      }
    }
    public ulong SampleCount
    {
      get;
      protected set;
    }
  }
}