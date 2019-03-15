/*
 * Copyright 2019, Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * A copy of the License is located at 
 * 
 *   http://www.apache.org/licenses/LICENSE-2.0
 *  
 * or in the "license" file accompanying this file. This file is distributed 
 * on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either 
 * express or implied. See the License for the specific language governing 
 * permissions and limitations under the License.
 *   
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;


public class ServiceAPI : MonoBehaviour
{
    protected readonly string endPoint = "https://api.amazongameon.com/v1";

    protected virtual void AddHeaders(UnityWebRequest request)
    {
        GameOn goTarget = GetComponent<GameOn>();
        request.SetRequestHeader("x-api-key", goTarget.PublicApiKey);
        print(request.url + " x-api-key " + goTarget.PublicApiKey);
        request.SetRequestHeader("content-type", "application/json");
    }

    #region HTTP GET Methods
    protected IEnumerator SyncGet(Request request, Action<string> onSuccess, Action<string> onFailure)
    {
        yield return StartCoroutine(HttpGet(request.uri, request.ToQueryString(), onSuccess, onFailure));
    }

    protected IEnumerator HttpGet(string uri, string queryString, Action<string> onSuccess, Action<string> onFailure)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(endPoint + uri + "?" + queryString))
        {
            AddHeaders(request);

            yield return request.SendWebRequest();
            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("RequestUsingGet( key, \"" + uri + "\" ) failed: " + request.error + " - " + request.responseCode);

                object obj = JsonConvert.DeserializeObject(request.downloadHandler.text);
                if (null != obj)
                    Debug.Log(obj.ToString());

                if (null != onFailure)
                    onFailure(request.error);
            }
            else
            {
                onSuccess(Encoding.UTF8.GetString(request.downloadHandler.data));
            }
        }
    }
    #endregion

    #region HTTP POST Methods
    protected IEnumerator SyncPost(Request request, Action<string> onSuccess, Action<string> onFailure)
    {
        yield return StartCoroutine(HttpPost(request.uri, request.ToJSON(), onSuccess, onFailure));
    }

    protected IEnumerator HttpPost(string uri, string parameters, Action<string> onSuccess, Action<string> onFailure)
    {
        using (UnityWebRequest request = UnityWebRequest.Post(endPoint + uri, new Dictionary<string, string>()))
        {
            request.uploadHandler = ("{}" == parameters) ? null : new UploadHandlerRaw(Encoding.UTF8.GetBytes(parameters));
            AddHeaders(request);

            yield return request.SendWebRequest();
            if (request.isHttpError || request.isNetworkError)
            {
                Debug.Log("RequestUsingPost( key, \"" + uri + "\" ) failed: " + request.error + " - " + request.responseCode);

                object obj = JsonConvert.DeserializeObject(request.downloadHandler.text);
                if (null != obj)
                    Debug.Log(obj.ToString());

                if (null != onFailure)
                    onFailure(request.error);
            }
            else
                onSuccess(Encoding.UTF8.GetString(request.downloadHandler.data));
        }
    }
    #endregion

    #region HTTP PUT Methods
    protected IEnumerator SyncPut(Request request, Action<string> onSuccess, Action<string> onFailure)
    {
        yield return StartCoroutine(HttpPut(request.uri, request.ToJSON(), onSuccess, onFailure));
    }

    protected IEnumerator HttpPut(string uri, string parameters, Action<string> onSuccess, Action<string> onFailure)
    {
        using (UnityWebRequest request = UnityWebRequest.Put(endPoint + uri, parameters))
        {
            request.uploadHandler = ("{}" == parameters) ? null : new UploadHandlerRaw(Encoding.UTF8.GetBytes(parameters));
            AddHeaders(request);

            yield return request.SendWebRequest();
            if (request.isHttpError || request.isNetworkError)
            {

                object obj = JsonConvert.DeserializeObject(request.downloadHandler.text);
                if (null != obj)
                    Debug.Log(obj.ToString());

                if (null != onFailure)
                    onFailure(request.error);
            }
            else
                onSuccess(Encoding.UTF8.GetString(request.downloadHandler.data));
        }
    }
    #endregion
}
