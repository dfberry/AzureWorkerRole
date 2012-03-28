
namespace AzureSharedLibrary
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.WindowsAzure.StorageClient;

    // My two properties are
    // processname and dateentered
    // The others are required by Azure Storage
    public class AzureDataModel : TableServiceEntity
    {
        // PhoneApp, PhoneAgent, AzureREST, AzureWorkerRole, AzureWeb
        // (App & Agent sent from phone)
        public String ProcessName {get;set;}

        // this is the UTC date 
        public DateTime DateEntered { get; set; }

        // Constructor Required and used by Table classes
        public AzureDataModel()
        {
        }

        // Constructor passing in rowkey and partitionkey
        public AzureDataModel(DateTime dateEntered)
            : base(CreatePartitionKey(dateEntered), CreateRowKey(dateEntered))
        {
            this.DateEntered = dateEntered;
        }

        // Azure row key using reverse time so can fetch top properly
        public static String CreateRowKey(DateTime dateEntered)
        {
            return (DateTime.MaxValue - dateEntered).TotalSeconds.ToString();
        }

        // Azure partition key using reverse year so can fetch top properly
        // Another good partition key would have been the ProcessName value
        public static String CreatePartitionKey(DateTime dateEntered)
        {
            return (DateTime.MaxValue.Year - dateEntered.Year).ToString();
        }
    }
}
