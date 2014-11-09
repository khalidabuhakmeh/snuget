using System.Web.Mvc;
using System.Web.Routing;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace Snuget
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static readonly IDocumentStore DocumentStore = new DocumentStore()
        {
            ConnectionStringName = "RavenDB"
        }.Initialize();

        protected void Application_Start()
        {
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            IndexCreation.CreateIndexes(typeof(MvcApplication).Assembly, DocumentStore);
        }
    }
}
