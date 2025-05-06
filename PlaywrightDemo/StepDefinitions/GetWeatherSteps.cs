using Microsoft.Playwright;
using PlaywrightDemo.ApiModels.GetWeather;
using PlaywrightDemo.Utilities;
using Reqnroll;

namespace PlaywrightDemo.StepDefinitions
{
    [Binding]
    public class GetWeatherSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly GetWeather _getWeather;
        [ThreadStatic]
        private string? _endPoint;
        [ThreadStatic]
        private string? _paramString;

        public GetWeatherSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _getWeather = new ((IPlaywright)_scenarioContext["PlaywrightInstance"]);
        }

        [Given("I have the endpoint {string} and the search param is {string} for GetWeather")]
        public async Task GivenIHaveTheEndpointAndTheSearchParamIsForGetWeather(string endpoint, string queryParams)
        {
            _endPoint = endpoint;
            _paramString = queryParams;
            await _getWeather.SetupRequestAsync();
        }

        [When("I send a GET request to the GetWeather Url")]
        public async Task WhenISendAGETRequestToTheGetWeatherUrl()
        {
            var appId = EncryptionHelper.Decrypt(ConfigHelper.ReadConfigValue(TestConstant.ConfigTypes.APIConfig, TestConstant.ConfigTypesKey.AppId));
            var searchParams = new Dictionary<string, object>
            {
                { _paramString.Split(";")[0].Split("=")[0], _paramString.Split(";")[0].Split("=")[1] },
                { _paramString.Split(";")[1].Split("=")[0], _paramString.Split(";")[1].Split("=")[1] },
                { "appid",appId}
            };
            _scenarioContext["Response"] = await _getWeather.GetWeatherAsync(_endPoint, searchParams);
        }

        [When("I send a GET request to the GetWeather Url without the api key")]
        public async Task WhenISendAGETRequestToTheGetWeatherUrlWithoutTheApiKey()
        {
            var searchParams = new Dictionary<string, object>
            {
                { _paramString.Split(";")[0].Split("=")[0], _paramString.Split(";")[0].Split("=")[1] },
                { _paramString.Split(";")[1].Split("=")[0], _paramString.Split(";")[1].Split("=")[1] },
            };
            _scenarioContext["Response"] = await _getWeather.GetWeatherAsync(_endPoint, searchParams);
        }


        [Then("the resonse should pass the schema for {string} for GetWeather")]
        public void ThenTheResonseShouldPassTheSchemaForForGetWeather(string responseSchemaName)
        {

            var res = _getWeather.ValidateResponseSchema((string)_scenarioContext["ResponseBody"],
                responseSchemaName.Contains("200")
                ? TestConstant.PathVariables.GetWeather200Schema
                :TestConstant.PathVariables.GetWeather401Schema);
            Assert.That(res, Is.True, "Response schema validation failed");
        }
    }
}
