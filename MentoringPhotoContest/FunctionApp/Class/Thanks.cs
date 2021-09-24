using System;
using Azure;
using Azure.Data.Tables;

namespace FunctionApp.Thanks
{
    public class Thanks : ITableEntity
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public string Tag { get; set; }

        public Thanks(string name, DateTime date)
        {
            Name = name;
            Date = date;
            PartitionKey = Guid.NewGuid().ToString();
            RowKey = "rowKey";
        }

        public Thanks()
        {
        }
        public static void UpdateEntity(TableClient table, string partitionKey, string tag)
        {
            string rowKey = "rowKey";
            Thanks entity = table.GetEntity<Thanks>(partitionKey, rowKey);
            entity.Tag = tag;
            table.UpdateEntity<Thanks>(entity, entity.ETag);
        }
    }
}
