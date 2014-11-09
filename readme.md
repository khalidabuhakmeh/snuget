# Snuget (Searchable / Sortable Nuget)

I am a big proponent of Open Source in the .NET community, and one of my favorite activies is going to Nuget.org and finding out what that community is doing. Sadly, Nuget.org has buckled under the enormous growth of Nuget and taken away the ability to search and sort by anything meaningful. As of writing this there is only one sort available.

> Sorted by recent installs

I'm not sure about you, but that is the most worthless sort you could have for a dataset:

1. Microsoft will always win, since the packages they promote are almost always a requirement for any new project.
2. The sort is so arbitrary, but the site still lets you page on the data. Anyone who pages on a random data set is borderline insane.
3. New and exciting projects get starved with no ability to gain traction, at least via Nuget.org because they are caught in a catch 22 situtation.

## Why Snuget

I understand that Nuget.org is not meant to be a promotional tool, but I feel that most of the battle with doing OSS is promoting your projects. Having a few myself, it is a great feeling to get someone to use the fruits of your labor. I wanted to see how difficult it would be to build a sortable and searchable Nuget repository. Funny enough, it wasn't very difficult.

## Technology

I decided to utilize [RavenDB 3](http://ravendb.net) to store the data pulled from the Nuget API. With a simple console application, I am able to pull all the latest versions of Nuget packages and load them into documents.

```
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
```

It takes about 2 to 5 minutes to load **~28,000** packages. Not too shabby if you ask me, considering that each API call is limited to pulling back 100 packages at a time. I also noticed that the API would randomly through serialization exceptions. Retrying requests multiple times cleared those issues up.

Once the database is seeded with Nuget data, it takes a simple RavenDB index to query the result. Take a look.

```
public class PackagesIndex : AbstractIndexCreationTask<V2FeedPackage, PackagesIndex.Result>
{
    public class Result
    {
        public bool IsPrerelease { get; set; }
        public string Language { get; set; }
        public DateTimeOffset LastEdited { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
        public string NormalizedVersion { get; set; }
        public long PackageSize { get; set; }
        public DateTimeOffset Published { get; set; }
        public string ReleaseNotes { get; set; }
        public bool RequireLicenseAcceptance { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public int VersionDownloadCount { get; set; }
        public string Title { get; set; }
        public DateTimeOffset Created { get; set; }
        public string[] LicenseNames { get; set; }
        public string[] Authors { get; set; }
        public string[] Dependencies { get; set; }
        public string[] Tags { get; set; }
        public object[] Search { get; set; }
    }

    public PackagesIndex()
    {
        Map = packages => from p in packages
            select new
            {
                p.IsPrerelease,
                p.Language,
                p.LastEdited,
                p.LastUpdated,
                p.NormalizedVersion,
                p.PackageSize,
                p.Published,
                p.ReleaseNotes,
                p.RequireLicenseAcceptance,
                p.Summary,
                p.Version,
                p.VersionDownloadCount,
                Title = string.IsNullOrWhiteSpace(p.Title) ? p.Id : p.Title,
                p.Description,
                p.Created,
                LicenseNames = (p.LicenseNames ?? string.Empty).Split(','),
                Authors = (p.Authors ?? string.Empty).Split(','),
                Dependencies = (p.Dependencies ?? string.Empty).Split(','),
                Tags = (p.Tags ?? string.Empty).Split(','),
                Search = new[] { p.Summary, p.Description, p.Tags, p.Title, p.Id, p.Authors, p.ReleaseNotes }
            };

        Index(x => x.Search, FieldIndexing.Analyzed);
        Index(x => x.Title, FieldIndexing.Default);
        Index(x => x.Description, FieldIndexing.Default);

        Sort(x => x.PackageSize, SortOptions.Long);
    }
}
```

Having the index written and processed, we just need a simple LINQ Query to be able to search and sort the dataset.

```
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
```

We can now sort of the following criteria:

```
{ "Last Edited", q => q.OrderByDescending(x => x.LastEdited) },
{ "Last Updated", q => q.OrderByDescending(x => x.LastUpdated) },
{ "Package Size", q => q.OrderBy(x => x.PackageSize) },
{ "Published", q => q.OrderByDescending(x => x.Published) },
{ "Download Count", q => q.OrderByDescending(x => x.VersionDownloadCount) },
{ "Created", q => q.OrderByDescending(x => x.Created) },
{ "Title", q => q.OrderByDescending(x => x.Title) }
```
To add more criteria is as simple as updating the dictionary above. If the data is in RavenDB, then we can sort on it.

## Conclusion

I love where OSS in the .NET community is going, but it is still fragile and growing up. The more we can do to promote projects, the more we can spur discussion and make progress. I see a searchable and sortable dataset as important part of the community and I hope I'm not alone. Hope you enjoy this project, and let me know what you think.

