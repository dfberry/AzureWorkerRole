namespace AzureSharedLibrary

{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.WindowsAzure.StorageClient;
    using Microsoft.WindowsAzure;
    using System.Data.Services.Client;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using System.Diagnostics;

    public class AzureDataModelFactory
    {
        // Azure Storage Table Name (exact name)
        private readonly string AzureTableName = "SampleData";

        // Azure Storage Table Access object
        private readonly CloudTableClient cloudTableClient;

        /// <summary>
        /// C'tor
        /// </summary>
        /// <param name="eventCloudStorageAccount">Event Storage Account</param>
        public AzureDataModelFactory(CloudTableClient cloudTableClient)
        {
            // get connection string from config
            // READ: @smarx blog post titled
            // how-to-resolve-setconfigurationsettingpublisher-needs-to-be-called-before-fromconfigurationsetting-can-be-used-after-moving-to-windows-azure-sdk-1-3
            //this.

            this.cloudTableClient = cloudTableClient;

            // Table Holding the data
            this.cloudTableClient.CreateTableIfNotExist(AzureTableName);
        }

        /// <summary>
        /// Create a New Object - doesn't put it in storage
        /// </summary>
        /// <returns></returns>
        public AzureDataModel Create(string processName, DateTime dateEntered)
        {
            // constructor uses dateEntered to generate
            // rowkey and partition key
            AzureDataModel entity = new AzureDataModel(dateEntered)
            {
                DateEntered = dateEntered,
                ProcessName = processName
            };

            return (entity);
        }

        /// <summary>
        /// Add Object to storage
        /// </summary>
        public void Add(AzureDataModel entity)
        {
            try
            {

                // Create storage context
                DataServiceContext dataServiceContext = this.cloudTableClient.GetDataServiceContext();

                // Add object to context
                dataServiceContext.AddObject(AzureTableName, entity);

                // Save context which saves object to storage
                DataServiceResponse response = dataServiceContext.SaveChanges();
            }
            catch(Exception ex)
            {
                Debug.WriteLine("*****AzureDataModelFactory:Add ex.Message=" + ex.Message);
            }
        }

        /// <summary>
        /// Fetch Most Recent Singleton
        /// </summary>
        /// <returns></returns>
        public AzureDataModel FetchNewest()
        {
            return FetchAll().FirstOrDefault();
        }

        /// <summary>
        /// Fetch Top N objects
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AzureDataModel> FetchTopN(int count)
        {
            if (count > 0)
            {
                return FetchAll().Take(count);
            }
            else if (count == -1) // -1 implies all wanted
            {
                return FetchAll();
            }
            else // count == 0 implies none wanted 
            {
                return null;
            }

        }

        /// <summary>
        /// Fetch All Entities (date desc because that is how they were entered)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AzureDataModel> FetchAll()
        {
            // Create storage context
            DataServiceContext dataServiceContext = this.cloudTableClient.GetDataServiceContext();

            // Get all items
            return dataServiceContext.CreateQuery<AzureDataModel>(AzureTableName).Execute();

        }
        /// <summary>
        /// Count all items in storage 
        /// </summary>
        /// <returns></returns>
        public int CountAll()
        {
            return FetchAll().Count();
        }
        /// <summary>
        /// Fetch all items between dateentered and last fetch 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AzureDataModel> FetchAllSince(DateTime lastFetchDate)
        {
            DataServiceContext dataServiceContext = this.cloudTableClient.GetDataServiceContext();

            var query = from entity in dataServiceContext.CreateQuery<AzureDataModel>(AzureTableName).Execute()
                        where entity.DateEntered > lastFetchDate
                        select entity;

            return query;
        }
        /// <summary>
        /// Fetch all items between dateentered and last fetch 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AzureDataModel> FetchAllBetween(DateTime DateMin, DateTime DateMax)
        {
            DataServiceContext dataServiceContext = this.cloudTableClient.GetDataServiceContext();

            var query = from entity in dataServiceContext.CreateQuery<AzureDataModel>(AzureTableName).Execute()
                        where (DateMax > entity.DateEntered) && (entity.DateEntered > DateMin) 
                        select entity;

            return query;
        }
        /// <summary>
        /// Fetch all items between dateentered and last fetch 
        /// </summary>
        /// <returns></returns>
        public int CountAllBetween(DateTime DateMin, DateTime DateMax)
        {
            return FetchAllBetween(DateMin, DateMax).Count();
        }      
        /// <summary>
        /// Count all between dateentered and last fetch 
        /// </summary>
        /// <returns></returns>
        public int CountAllSince(DateTime lastFetchDate)
        {
            return FetchAllSince(lastFetchDate).Count();
        }
       
        /// <summary>
        /// Fetch all by process name
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AzureDataModel> FetchByProcess(String processName)
        {
            DataServiceContext dataServiceContext = this.cloudTableClient.GetDataServiceContext();

            var query = from entity in dataServiceContext.CreateQuery<AzureDataModel>(AzureTableName).Execute()
                        where entity.ProcessName == processName
                        select entity;

            return query;
        }

    }
}
