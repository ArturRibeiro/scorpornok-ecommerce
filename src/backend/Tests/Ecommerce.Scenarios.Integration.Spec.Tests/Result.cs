using System.Runtime.Serialization;

namespace Ecommerce.Scenarios.Integration.Spec.Tests
{
    public class Result<T>
    {
        [DataMember(Name = "data")]
        public T Data { get; set; }
        
        [DataMember(Name = "success")]
        public bool Success { get; set; }
    }
}