using Microsoft.Playwright;
using PlaywrightDemo.ApiModels.GetWeatherById;
using PlaywrightDemo.Utilities;
using Reqnroll;

namespace PlaywrightDemo.StepDefinitions
{
    [Binding]
    public class GetWeatherStepsById
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly GetWeatherById _getWeatherById;
        [ThreadStatic]
        private string? _endPoint;
        [ThreadStatic]
        private string? _paramString;

        public GetWeatherStepsById(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _getWeatherById = new ((IPlaywright)_scenarioContext["PlaywrightInstance"]);
        }

        [Given("I have the endpoint {string} and the search param is {string} for GetWeatherById")]
        public async Task GivenIHaveTheEndpointAndTheSearchParamIsForGetWeatherById(string endpoint, string queryParams)
        {
            _endPoint = endpoint;
            _paramString = queryParams;
            await _getWeatherById.SetupRequestAsync();
        }

        [When("I send a GET request to the GetWeatherById Url")]
        public async Task WhenISendAGETRequestToTheGetWeatherByIdUrl()
        {
            var appId = EncryptionHelper.Decrypt(ConfigHelper.ReadConfigValue(TestConstant.ConfigTypes.APIConfig, TestConstant.ConfigTypesKey.AppId));
            var searchParams = new Dictionary<string, object>
            {
                { _paramString.Split("=")[0], _paramString.Split("=")[1] },
                { "appid",appId}
            };
            _scenarioContext["Response"] = await _getWeatherById.GetWeatherByIdAsync(_endPoint, searchParams);
        }

        [When("I send a GET request to the GetWeatherById Url without the api key")]
        public async Task WhenISendAGETRequestToTheGetWeatherByIdUrlWithoutTheApiKey()
        {
            var searchParams = new Dictionary<string, object>
            {
                { _paramString.Split("=")[0], _paramString.Split("=")[1] },
            };
            _scenarioContext["Response"] = await _getWeatherById.GetWeatherByIdAsync(_endPoint, searchParams);
        }


        [Then("the resonse should pass the schema for {string} for GetWeatherById")]
        public void ThenTheResonseShouldPassTheSchemaForForGetWeatherById(string responseSchemaName)
        {

            var res = _getWeatherById.ValidateResponseSchema((string)_scenarioContext["ResponseBody"],
                responseSchemaName.Contains("200")
                ? TestConstant.PathVariables.GetWeatherById200Schema
                :TestConstant.PathVariables.GetWeatherById401Schema);
            Assert.That(res, Is.True, "Response schema validation failed");
        }
    }
}
