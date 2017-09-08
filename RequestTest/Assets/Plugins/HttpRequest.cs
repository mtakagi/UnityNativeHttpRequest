using System;
using System.Runtime.InteropServices;

public class HttpRequest : IDisposable
{
    private delegate void ResponseCallback(IntPtr context);
    private delegate void DownloadPorgressCallback(IntPtr context, IntPtr data, long length);
    private delegate void RequestFailedCallback(IntPtr context);
    private delegate void RequestFinishedCallback(IntPtr context);
    
    private const string DllName = "HttpRequest";

    [DllImport(DllName)]
    private static extern IntPtr HttpRequest_Create(string url, IntPtr responseContext, ResponseCallback responseCallback,
        IntPtr downloadContext, DownloadPorgressCallback downloadProgressCallback,
        IntPtr requestFailedContext, RequestFailedCallback requestFailedCallback,
        IntPtr requestFinishedContext, RequestFinishedCallback requestFinishedCallback);
    
    [DllImport(DllName)]
    private static extern void HttpRequest_Start(IntPtr request);
    
    [DllImport(DllName)]
    private static extern void HttpRequest_Cancel(IntPtr request);
    
    [DllImport(DllName)]
    private static extern void HttpRequest_Dispose(IntPtr request);

#if UNITY_IOS && !UNITY_EDITOR
    [AOT.MonoPInvokeCallback(typeof(ResponseCallback))]
    private static void Response(IntPtr context)
    {
        
    }
    
    [AOT.MonoPInvokeCallback(typeof(DownloadPorgressCallback))]
    private static void DownloadProgress(IntPtr context, IntPtr data, long length)
    {
        var handle = GCHandle.FromIntPtr(context);
        var callback = (Action<byte[]>) handle.Target;
        var dataArray = new byte[length];
        
        Marshal.Copy(data, dataArray, 0, (int)length);
        callback(dataArray);
    }

    [AOT.MonoPInvokeCallback(typeof(RequestFailedCallback))]
    private static void RequestFailed(IntPtr context)
    {
        
    }
    
    [AOT.MonoPInvokeCallback(typeof(RequestFinishedCallback))]
    private static void RequestFinished(IntPtr context)
    {
        var handle = GCHandle.FromIntPtr(context);
        var callback = (Action) handle.Target;
        
        callback();
    }
#endif

    private IntPtr _request;
    private string _url;
    private bool _isProgress;

    public event Action<byte[]> onDownloadProgress;
    public event Action onRequestFinished;

    public HttpRequest(string url)
    {
        _url = url;
        Init(url);
    }

#if UNITY_IOS && !UNITY_EDITOR
    private void Init(string url)
    {
        var downloadHandle = GCHandle.Alloc((Action<byte[]>)OnDownloadProgress, GCHandleType.Normal);
        var downloadPtr = GCHandle.ToIntPtr(downloadHandle);
        var finishHandle = GCHandle.Alloc((Action) OnRequestFinished, GCHandleType.Normal);
        var finishPtr = GCHandle.ToIntPtr(finishHandle);
        _request = HttpRequest_Create(url, IntPtr.Zero, null, downloadPtr, DownloadProgress, IntPtr.Zero, null, finishPtr, RequestFinished);
        downloadHandle.Free();
        finishHandle.Free();
    }
#else
    private void Init(string url)
    {
        _request = HttpRequest_Create(url, IntPtr.Zero, null, 
            IntPtr.Zero, (context, data, length) =>
            {
                var dataArray = new byte[length];
        
                Marshal.Copy(data, dataArray, 0, (int)length);
                OnDownloadProgress(dataArray);
            }, 
            IntPtr.Zero, null,
            IntPtr.Zero, context => OnRequestFinished());
    }
#endif

    public void Start()
    {
        if (_request != IntPtr.Zero && !_isProgress)
        {
            HttpRequest_Start(_request);
            _isProgress = true;
        }
    }

    public void Cancel()
    {
        if (_request != IntPtr.Zero && _isProgress)
        {
            HttpRequest_Cancel(_request);
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
        if (_request != IntPtr.Zero)
        {
            HttpRequest_Dispose(_request);
            _request = IntPtr.Zero;
        }
    }
}
