namespace Measurements
{
#if NETFX_CORE
  using WindowsPreview.Kinect;
#else
  using Microsoft.Kinect;
#endif
  using System;
  using Measurements.Extensions;
  using Newtonsoft.Json;
  using Newtonsoft.Json.Converters;

  public class JointPair : NamedValue
  {
    public JointPair(string name, JointType start, JointType end) : base(name)
    {
      this.Start = start;
      this.End = end;
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public JointType Start { get; internal set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public JointType End { get; internal set; }

    public bool IsPartitionKey { get; set; }
  }
}
