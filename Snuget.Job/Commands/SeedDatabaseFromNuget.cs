using System;
using System.Linq;
using System.Threading.Tasks;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Document;
using Snuget.Nuget;

namespace Snuget.Job.Commands
{
    public class SeedDatabaseFromNuget : ICommand
    {
        public SeedDatabaseFromNuget()
        {
            Context = new Nuget.V2FeedContext(new Uri("https://nuget.org/api/v2"));
            DocumentStore = new DocumentStore()
            {
                ConnectionStringName = "RavenDB"
            }.Initialize();

            Context.IgnoreMissingProperties = true;
            Context.IgnoreResourceNotFoundException = true;
        }

        public IDocumentStore DocumentStore { get; protected set; }
        public V2FeedContext Context { get; protected set; }

        public void Execute()
        {
            var pageSize = 100;
            var query = Context.Packages.OrderBy(x => x.Created).Where(x => x.IsLatestVersion);
            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (decimal)pageSize);

           Console.WriteLine("# of Nuget Packages : {0}", totalCount);

            Parallel.For(0, totalPages, page =>
            {
                var retry = 0;
                while (retry < 3)
                {
                    try
                    {
                        Console.WriteLine("processing page {0} of {1}", page, totalPages);
                        var packages = query.Skip(page*pageSize).Take(pageSize).ToList();
                        using (var bulk = DocumentStore.BulkInsert(options: new BulkInsertOptions {OverwriteExisting = true}))
                        {
                            foreach (var package in packages)
                                bulk.Store(package);
                        }

                        return; // success!
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("FAIL: processing page {0} of {1}", page, totalPages);
                        retry++;
                    }
                }
            });
        }
    }
}
