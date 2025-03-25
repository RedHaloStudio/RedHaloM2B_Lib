using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace RedHaloM2B.Textures
{
    public class Property<T> //: BaseTex
    {
        public string Key {  get; set; }

        public T Value { get; set; }

        public Property(string key, T value)
        {
            Key = key ?? throw (new ArgumentException(nameof(key)));
            Value = value ?? throw (new ArgumentException(nameof(value)));
        }
    }

    public class PropertyManager
    {
        private static Dictionary<string, object> properties = new Dictionary<string, object>();

        public static void AddPropetry<T>(Property<T> property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            if (!properties.ContainsKey(property.Key))
            {
                properties.Add(property.Key, property);
            }
            else
            {
                properties[property.Key] = property;
            }
        }

        public static T GetProperty<T>(string key)
        {
            if (properties.ContainsKey(key))
            {
                var property = (Property<T>)properties[key];
                return property.Value;
            }
            throw new ArgumentException($"Property width key '{key}' not found");
        }
    }
}