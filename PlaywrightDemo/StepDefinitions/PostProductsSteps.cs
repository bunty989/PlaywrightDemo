using Microsoft.Playwright;
using PlaywrightDemo.ApiModels.PostProducts;
using PlaywrightDemo.Utilities;
using Reqnroll;

namespace PlaywrightDemo.StepDefinitions
{
    [Binding]
    internal class PostProductsSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly PostProducts _postProducts;
        [ThreadStatic]
        private string? _endPoint;

        public PostProductsSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _postProducts = new((IPlaywright)_scenarioContext["PlaywrightInstance"]);
        }

        [Given("I have the endpoint {string} for Post Products")]
        public async Task GivenIHaveTheEndpointForPostProducts(string endPoint)
        {
            _endPoint = endPoint;
        }

        [Given("I have the request body for Post Products")]
        public async Task GivenIHaveTheRequestBodyForPostProducts()
        {
            await _postProducts.SetupRequestAsync();
        }

        [When("I send a POST request to the Post Products Url")]
        public async Task WhenISendAPOSTRequestToThePostProductsUrl()
        {
            _scenarioContext["Response"] = await _postProducts.PostProductsAsync(_endPoint);
        }

        [Then("the resonse should pass the schema for {string} for Post Products")]
        public void ThenTheResonseShouldPassTheSchemaForForPostProducts(string responseSchemaName)
        {
            var res = _postProducts.ValidateResponseSchema((string)_scenarioContext["ResponseBody"],
                 responseSchemaName.Contains("201")
                 ? TestConstant.PathVariables.PostProducts201Schema
                 : TestConstant.PathVariables.GetWeather401Schema);
            Assert.That(res, Is.True, "Response schema validation failed");
        }

    }
}
