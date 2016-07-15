namespace Storage
{
  using Measurements;
  using Microsoft.WindowsAzure.Storage;
  using Microsoft.WindowsAzure.Storage.Table;
  using System;
  using System.Collections.Generic;
  using System.Text;
  using System.Linq;

  public class CloudMeasurementStoreEntry : TableEntity
  {
    // this would be private but azure querying requires a default constructor.
    public CloudMeasurementStoreEntry()
    {
    }
    public Guid Id { get; private set; }
    public IReadOnlyList<NamedValue> Values { get; private set; }

    public override void ReadEntity(IDictionary<string, EntityProperty> properties, 
      OperationContext operationContext)
    {
      base.ReadEntity(properties, operationContext);

      var valueList = new List<NamedValue>();

      foreach (var item in properties)
      {
        if (!standardProperties.Contains(item.Key))
        {
          valueList.Add(
            new NamedValue(item.Key, item.Value.DoubleValue.Value));
        }
      }
      this.Values = valueList;
    }
    public override IDictionary<string, EntityProperty> WriteEntity(
      OperationContext operationContext)
    {
      var dictionary = base.WriteEntity(operationContext);

      foreach (var value in this.Values)
      {
        dictionary[value.Name] =
          EntityProperty.GeneratePropertyForDouble(value.Value);
      }
      return (dictionary);
    }
    public static CloudMeasurementStoreEntry MakeNew(IReadOnlyList<NamedValue> values,
      NamedValue partitionKeyValue)
    {
      CloudMeasurementStoreEntry entry = new CloudMeasurementStoreEntry();
      
      entry.Id = Guid.NewGuid();
      entry.Values = values;

      entry.PartitionKey = MakePartitionKey(partitionKeyValue);
      entry.RowKey = entry.Id.ToString();

      return (entry);
    }
    static string MakePartitionKey(NamedValue rowKeyValue)
    {
      return (
        Math.Floor(rowKeyValue.Value * 10.0d).ToString());
    }
    public double SumOfSquaresComparisonTo(CloudMeasurementStoreEntry rhs)
    {
      // this will blow up if a measurement is missing...
      return (
        this.Values
        .Select(m =>
          {
            double rhsValue = rhs.Values.Single(r => r.Name == m.Name).Value;
            return (Math.Pow(m.Value, rhsValue));
          }
        )
        .Sum()
      );
    }
    static string[] standardProperties = { "PartitionKey", "RowKey" };
  }
}