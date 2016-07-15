namespace Measurements
{
#if NETFX_CORE
  using WindowsPreview.Kinect;
#else
  using Microsoft.Kinect;
#endif
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public class JointPairAverageMeasurementSet
  {
    public JointPairAverageMeasurementSet(
      IReadOnlyList<JointPairAverageMeasurement> measurements,
      ulong? maximumSampleCount = null)
    {
      this.measurements = 
        measurements
          .Select(
            m => new JointPairAverageMeasurement(m.Name, m.Start, m.End))
          .ToList();

      this.maximumSampleCount = maximumSampleCount;
    }
    public void Measure(Body body)
    {
      foreach (var measurement in this.measurements)
      {
        if (ShouldMeasure(measurement, body))
        {
          measurement.Measure(body);
        }
      }
    }
    public void Reset()
    {
      foreach (var measurement in this.measurements)
      {
        measurement.Reset();
      }
    }
    public bool HasReachedMaximumSampleCount
    {
      get
      {
        return (
          this.HasMaximumSampleCount &&
          (this.measurements.All(m => m.SampleCount >= this.maximumSampleCount)));
      }
    }
    public IReadOnlyList<JointPairAverageMeasurement> Measurements
    {
      get
      {
        return (this.measurements);
      }
    }
    public int CompletedMeasurementCount
    {
      get
      {
        return (this.measurements.Where(m => m.SampleCount ==
          this.maximumSampleCount).Count());
      }
    }

    bool HasMaximumSampleCount
    {
      get
      {
        return (this.maximumSampleCount != null);
      }
    }
    bool ShouldMeasure(JointPairAverageMeasurement measurement,
      Body body)
    {
      bool possible = measurement.CanMeasure(body);
      
      bool limited = 
        (this.HasMaximumSampleCount) &&
        (measurement.SampleCount >= this.maximumSampleCount);

      return (possible && !limited);
    }
    JointPairAverageMeasurement GetMeasurementByName(string name)
    {
      return (this.measurements.SingleOrDefault(m => m.Name == name));
    }
    ulong? maximumSampleCount;
    IReadOnlyList<JointPairAverageMeasurement> measurements;
  }
}