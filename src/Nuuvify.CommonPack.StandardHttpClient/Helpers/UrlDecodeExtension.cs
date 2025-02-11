using System.Net;

namespace System;

public static class UrlDecodeExtension
{

    public static string GetFullUrlDecoded(this Uri uri)
    {

        var decodedUrl = WebUtility.UrlDecode(uri.ToString());
        return decodedUrl;
    }

    public static string GetFullUrlDecoded(this Uri uri, string urlEncoded)
    {
        var decodedUrl = WebUtility.UrlDecode(urlEncoded);
        return decodedUrl;
    }

}
