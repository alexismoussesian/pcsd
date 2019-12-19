using ININ.PureCloudApi.Api;
using ININ.PureCloudApi.Client;
using ININ.PureCloudApi.Model;
using log4net;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pcsd
{
    /// <summary>
    /// This class used to connect to purecloud environment and pull statistics
    /// </summary>
    public class purecloudService
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public purecloudService()
        {

        }

        public void getQueues()
        {

            try
            {
                /*
                var client = new RestClient("https://api.ininsca.com/api/v2/routing/queues");
                var request = new RestRequest(Method.GET);
                request.AddHeader("postman-token", "4fe9c331-3f04-c209-6269-537b27bbc5f0");
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("authorization", "Bearer 0PmmyDJOka3rSK6MPxyT7_7519dC-5Gs7J0mkDHoBhMi3atbI2Y_5RUJg-J3_bZwhiWQ7O3vmTvAKdeelbTw9Q");
                request.AddHeader("content-type", "application/json");
                IRestResponse response = client.Execute(request);
                */






                Configuration.Default.AccessToken = "Bearer tGTVtwBo6hcm3m376LQKoIYsSi1RvaOavCdp-Ui5-dedf-l7JeLtXH2PkQXNBo5C8eAqh90ETmqAkNw0umVcSg";

                Configuration.Default.AddDefaultHeader("postman-token", "4fe9c331-3f04-c209-6269-537b27bbc5f0");
                Configuration.Default.AddDefaultHeader("content-type", "application/json");



                var api = new RoutingApi();
                var pageSize = 25;
                var pageNumber = 1;
                var sortBy = "";
                var name = "";
                var active = true;

                QueueEntityListing result = api.GetQueues(pageSize, pageNumber, sortBy, name, active);

                log.Info("execute getQueues");
            }
            catch(Exception ex)
            {
                log.Error("error " + ex.Message);
            }

        }
    }
}
