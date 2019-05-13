using Frameworker.Scorponok.AspNet.Mvc.Result;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Catalog.Web.Api.Controllers
{
    public static class ControllerBaseExtension
    {
        /// <summary>
        /// Retorna código HTTP de OK 
        /// </summary>
        /// <returns></returns>
        public static ActionResult Ok2<TResult>(this ControllerBase controllerBase, TResult obj)
        {
            ResultMessageResponse<TResult> result = obj;

            return controllerBase.Ok(result);
        }
    }
}
