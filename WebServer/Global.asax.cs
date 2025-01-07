using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Json.ValueFactory;

namespace MiceDataSnap
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            //ModelBinders.Binders.Add(typeof(TMiceApplyUpdatesRequest), new TMiceApplyUpdatesRequestModelBinder());
            // remove default implementation    
            ValueProviderFactories.Factories.Remove(ValueProviderFactories.Factories.OfType<JsonValueProviderFactory>().FirstOrDefault());
            // add our custom one
            ValueProviderFactories.Factories.Add(new JsonNetValueProviderFactory());
        }
    }
}
