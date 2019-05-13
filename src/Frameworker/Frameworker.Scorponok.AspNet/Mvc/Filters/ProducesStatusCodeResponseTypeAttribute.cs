using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;

namespace Frameworker.Scorponok.AspNet.Mvc.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ProducesStatusCodeResponseTypeAttribute : ProducesResponseTypeAttribute
    {
        public ProducesStatusCodeResponseTypeAttribute(HttpStatusCode status)
            : base((int)status)
        {

        }

        public ProducesStatusCodeResponseTypeAttribute(Type type, HttpStatusCode status)
        : base(type, (int)status)
        {

        }
    }
}
