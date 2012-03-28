using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AzureSharedLibrary;
using MvcWebRole1.Attributes;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Diagnostics;

namespace MvcWebRole1.Controllers
{
    public class PhoneController : Controller
    {
        //
        // GET: /Phone/

        private static CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(RoleEnvironment.GetConfigurationSettingValue("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"));

        // HTML Web page display
        // Refreshes every 5 seconds
        public ViewResult Index()
        {
            Response.AddHeader("Refresh", "5");

            String processName = "AzureWeb";
            WebRoleMgr webRoleMgr = new WebRoleMgr(cloudStorageAccount, processName, DateTime.UtcNow);

            // Add row
            webRoleMgr.AddObject();

            // Call SharedLibrary which calls AzureStorage
            List<ResponseDataItem> list = webRoleMgr.LastItemsEntered(100).ToList();
            
            return View(list);
        }

        // this REST API call looks like
        // http://webserver/Phone/BackgroundAgent/?LastFetchDate=11/29/2011&ProcessName=PhoneAgent
        // call needs to insert an object with Request params then
        // the JSON result should be an object that returns:
        // 1) total count of items since last fetch 
        // 2) most recent item details [ProcessName & DateEntered]
        // 3) success/failure status about REST request
        [NoCacheAttribute]
        public ActionResult BackgroundAgent()
        {
            try
            {
                // Test Params
                if ((HttpContext.Request.QueryString["LastFetchDate"].Trim() == String.Empty) ||
                    (HttpContext.Request.QueryString["ProcessName"].Trim() == String.Empty))
                {
                    return base.Json(new RESTResponse() { ResponseStatus = ResponseStatus.IllegalRequest }, JsonRequestBehavior.AllowGet);
                }
                // Get Params
                String processName = HttpContext.Request.QueryString["ProcessName"].Trim();
                DateTime dateEntered = DateTime.Parse(HttpContext.Request.QueryString["LastFetchDate"].Trim());
                WebRoleMgr webRoleMgr = new WebRoleMgr(cloudStorageAccount, processName, dateEntered);
                
                // Add row
                webRoleMgr.AddObject();

                // Call SharedLibrary which calls AzureStorage
                int countSinceLastFetchDate = webRoleMgr.CountOfItemsSince(dateEntered);
                ResponseDataItem lastItem = webRoleMgr.LastItemEntered();
                ResponseDataItem[] SampleDataModelItems = new ResponseDataItem[1];
                SampleDataModelItems[0] = lastItem;

                // Build repsonse
                RESTResponse response = new RESTResponse()
                {
                    ResponseStatus = ResponseStatus.Success,
                    Count = countSinceLastFetchDate,
                    Items = SampleDataModelItems
                };

                Debug.WriteLine("*****MvcWebRole1.Controllers.PhoneController.BackgroundAgent Count=" + response.Count.ToString());

                // Convert and return object to JSON
                return base.Json(response, JsonRequestBehavior.AllowGet);
            }
            catch 
            {
                // Convert and return Unknown Exception
                return base.Json(new RESTResponse() { ResponseStatus = ResponseStatus.UnknownException }, JsonRequestBehavior.AllowGet);
            }
        }

        // this REST API call looks like
        // http://webserver/Phone/App/?LastFetchDate=11/29/2011&ProcessName=PhoneApp
        // call needs to insert an object with Request params then
        // the JSON result should be an object that returns:
        // 1) total count of all items 
        // 2) list of 20 most recent items [ProcessName & DateEntered]
        // 3) success/failure status about REST request
        [NoCacheAttribute]
        public ActionResult App()
        {
            try
            {
                // Test Params
                if ((HttpContext.Request.QueryString["LastFetchDate"].Trim() == String.Empty) ||
                    (HttpContext.Request.QueryString["FetchCount"].Trim() == String.Empty) ||
                    (HttpContext.Request.QueryString["ProcessName"].Trim() == String.Empty))
                {
                    return base.Json(new RESTResponse() { ResponseStatus = ResponseStatus.IllegalRequest }, JsonRequestBehavior.AllowGet);
                }

                // Get Params
                String processName = HttpContext.Request.QueryString["ProcessName"].Trim();
                DateTime dateEntered = DateTime.Parse(HttpContext.Request.QueryString["LastFetchDate"].Trim());
                int fetchCount = int.Parse(HttpContext.Request.QueryString["FetchCount"].Trim()); 
                WebRoleMgr webRoleMgr = new WebRoleMgr(cloudStorageAccount, processName, dateEntered);

                // Add row
                webRoleMgr.AddObject();

                // Call SharedLibrary which calls AzureStorage
                int totalCount = webRoleMgr.CountOfItems();
                ResponseDataItem[] lastNItems=null;

                if (totalCount > 0)
                {
                    lastNItems = webRoleMgr.LastItemsEntered(fetchCount).ToArray();
                }

                // Build repsonse
                RESTResponse response = new RESTResponse()
                {
                    ResponseStatus = ResponseStatus.Success,
                    Items = lastNItems,
                    Count = totalCount
                };
                Debug.WriteLine("*****MvcWebRole1.Controllers.PhoneController.App TotalCount=" + response.Count.ToString());
                Debug.WriteLine("*****MvcWebRole1.Controllers.PhoneController.App ItemCount=" + response.Items.Length.ToString());


                // Convert and return object to JSON
                return base.Json(response, JsonRequestBehavior.AllowGet);

            }
            catch
            {
                // Convert and return Unknown Exception
                return base.Json(new RESTResponse() { ResponseStatus = ResponseStatus.UnknownException }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}
