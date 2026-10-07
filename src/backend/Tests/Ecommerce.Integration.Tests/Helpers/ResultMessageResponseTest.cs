using System.Runtime.Serialization;

namespace Ecommerce.Integration.Tests.Helpers
{
    [DataContract]
    public struct ResultMessageResponseTest<TResult>
    {
        [DataMember]
        public string[] Errors { get; set; }

        [DataMember]
        public TResult Data { get; set; }

        [DataMember]
        public bool Success { get; set; }

    }
}
