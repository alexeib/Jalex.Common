using Newtonsoft.Json;

namespace Jalex.Repository.Cassandra
{
    public class ComplexTypeContainer<TObj>
    {
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto, PropertyName = "o")]
        public TObj Object { get; set; }
    }
}
