using UnityEngine;
using System.IO;
using System;

public class DataLogger : MonoBehaviour
{
    public string participantID = "test1";
    public float GazeX;
    public float GazeY;
    public float GazeZ;

    public string fileName = "gaze_tracking";

    private string internalPath;  // Application.persistentDataPath
    private string externalPath;  // /sdcard/Download/

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageWrite))
        {
            UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.ExternalStorageWrite);
        }
#endif
        internalPath = Application.persistentDataPath + $"/{participantID}_{fileName}.txt";
        externalPath = $"/sdcard/Download/{participantID}_{fileName}.txt";

        string header = $"=== Gaze Log Start ===\n";
        try
        {
            File.WriteAllText(internalPath, header);
            File.WriteAllText(externalPath, header);
            Debug.Log("📄 Gaze logger initialized.\nInternal: " + internalPath + "\nExternal: " + externalPath);

            TriggerMediaScan(externalPath); // 🔄 让 Download 文件对用户可见
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Init log file failed: " + e.Message);
        }
    }

    public void AppendGazeToTxt()
    {
        string logLine = $"{DateTime.Now:HH:mm:ss}, X={GazeX:F4}, Y={GazeY:F4}, Z={GazeZ:F4}\n";
        try
        {
            File.AppendAllText(internalPath, logLine);
            File.AppendAllText(externalPath, logLine);
            Debug.Log("✅ Gaze written: " + logLine);

            TriggerMediaScan(externalPath);
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Gaze log write failed: " + e.Message);
        }
    }

    public void SaveSingleFrameToTxt()
    {
        string logLine = $"{DateTime.Now:HH:mm:ss} [Blink], X={GazeX:F4}, Y={GazeY:F4}, Z={GazeZ:F4}\n";
        try
        {
            File.AppendAllText(internalPath, logLine);
            File.AppendAllText(externalPath, logLine);
            Debug.Log("📌 Blink-triggered Gaze Saved: " + logLine);

            TriggerMediaScan(externalPath);
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Failed to save blink gaze: " + e.Message);
        }
    }

    private void TriggerMediaScan(string path)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext"))
            using (AndroidJavaClass mediaScanner = new AndroidJavaClass("android.media.MediaScannerConnection"))
            {
                mediaScanner.CallStatic(
                    "scanFile",
                    context,
                    new string[] { path },
                    null,
                    null
                );
            }
            Debug.Log("📂 MediaScanner scan triggered for: " + path);
        }
        catch (Exception e)
        {
            Debug.LogError("❌ Failed to trigger MediaScanner: " + e.Message);
        }
#endif
    }
}
