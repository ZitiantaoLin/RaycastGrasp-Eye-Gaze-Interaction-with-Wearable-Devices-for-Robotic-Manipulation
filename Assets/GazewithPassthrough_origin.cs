using UnityEngine;
using TMPro;
using System;
using System.IO;
using VIVE.OpenXR;
using VIVE.OpenXR.EyeTracker;

public class GazeWithPassthrough_origin : MonoBehaviour
{
    public Transform CameraPos;
    public GameObject hmd;
    public Mesh passthroughMesh;
    public Transform passthroughMeshTransform;
    public Transform gazeRaycastPlane;
    public TextMeshPro coordinateText;
    public GameObject gazeHitMarker;
    public DataLogger dataLogger;
    public float scaleModifier = 0.2f;

    private string gazeLogPath;
    private string debugLogPath;
    private float lastLogTime = 0f;
    private float logCooldown = 0.2f;

    void Start()
    {
        if (CameraPos == null && Camera.main != null)
            CameraPos = Camera.main.transform;

        if (hmd == null)
            hmd = Camera.main.gameObject;

        gazeLogPath = "/sdcard/Download/gaze_log.txt";
        debugLogPath = "/sdcard/Download/gaze_debug.txt";
        File.WriteAllText(gazeLogPath, "=== Gaze Log Start ===\n");
        File.WriteAllText(debugLogPath, "=== Debug Log Start ===\n");
    }

    void Update()
    {
        // 1. Plane 和 passthrough mesh 始终跟随摄像头的位置和旋转
        if (gazeRaycastPlane != null && CameraPos != null)
        {
            //gazeRaycastPlane.position = CameraPos.position + CameraPos.forward * 1.0f;
            gazeRaycastPlane.position = Camera.main.transform.position + Camera.main.transform.forward * 10f;
            gazeRaycastPlane.rotation = CameraPos.rotation;
            // gazeRaycastPlane.localScale = passthroughMeshTransform.localScale; // 如需同步大小
            //gazeRaycastPlane.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up); // 固定朝向世界z正方向
            //gazeRaycastPlane.rotation = Quaternion.identity;
        }

        // 2. 获取 gaze origin 和 direction（真实右眼 gaze）
        Vector3 origin = Vector3.zero;
        Vector3 direction = Vector3.forward;
        bool isValidGaze = false;

        XR_HTC_eye_tracker.Interop.GetEyeGazeData(out XrSingleEyeGazeDataHTC[] out_gazes);
        XrSingleEyeGazeDataHTC rightGaze = out_gazes[(int)XrEyePositionHTC.XR_EYE_POSITION_RIGHT_HTC];
        if (rightGaze.isValid)
        {
            origin = rightGaze.gazePose.position.ToUnityVector();
            direction = rightGaze.gazePose.orientation.ToUnityQuaternion() * Vector3.forward;
            isValidGaze = true;
        }
        else
        {
            origin = hmd.transform.position;
            direction = hmd.transform.forward;
        }

        // 3. Raycast 检测
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, 10f))
        {
            if (gazeHitMarker != null)
            {
                gazeHitMarker.transform.position = hit.point;
                gazeHitMarker.SetActive(true);
            }
            if (coordinateText != null)
            {
                coordinateText.text = $"X: {hit.point.x:F2}\nY: {hit.point.y:F2}\nZ: {hit.point.z:F2}";
                coordinateText.transform.position = hit.point + new Vector3(0, 0.15f, 0);
            }

            if (Time.time - lastLogTime > logCooldown)
            {
                string log = $"{DateTime.Now:HH:mm:ss}, X={hit.point.x:F4}, Y={hit.point.y:F4}, Z={hit.point.z:F4}\n";
                File.AppendAllText(gazeLogPath, log);
                File.AppendAllText(debugLogPath, $"✅ {DateTime.Now:HH:mm:ss} HIT at {hit.point}\n");

                if (dataLogger != null)
                {
                    dataLogger.GazeX = hit.point.x;
                    dataLogger.GazeY = hit.point.y;
                    dataLogger.GazeZ = hit.point.z;
                    dataLogger.AppendGazeToTxt();
                }

                lastLogTime = Time.time;
            }
        }
        else
        {
            if (gazeHitMarker != null)
                gazeHitMarker.SetActive(false);

            if (Time.time - lastLogTime > logCooldown)
            {
                File.AppendAllText(debugLogPath, $"❌ {DateTime.Now:HH:mm:ss} No Hit Detected\n");
                lastLogTime = Time.time;
            }
        }
    }
}
