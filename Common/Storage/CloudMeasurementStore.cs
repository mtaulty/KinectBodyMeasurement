namespace Storage
{
  using Measurements;
  using Microsoft.WindowsAzure.Storage;
  using Microsoft.WindowsAzure.Storage.Auth;
  using Microsoft.WindowsAzure.Storage.Blob;
  using Microsoft.WindowsAzure.Storage.Table;
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.IO;
  using System.Text;
  using System.Threading.Tasks;

  public class CloudMeasurementStore
  {
    public CloudMeasurementStore(
      string accountName, 
      string keyValue,
      string skeletalTableName,
      string blobContainerName,
      int cloudRowScanSize)
    {
      this.credentials = new StorageCredentials(accountName, keyValue);
      this.skeletalTableName = skeletalTableName;
      this.blobContainerName = blobContainerName;
      this.cloudRowScanSize = cloudRowScanSize;
    }
    public async Task StoreMeasurementAsync(CloudMeasurementStoreEntry entry)
    {
      await this.GetCloudStorageAsync();
      TableOperation operation = TableOperation.Insert(entry);
      await this.dataTable.ExecuteAsync(operation);
    }
    public async Task StorePhotoForMeasurementsAsync(CloudMeasurementStoreEntry entry,
      byte[] photoStream)
    {
      await this.GetCloudStorageAsync();

      var blockReference = this.blobContainer.GetBlockBlobReference(
        string.Format("{0}.jpg", entry.Id.ToString()));

      await blockReference.UploadFromByteArrayAsync(photoStream, 0, photoStream.Length);
    }
    public async Task<IEnumerable<CloudMeasurementStoreEntry>> MatchMeasurementsAsync(
      CloudMeasurementStoreEntry entry,
      double matchTolerance)
    {
      await this.GetCloudStorageAsync();

      StringBuilder partitionFilter = new StringBuilder(
        TableQuery.GenerateFilterCondition(
          "PartitionKey", QueryComparisons.Equal, entry.PartitionKey));

      string valueFilter = string.Empty;

      foreach (var value in entry.Values)
      {
        double lowValue = value.Value - (matchTolerance / 2.0d);
        double highValue = value.Value + (matchTolerance / 2.0d);

        partitionFilter.AppendFormat(" {0} ({1}) {2} ({3})",
          TableOperators.And,
          TableQuery.GenerateFilterConditionForDouble(
            value.Name, QueryComparisons.GreaterThanOrEqual, lowValue),
          TableOperators.And,
          TableQuery.GenerateFilterConditionForDouble(
            value.Name, QueryComparisons.LessThanOrEqual, highValue));
      }
      var query = new TableQuery<CloudMeasurementStoreEntry>();

      query.FilterString = partitionFilter.ToString();

      query = query.Take(this.cloudRowScanSize);

      Debug.WriteLine(
        string.Format("Executing query [{0}]", query.FilterString));

      var results = await this.dataTable.ExecuteQuerySegmentedAsync(query, null);

      Debug.WriteLine(
        string.Format("Retrieved {0} results records from the table", results.Results.Count));

      return (results.Results);
    }
    private async Task GetCloudStorageAsync()
    {
      if (this.dataTable == null)
      {
        this.storageAccount = new CloudStorageAccount(this.credentials, true);

        CloudTableClient client = this.storageAccount.CreateCloudTableClient();
        this.dataTable = client.GetTableReference(this.skeletalTableName);
        await this.dataTable.CreateIfNotExistsAsync();

        CloudBlobClient blobClient = this.storageAccount.CreateCloudBlobClient();
        this.blobContainer = blobClient.GetContainerReference(this.blobContainerName);
        await this.blobContainer.CreateIfNotExistsAsync();
      }
    }
    string blobContainerName;
    string skeletalTableName;
    StorageCredentials credentials;
    CloudStorageAccount storageAccount;
    CloudTable dataTable;
    CloudBlobContainer blobContainer;
    int cloudRowScanSize;
  }
}