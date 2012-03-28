using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AzureSharedLibrary
{
    // Status of Request to Server
    public enum ResponseStatus
    {
        Success=0, 
        UnknownException,
        ServerNotFound,     
        IllegalRequest,      //required params not found
        IllegalResponse      //json doesn't match object
    }
    public class ResponseDataItem
    {
        // PhoneApp, PhoneAgent, AzureREST, AzureWorkerRole, AzureWeb
        // (App & Agent sent from phone)
        public String ProcessName { get; set; }

        // this is the date we enter this in
        public DateTime DateEntered { get; set; }
    }
    public class RESTResponse
    {
        public ResponseStatus ResponseStatus { get; set; }

        public int Count { get; set; }

        public ResponseDataItem[] Items { get; set; }
    }

}
