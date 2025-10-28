using Microsoft.Playwright;
using PlaywrightDemo.Utilities;

namespace PlaywrightDemo.ApiModels.PostProducts
{
    internal class PostProducts
    {
        private ApiHelper _apiHelper;
        [ThreadStatic]
        private IAPIResponse? _response;

        public PostProducts(IPlaywright playwright)
        {
            _apiHelper = new(playwright);
        }

        public async Task SetupRequestAsync()
        {
            var baseurl = "https://fakestoreapi.com";
            var headers = new Dictionary<string, string>
            {
                { "Accept", "*/*" },
                {"Accept-Encoding", "gzip, deflate, br" },
                {"Content-Type", "application/json" }
            };
            await _apiHelper.SetupApiRequestClient(baseurl, headers);
        }

        public async Task<IAPIResponse> PostProductsAsync(string? endPoint)
        {
            var data = _apiHelper.DeserializeJson<Dictionary<string, object>>(File.ReadAllText(TestConstant.PathVariables.PostProductsBody));
            _response = await _apiHelper.PostAsync(endPoint, data);
            return _response;
        }

        public bool ValidateResponseSchema(string responseBody, string schemaPath)
        {
            return _apiHelper.ValidateResponseSchema(responseBody,
                File.ReadAllText(schemaPath));
        }
    }
}
