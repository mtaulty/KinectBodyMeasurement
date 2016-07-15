namespace TestWinRTProject.Configuration
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using System.IO;
  using Newtonsoft.Json.Linq;
  using Newtonsoft.Json;

#if NETFX_CORE
  using Windows.Storage;
#endif

  public class GlobalConfiguration
  {
    private GlobalConfiguration()
    {

    }
    public static async Task LoadConfigurationFromFileAsync(string fileLocation)
    {
      Stream stream = null;

#if NETFX_CORE
      StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(
        new Uri(fileLocation));

      using (stream = await file.OpenStreamForReadAsync())
#else
      using (stream = File.OpenRead(fileLocation))
#endif
      {
        using (StreamReader reader = new StreamReader(stream))
        {
          instance = JsonConvert.DeserializeObject<GlobalConfiguration>(
            reader.ReadToEnd());
        }
      }
    }
    public static GlobalConfiguration Instance
    {
      get
      {
        return (instance);
      }
    }
    static GlobalConfiguration instance;
    [JsonProperty]
    public string CloudAccount { get; private set; }
    [JsonProperty]
    public string CloudKey { get; private set; }
    [JsonProperty]
    public string CloudTable { get; private set; }
    [JsonProperty]
    public double MeasurementTolerance { get; private set; }
    [JsonProperty]
    public double LeastSquaresTolerance { get; private set; }
    [JsonProperty]
    public ulong MeasurementFrameCount { get; private set; }
    [JsonProperty]
    public byte IrScaleRange { get; private set; }
    [JsonProperty]
    public byte IrScaleLow { get; private set; }
    [JsonProperty]
    public string CloudBlobContainerName { get; private set; }
    [JsonProperty]
    public int CloudRowScanSize { get; private set; }
  }
}