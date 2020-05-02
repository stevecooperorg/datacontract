using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Linq;

namespace datacontract
{   
    [DataContract(Name = "EventResult", Namespace = "ROOT")]
    [XmlRoot(Namespace = "ROOT")]
    public class EventResult
    {
        [DataMember]
        [XmlElement(Namespace = "OPER")]
        public OperationResult Result;
    }

    [DataContract(Name = "OperationResult", Namespace = "OPER")]
    [XmlRoot(Namespace = "OPER")]
    public class OperationResult
    {

        [DataMember]
        public String Value1;
        [DataMember]
        public String Value2;
    }

    [TestClass]
    public class UnitTest1
    {
        const string expectedFile = "./expected2.xml";

        [AssemblyInitialize]
        public static void Init(TestContext ctx)
        {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
        }

        [TestMethod]
        public void DataContractProducesXml()
        {
            UseSerializer<EventResult> useDataContract = (xmlWriter, objectGraph) =>
                new DataContractSerializer(typeof(EventResult)).WriteObject(xmlWriter, objectGraph);
            AssertSerializerProducesExpectedXmlStructure(useDataContract);
        }

        [TestMethod]
        public void XmlSerializerProducesXml()
        {
            UseSerializer<EventResult> useXmlSerializer = (xmlWriter, objectGraph) =>
                new XmlSerializer(typeof(EventResult)).Serialize(xmlWriter, objectGraph);
            AssertSerializerProducesExpectedXmlStructure(useXmlSerializer);
        }
         public delegate void UseSerializer<T>(XmlWriter writer, T objectGraph);

        private void AssertSerializerProducesExpectedXmlStructure(UseSerializer<EventResult> serializer) {
            // create a test object;
            var testObject = new EventResult
            {
                Result = new OperationResult
                {
                    Value1 = "foo",
                    Value2 = "bar"
                }
            };

            // get our expected result from a text file;
            var expected = File.ReadAllText(expectedFile).Trim();

            // get our actual by using a serialiser on the test object;
            var actual = Serialize(testObject, serializer);

            // make sure the two structures are comparable;
            expected = PrintStructureOfXml(expected);
            actual = PrintStructureOfXml(actual);
            Assert.AreEqual(expected, actual);
        }

       
        private String Serialize<T>(T objectGraph, UseSerializer<T> useSerialiser)
        {
            var memStream = new MemoryStream();
            var xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;
            var xmlWriter = XmlWriter.Create(memStream, xmlSettings);
            useSerialiser(xmlWriter, objectGraph);

            xmlWriter.Flush();
            memStream.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(memStream);
            var result = reader.ReadToEnd();
            return result.Trim();
        }

        string PrintStructureOfXml(string doc)
        {
            var xdoc = new XmlDocument();
            xdoc.LoadXml(doc);

            var sb = new System.Text.StringBuilder();
            var indent = 0;
            sb.AppendLine();

            var visitor = new XmlDocumentVisitor();
            visitor.BeginVisitElement += (sender, args) =>
            {
                sb.Append(new String(' ', indent * 2));
                sb.AppendLine($"{args.element.NamespaceURI}:{args.element.LocalName}");
            };

            visitor.Indent += (sender, args) =>
            {
                indent++;
            };

            visitor.Dedent += (sender, args) =>
            {
                indent--;
            };

            visitor.Visit(xdoc);
            sb.AppendLine();

            return sb.ToString();
        }
    }
}
