using System;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

namespace datacontract
{

    [DataContract(Namespace = "OUTER")]
    [XmlRoot(Namespace = "OUTER")]
    public class OuterType
    {
        [DataMember]
        [XmlElement(Namespace = "INNER")]
        public InnerType Result;
    }
}
