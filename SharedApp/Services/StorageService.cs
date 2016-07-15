namespace TestWinRTProject.Services
{
  using Measurements;
  using Storage;
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using TestWinRTProject.Configuration;

  static class StorageService
  {
    static public async Task<CloudMeasurementStoreEntry> MatchMeasurementAsync(
      JointPairAverageMeasurementSet measurements)
    {
      Initialise();

      CloudMeasurementStoreEntry matchedEntry = null;

      CloudMeasurementStoreEntry candidateEntry = CloudMeasurementStoreEntry.MakeNew(
        measurements.Measurements,
        measurements.Measurements.Single(
          m => m.Name == MeasurementConfiguration.PartitionKeyMeasurement));

      var entries = await store.MatchMeasurementsAsync(
        candidateEntry,
        GlobalConfiguration.Instance.MeasurementTolerance);

      // We have some set of entries that are within our tolerance?
      if ((entries != null) && (entries.Count() > 0))
      {
        double min = double.MaxValue;
        foreach (var entry in entries)
        {
          double comparison = entry.SumOfSquaresComparisonTo(candidateEntry);

          if ((comparison < min) &&
            (comparison <= GlobalConfiguration.Instance.LeastSquaresTolerance))
          {
            min = comparison;
            matchedEntry = entry;
          }
        }
        Debug.WriteLine(string.Format(
          "Minimum difference of squares for comparison was [{0}]", min));
      }
      return (matchedEntry);
    }
    static public async Task StoreMeasurementSetAsync(
      JointPairAverageMeasurementSet measurements,
      byte[] photoStream)
    {
      Initialise();

      CloudMeasurementStoreEntry entry = CloudMeasurementStoreEntry.MakeNew(
        measurements.Measurements,
        measurements.Measurements.Single(
          m => m.Name == MeasurementConfiguration.PartitionKeyMeasurement));

      await store.StoreMeasurementAsync(entry);
      await store.StorePhotoForMeasurementsAsync(entry, photoStream);
    }
    static void Initialise()
    {
      if (store == null)
      {
        store = new CloudMeasurementStore (
          GlobalConfiguration.Instance.CloudAccount,
          GlobalConfiguration.Instance.CloudKey,
          GlobalConfiguration.Instance.CloudTable,
          GlobalConfiguration.Instance.CloudBlobContainerName,
          GlobalConfiguration.Instance.CloudRowScanSize);
      }
    }
    static CloudMeasurementStore store;
  }
}