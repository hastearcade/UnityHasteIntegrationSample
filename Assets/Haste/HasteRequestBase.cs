
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class HasteRequestBase
{
    protected IEnumerator GetRequest<T>(string uri, System.Action<T> callback, string token = null)
    where T : class?
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            if (!String.IsNullOrEmpty(token))
            {
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
            }

            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;


            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);

                    var result = JsonConvert.DeserializeObject<T>(webRequest.downloadHandler.text);
                    callback(result);
                    break;
            }

            callback(null);
        }
    }

    protected IEnumerator PostRequest<T>(string uri, Dictionary<string, string> data, System.Action<T> callback, string token = null)
    where T : class?
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Post(uri, data))
        {
            if (!String.IsNullOrEmpty(token))
            {
                webRequest.SetRequestHeader("Authorization", $"Bearer {token}");
            }

            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);

                    var result = JsonConvert.DeserializeObject<T>(webRequest.downloadHandler.text);
                    callback(result);
                    break;
            }

            callback(null);
        }
    }
}