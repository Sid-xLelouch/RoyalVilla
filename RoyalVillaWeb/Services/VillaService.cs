using RoyalVilla.DTO;
using RoyalVillaWeb.Models;
using RoyalVillaWeb.Services.IServices;
using static RoyalVillaWeb.SD;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RoyalVillaWeb.Services
{
    public class VillaService : BaseService ,IVillaService
    {
        private readonly string _apiEndpoint;
        public VillaService(IHttpClientFactory httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
            : base(httpClient, httpContextAccessor)
        {
            var apiVersion = configuration.GetValue<string>("ServiceUrls:VillaApiVersion");
            if (string.IsNullOrWhiteSpace(apiVersion))
            {
                apiVersion = "v1";
            }

            _apiEndpoint = $"/api/{apiVersion}/villa";
        }

        public Task<T?> CreateAsync<T>(VillaCreateDTO dto)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Data = dto,
                Url = _apiEndpoint
            });
        }

        public Task<T?> DeleteAsync<T>(int id)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.DELETE,
                Url = $"{_apiEndpoint}/{id}"
            });
        }

        public Task<T?> GetAllAsync<T>()
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = _apiEndpoint
            });
        }

        public Task<T?> GetAsync<T>(int id)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                Url = $"{_apiEndpoint}/{id}"
            });
        }

        public Task<T?> UpdateAsync<T>(VillaUpdateDTO dto)
        {
            return SendAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.PUT,
                Data = dto,
                Url = $"{_apiEndpoint}/{dto.Id}"
            });
        }
    }
}
