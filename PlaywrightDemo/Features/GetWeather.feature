@Retry
Feature: Get Weather

@api
Scenario: Verify the Api response 200 for valid GetWeather request
	Given I have the endpoint '/data/2.5/weather' and the search param is 'lat=33.44;lon=-94.04' for GetWeather
	When I send a GET request to the GetWeather Url
	And I should get a response for the api call
	Then the response status code should be 200
	And the resonse should pass the schema for 'Response 200' for GetWeather
	And the value of the 'name' is 'Texarkana' in the response
	And the value of the 'coord.lon' is '-94.04' in the response
	And the value of the 'coord.lat' is '33.44' in the response

	@api
Scenario: Verify the Api response 401 for invalid GetWeather request
	Given I have the endpoint '/data/2.5/weather' and the search param is 'lat=33.44;lon=-94.04' for GetWeather
	When I send a GET request to the GetWeather Url without the api key
	And I should get a response for the api call
	Then the response status code should be 401
	And the resonse should pass the schema for 'Response 401' for GetWeather
	And the value of the 'cod' is '401' in the response
	And the value of the 'message' is 'Invalid API key. Please see https://openweathermap.org/faq#error401 for more info.' in the response