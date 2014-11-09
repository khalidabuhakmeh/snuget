using System;
using System.Linq;
using Raven.Client;
using Raven.Client.Linq;

namespace Snuget.Models.Extensions
{
    public static class RavenHelpers
    {
        public static IRavenQueryable<T> If<T>(this IRavenQueryable<T> query, bool should, params Func<IRavenQueryable<T>, IRavenQueryable<T>>[] transforms)
        {
            return should ? transforms.Aggregate(query, (current, transform) => transform.Invoke(current)) : query;
        }

        public static IQueryable<T> If<T>(this IQueryable<T> query, bool should, params Func<IQueryable<T>, IQueryable<T>>[] transforms)
        {
            return should ? transforms.Aggregate(query, (current, transform) => transform.Invoke(current)) : query;
        }

        public static IDocumentQuery<T> If<T>(this IDocumentQuery<T> query, bool should, params Func<IDocumentQuery<T>, IDocumentQuery<T>>[] transforms)
        {
            return should ? transforms.Aggregate(query, (current, transform) => transform.Invoke(current)) : query;
        }
    }
}