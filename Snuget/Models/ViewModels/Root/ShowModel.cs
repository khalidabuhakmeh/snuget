using System.Collections.Generic;
using Snuget.Nuget;

namespace Snuget.Models.ViewModels.Root
{
    public class ShowModel
    {
        public IList<V2FeedPackage> Latest { get; set; }
        public int TotalCount { get; set; }
    }
}