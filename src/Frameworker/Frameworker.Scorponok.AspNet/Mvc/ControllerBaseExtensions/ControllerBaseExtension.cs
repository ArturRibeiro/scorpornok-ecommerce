using Frameworker.Scorponok.AspNet.Mvc.Result;
using Microsoft.AspNetCore.Mvc;

namespace Frameworker.Scorponok.AspNet.Mvc.ControllerBaseExtensions
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
