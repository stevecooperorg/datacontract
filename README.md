A little exploratory codde to see if I can solve a problem in XML serialization.

The core problem is this. We have two classes which nest and must be serialised;

```
    public class OuterType
    {
        public InnerType Result;
    }

    public class InnerType
    {
        public String Value1;
        public String Value2;
    }
```

When we serialize, we want `OuterType` to be namespaced in one namespace (let's say `xmlns="OUTER"`), and `InnerType` and it's children to be namespaced differently; so `xmlns="INNER"`. Structurally,

```
OUTER:OuterType
  INNER:Result
    INNER:Value1
    INNER:Value2
```

And in XML;

```
<?xml version="1.0" encoding="utf-8"?>
<OuterType xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns="OUTER">
  <Result xmlns="INNER">
    <Value1>foo</Value1>
    <Value2>bar</Value2>
  </Result>
</OuterType>
```

However, this was tricky! The code we started with used [`DataContractSerializer`](https://docs.microsoft.com/en-us/dotnet/framework/wcf/feature-details/data-contract-serializer), and it was proving hard to make it do the right thing - instead we could only make it do this;

```
OUTER:OuterType
  OUTER:Result
    INNER:Value1
    INNER:Value2
```

Note the second line has the `<Result>` element in the `OUTER` namespace rather than the required `INNER`. There seems to be no way to use the `[DataContract]` and `[DataMember]` attributes to give the intended result.
This has been fixed by switching to [`XmlSerializer`](https://docs.microsoft.com/en-us/dotnet/api/system.xml.serialization.xmlserializer?view=netcore-3.1) as [recommended by Microsoft](#Microsoft-Advises-XmlSerializer).

# Microsoft Advises XmlSerializer

From [Microsoft's own documents](https://docs.microsoft.com/en-us/dotnet/framework/wcf/feature-details/using-the-xmlserializer-class#when-to-use-the-xmlserializer-class):

> At times, you may have to manually switch to the XmlSerializer. This happens, for example, in the following cases:
> * When migrating an application from ASP.NET Web services to WCF, you may want to reuse existing, XmlSerializer-compatible types instead of creating new data contract types.
> * When precise control over the XML that appears in messages is important, but a Web Services Description Language (WSDL) document is not available, for example, when creating a service with types that have to comply to a certain standardized, published schema that is not compatible with the DataContractSerializer.
> * When creating services that follow the legacy SOAP Encoding standard.
> In these and other cases, you can manually switch to the XmlSerializer class by applying the `XmlSerializerFormatAttribute` attribute to your service, as shown in the following code. (code sample continues in the docs)

As an extra puzzle to the reader - can this be managed inside the `DataContractSerializer`? I've not found a way to do that.

# Running the tests and developing interactively

Run the code by cloning this repo, `cd` into the directory, and run

```
dotnet watch test
```

which will run the tests every time there is a change to a C-sharp file. This gives you an interactive playground to try out different approaches.

# Testing Approach

We test by serializing with both the `DataContractSerializer` and the `XmlSerializer` and comparing the output with `Expected.xml`. Note that comparing XML documents is difficult -- you can't just compare the strings because the same document can have different representations -- especially around namespaces. So these two document fragments are considered identical;

```
<Foo xmlns="my-namespace">
    <Bar />
    <Baz />
</Foo>
```

```
<a:Foo xmlns:a="my-namesace">
    <a:Bar />
    <a:Baz />
</a:Foo>
```

since these are both shortcuts for the fully-qualified;

```
<my-namespace:Foo>
    <my-namespace:Bar />
    <my-namespace:Baz />
</my-namespace:Foo>
```

Instead, the tests attempt to compare the 'real' sructure of namespaces and elements by pretty-printing them to a format like this;

```
my-namespace:Foo
  my-namespace:Bar
  my-namespace:Baz  
```

And then using a string comparison on the output. That removes any differences around XML namespace prefixes, by giving you a fully-qualified name for each element, and that allows you to do string comparison between two different documents.

# Source Code Structure

- [OuterType.cs](OuterType.cs) -- the outer type, which should be serialized in `OUTER` and have a child property in `INNER`
- [InnerType.cs](InnerType.cs) -- which is entirely in the `INNER` namespace
- [UnitTests.cs](UnitTests.cs) -- unit test for both `XmlSerializer` (passing) and `DataContractSerializer`.
- [XmlDocumentVisitor.cs](XmlDocumentVisitor.cs) -- a testing utility which helps you walk around the elements of an [`XmlDocument`](https://docs.microsoft.com/en-us/dotnet/api/system.xml.xmldocument?view=netcore-3.1)


This can be solved by swapping the serialization method to the more powerful, but less idiomatic, `XmlSerializer` -- the granddaddy serialiser in dotnet.

