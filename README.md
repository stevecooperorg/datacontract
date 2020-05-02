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

When we serialize, we want `OuterType` to be namespaced in one namespace (let's say `xmlns="OUTER"`), and `InnerType` and it's children to be namespaced differently; so `xmlns="INNER"`. Eg;

```
<?xml version="1.0" encoding="utf-8"?>
<OuterType xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns="OUTER">
  <Result xmlns="INNER">
    <Value1>foo</Value1>
    <Value2>bar</Value2>
  </Result>
</OuterType>
```

Structurally, we could express that as;

```
OUTER:OuterType
  INNER:Result
    INNER:Value1
    INNER:Value2
```


The original code that revealed the issue was using `DataContractSerializer`, which produced something with this structure;


```
OUTER:OuterType
  OUTER:Result
    INNER:Value1
    INNER:Value2
```

Note the second line has the `<Result>` element in the `OUTER` namespace rather than the required `INNER`.

This can be solved by swapping the serialization method to the more powerful, but less idiomatic, `XmlSerializer` -- the granddaddy serialiser in dotnet.

From [Microsoft's own documents](https://docs.microsoft.com/en-us/dotnet/framework/wcf/feature-details/using-the-xmlserializer-class#when-to-use-the-xmlserializer-class):

> At times, you may have to manually switch to the XmlSerializer. This happens, for example, in the following cases:
> * When migrating an application from ASP.NET Web services to WCF, you may want to reuse existing, XmlSerializer-compatible types instead of creating new data contract types.
> * When precise control over the XML that appears in messages is important, but a Web Services Description Language (WSDL) document is not available, for example, when creating a service with types that have to comply to a certain standardized, published schema that is not compatible with the DataContractSerializer.
> * When creating services that follow the legacy SOAP Encoding standard.
> In these and other cases, you can manually switch to the XmlSerializer class by applying the `XmlSerializerFormatAttribute` attribute to your service, as shown in the following code. (code sample continues in the docs)

As an extra puzzle to the reader - can this be managed inside the `DataContractSerializer`? I've not found a way to do that.
