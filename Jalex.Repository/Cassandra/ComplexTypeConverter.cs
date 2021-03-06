﻿using System;
using System.Collections.Generic;
using Cassandra.Mapping.TypeConversion;
using Jalex.Infrastructure.Extensions;
using Jalex.Infrastructure.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Jalex.Repository.Cassandra
{
    public class ComplexTypeConverter : TypeConverter
    {
        private readonly JsonMissingTypeObjectRemover _jsonMissingTypeObjectRemover = new JsonMissingTypeObjectRemover();

        private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
                                                                             {
                                                                                 DateFormatHandling = DateFormatHandling.IsoDateFormat,
                                                                                 MissingMemberHandling = MissingMemberHandling.Ignore,
                                                                                 NullValueHandling = NullValueHandling.Ignore,
                                                                                 DefaultValueHandling = DefaultValueHandling.Ignore,
                                                                                 TypeNameHandling = TypeNameHandling.Auto,
                                                                                 Converters = new List<JsonConverter>
                                                                                              {
                                                                                                  new StringEnumConverter()
                                                                                              }
                                                                             };
        #region Overrides of TypeConverter

        /// <summary>
        /// Gets any user defined conversion functions that can convert a value of type <typeparamref name="TDatabase"/> (coming from Cassandra) to a
        ///             type of <typeparamref name="TPoco"/> (a field or property on a POCO).  Return null if no conversion Func is available.
        /// </summary>
        /// <typeparam name="TDatabase">The Type of the source value from Cassandra to be converted.</typeparam><typeparam name="TPoco">The Type of the destination value on the POCO.</typeparam>
        /// <returns>
        /// A Func that can convert between the two types or null if one is not available.
        /// </returns>
        protected override Func<TDatabase, TPoco> GetUserDefinedFromDbConverter<TDatabase, TPoco>()
        {
            if (typeof(TDatabase) != typeof(string))
            {
                return null;
            }

            if (typeof (TPoco).GetNullableUnderlyingType().IsEnum)
            {
                return dbObj => dbObj == null ? default(TPoco) : (TPoco)Enum.Parse(typeof(TPoco), Convert.ToString(dbObj));
            }

            return dbObj =>
                   {
                       var objStr = _jsonMissingTypeObjectRemover.RemoveMissingTypesFromJsonString(dbObj as string);
                       return JsonConvert.DeserializeObject<ComplexTypeContainer<TPoco>>(objStr, _serializerSettings)
                                         .Object;
                   };
        }

        /// <summary>
        /// Gets any user defined conversion functions that can convert a value of type <typeparamref name="TPoco"/> (coming from a property/field on a
        ///             POCO) to a type of <typeparamref name="TDatabase"/> (the Type expected by Cassandra for the database column).  Return null if no conversion
        ///             Func is available.
        /// </summary>
        /// <typeparam name="TPoco">The Type of the source value from the POCO property/field to be converted.</typeparam><typeparam name="TDatabase">The Type expected by C* for the database column.</typeparam>
        /// <returns>
        /// A Func that can converter between the two Types or null if one is not available.
        /// </returns>
        protected override Func<TPoco, TDatabase> GetUserDefinedToDbConverter<TPoco, TDatabase>()
        {
            if (typeof(TDatabase) != typeof(string))
            {
                return null;
            }

            if (typeof(TPoco).GetNullableUnderlyingType().IsEnum)
            {
                return obj => Equals(obj, default(TPoco)) ? default(TDatabase) : (TDatabase)(object)obj.ToString();
            }

            return obj => (TDatabase) (object) JsonConvert.SerializeObject(new ComplexTypeContainer<TPoco>
                                                                           {
                                                                               Object = obj
                                                                           },
                                                                           _serializerSettings);
        }

        #endregion        
    }
}
