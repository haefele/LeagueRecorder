using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LeagueRecorder.Shared.Abstractions.Results
{
    [Serializable]
    public class ResultException : Exception
    {
        public ResultException()
        {
            this.AdditionalData = new Dictionary<string, object>();
        }

        public ResultException(string message)
            : base(message)
        {
            this.AdditionalData = new Dictionary<string, object>();
        }

        public ResultException(string message, Exception inner)
            : base(message, inner)
        {
            this.AdditionalData = new Dictionary<string, object>();
        }

        protected ResultException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.AdditionalData = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets the additional data.
        /// </summary>
        public Dictionary<string, object> AdditionalData { get; internal set; }
    }
}