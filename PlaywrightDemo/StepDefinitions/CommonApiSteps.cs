using Microsoft.Playwright;
using PlaywrightDemo.Utilities;
using Reqnroll;

namespace PlaywrightDemo.StepDefinitions
{
    [Binding]
    public class CommonApiSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly ApiHelper _apiHelper;
        [ThreadStatic]
        private IAPIResponse? _response;
        public CommonApiSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _apiHelper = new ApiHelper((IPlaywright)_scenarioContext["PlaywrightInstance"]);
        }

        [When("I should get a response for the api call")]
        public void ThenIShouldGetAResponseForTheApiCall()
        {
           _response = (IAPIResponse)_scenarioContext["Response"];
            _scenarioContext["ResponseBody"]= _apiHelper.GetResponseBodyAsJson(_response).Result.ToString();
        }

        [Then("the response status code should be {int}")]
        public void ThenTheResponseStatusCodeShouldBe(int statusCode)
        {
            Assert.That(_apiHelper.GetStatusCode(_response), Is.EqualTo(statusCode));
        }

        [Then("the value of the {string} is {string} in the response")]
        public void ThenTheValueOfTheIsInTheResponse(string keyNode, string nodeValue)
        {
            var nodeValueFromResponse = _apiHelper.FindJsonNodeValueByPath((string)_scenarioContext["ResponseBody"], keyNode);
            Assert.That(nodeValueFromResponse, Is.EqualTo(nodeValue));
        }
    }
}
