using System;
using System.Xml;
using System.Linq;

namespace datacontract
{
    class XmlDocumentVisitor
    {
        public class XmlElementEventArgs : EventArgs
        {
            public XmlElement element { get; private set; }
            public XmlElementEventArgs(XmlElement element)
            {
                this.element = element;
            }
        }

        public delegate void XmlElementEventHandler(object sender, XmlElementEventArgs e);


        public event XmlElementEventHandler BeginVisitElement;
        public event XmlElementEventHandler EndVisitElement;

        public event XmlElementEventHandler Indent;
        public event XmlElementEventHandler Dedent;

        public void Visit(XmlDocument doc)
        {
            var currentNode = doc.DocumentElement;
            VisitElement(currentNode);
        }

        private void VisitElement(XmlElement element)
        {
            if (this.BeginVisitElement != null)
                this.BeginVisitElement(this, new XmlElementEventArgs(element));

            if (this.Indent != null)
                this.Indent(this, new XmlElementEventArgs(element));

            foreach (var child in element.ChildNodes.OfType<XmlElement>())
            {
                VisitElement(child);
            }

            if (this.Dedent != null)
                this.Dedent(this, new XmlElementEventArgs(element));

            if (this.EndVisitElement != null)
                this.EndVisitElement(this, new XmlElementEventArgs(element));
        }
    }
}