using System.Xml;

namespace Nuuvify.CommonPack.StandardHttpClient.Results;


public class HttpStandardXmlReturn
{
    public bool Success { get; set; }
    public string ReturnCode { get; set; }
    public XmlDocument ReturnMessage { get; set; }

}
