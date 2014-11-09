using System;
using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using Snuget.Nuget;

namespace Snuget.Models.Indexes
{
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
}