using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web;
using Jalex.Infrastructure.Extensions;
using Magnum.Reflection;
using Newtonsoft.Json;

namespace Jalex.Infrastructure.Net
{
    public class HttpCommunicator : IHttpCommunicator
    {
        #region Implementation of IHttpCommunicator

        /// <summary>
        /// Gets http response converted to type TRet for a given URI, with the given stream
        /// </summary>
        /// <typeparam name="TRet">The return type of the request</typeparam>
        /// <typeparam name="TParam">The type of parameters</typeparam>
        /// <param name="uri">The uri to which to send the request</param>
        /// <param name="parameters">The parameters to include in the request</param>
        /// <param name="method">The HTTP method to use</param>
        /// <param name="timeout">The timeout for the request</param>
        /// <param name="headers">headers to add to the request</param>
        /// <returns>The response retrieved from the remote server</returns>
        public TRet GetHttpResponse<TRet, TParam>(Uri uri, TParam parameters, HttpMethod method, TimeSpan timeout, NameValueCollection headers)
        {
            if (!(new[] { HttpMethod.Get, HttpMethod.Post, HttpMethod.Put, HttpMethod.Delete }.Contains(method)))
            {
                throw new NotSupportedException(string.Format("HttpMethod '{0}' is not supported.", method));
            }

            // embed stream(s) into URL into Get and Delete methods
            if (method == HttpMethod.Get || method == HttpMethod.Delete)
            {
                uri = addQueryStringParameters(uri, parameters);
            }

            return getHttpResponse<TRet>(uri, req => writeBody(parameters, req), method, timeout, headers);
        }

        /// <summary>
        /// Gets http resposne converted to type TRet for a given URI with the given stream to send to that uri
        /// </summary>
        /// <typeparam name="TRet">The return type of the request</typeparam>
        /// <param name="uri">The uri to which to send the request</param>
        /// <param name="stream">The stream to include in the request</param>
        /// <param name="method">The HTTP method to use</param>
        /// <param name="timeout">The timeout for the request</param>
        /// <param name="headers">headers to add to the request</param>
        /// <returns>The response retrieved from the remote server</returns>
        public TRet GetHttpResponseForStream<TRet>(Uri uri, Stream stream, HttpMethod method, TimeSpan timeout, NameValueCollection headers)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            if (!(new[] { HttpMethod.Post, HttpMethod.Put }.Contains(method)))
            {
                throw new NotSupportedException(string.Format("HttpMethod '{0}' is not supported.", method));
            }

            return getHttpResponse<TRet>(uri, req => writeStream(stream, req), method, timeout, headers);
        }

        #endregion

