using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace org.random.JSONRPC
{
    [Serializable]
    class RandomJSONRPCException : Exception
    {
        public RandomJSONRPCException(string message)
            : base(message) { }

        public RandomJSONRPCException(string message, Exception innerException)
            : base(message, innerException) { }
     }

    [Serializable]
    class RandomJSONRPCRunTimeException : Exception
    {
        public RandomJSONRPCRunTimeException(string message)
            : base(message) { }

        public RandomJSONRPCRunTimeException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}