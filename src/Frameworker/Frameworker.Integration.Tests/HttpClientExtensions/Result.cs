using System.Runtime.Serialization;

namespace Frameworker.Integration.Tests.HttpClientExtensions
{
    public class Result<T>
    {
        [DataMember(Name = "data")]
        public T Data { get; set; }
        
        [DataMember(Name = "success")]
        public bool Success { get; set; }
    }
}