using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace AzureSharedLibrary
{
    public class WebRoleMgr
    {
        private readonly CloudTableClient cloudTableClient;
        private AzureDataModelFactory modelFactory;
        private String ProcessName { get; set; }
        private DateTime DateEntered { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cloudStorageAccount">connection to table</param>
        /// <param name="processName"></param>
        /// <param name="dateEntered"></param>
        public WebRoleMgr(CloudStorageAccount cloudStorageAccount)
        {
            this.cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            modelFactory = new AzureDataModelFactory(this.cloudTableClient);
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cloudStorageAccount">connection to table</param>
        /// <param name="processName"></param>
        /// <param name="dateEntered"></param>
        public WebRoleMgr(CloudStorageAccount cloudStorageAccount, String processName, DateTime dateEntered)
        {
            this.cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            modelFactory = new AzureDataModelFactory(this.cloudTableClient);

            ProcessName = processName;
            DateEntered = dateEntered;
        }
        /// <summary>
        /// Add object to table
        /// </summary>
        public void AddObject()
        {
            // create data object
            // "AzureWorkerRole" means the worker role did the insert into the table
            // ProcessName is who is entering the data
            // DateTime is now because the app will send DateTime.MinValue on first call 
            AzureDataModel newDataObject = modelFactory.Create(ProcessName, DateTime.UtcNow);

            // add/save data to table
            modelFactory.Add(newDataObject);
        }

        /// <summary>
        /// Count of objects since last fetch 
        /// </summary>
        /// <param name="lastFetch"></param>
        /// <returns></returns>
        public int CountOfItemsSince(DateTime lastFetch)
        {
            return modelFactory.CountAllSince(lastFetch);
        }
        /// <summary>
        /// Total count of items in Azure Storage table
        /// </summary>
        /// <returns></returns>
        public int CountOfItems()
        {
            return modelFactory.CountAll();
        }
        /// <summary>
        /// Last Item Entered
        /// </summary>
        /// <returns></returns>
        public ResponseDataItem LastItemEntered()
        {
            AzureDataModel azureDataModel = modelFactory.FetchNewest();

            if (azureDataModel != null)
            {
                return new ResponseDataItem()
                {
                    DateEntered = azureDataModel.DateEntered,
                    ProcessName= azureDataModel.ProcessName
                };

            }
            return null;
        }
        // Last N Items Entered
        public List<ResponseDataItem> LastItemsEntered(int count)
        {
            IEnumerable<AzureDataModel> azureDataModels = modelFactory.FetchTopN(count);
            List<ResponseDataItem> dataItems = null;

            if (azureDataModels!=null)
            {
                dataItems = new List<ResponseDataItem>();

                foreach (AzureDataModel azureDataModel in azureDataModels)
                {
                    dataItems.Add(new ResponseDataItem()
                    {
                        ProcessName = azureDataModel.ProcessName,
                        DateEntered = azureDataModel.DateEntered
                    });

                }

            }
            return dataItems;

        }
        // Items between 2 dates
        public List<ResponseDataItem> ItemsBetweenD1AndD2(DateTime d1, DateTime d2)
        {
            IEnumerable<AzureDataModel> azureDataModels = modelFactory.FetchAllBetween(d1,d2);
            List<ResponseDataItem> dataItems = null;

            if (azureDataModels != null)
            {
                dataItems = new List<ResponseDataItem>();

                foreach (AzureDataModel azureDataModel in azureDataModels)
                {
                    dataItems.Add(new ResponseDataItem()
                    {
                        ProcessName = azureDataModel.ProcessName,
                        DateEntered = azureDataModel.DateEntered
                    });

                }

            }
            return dataItems;
        }
    }
}
