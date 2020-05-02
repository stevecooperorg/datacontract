using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace datacontract
{
    [DataContract(Namespace = "INNER")]
    [XmlRoot(Namespace = "INNER")]
    public class InnerType
    {
        [DataMember]
        public String Value1;
        [DataMember]
        public String Value2;
    }
}