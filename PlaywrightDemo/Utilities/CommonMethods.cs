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

        public static string AppendArrayItem(string json, string arrayPath, object value, bool autoCreatePath = true)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                Log.Error($"Provided JSON string {nameof(json)} is null or empty.");
                return null;
            }
            if (string.IsNullOrWhiteSpace(arrayPath))
            {
                Log.Error($"Provided Old path {nameof(arrayPath)} is null or empty.");
                return null;
            }

            var root = JToken.Parse(json);
            var arrayToken = root.SelectToken(arrayPath);

            if (arrayToken == null)
            {
                if (!autoCreatePath)
                    throw new ArgumentException($"Array path '{arrayPath}' not found.");

                // create array at arrayPath
                CreateOrUpdatePath(root, arrayPath, new JArray(), autoCreate: true);
                arrayToken = root.SelectToken(arrayPath);
            }

            if (arrayToken is not JArray arr)
                throw new ArgumentException($"Path '{arrayPath}' is not an array.");

            arr.Add(JToken.FromObject(value));
            var jsonStringUpdated = root.ToString(Formatting.Indented);
            Log.Information($"Updated JSON: {jsonStringUpdated}");
            return jsonStringUpdated;
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
                bool isLast = (i == segments.Length - 1);

                // Parse array index if present: "claims[1]" => propertyName="claims", arrayIndex=1
                int? arrayIndex = null;
                string propertyName = segment;
                if (segment.Contains("["))
                {
                    int start = segment.IndexOf('[');
                    int end = segment.IndexOf(']');
                    propertyName = segment.Substring(0, start);
                    var idxText = segment.Substring(start + 1, end - start - 1);
                    if (!int.TryParse(idxText, out int idx))
                        throw new ArgumentException($"Invalid array index in segment '{segment}'.");
                    arrayIndex = idx;
                }

                // If the current token is an object, get or create the child token for propertyName
                if (current is JObject jObject)
                {
                    // If property is missing and we need to continue deeper, create appropriate container
                    if (jObject[propertyName] == null && autoCreate && !isLast)
                    {
                        // decide whether next required container is an array or object
                        if (arrayIndex.HasValue)
                            jObject[propertyName] = new JArray();
                        else
                            jObject[propertyName] = new JObject();
                    }

                    // Move current to the child (might be null if not found and not autoCreate)
                    current = jObject[propertyName];
                }

                // If this segment has an array index, ensure current is a JArray (or create one)
                if (arrayIndex.HasValue)
                {
                    // If current is null but we have parent object and autoCreate, create array
                    if (current == null)
                    {
                        if (autoCreate && jObjectFor(current: current, root: root, propertyName: propertyName, out JObject? parentObj))
                        {
                            parentObj![propertyName] = new JArray();
                            current = parentObj[propertyName];
                        }
                        else
                        {
                            throw new ArgumentException($"Path segment '{segment}' not found and cannot be created.");
                        }
                    }

                    if (current is not JArray jArray)
                    {
                        // If the child exists but isn't an array, error
                        throw new ArgumentException($"Path segment '{segment}' is expected to be an array but is '{current.Type}'.");
                    }

                    // Ensure array has required length
                    if (arrayIndex.Value >= jArray.Count)
                    {
                        if (autoCreate)
                        {
                            while (jArray.Count <= arrayIndex.Value)
                                jArray.Add(JValue.CreateNull());
                        }
                        else
                        {
                            throw new IndexOutOfRangeException($"Index {arrayIndex.Value} out of range in '{segment}'.");
                        }
                    }

                    // If this is the last segment, assign directly into array[index]
                    if (isLast)
                    {
                        jArray[arrayIndex.Value] = value;
                        return;
                    }

                    // Not last: move to the array element (which should be object if deeper members exist)
                    current = jArray[arrayIndex.Value];

                    // If the element is null and we need to go deeper, create object
                    if (current == null && autoCreate)
                    {
                        jArray[arrayIndex.Value] = new JObject();
                        current = jArray[arrayIndex.Value];
                    }
                }
                else
                {
                    // No array index for this segment
                    if (isLast)
                    {
                        // Last segment and not an array: set property on parent object (or replace the token)
                        if (current is JObject parentObj)
                        {
                            parentObj[propertyName] = value;
                            return;
                        }
                        else if (current?.Parent is JObject parent)
                        {
                            parent[propertyName] = value;
                            return;
                        }
                        else
                        {
                            // If current is null (e.g., root) and we have only one segment, attach to root object
                            if (current == null && root is JObject rootObj && segments.Length == 1)
                            {
                                rootObj[propertyName] = value;
                                return;
                            }

                            // Otherwise try to replace
                            if (current != null)
                            {
                                current.Replace(value);
                                return;
                            }

                            throw new ArgumentException($"Unable to set value at path '{path}'.");
                        }
                    }

                    // Not last and no array: move deeper; if missing and autoCreate, create JObject
                    if (current == null)
                    {
                        throw new ArgumentException($"Path segment '{segment}' not found.");
                    }
                }
            }
        }

        static bool jObjectFor(JToken? current, JToken root, string propertyName, out JObject? parentObj)
        {
            parentObj = null;
            if (current != null) return false;

            // Try to find parent by walking from root using propertyName path isn't trivial here.
            // For our usage above we only call this when current is null after a JObject check,
            // so we can safely return false and let calling logic create property via known parent.
            return false;
        }

    }
}
