using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace AzureSharedLibrary
{
    // Called by Azure Worker Role
    public static class WorkerRoleMgr
    {
        // WorkerRoleMgr.cs 
        // Main function to do work 
        // this should call any subsequent
        // functions
        public static void DoWork()
        {
            // DFB: Get connection to Azure Storage table
            CloudTableClient cloudTableClient = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString")).CreateCloudTableClient();

            // DFB: create data factory to table
            AzureDataModelFactory dataModelFactory = new AzureDataModelFactory(cloudTableClient);

            // DFB: create data object
            // "AzureWorkerRole" means the worker role did the insert into the table
            AzureDataModel newWorkerRoleData = dataModelFactory.Create("AzureWorkerRole", DateTime.UtcNow);

            // DFB: add data to table
            dataModelFactory.Add(newWorkerRoleData);
        }
        // WorkerRoleMgr.cs verify data descending inserting
        public static AzureDataModel ShowWork()
        {
            // DFB:Get connection to Azure Storage table
            CloudTableClient cloudTableClient = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString")).CreateCloudTableClient();

            // DFB:create data factory
            AzureDataModelFactory dataModelFactory = new AzureDataModelFactory(cloudTableClient);

            // DFB:get first or default
            return dataModelFactory.FetchNewest();
        }
    }
}

