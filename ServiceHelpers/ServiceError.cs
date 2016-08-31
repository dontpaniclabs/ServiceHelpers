using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DontPanic.Helpers
{
    [Serializable]
    [DataContract]
    public class ServiceError
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string ExceptionType { get; set; }

        [DataMember]
        public string ExceptionMessage { get; set; }

        [DataMember]
        public string ExceptionDetail { get; set; }
    }
}
