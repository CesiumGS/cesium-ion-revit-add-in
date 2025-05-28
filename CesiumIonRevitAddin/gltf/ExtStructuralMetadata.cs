using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CesiumIonRevitAddin.Gltf
{
    // appended to nodes
    internal class ExtStructuralMetadata
    {
        [JsonProperty("class")]
        public String Class;

        [JsonProperty("properties")]
        Dictionary<String, ParameterValue> properties;

        // Adds a property to the properties dictionary. Will append a number if the property already exists.
        public string AddProperty(String key, ParameterValue parameterValue)
        {
            properties = properties ?? new Dictionary<String, ParameterValue>();
            string addedKey = key;

            if (properties.ContainsKey(addedKey))
            {
                int i = 1;
                while (properties.ContainsKey(addedKey + i))
                {
                    i++;
                }
                addedKey = key + i;
            }
            properties.Add(addedKey, parameterValue);
            Logger.Instance.Log("Parameter " + key + " had a name collision, is now " + addedKey);
            return addedKey;
        }

        public bool HasProperty(String key)
        {
            if (properties == null) return false;

            return properties.ContainsKey(key);
        }

        public ParameterValue? GetPropertyValue(String propertyname)
        {
            if (properties == null) return null;

            if (properties.TryGetValue(propertyname, out ParameterValue value))
            {
                return value;
            }
            return null;
        }
    }
}
