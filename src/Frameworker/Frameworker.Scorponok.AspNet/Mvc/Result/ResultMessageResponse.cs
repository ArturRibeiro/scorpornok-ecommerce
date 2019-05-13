using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Frameworker.Scorponok.AspNet.Mvc.Result
{
    [DataContract]
    public struct ResultMessageResponse<TResult>
    {
        [DataMember(Name = "errors")]
        public string[] Errors { get; set; }

        [DataMember(Name = "data")]
        public TResult Data { get; }

        [DataMember(Name = "success")]
        public bool Success => !(Errors?.Length > 0);

        public ResultMessageResponse(TResult data, string[] errors)
        {
            this.Data = data;
            this.Errors = errors;
        }

        public static implicit operator ResultMessageResponse<TResult>(TResult value) => new ResultMessageResponse<TResult>(value, null);


    }
}
