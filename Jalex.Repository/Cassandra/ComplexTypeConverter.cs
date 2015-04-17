using System;
using Cassandra.Mapping.TypeConversion;
using Newtonsoft.Json;

namespace Jalex.Repository.Cassandra
{
    public class ComplexTypeConverter : TypeConverter
    {
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
            if (typeof (TDatabase) != typeof (string))
            {
                return null;
            }
            return dbObj => JsonConvert.DeserializeObject<TPoco>(dbObj as string);
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
            if (typeof (TDatabase) != typeof (string))
            {
                return null;
            }
            return obj => (TDatabase)(object)JsonConvert.SerializeObject(obj);
        }

        #endregion
    }
}
