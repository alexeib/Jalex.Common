using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Jalex.Infrastructure.Extensions
{
    public static class JsonExtensions
    {
        #region Static Fields

        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            NullValueHandling = NullValueHandling.Ignore,
            FloatFormatHandling = FloatFormatHandling.Symbol,
            DefaultValueHandling = DefaultValueHandling.Include,
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter(),
                new IsoDateTimeConverter {DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffK"}
            }
        };
        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Deserializes a given json string to an object of type T
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="jsonString">The json string</param>
        /// <returns>Object deserialized from json string</returns>
        public static T FromJson<T>(this string jsonString)
        {
            if (jsonString == null)
            {
                return default(T);
            }

            if (typeof (T) == typeof (object))
            {
                return (dynamic)JsonConvert.DeserializeObject(jsonString, SerializerSettings);
            }

            return JsonConvert.DeserializeObject<T>(jsonString, SerializerSettings);
        }

        /// <summary>
        /// Deserializes a given json string to an object of type T
        /// </summary>
        /// <param name="jsonString">The json string</param>
        /// <param name="type">The type to deserialize to</param>
        /// <returns>Object deserialized from json string</returns>
        public static object FromJson(this string jsonString, Type type)
        {
            object @object = JsonConvert.DeserializeObject(jsonString, type, SerializerSettings);
            return @object;
        }

        /// <summary>
        /// Serializes the given object to Json
        /// </summary>
        /// <param name="obj">The object to serialize</param>
        /// <returns>Json representation of the object</returns>
        public static string ToJson(this object obj)
        {
            string json = JsonConvert.SerializeObject(obj, SerializerSettings);
            return json;
        }

        #endregion
    }
}
