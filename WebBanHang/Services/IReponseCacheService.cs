namespace WebBanHang.Services
{
    public interface IReponseCacheService
    {
        Task SetCacheReponseAync(string cacheKy, object reponse, TimeSpan timeOut);
        Task<string> GetCacheReponseAync(string cacheKey);
    }
}
