using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class HttpRequest : IDisposable {

    public enum HttpMethod
    {
        GET = 0,
        HEAD,
        POST,
        OPTIONS,
        PUT,
        DELETE,
        TRACE
    }

    private static readonly string[] HttpMethods = {"GET", "HEAD", "POST", "OPTIONS", "PUT", "DELETE", "TRACE"};
    
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
    private const string DllName = "HttpRequest";
#elif UNITY_IOS && !UNITY_EDITOR
    private const string DllName = "__Internal";
#endif

    [DllImport(DllName)]
    private static extern IntPtr HttpRequest_Create(string url);
    [DllImport(DllName)]
    private static extern void HttpRequest_SetHeader(IntPtr request, string key, string value);
    [DllImport(DllName)]
    private static extern void HttpRequest_SetMethod(IntPtr request, string method);
    [DllImport(DllName)]
    private static extern void HttpRequest_SetHttpBody(IntPtr request, byte[] body, int length);
    [DllImport(DllName)]
    private static extern void HttpRequest_SetTimeout(IntPtr request, double timeout);
    [DllImport(DllName)]
    private static extern void HttpRequest_Release(IntPtr request);

    public string Url { get; private set; }
    public Dictionary<string, string> Headers { get; private set; }
    
    internal readonly IntPtr Request;
    
    public HttpRequest(string url)
    {
        Url = url;
        Headers = new Dictionary<string, string>();
        Request = HttpRequest_Create(url);
    }
    
    public HttpRequest(string url, double timeout) : this(url)
    {
        SetTimeout(timeout);
    }
    
    public HttpRequest(string url, HttpMethod method) : this(url)
    {
        SetHttpMethod(method);
    }
    
    public HttpRequest(string url, HttpMethod method, double timeout) : this(url)
    {
        SetTimeout(timeout);
        SetHttpMethod(method);
    }

    public void SetHttpMethod(HttpMethod method)
    {
        HttpRequest_SetMethod(Request, HttpMethods[(int)method]);
    }
    
    public void AddHeader(string field, string value)
    {
        Headers[field] = value;
        HttpRequest_SetHeader(Request, field, value);
    }

    public void SetHttpBody(byte[] body)
    {
        HttpRequest_SetHttpBody(Request, body, body.Length);
    }

    public void SetTimeout(double timeout)
    {
        HttpRequest_SetTimeout(Request, timeout);
    }
    
    public void Dispose()
    {
        if (Request != IntPtr.Zero)
        {
            HttpRequest_Release(Request);
        }
    }
}
