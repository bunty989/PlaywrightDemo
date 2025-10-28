using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace PlaywrightDemo.Utilities
{
    internal class CommonMethods
    {
        //public static string GetCurrentDisplayResolution()
        //{
        //    var screenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
        //    var screenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
        //    return $"{screenWidth}x{screenHeight}";
        //}

        public static string UpdateJsonNodeValue(string json, string path, object newValue)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                Log.Error($"Provided JSON string {nameof(json)} is null or empty.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                Log.Error($"Provided path {nameof(path)} is null or empty.");
                return null;
            }

            try
            {
                var jObject = JObject.Parse(json);

                var token = jObject.SelectToken(path) ??
                throw new ArgumentException($"Path '{path}' not found in JSON.");

                token.Replace(JToken.FromObject(newValue));

                var jsonStringUpdated = jObject.ToString(Formatting.None);
                Log.Information($"Updated JSON: {jsonStringUpdated}");
                return jsonStringUpdated;
            }
            catch (JsonReaderException ex)
            {
                Log.Error($"JSON parsing error: {ex.Message}");
                return null;
            }
        }

        public static string UpdateJsonNodeName(
        string json,
        string oldPath,
        string? newPath = null,
        object? newValue = null,
        bool autoCreatePath = false)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                Log.Error($"Provided JSON string {nameof(json)} is null or empty.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(oldPath))
            {
                Log.Error($"Provided Old path {nameof(oldPath)} is null or empty.");
                return null;
            }

            try
            {
                var root = JToken.Parse(json);

                // --- STEP 1: Find old node ---
                JToken? oldToken = root.SelectToken(oldPath);

                if (oldToken == null)
                    return root.ToString(Formatting.None); // Nothing to remove or update

                // --- CASE 1: newPath == null → just remove old node ---
                if (string.IsNullOrWhiteSpace(newPath))
                {
                    RemoveToken(oldToken);
                    return root.ToString(Formatting.None);
                }

                // --- STEP 2: Capture value ---
                JToken valueToUse = newValue != null
                    ? JToken.FromObject(newValue)
                    : oldToken.DeepClone();

                // --- STEP 3: Remove old node safely ---
                RemoveToken(oldToken);

                // --- STEP 4: Create or update new path ---
                CreateOrUpdatePath(root, newPath, valueToUse, autoCreatePath);

                return root.ToString(Formatting.None);
            }
            catch (JsonReaderException ex)
            {
                Log.Error($"Invalid JSON format.: {ex.Message}");
                return null;
            }
        }

        private static void RemoveToken(JToken token)
        {
            if (token.Parent is JProperty prop)
                prop.Remove();
            else if (token.Parent is JArray array)
                array.Remove(token);
            else
                token.Remove();
        }

        private static void CreateOrUpdatePath(JToken root, string path, JToken value, bool autoCreate)
        {
            var segments = path.Split('.');
            JToken current = root;

            for (int i = 0; i < segments.Length; i++)
            {
                string segment = segments[i];
                bool isLast = i == segments.Length - 1;

                // Handle array notation like items[0]
                int? arrayIndex = null;
                string propertyName = segment;

                if (segment.Contains("["))
                {
                    int start = segment.IndexOf("[");
                    int end = segment.IndexOf("]");
                    propertyName = segment[..start];
                    arrayIndex = int.Parse(segment[(start + 1)..end]);
                }

                if (current is JObject jObject)
                {
                    if (jObject[propertyName] == null && autoCreate && !isLast)
                        jObject[propertyName] = new JObject();

                    current = jObject[propertyName];
                }

                if (arrayIndex.HasValue)
                {
                    if (current is not JArray jArray)
                        throw new ArgumentException($"Path segment '{segment}' is not an array.");

                    if (arrayIndex.Value >= jArray.Count)
                    {
                        if (autoCreate)
                        {
                            while (jArray.Count <= arrayIndex.Value)
                                jArray.Add(new JObject());
                        }
                        else
                        {
                            throw new IndexOutOfRangeException($"Index {arrayIndex.Value} out of range in '{segment}'.");
                        }
                    }

                    current = jArray[arrayIndex.Value];
                }

                if (current == null)
                    throw new ArgumentException($"Path segment '{segment}' not found.");

                if (isLast)
                {
                    if (current is JObject parentObj)
                    {
                        parentObj[propertyName] = value;
                    }
                    else if (current.Parent is JObject parent)
                    {
                        parent[propertyName] = value;
                    }
                    else
                    {
                        current.Replace(value);
                    }
                }
            }
        }
    }
}
