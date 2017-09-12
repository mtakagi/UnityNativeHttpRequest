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
		var request = new HttpRequest("https://httpbin.org/post", HttpRequest.HttpMethod.POST);
		request.SetHttpBody(Encoding.UTF8.GetBytes("fuck"));
		var client = new HttpClient(request);
		client.onDownloadProgress += bytes =>
		{
			Debug.Log(GetThredInfo());
			data.AddRange(bytes);
		};
		client.onRequestFinished += () =>
		{
			Debug.Log(GetThredInfo());
			Debug.Log(Encoding.UTF8.GetString(data.ToArray()));
			client.Dispose();
		};
		client.Start();
	}

	private string GetThredInfo()
	{
		var current = Thread.CurrentThread;
		return string.Format("Background: {0}\nThread Pool: {1}\nThread ID: {2}\n", current.IsBackground,
			current.IsThreadPoolThread, current.ManagedThreadId);
	}
}
