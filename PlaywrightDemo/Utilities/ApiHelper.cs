using Microsoft.Playwright;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Serilog;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace PlaywrightDemo.Utilities
{
    public class ApiHelper
    {
        [ThreadStatic]
        private readonly IPlaywright _playwright;
        [ThreadStatic]
        private static IAPIRequestContext? _requestContext;

        public ApiHelper(IPlaywright playwright)
        {
            _playwright = playwright;
        }

        public async Task SetupApiRequestClient(string? baseUrl, Dictionary<string, string> headers, int timeOut = 0)
        {
            var requestContextOptions = new APIRequestNewContextOptions
            {
                BaseURL = baseUrl,
                Timeout = timeOut == 0 ? (float)(int.Parse(ConfigHelper.ReadConfigValue(
                                            TestConstant.ConfigTypes.APIConfig, TestConstant.ConfigTypesKey.TimeOut)
                                            ?? "0") * 1000) : timeOut,
                ExtraHTTPHeaders = headers,
                IgnoreHTTPSErrors = true,
            };
            _requestContext = await _playwright.APIRequest.NewContextAsync(requestContextOptions);
        }

        // GET request
        public async Task<IAPIResponse> GetAsync(string? url, APIRequestContextOptions? options = null)
        {
            return await _requestContext.GetAsync(url,options);
        }

        // POST request
        public async Task<IAPIResponse> PostAsync(string url, object body)
        {
            return await _requestContext.PostAsync(url, new()
            {
                DataObject = body
            });
        }

        // PUT request
        public async Task<IAPIResponse> PutAsync(string url, object body)
        {
            return await _requestContext.PutAsync(url, new()
            {
                DataObject = body
            });
        }

        // PATCH request
        public async Task<IAPIResponse> PatchAsync(string url, object body)
        {
            return await _requestContext.PatchAsync(url, new()
            {
                DataObject = body
            });
        }

        // DELETE request
        public async Task<IAPIResponse> DeleteAsync(string url, APIRequestContextOptions? options = null)
        {
            return await _requestContext.DeleteAsync(url, options);
        }

        // JSON Serialization
        public string SerializeJson(object obj)
        {
            return JsonSerializer.Serialize(obj, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true
            });
        }

        // JSON Deserialization
        public T? DeserializeJson<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        // Response Schema Validation using Newtonsoft.Json.Schema
        public bool ValidateResponseSchema(string responseContent, string schemaContent)
        {
            var schema = JSchema.Parse(schemaContent);
            var jsonObject = JObject.Parse(responseContent);

            if (!jsonObject.IsValid(schema, out IList<string> errors))
            {
                Log.Error($"Schema validation failed: {string.Join(", ", errors)}");
                return false;
            }
            Log.Information("Schema validation passed.");
            return true;
        }

        // Extract Status Code
        public int GetStatusCode(IAPIResponse response)
        {
            var statusCode = response.Status;
            Log.Information($"Response Status Code: {statusCode}");
            return statusCode;
        }

        // Extract Response Body as JSON
        public async Task<JsonElement> GetResponseBodyAsJson(IAPIResponse response)
        {
            var jsonElement = (JsonElement)await response.JsonAsync();
            Log.Information($"Response Body: {jsonElement.ToString()}");
            return jsonElement;
        }

        public object? FindJsonNodeValueByPath(string jsonString, string keyPath, bool returnMultiple = false)
        {
            try
            {
                // Deserialize the JSON string into a dynamic object
                var jObj = JsonConvert.DeserializeObject(jsonString) as dynamic;

                if (returnMultiple)
                {
                    // Use SelectTokens to find all matching nodes for the given keyPath
                    var jTokens = jObj?.SelectTokens(keyPath);
                    if (jTokens != null)
                    {
                        var tokenList = ((IEnumerable<JToken>)jTokens).Select(token => token?.ToString()).ToList();
                        Log.Information($"Selected Tokens: {string.Join(", ", tokenList)}");
                        return tokenList;
                    }
                }
                else
                {
                    // Use SelectToken to find a single matching node for the given keyPath
                    var jToken = jObj?.SelectToken(keyPath);
                    Log.Information($"Selected Token: {jToken?.ToString()} of Node {keyPath}");
                    return jToken?.ToString();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error while searching JSON: {ex.Message}");
            }

            return returnMultiple ? new List<string?>() : null;
        }
    }
}
