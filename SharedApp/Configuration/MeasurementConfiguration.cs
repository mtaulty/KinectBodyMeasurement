namespace TestWinRTProject.Configuration
{
  using Measurements;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;

#if NETFX_CORE
  using Windows.Storage;
#endif

  static class MeasurementConfiguration
  {
    public static async Task LoadConfigurationFromFileAsync(string fileLocation)
    {
#if NETFX_CORE
      StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(
        new Uri(fileLocation));

      using (var stream = await file.OpenStreamForReadAsync())
#else
      using (var stream = File.OpenRead(fileLocation))
#endif
      {
        measurements = 
          JointPairSerialization.DeserializeFromJsonStream<JointPairAverageMeasurement>(stream);
      }
    }
    public static string PartitionKeyMeasurement
    {
      get
      {
        return (measurements.Single(m => m.IsPartitionKey).Name);
      }
    }
    public static IReadOnlyList<JointPairAverageMeasurement> Measurements
    {
      get
      {
        return (measurements);
      }
    }
    public static uint MaxMeasurementCount
    {
      get
      {
        return (MEASUREMENT_COUNT);
      }
    }
    static uint MEASUREMENT_COUNT = 50;
    static List<JointPairAverageMeasurement> measurements;
  }
}