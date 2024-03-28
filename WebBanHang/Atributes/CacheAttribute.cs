//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Filters;
//using System.Text;
//using WebBanHang.Configurations;
//using WebBanHang.Services;

//namespace WebBanHang.Atributes
//{
//    public class CacheAttribute : Attribute, IAsyncActionFilter
//    {
//        private readonly int _timeToLiveSenconds;
//        public CacheAttribute(int timeToLiveSenconds)
//        {
//            _timeToLiveSenconds = timeToLiveSenconds;
//        }
//        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
//        {
//            var cacheConfiguration = context.HttpContext.RequestServices.GetRequiredService<RedisConfiguration>();
//            if (!cacheConfiguration.Enabled)
//            {
//                await next();
//                return;
//            }
//            var cacheService=context.HttpContext.RequestServices.GetRequiredService<IReponseCacheService>();
//            var cacheKey = GenerateCacheKeyFromRequest(context.HttpContext.Request);
//            var cacheResponse = await cacheService.GetCacheReponseAync(cacheKey);
//            if(!string.IsNullOrEmpty(cacheResponse))
//            {
//                var contextResult = new ContentResult
//                {
//                    Content = cacheResponse,
//                    ContentType = "application/json",
//                    StatusCode = 200
//                };
//                context.Result=contextResult;
//                return;
//            }    
//            var excutedContext=await next();
//            if(excutedContext.Result is OkObjectResult objectResult)
//            {
//                await cacheService.SetCacheReponseAync(cacheKey, objectResult.Value, TimeSpan.FromSeconds(_timeToLiveSenconds));
//            }
//        }
//        private static string GenerateCacheKeyFromRequest(HttpRequest request)
//        {
//            var keyBuilder = new StringBuilder();
//            keyBuilder.Append($"{request.Path}");
//            foreach (var (key,value) in request.Query.OrderBy(t=>t.Key))
//            {
//                keyBuilder.Append($"{key}--{value}");
//            }
//            return keyBuilder.ToString();
//        }
//    }
//}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WebBanHang.Configurations;
using WebBanHang.Services;

namespace WebBanHang.Atributes
{
    public class CacheAttribute : Attribute, IAsyncActionFilter
    {
        private readonly int _timeToLiveSenconds;
        public CacheAttribute(int timeToLiveSenconds)
        {
            _timeToLiveSenconds = timeToLiveSenconds;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cacheConfiguration = context.HttpContext.RequestServices.GetRequiredService<RedisConfiguration>();
            if (!cacheConfiguration.Enabled)
            {
                await next();
                return;
            }

            var cacheService = context.HttpContext.RequestServices.GetRequiredService<IReponseCacheService>();
            var cacheKey = GenerateCacheKeyFromRequest(context.HttpContext.Request);

            var cacheResponse = await cacheService.GetCacheReponseAync(cacheKey);
            if (!string.IsNullOrEmpty(cacheResponse))
            {
                // Cache hit: Return cached view
                var viewResult = new ViewResult
                {
                    ViewName = GetViewName(context)// You can pass any model or data to your view
                };
                context.Result = viewResult;
                return;
            }

            // Execute the action
            var executedContext = await next();
            if (executedContext.Result is ViewResult)
            {
                var viewResult = executedContext.Result as ViewResult;
                await cacheService.SetCacheReponseAync(cacheKey, RenderViewToString(context, viewResult), TimeSpan.FromSeconds(_timeToLiveSenconds));
            }
        }

        private static string GenerateCacheKeyFromRequest(HttpRequest request)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append($"{request.Path}");
            foreach (var (key, value) in request.Query.OrderBy(t => t.Key))
            {
                keyBuilder.Append($"{key}--{value}");
            }
            return keyBuilder.ToString();
        }

        // Get view name dynamically based on action and controller names
        private static string GetViewName(ActionExecutingContext context)
        {
            var controllerName = context.RouteData.Values["controller"] as string;
            var actionName = context.RouteData.Values["action"] as string;
            return $"~/Views/{controllerName}/{actionName}.cshtml";
        }
        private static string RenderViewToString(ActionExecutingContext context, ViewResult viewResult)
        {
            var httpContext = context.HttpContext;
            var serviceScope = httpContext.RequestServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var viewEngine = serviceScope.ServiceProvider.GetService<ICompositeViewEngine>();

            var actionContext = new ActionContext(httpContext, httpContext.GetRouteData(), context.ActionDescriptor);
            var view = viewEngine.FindView(actionContext, viewResult.ViewName, false).View;

            using (var output = new StringWriter())
            {
                var viewContext = new ViewContext(actionContext, view, viewResult.ViewData, viewResult.TempData, output, new HtmlHelperOptions());
                view.RenderAsync(viewContext).GetAwaiter().GetResult();
                return output.ToString();
            }
        }
    }
}