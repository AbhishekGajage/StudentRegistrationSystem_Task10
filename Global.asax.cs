using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace StudentRegistrationSystem
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}