        private TRet getHttpResponse<TRet>(Uri uri, Action<HttpWebRequest> writeRequestBody, HttpMethod method, TimeSpan timeout, NameValueCollection headers)
        {           
            // setup http client
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(uri);
            webRequest.Timeout = (int)timeout.TotalMilliseconds;
            webRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            webRequest.Method = method.ToString();

            if (headers != null)
            {
                webRequest.Headers.Add(headers);
            }

            if (method == HttpMethod.Post || method == HttpMethod.Put)
            {
                writeRequestBody(webRequest);
            }

            try
            {
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    var webResponseContent = retrieveWebResponseContent(webResponse);
                    if (isSuccessfull(webResponse))
                    {
                        return webResponseContent.FromJson<TRet>();
                    }

                    throw new UnsuccessfulResponseException(webResponse, webResponseContent);
                }
            }
            catch (WebException ex)
            {
                var webResponse = ex.Response as HttpWebResponse;
                var webResponseContent = retrieveWebResponseContent(webResponse);
                throw new UnsuccessfulResponseException(webResponse, webResponseContent);
            }
        }

        private static Uri addQueryStringParameters<T>(Uri baseUri, T parameters)
        {
            UriBuilder builder = new UriBuilder(baseUri);

            // no change if no parameters
            if (parameters == null)
            {
                return builder.Uri;
            }

            Type type = typeof(T);
            if (type.IsSubclassOf(typeof(ValueType)) || type == typeof(string) || type == typeof(DateTime))
            {
                builder.Path += "/" + HttpUtility.UrlEncode(parameters is DateTime ? ((DateTime)(object)parameters).ToString("O") : parameters.ToString());
                return builder.Uri;
            }

            var members = type.IsAnonymousType()
                              ? type.GetFields()
                                    .Select(f => new
                                    {
                                        f.Name,
                                        Value = f.GetValue(parameters),
                                        JsonProperty = (JsonPropertyAttribute)null,
                                        JsonConverter = (JsonConverterAttribute)null
                                    })
                              : type.GetProperties()
                                    .Select(p => new
                                    {
                                        p.Name,
                                        Value = p.GetValue(parameters, null),
                                        JsonProperty = p.GetCustomAttribute<JsonPropertyAttribute>(),
                                        JsonConverter = p.GetCustomAttribute<JsonConverterAttribute>()
                                    });

            // get all the properties
            IEnumerable<string> properties = from p in members
                                             let name = p.JsonProperty != null ? p.JsonProperty.PropertyName ?? p.Name : p.Name
                                             let val = p.Value
                                             where val != null
                                             select
                                                 (val is IEnumerable && !(val is string))
                                                     ? getEnumerableQueryString((IEnumerable)val, name, p.JsonConverter)
                                                     : name + "=" + HttpUtility.UrlEncode(getValueAsString(val, p.JsonConverter));
            builder.Query = string.Join("&", properties.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray());

            return builder.Uri;
        }

        private static string getValueAsString(object value, JsonConverterAttribute jsonConverter)
        {
            if (jsonConverter != null && jsonConverter.ConverterType != null)
            {
                var converterInstance = (JsonConverter) FastActivator.Create(jsonConverter.ConverterType);

                if (converterInstance.CanWrite)
                {
                    using (var stringWriter = new StringWriter())
                    using (var jsonWriter = new JsonTextWriter(stringWriter))
                    {
                        converterInstance.WriteJson(jsonWriter, value, JsonSerializer.CreateDefault());
                        return stringWriter.ToString();
                    }
                }
            }

            return value is DateTime ? ((DateTime) value).ToString("O") : value.ToString();
        }

        private static string getEnumerableQueryString(IEnumerable collection, string collectionName, JsonConverterAttribute jsonConverter)
        {
            List<string> q = new List<string>();
            int i = 0;

            foreach (object val in collection)
            {
                if (val == null)
                {
                    continue;
                }

                Type valType = val.GetType();

                if (valType.IsPrimitive || valType == typeof(string) || valType == typeof(DateTime) || valType.IsEnum)
                {
                    q.Add(string.Format("{0}[{1}]={2}", collectionName, i, HttpUtility.UrlEncode(getValueAsString(val, jsonConverter))));
                }
                else
                {
                    object val1 = val;
                    var i1 = i;
                    IEnumerable<string> properties = from p in valType.GetProperties()
                                                     let temp = p.GetValue(val1, null)
                                                     where temp != null
                                                     let finalVal = (temp is IEnumerable && !(temp is string)) ? getEnumerableQueryString((IEnumerable)temp, p.Name, null) : val1
                                                     select string.Format("{0}[{1}].{2}={3}", collectionName, i1, p.Name, HttpUtility.UrlEncode(finalVal.ToString()));
                    q.Add(string.Join("&", properties.ToArray()));
                }

                i++;
            }

            return string.Join("&", q.ToArray());
        }

        private static void writeStream(Stream stream, HttpWebRequest webRequest)
        {
            string boundary = string.Format("{0}//{1}", Guid.NewGuid(), DateTime.Now.Ticks);
            webRequest.ContentType = string.Format("multipart/form-data; boundary=\"{0}\"", boundary);
            using (Stream remoteStream = webRequest.GetRequestStream())
            using (StreamWriter writer = new StreamWriter(remoteStream))
            {
                // *** multipart/form-data format ***
                // --boundary\r\n
                // header: value\r\n
                // \r\n
                // stream data\r\n
                // --boundary--\r\n
                writer.WriteLine(string.Join(string.Empty, "--", boundary));
                writer.WriteLine("Content-Disposition: attachment");
                writer.WriteLine("Content-Type: application/octet-stream");
                writer.WriteLine();
                writer.Flush();
                stream.CopyTo(remoteStream);
                writer.WriteLine();
                writer.WriteLine(string.Join(string.Empty, "--", boundary, "--"));
            }
        }

        private static void writeBody<T>(T parameters, HttpWebRequest webRequest)
        {
            string contentString = parameters == null ? string.Empty : parameters.ToJson();
            byte[] bytes = Encoding.UTF8.GetBytes(contentString);
            webRequest.ContentType = "application/json";
            webRequest.ContentLength = bytes.Length;
            using (Stream remoteStream = webRequest.GetRequestStream())
            {
                remoteStream.Write(bytes, 0, bytes.Length);
            }
        }

        private static bool isSuccessfull(HttpWebResponse webResponse)
        {
            return webResponse.StatusCode >= HttpStatusCode.OK &&
                webResponse.StatusCode <= (HttpStatusCode)299;
        }

        private string retrieveWebResponseContent(HttpWebResponse webResponse)
        {
            if (webResponse == null)
            {
                return null;
            }

            Stream stream = webResponse.GetResponseStream();
            if (stream == null)
            {
                return null;
            }

            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
