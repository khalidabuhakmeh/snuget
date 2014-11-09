using System;
using System.Web.Mvc;
using Raven.Client;
using RestfulRouting.Format;

namespace Snuget.Controllers
{
    public abstract class ApplicationController : Controller
    {
        protected IDocumentStore Db
        {
            get { return MvcApplication.DocumentStore; }
        }

        protected ActionResult RespondTo(Action<FormatCollection> format)
        {
            return new FormatResult(format);
        }
    }
}