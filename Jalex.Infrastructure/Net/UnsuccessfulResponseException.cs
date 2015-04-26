using System.Net;
using Jalex.Infrastructure.Objects;

namespace Jalex.Infrastructure.Net
{
    public class UnsuccessfulResponseException : JalexException
    {
        public HttpStatusCode StatusCode { get; private set; }
        public string Reason { get; private set; }

        public UnsuccessfulResponseException(HttpWebResponse webResponse, string content)
            : base(string.Format("{0} - {1}: {2}", webResponse.StatusCode, webResponse.StatusDescription, content))
        {
            StatusCode = webResponse.StatusCode;
            Reason = webResponse.StatusDescription;
        }
    }
}
