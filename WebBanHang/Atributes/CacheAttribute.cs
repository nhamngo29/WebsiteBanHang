using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;
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
            var cacheService=context.HttpContext.RequestServices.GetRequiredService<IReponseCacheService>();
            var cacheKey = GenerateCacheKeyFromRequest(context.HttpContext.Request);
            var cacheResponse = await cacheService.GetCacheReponseAync(cacheKey);
            if(!string.IsNullOrEmpty(cacheResponse))
            {
                var contextResult = new ContentResult
                {
                    Content = cacheResponse,
                    ContentType = "application/json",
                    StatusCode = 200
                };
                context.Result=contextResult;
                return;
            }    
            var excutedContext=await next();
            if(excutedContext.Result is OkObjectResult objectResult)
            {
                await cacheService.SetCacheReponseAync(cacheKey, objectResult.Value, TimeSpan.FromSeconds(_timeToLiveSenconds));
            }
        }
        private static string GenerateCacheKeyFromRequest(HttpRequest request)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append($"{request.Path}");
            foreach (var (key,value) in request.Query.OrderBy(t=>t.Key))
            {
                keyBuilder.Append($"{key}--{value}");
            }
            return keyBuilder.ToString();
        }
    }
}
