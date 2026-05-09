using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class WebGLBridge
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void reportUnitySessionResult(string payload);
#else
    private static void reportUnitySessionResult(string payload)
    {
        Debug.Log("WebGL result: " + payload);
    }
#endif

    public static void SendFinalResult(int score, bool completed)
    {
        var payload = JsonUtility.ToJson(new SimulationResultPayload
        {
            score = score,
            completed = completed,
            resultId = Guid.NewGuid().ToString()
        });

        reportUnitySessionResult(payload);
    }

    [Serializable]
    private class SimulationResultPayload
    {
        public int score;
        public bool completed;
        public string resultId;
    }
}
