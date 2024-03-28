using StackExchange.Redis;
using WebBanHang.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebBanHang.Services;

namespace WebBanHang.Installers
{
    public class CacheInstaller : IInstaller
    {
        public void InstrallServices(IServiceCollection services, IConfiguration configuration)
        {
            var redisConfiguration = new RedisConfiguration();
            configuration.GetSection("RedisConfiguration").Bind(redisConfiguration);
            services.AddSingleton(redisConfiguration);
            if (!redisConfiguration.Enabled)
                return;
            services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConfiguration.ConnectionString));
            services.AddStackExchangeRedisCache(option=>option.Configuration=redisConfiguration.ConnectionString);
            services.AddSingleton<IReponseCacheService,ReponseCacheService>();

        }
    }
}