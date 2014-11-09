using System.Web.Mvc;
using PagedList;
using Raven.Client;
using Raven.Client.Linq.Indexing;
using Snuget.Models.Extensions;
using Snuget.Models.Indexes;
using Snuget.Models.ViewModels.Packages;
using Snuget.Nuget;

namespace Snuget.Controllers
{
    public class PackagesController : ApplicationController
    {
        // GET: Packages
        public ActionResult Index(SearchModel input)
        {
            using (var session = Db.OpenSession())
            {
                var model = new IndexModel(input);

                model.Packages = session.Query<PackagesIndex.Result, PackagesIndex>()
                    .If(model.Search.HasQuery, q => q
                                .Search(x => x.Title, model.Search.WildcardQuery, boost: 10, escapeQueryOptions: EscapeQueryOptions.AllowPostfixWildcard)
                                .Search(x => x.Search, model.Search.WildcardQuery, boost: 5))
                    .If(model.Search.HasSort, q => model.Search.SetOrderBy(q))
                    .As<V2FeedPackage>()
                    .ToPagedList(model.Search.Page, SearchModel.Size);

                return View(model);
            }
        }
    }
}