using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using System.IO;

namespace datacontract
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void DataContractProducesXml()
        {
            var testObject = new EventResult {
                Result = new OperationResult {
                    Value1="foo",
                    Value2="bar"
                }
            };

            var expected = File.ReadAllText("./expected.xml").Trim();
            var actual = Serialize(testObject);
            Assert.AreEqual(expected, actual);
        }

        private String Serialize(EventResult eventResult) {
            var memStream = new MemoryStream();
            //var streamWriter = new StreamWriter(memStream);
            var ser = new DataContractSerializer(typeof(EventResult));
            var xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;
            var xmlWriter = XmlWriter.Create(memStream, xmlSettings);
            ser.WriteObject(xmlWriter, eventResult);
            xmlWriter.Flush();
            memStream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(memStream);
            var result = reader.ReadToEnd();
            return result.Trim();
        }
    }

    [DataContract(Name="EventResult", Namespace="RootNamespace")]
    class EventResult {
        [DataMember]
        public OperationResult Result;
    }

    [DataContract(Name="OperationResult", Namespace="OperationResultNamespace")]
    class OperationResult {
        [DataMember]
        public String Value1;
        [DataMember]
        public String Value2;
    }
}
