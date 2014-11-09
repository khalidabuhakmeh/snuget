using System.Web.Mvc;
using System.Web.Routing;
using RestfulRouting;
using Snuget.Controllers;

namespace Snuget
{
    public class RouteConfig : RouteSet
    {
        public override void Map(IMapper map)
        {
            map.DebugRoute("routedebug");
            map.Root<RootController>(x => x.Show());
            map.Resources<PackagesController>(p => p.Only("index", "show"));
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoutes<RouteConfig>();
        }
    }
}
