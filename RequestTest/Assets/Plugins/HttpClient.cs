using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class HttpClient : IDisposable
{
    public class HttpResponse
    {
        public int StatusCode { get; private set; }
        public string StatusString { get; private set; }
        public Dictionary<string, string> Heaader { get; private set; }

        public HttpResponse(int statusCode, string statusString, Dictionary<string, string> header)
        {
            StatusCode = statusCode;
            StatusString = statusString;
            Heaader = header;
        }
    }
    
    private delegate void ResponseCallback(IntPtr context, long statusCode, string statusString, int headerCount, IntPtr keys, IntPtr values);
    private delegate void DownloadPorgressCallback(IntPtr context, IntPtr data, long length);
    private delegate void RequestFailedCallback(IntPtr context);
    private delegate void RequestFinishedCallback(IntPtr context);

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
    private const string DllName = "HttpRequest";
#elif UNITY_IOS && !UNITY_EDITOR
    private const string DllName = "__Internal";
#endif

    [DllImport(DllName)]
    private static extern IntPtr HttpClient_CreateWithRequest(IntPtr request, IntPtr context, ResponseCallback responseCallback,
        DownloadPorgressCallback downloadProgressCallback,
        RequestFailedCallback requestFailedCallback,
        RequestFinishedCallback requestFinishedCallback);
    
    [DllImport(DllName)]
    private static extern IntPtr HttpClient_Create(string url, IntPtr context, ResponseCallback responseCallback,
        DownloadPorgressCallback downloadProgressCallback,
        RequestFailedCallback requestFailedCallback,
        RequestFinishedCallback requestFinishedCallback);
    
    [DllImport(DllName)]
    private static extern void HttpClient_Start(IntPtr request);
    
    [DllImport(DllName)]
    private static extern void HttpClient_Cancel(IntPtr request);
    
    [DllImport(DllName)]
    private static extern void HttpClient_Dispose(IntPtr request);

    private IntPtr _request;
    private bool _isProgress;
    private byte[] _buffer;
    private GCHandle _this;
    private bool _isDisposed;

    public string Url { get; private set; }
    public HttpResponse Response { get; private set; }

    public event Action<byte[]> onDownloadProgress;
    public event Action onRequestFinished;

    public HttpClient(HttpRequest request, byte[] buffer)
    {
        Url = request.Url;
        _buffer = buffer;
        Init(request);
#if UNITY_EDITOR
        EditorApplication.playmodeStateChanged += OnPlayModeChanged;
#endif
    }
    
    public HttpClient(HttpRequest request) : this(request, new byte[1024 * 4])
    {
    }

    public HttpClient(string url, byte[] buffer)
    {
        Url = url;
        _buffer = buffer;
        Init(url);
#if UNITY_EDITOR
        EditorApplication.playmodeStateChanged += OnPlayModeChanged;
#endif
    }
    
    public HttpClient(string url) : this(url, new byte[1024 * 4])
    {
    }

    private void Init(HttpRequest request)
    {
        _this = GCHandle.Alloc(this, GCHandleType.Normal);
        var ptr = GCHandle.ToIntPtr(_this);
        _request = HttpClient_CreateWithRequest(request.Request, ptr, ResponseDelegate,  DownloadDelegate, null, FinishDelegate);
        request.Dispose();
    }
    
    private void Init(string url)
    {
        _this = GCHandle.Alloc(this, GCHandleType.Normal);
        var ptr = GCHandle.ToIntPtr(_this);
        _request = HttpClient_Create(url, ptr, ResponseDelegate,  DownloadDelegate, null, FinishDelegate);
    }

    public void Start()
    {
        if (_request != IntPtr.Zero && !_isProgress)
        {
            HttpClient_Start(_request);
            _isProgress = true;
        }
    }

    public void Cancel()
    {
        if (_request != IntPtr.Zero && _isProgress)
        {
            HttpClient_Cancel(_request);
            _isProgress = false;
        }
    }

    private void OnDownloadProgress(byte[] data)
    {
        if (onDownloadProgress != null)
        {
            onDownloadProgress(data);
        }
    }

    private void OnRequestFinished()
    {
        _isProgress = false;

        if (onRequestFinished != null)
        {
            onRequestFinished();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool isDisposing)
    {
        if (_isDisposed) return;

        if (isDisposing)
        {
            _buffer = null;
            Url = null;
            Response = null;
        }

        if (_request != IntPtr.Zero)
        {
            HttpClient_Dispose(_request);
            _request = IntPtr.Zero;
            _this.Free();
        }
        _isDisposed = true;
    }

    ~HttpClient()
    {
        Dispose(false);
    }
    
#if (UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_EDITOR
    [AOT.MonoPInvokeCallback(typeof(ResponseCallback))]
#endif
    private static void ResponseDelegate(IntPtr context, long statusCode, string statusString, int headerCount, IntPtr keys, IntPtr values)
    {
        try
        {
            var handle = GCHandle.FromIntPtr(context);
            var request = (HttpClient) handle.Target;
            var keyArray = PtrToStringArray(headerCount, keys);
            var valueArray = PtrToStringArray(headerCount, values);
            var header = new Dictionary<string, string>(headerCount);

            for (var i = 0; i < headerCount; i++)
            {
                header[keyArray[i]] = valueArray[i];
            }
            
            request.Response = new HttpResponse((int)statusCode, statusString, header);
        }
        catch (System.Exception e)
        {
            
        }
    }
    
    
#if (UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_EDITOR
    [AOT.MonoPInvokeCallback(typeof(DownloadPorgressCallback))]
#endif
    private static void DownloadDelegate(IntPtr context, IntPtr data, long length)
    {
        try
        {
            var handle = GCHandle.FromIntPtr(context);
            var request = (HttpClient)handle.Target;
            var buffer = request._buffer;
            while (length > 0)
            {
                if (data == IntPtr.Zero) break;
                if (buffer == null || buffer.Length > length)
                {
                    buffer = new byte[length];
                }
                Marshal.Copy(data, buffer, 0, buffer.Length);
                request.OnDownloadProgress(buffer);
                data = new IntPtr(data.ToInt64() + buffer.Length);
                length -= buffer.Length;
            }
        }
        catch (System.Exception e)
        {
        }
    }
    
#if (UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_EDITOR
    [AOT.MonoPInvokeCallback(typeof(RequestFinishedCallback))]
#endif
    private static void FinishDelegate(IntPtr context)
    {
        try
        {
            var handle = GCHandle.FromIntPtr(context);
            var request = (HttpClient)handle.Target;
            request.OnRequestFinished();
        }
        catch (System.Exception e)
        {
        }
    }
    
    private static string PtrToString (IntPtr p)
    {
        // TODO: deal with character set issues.  Will PtrToStringAnsi always
        // "Do The Right Thing"?
        if (p == IntPtr.Zero)
            return null;
        return Marshal.PtrToStringAnsi (p);
    }
 
    private static string[] PtrToStringArray (int count, IntPtr stringArray)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException ("count", "< 0");
        if (stringArray == IntPtr.Zero)
            return new string[count];
 
 
        var members = new string[count];
        for (var i = 0; i < count; ++i) {
            var s = Marshal.ReadIntPtr (stringArray, i * IntPtr.Size);
            members[i] = PtrToString (s);
        }
 
        return members;
    }

    
#if UNITY_EDITOR
    private void OnPlayModeChanged()
    {
        if (!EditorApplication.isPlaying)
        {
            Cancel();
            Dispose();                                
        }
    }
#endif
}
