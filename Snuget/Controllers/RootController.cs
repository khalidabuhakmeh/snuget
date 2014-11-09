using System.Linq;
using System.Web.Mvc;
using Raven.Client;
using Snuget.Models.Indexes;
using Snuget.Models.ViewModels.Root;
using Snuget.Nuget;

namespace Snuget.Controllers
{
    public class RootController : ApplicationController
    {
        public ActionResult Show()
        {
            using (var session = Db.OpenSession())
            {
                var model = new ShowModel();
                RavenQueryStatistics stats;
                model.Latest = session.Query<PackagesIndex.Result, PackagesIndex>()
                    .Statistics(out stats)
                    .OrderByDescending(x => x.LastUpdated)
                    .Take(6)
                    .As<V2FeedPackage>()
                    .ToList();
                model.TotalCount = stats.TotalResults;

                return View(model);    
            }            
        }
    }
}