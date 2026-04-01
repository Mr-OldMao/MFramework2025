using System;
using System.Collections;
using TTSDK;
using UnityEngine;
using UnityEngine.Networking;

public static class MMPOceanEngine
{
    public const string active_event = "active";
    public const string game_addiction_event = "game_addiction";
    
    [Serializable]
    private class OceanEvent
    {
        public string event_type;

        public Context context;
        [Serializable]
        public class Context
        {
            public Ad ad;
            [Serializable]
            public class Ad
            {
                public string callback;
            }
        }

        public long timestamp;

        public OceanEvent(string n, string cid)
        {
            event_type = n;
            context = new Context
            {
                ad = new Context.Ad
                {
                    callback = cid
                }
            };
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }

    public static void OceanPostback(string n)
    {
        // 处理巨量回传
        var launchOption = TT.GetLaunchOptionsSync();
        if (launchOption != null)
        {
            if (launchOption.Query.TryGetValue("clickid", out var cid))
            {
                if (string.IsNullOrEmpty(cid))
                    return;

                var e = new OceanEvent(n, cid);
                //var json = JsonConvert.SerializeObject(e);
                var json = JsonUtility.ToJson(e);
                Debug.LogError("OceanPostback json => " + json);
                CoroutineRunner.Start(Postback(
                    "https://analytics.oceanengine.com/api/v2/conversion",
                    json));
            }
            else
            {
                Debug.LogError("clickid is null");
            }
        }
    }

    private static IEnumerator Postback(string url, string json)
    {
        Debug.LogError($"active json => {json}");
        var request = UnityWebRequest.Post(url, json, "application/json;charset=utf-8");
        yield return request.SendWebRequest();
        if (request.result is UnityWebRequest.Result.Success)
        {
            Debug.LogError("request.downloadHandler.text: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("request.error: " + request.error);
        }
    }
}