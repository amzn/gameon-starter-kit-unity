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

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Request
{
    public Request(string uri)
    {
        this.uri = uri;
    }

    [JsonIgnore] public string uri { get; protected set; }

    public string ToJSON()
    {
        return JsonConvert.SerializeObject(this);
    }

    public string ToQueryString()
    {
        var dict = this.ToKeyValue();
        var list = new List<string>();

        foreach (var item in dict)
            list.Add(item.Key + "=" + item.Value);

        return string.Join("&", list.ToArray());
    }
}

public static class LinqHelper
{
    public static IDictionary<string, string> ToKeyValue(this object metaToken)
    {
        IDictionary<string, string> dict = null;

        if (null != metaToken)
        {
            var token = metaToken as JToken;

            if (null == token)
            {
                dict = ToKeyValue(JObject.FromObject(metaToken));
            }
            else
            {
                if (token.HasValues)
                {
                    var contentData = new Dictionary<string, string>();

                    foreach (var child in token.Children().ToList())
                    {
                        var childContent = child.ToKeyValue();
                        if (null != childContent)
                            contentData = contentData.Concat(childContent).ToDictionary(k => k.Key, v => v.Value);
                    }

                    dict = contentData;
                }
                else
                {
                    var jValue = token as JValue;

                    if (null != jValue && null != jValue.Value)
                    {
                        var value = jValue.Type == JTokenType.Date
                            ? jValue.ToString("o", CultureInfo.InvariantCulture)
                            : jValue.ToString(CultureInfo.InvariantCulture);

                        dict = new Dictionary<string, string> {{token.Path, value}};
                    }
                }
            }
        }

        return dict;
    }
}