using Azure;
using Azure.Data.Tables;
using System;

namespace MentoringPhotoContest.Thanks
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
            Tag = "empty string";
        }

        public Thanks()
        {
        }
        public static string CreateMessage(TableClient table, Thanks message)
        {
            table.AddEntity(message);
            return message.PartitionKey;
        }
    }
}
