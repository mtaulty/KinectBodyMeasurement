namespace Measurements
{
  using Newtonsoft.Json;
  using System.Collections.Generic;
  using System.IO;

  public static class JointPairSerialization
  {
    public static List<T> DeserializeFromJsonStream<T>(Stream stream)
      where T : JointPair
    {
      List<T> results = null;

      using (var reader = new StreamReader(stream))
      {
        string content = reader.ReadToEnd();

        results = JsonConvert.DeserializeObject<List<T>>(content);
      }
      return (results);
    }
  }
}