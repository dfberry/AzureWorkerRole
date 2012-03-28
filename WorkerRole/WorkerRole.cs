using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using AzureSharedLibrary;

namespace WorkerRole1
{
    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.WriteLine("WorkerRole1 entry point called", "Information");

            while (true)
            {
                // DFB: Azure Sample
                Work();

                Thread.Sleep(5000);
                Trace.WriteLine("Working", "Information");
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }
        // DFB: Azure Sample
        private void Work()
        {
            // DFB: Call into Shared Library
            // Add row to Azure Storage table
            WorkerRoleMgr.DoWork();

            // DFB: ShowWork gets Get first or default row from Azure
            Trace.WriteLine("ShowWork " + WorkerRoleMgr.ShowWork().ProcessName + " -" + WorkerRoleMgr.ShowWork().DateEntered, "Information");


        }
    }
}
