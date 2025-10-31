using UnityEngine;
using TMPro;
using System;
using System.IO;
using System.Collections;
using VIVE.OpenXR;
using VIVE.OpenXR.EyeTracker;
using VIVE.OpenXR.Passthrough;
using VIVE.OpenXR.CompositionLayer;

public class GazeWithPassthrough : MonoBehaviour
{
    [Header("基础对象")]
    public Transform CameraPos;
    public Transform gazeRaycastPlane;    // 实体Plane
    public GameObject gazeHitMarker;
    public TextMeshPro coordinateText;
    public DataLogger dataLogger;

    [Header("Passthrough窗口设置")]
    public Mesh passthroughMesh;           // XR Passthrough的mesh（如Quad或Cube）
    public Transform passthroughMeshTransform; // Passthrough mesh的Transform

    private VIVE.OpenXR.Passthrough.XrPassthroughHTC passthroughID;

    private string gazeLogPath;

    // 盯视判定参数
    private Vector3 lastGazePoint = Vector3.zero;
    private float gazeStayTime = 0f;
    private bool gazeLogTriggered = false;
    [Tooltip("判定注视“同一处”的空间距离（米）")]
    public float gazeThreshold = 0.05f;   // 5cm
    [Tooltip("需要持续盯住多少秒才输出日志")]
    public float gazeRequiredTime = 2f;   // 2秒

    void Start()
    {
        if (CameraPos == null && Camera.main != null)
            CameraPos = Camera.main.transform;

        // 日志文件开头只写一次分辨率
        gazeLogPath = Application.persistentDataPath + "/gaze_log.txt";
        File.WriteAllText(gazeLogPath, $"=== Gaze Log Start ===\nScreenW={Screen.width}, ScreenH={Screen.height}\n");

        StartCoroutine(InitPassthroughLayer());
    }

    IEnumerator InitPassthroughLayer()
    {
        yield return new WaitForSeconds(1f); // 保证XR初始化
        PassthroughAPI.CreateProjectedPassthrough(
            out passthroughID,
            LayerType.Overlay,
            (id) => PassthroughAPI.DestroyPassthrough(id)
        );
        if (passthroughMesh != null)
        {
            PassthroughAPI.SetProjectedPassthroughMesh(
                passthroughID,
                passthroughMesh.vertices,
                passthroughMesh.triangles
            );
        }
    }

    void Update()
    {
        // 1. Plane和Passthrough窗口同步到摄像头正前方2m
        if (gazeRaycastPlane != null && CameraPos != null)
        {
            gazeRaycastPlane.position = CameraPos.position + CameraPos.forward * 2f;
            gazeRaycastPlane.rotation = CameraPos.rotation;
        }

        if (passthroughMeshTransform != null && gazeRaycastPlane != null)
        {
            passthroughMeshTransform.position = gazeRaycastPlane.position;
            passthroughMeshTransform.rotation = gazeRaycastPlane.rotation;
            passthroughMeshTransform.localScale = gazeRaycastPlane.localScale;

            PassthroughAPI.SetProjectedPassthroughMeshTransform(
                passthroughID,
                ProjectedPassthroughSpaceType.Headlock,
                passthroughMeshTransform.position,
                passthroughMeshTransform.rotation,
                passthroughMeshTransform.lossyScale
            );
        }

        // 2. 眼动追踪+盯视日志
        XR_HTC_eye_tracker.Interop.GetEyeGazeData(out XrSingleEyeGazeDataHTC[] out_gazes);
        if (out_gazes == null || out_gazes.Length <= (int)XrEyePositionHTC.XR_EYE_POSITION_LEFT_HTC) return;

        XrSingleEyeGazeDataHTC rightGaze = out_gazes[(int)XrEyePositionHTC.XR_EYE_POSITION_LEFT_HTC];
        if (!rightGaze.isValid) return;

        Vector3 gazeDirection = rightGaze.gazePose.orientation.ToUnityQuaternion() * Vector3.forward;
        Vector3 rayOrigin = CameraPos != null ? CameraPos.position : rightGaze.gazePose.position.ToUnityVector();

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, gazeDirection, out hit, 20f))
        {
            // Gaze marker和坐标显示
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

            // 注视同一处计时判定
            if (Vector3.Distance(hit.point, lastGazePoint) < gazeThreshold)
            {
                gazeStayTime += Time.deltaTime;
                if (gazeStayTime >= gazeRequiredTime && !gazeLogTriggered)
                {
                    gazeLogTriggered = true;

                    // 日志只记录 gaze 世界坐标 + 像素坐标
                    Vector3 screenPos = Camera.main.WorldToScreenPoint(hit.point);
                    float imgX = screenPos.x * 3;
                    float imgY = (Screen.height - screenPos.y); // 图片原点在左上

                    string log = $"{DateTime.Now:HH:mm:ss}, GazeFixation X={hit.point.x:F4}, Y={hit.point.y:F4}, Z={hit.point.z:F4}, ImgX={imgX:F1}, ImgY={imgY:F1}\n";
                    File.AppendAllText(gazeLogPath, log);

                    if (dataLogger != null)
                    {
                        dataLogger.GazeX = hit.point.x;
                        dataLogger.GazeY = hit.point.y;
                        dataLogger.GazeZ = hit.point.z;
                        dataLogger.AppendGazeToTxt();
                    }
                    Debug.Log("Gaze fixation logged at: " + hit.point + $" | ImgX={imgX:F1}, ImgY={imgY:F1}");
                }
            }
            else
            {
                // gaze发生偏移，重置
                gazeStayTime = 0f;
                gazeLogTriggered = false;
                lastGazePoint = hit.point;
            }
        }
        else
        {
            if (gazeHitMarker != null)
                gazeHitMarker.SetActive(false);

            // gaze未命中plane时重置
            gazeStayTime = 0f;
            gazeLogTriggered = false;
        }
    }
}