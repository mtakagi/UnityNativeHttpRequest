using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

public class Test : MonoBehaviour
{

	private List<byte> data = new List<byte>();
	
	// Use this for initialization
	void Start () {
		Debug.Log(GetThredInfo());
		var request = new HttpRequest("https://httpbin.org/stream/100");
		request.onDownloadProgress += bytes =>
		{
			Debug.Log(GetThredInfo());
			data.AddRange(bytes);
		};
		request.onRequestFinished += () =>
		{
			Debug.Log(GetThredInfo());
			Debug.Log(Encoding.UTF8.GetString(data.ToArray()));
			request.Dispose();
		};
		request.Start();
	}

	private string GetThredInfo()
	{
		var current = Thread.CurrentThread;
		return string.Format("Background: {0}\nThread Pool: {1}\nThread ID: {2}\n", current.IsBackground,
			current.IsThreadPoolThread, current.ManagedThreadId);
	}
}
