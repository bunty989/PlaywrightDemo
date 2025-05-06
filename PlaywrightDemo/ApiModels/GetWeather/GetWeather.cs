using Microsoft.Playwright;
using PlaywrightDemo.Utilities;

namespace PlaywrightDemo.ApiModels.GetWeather
{
    internal class GetWeather
    {
        private ApiHelper _apiHelper;
        [ThreadStatic]
        private IAPIResponse? _response;

        public GetWeather(IPlaywright playwright)
        {
            _apiHelper = new(playwright);
        }

        public async Task SetupRequestAsync()
        {
            var baseurl = ConfigHelper.ReadConfigValue(TestConstant.ConfigTypes.APIConfig,
                TestConstant.ConfigTypesKey.BaseUrl);
            var headers = new Dictionary<string, string>
            {
                { "Accept", "*/*" },
                {"Accept-Encoding", "gzip, deflate, br" }
            };
            await _apiHelper.SetupApiRequestClient(baseurl, headers);
        }

        public async Task<IAPIResponse> GetWeatherAsync(string? endPoint, Dictionary<string, object> searchParams)
        {
            _response = await _apiHelper.GetAsync(endPoint, new APIRequestContextOptions
            {
                Params = searchParams,
            });
            return _response;
        }

        public bool ValidateResponseSchema(string responseBody, string schemaPath)
        {
            return _apiHelper.ValidateResponseSchema(responseBody,
                File.ReadAllText(schemaPath));
        }
    }
}
