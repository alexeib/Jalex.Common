using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Jalex.Infrastructure.Attributes;
using Jalex.Infrastructure.Extensions;
using Jalex.Infrastructure.Objects;
using Jalex.Infrastructure.Utils;
using Jalex.Logging;
using Amazon.DynamoDBv2.DocumentModel;

namespace Jalex.Repository.DynamoDB
{
    public class DynamoDBRepository<T> : IRepository<T> where T: new()
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private static string idPropertyName;
        private static PropertyInfo metaProperty;
        private static PropertyInfo[] props;
        private static List<Type> ignoreAttr = new List<Type>();
        private static Dynamo dynamo;
        static DynamoDBRepository()
        {
            props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            PropertyInfo idprop = props.Where(p => System.Attribute.IsDefined(p, typeof(IdAttribute))).FirstOrDefault();
            if (idprop == null)
                throw new RepositoryException("DynamoDB: Id property not defined");
            idPropertyName = idprop.Name;

            metaProperty = props.Where(p => p.PropertyType.Equals(typeof(Metadata))).FirstOrDefault();
            if (metaProperty == null)
                throw new RepositoryException("DynamoDB: Metadata property not defined");

            ignoreAttr.Add(typeof(IgnoreAttribute));

            dynamo = new Dynamo();
            dynamo.Init(idPropertyName);
        }
        public static void IgnoreAttributes(List<Type> attr)
        {
            if (attr.All(a => typeof(Attribute).IsAssignableFrom(a)))
                ignoreAttr.AddRange(attr);
        }
        protected void FromDynamo(ref T person, IDictionary<string, DynamoDBEntry> dict)
        {
            Object objmeta = metaProperty.GetValue(person);
            Metadata meta = objmeta == null ? null : (Metadata)objmeta;
            List<string> niprofiles = new List<string>();
            if (dict.ContainsKey("createTimeStamp"))
            {
                if (objmeta == null) 
                {
                    meta = new Metadata();
                    metaProperty.SetValue(person, meta);
                }
                meta.created = dict["createTimeStamp"].AsPrimitive().Value.ToString();
            }
            if (dict.ContainsKey("modifyTimeStamp"))
            {
                if (objmeta == null) 
                {
                    meta = new Metadata();
                    metaProperty.SetValue(person, meta);
                }
                meta.lastModified = dict["modifyTimeStamp"].AsPrimitive().Value.ToString();
            }
            if (dict.ContainsKey("NIProfiles"))
            {
                niprofiles = dict["NIProfiles"].AsListOfString();
            }
            foreach (var prop in props)
            {
                string attrName = prop.Name;
                if (!dict.ContainsKey(attrName)) continue;
                if (niprofiles.Contains(attrName))
                    prop.SetValue(person, dict[attrName].AsString().FromJson(prop.PropertyType));
                else if (prop.PropertyType.IsArray)
                {
                    List<Primitive> vals = dict[attrName].AsListOfPrimitives();
                    prop.SetValue(person, vals.Select(v => v.Value.ToString()).ToArray());
                }
                else if (prop.PropertyType.IsEnum)
                    prop.SetValue(person, Enum.Parse(prop.PropertyType, dict[attrName].AsPrimitive().Value.ToString(), true));
                else
                    prop.SetValue(person, Convert.ChangeType(dict[attrName].AsPrimitive().Value, prop.PropertyType));
            }
        }
        protected Dictionary<string, DynamoDBEntry> ToDynamo(T person)
        {
            Dictionary<string, DynamoDBEntry> dict = new Dictionary<string, DynamoDBEntry>();
            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            List<string> nielements = new List<string>();

            foreach (var prop in props)
            {
                if (ignoreAttr.Any(a => System.Attribute.IsDefined(prop, a)) || prop.GetValue(person) == null) continue;

                string attrName = prop.Name;
                DynamoDBEntry element;
                if (System.Attribute.IsDefined(prop, typeof(NIProfileAttribute)))
                {
                    object o = prop.GetValue(person);
                    element = o.ToJson();
                    nielements.Add(attrName);
                }
                else if (prop.PropertyType.IsArray)
                {
                    if (prop.PropertyType.GetElementType().Equals(typeof(byte)))
                        element = (byte[])prop.GetValue(person);
                    else
                    {
                        object[] vals = (object[])prop.GetValue(person);
                        element = vals.Select(v => v.ToString()).ToList();
                    }
                }
                else
                {
                    object o = prop.GetValue(person);
                    TypeCode tyc = Type.GetTypeCode(prop.PropertyType);
                    if (tyc == TypeCode.Double || tyc == TypeCode.Single)
                        element = (double)Convert.ChangeType(o, typeof(double));
                    else if (IsNumber(prop.PropertyType))
                        element = (decimal)Convert.ChangeType(o, typeof(decimal));
                    else
                        element = o.ToString();
                }
                dict.Add(attrName, element);
            }

            if (nielements.Count > 0)
                dict.Add("NIProfiles", nielements);

            return dict;
        }
        IEnumerable<T> IReader<T>.GetByIds(IEnumerable<string> ids)
        {
            var results = dynamo.BatchGetPerson(ids);
            List<T> list = new List<T>();
            foreach (var res in results)
            {
                T person = new T();
                FromDynamo(ref person, res);
                list.Add(person);
            }
            return list;
        }

        IEnumerable<T> IReader<T>.Query(Func<T, bool> query)
        {
            var results = dynamo.ScanPerson();
            List<T> list = new List<T>();
            foreach (var res in results)
            {
                T person = new T();
                FromDynamo(ref person, res);
                list.Add(person);
            }
            return list;
        }

        IEnumerable<OperationResult> IDeleter<T>.Delete(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        OperationResult IUpdater<T>.Update(T objectToUpdate)
        {
            OperationResult<string> result;
            Dictionary<string, DynamoDBEntry> dict = ToDynamo(objectToUpdate);
            string key = string.Empty;
            try
            {
                key = dict[idPropertyName].AsString();
                dynamo.Update(dict);
                result = new OperationResult<string>(true, key);
            }
            catch (Exception ex)
            {
                _logger.Error("DynamoDB Insert failed. key: {0}. Message: {1}", key, ex.Message);
                result = new OperationResult<string>(false, key);
            }
            return result;
        }

        IEnumerable<OperationResult<string>> IInserter<T>.Create(IEnumerable<T> newObjects)
        {
            List<OperationResult<string>> result = new List<OperationResult<string>>();
            foreach (var person in newObjects)
            { 
                Dictionary<string, DynamoDBEntry> dict = ToDynamo(person);
                string key = string.Empty;
                try
                {
                    key = dict[idPropertyName].AsString();
                    dynamo.InsertPerson(dict);
                    result.Add(new OperationResult<string>(true, key));
                }
                catch (Exception ex)
                {
                    _logger.Error("DynamoDB Insert failed. key: {0}. Message: {1}", key, ex.Message);
                    result.Add(new OperationResult<string>(false, key));
                }
            }
            return result;
        }

        private static bool IsNumber(Type ty)
        {
            switch (Type.GetTypeCode(ty))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }
    }
}
