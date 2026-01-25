using UnityEngine;
using System.Collections.Generic;

// [RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField, Tooltip("注视目标数组")]
    CapsuleCollider[] targetArray;


    [SerializeField, Tooltip("最近距离")]
    float minDistance = 10.0f;
    [SerializeField, Tooltip("最近距离调整比例")]
    float minScale = 0.3f;


    [SerializeField, Tooltip("胸部位置")]
    float chestOffset = 0.4f;
    [SerializeField, Tooltip("留白")]
    float maxMargin = 0.5f;
    [SerializeField, Tooltip("相机基础角度")]
    Vector3 cameraVector = new Vector3(0, 0.8f, -1f);//不是世界坐标位置，是相机相对于目标的方向


    [SerializeField, Tooltip("相机的追随速度")]
    float followSpeed = 3.0f;
    [SerializeField, Tooltip("相机追随的最大速度")]
    float maxFollowSpeed = 15.0f;
    [SerializeField, Tooltip("达到最大追随速度的距离阈值")]
    float maxFollowDistance = 10.0f;


    [SerializeField, Tooltip("旋回速度")]
    float orbitSpeed = 0.0f;
    [SerializeField, Tooltip("当前旋回角度")]
    float orbitAngle = 0f;


    [SerializeField, Tooltip("UI相机")]
    Transform uiCameraTransform;


    public Camera Camera => targetCamera;
    public float MinDistance { get => minDistance; set => minDistance = value; }
    public float LookAtYMargin { get => chestOffset; set => chestOffset = value; }
    public Vector3 CameraVector { get => cameraVector; set => cameraVector = value; }
    public float MinScale { get => minScale; set => minScale = value; }
    public float MaxMargin { get => maxMargin; set => maxMargin = value; }
    public float FollowSpeed { get => followSpeed; set => followSpeed = value; }
    public float MaxFollowSpeed { get => maxFollowSpeed; set => maxFollowSpeed = value; }
    public float MaxFollowDistance { get => maxFollowDistance; set => maxFollowDistance = value; }
    public float OrbitSpeed { get => orbitSpeed; set => orbitSpeed = value; }

    [SerializeField, Tooltip("UI 相机")]
    Camera targetCamera;
    Vector3 targetCameraPosition;
    Quaternion targetCameraRotation;
    List<Vector3> cornerList;

    public void SetTargetArray(CapsuleCollider[] colliders) => targetArray = colliders;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
        }
    }

    void LateUpdate()
    {
        if (targetArray == null || targetArray.Length == 0)
        {
            return;
        }

        // 计算旋转角度
        // 累加旋转角度并保持在0-360度之间，
        orbitAngle = Mathf.Repeat(orbitAngle + orbitSpeed * Time.deltaTime, 360f);

        // 计算相机基准方向
        var initialCameraDirection = cameraVector.normalized;
        var orbitRotation = Quaternion.Euler(0f, orbitAngle, 0f);
        var targetToCameraDirection = (orbitRotation * initialCameraDirection).normalized;

        var cameraForward = -targetToCameraDirection;
        var worldUp = Vector3.up;
        if (Mathf.Abs(Vector3.Dot(targetToCameraDirection, worldUp)) > 0.99f)
        {
            worldUp = Vector3.right;
        }
        var cameraRight = Vector3.Cross(worldUp, cameraForward).normalized;
        var cameraUp = Vector3.Cross(cameraForward, cameraRight).normalized;


        // 收集所有目标的边界点，计算中心位置
        cornerList ??= new(targetArray.Length * 8);
        cornerList.Clear();

        var totalTargetCenterPosition = Vector3.zero;
        var totalTargetCount = 0;
        foreach (var target in targetArray)
        {
            if (target == null)
            {
                continue;
            }

            foreach (var corner in GetBoundscornerList(target.bounds))
            {
                cornerList.Add(corner);
            }

            totalTargetCenterPosition += target.bounds.center;
            totalTargetCount++;
        }

        if (cornerList.Count == 0)
        {
            return;
        }

        var averageCenterPosition = totalTargetCount > 0 ? totalTargetCenterPosition / totalTargetCount : Vector3.zero;


        // 获取相机视锥体参数
        var fov = targetCamera.fieldOfView;//垂直视野角度
        var aspect = targetCamera.aspect;//宽高比
        var verticalTangent = Mathf.Tan(Mathf.Deg2Rad * fov * 0.5f);//计算垂直方向的正切值，Mathf.Deg2Rad：度数转换成弧度
        var horizontalTangent = verticalTangent * aspect;//计算水平方向的正切值

        // 计算各点在相机坐标系的范围
        float maxHorizontalDistance, maxVerticalDistance, nearestDepth, farthestDepth;
        ComputeExtents(cornerList, averageCenterPosition, cameraRight, cameraUp, cameraForward, out maxHorizontalDistance, out maxVerticalDistance, out nearestDepth, out farthestDepth);
        var depthHalf = (farthestDepth - nearestDepth) * 0.5f;//最远目标和最近目标深度的一半

        // 计算动态边距
        var horizontalMarginFactor = Mathf.InverseLerp(minDistance, 3f, maxHorizontalDistance);
        var verticalMarginFactor = Mathf.InverseLerp(minDistance, 3f, maxVerticalDistance);//Mathf.InverseLerp 是一个线性插值函数，用于计算maxU在minDistance到3f之间的位置比例
        var horizontalMargin = Mathf.Lerp(0, maxMargin, horizontalMarginFactor);
        var verticalMargin = Mathf.Lerp(0, maxMargin, verticalMarginFactor);//根据比例计算实际边距

        // 计算相机所需距离
        var distanceForVerticalView = (maxVerticalDistance + verticalMargin) / verticalTangent;
        var distanceForHorizontalView = (maxHorizontalDistance + horizontalMargin) / horizontalTangent;
        var distanceForDepthView = depthHalf;

        var distance = Mathf.Max(Mathf.Max(distanceForHorizontalView, distanceForVerticalView), distanceForDepthView);
        distance = Mathf.Max(distance, minDistance);


        // 计算目标间距和高度
        var maxPositionY = 0f;//最大高度
        var minPairDistance = float.MaxValue;//最小距离
        for (var i = 0; i < targetArray.Length; i++)
        {
            if (targetArray[i] == null)
            {
                continue;
            }

            var currentPositionY = targetArray[i].transform.position.y;
            if (currentPositionY > maxPositionY)
            {
                maxPositionY = currentPositionY;
            }

            for (var j = i + 1; j < targetArray.Length; j++)
            {
                if (targetArray[j] == null)
                {
                    continue;
                }
                var currentPairDistance = Vector3.Distance(targetArray[i].bounds.center, targetArray[j].bounds.center);
                minPairDistance = Mathf.Min(minPairDistance, currentPairDistance);
            }
        }

        // 近距离高度补正
        distance += Mathf.Lerp(maxPositionY, 0f, distance / 8f);

        // 近距离缩放（双人特写）
        if (targetArray.Length <= 2)
        {
            var pairDistanceFactor = Mathf.InverseLerp(1.0f, 3.0f, minPairDistance);
            var zoomMultiplier = Mathf.Lerp(minScale, 1.0f, pairDistanceFactor);
            distance *= zoomMultiplier;
        }


        // 计算临时相机位置
        var initialCameraPosition = averageCenterPosition + targetToCameraDirection * distance;
        float viewportCenterX, viewportCenterY;
        ComputeViewportCenter(cornerList, initialCameraPosition, cameraRight, cameraUp, cameraForward, horizontalTangent, verticalTangent, out viewportCenterX, out viewportCenterY);

        // 视觉重心居中补正
        var screenWidthInWorld = 2f * horizontalTangent * distance;  // 屏幕宽度（米）
        var screenHeightInWorld = 2f * verticalTangent * distance;   // 屏幕高度（米）
        var horizontalOffsetInViewport = viewportCenterX - 0.5f;
        var verticalOffsetInViewport = viewportCenterY - 0.5f;
        var horizontalShiftDistance = horizontalOffsetInViewport * screenWidthInWorld;  // 需要移动的横向距离（米）
        var verticalShiftDistance = verticalOffsetInViewport * screenHeightInWorld;     // 需要移动的纵向距离（米）

        // 近距离时补正较强，远距离时补正较弱
        var correctionStrength = Mathf.InverseLerp(2f, 6f, distance);
        averageCenterPosition += (cameraRight * horizontalShiftDistance + cameraUp * verticalShiftDistance) * correctionStrength;


        // 计算相机和注视点的位置
        targetCameraPosition = averageCenterPosition + targetToCameraDirection * distance;//相机的位置
        var lookPosition = averageCenterPosition + Vector3.up * chestOffset;//注视点的位置
        targetCameraRotation = Quaternion.LookRotation(lookPosition - targetCameraPosition, cameraUp);//计算目标旋转

        // 距离越远，速度越快；距离越近，速度越慢
        var distanceToTargetCameraPosition = Vector3.Distance(transform.position, targetCameraPosition);
        var adaptiveFollowSpeed = Mathf.Lerp(followSpeed, maxFollowSpeed, Mathf.InverseLerp(0f, maxFollowDistance, distanceToTargetCameraPosition));

        // 平滑移动相机位置和旋转
        transform.position = Vector3.Lerp(transform.position, targetCameraPosition, Time.deltaTime * adaptiveFollowSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetCameraRotation, Time.deltaTime * adaptiveFollowSpeed);
        if (uiCameraTransform)
        {
            uiCameraTransform.position = transform.position;
            uiCameraTransform.rotation = transform.rotation;
        }
    }

    // Bounds的八个顶点坐标
    IEnumerable<Vector3> GetBoundscornerList(Bounds b)
    {
        var e = b.extents;
        var c = b.center;
        yield return c + new Vector3(+e.x, +e.y, +e.z);
        yield return c + new Vector3(+e.x, +e.y, -e.z);
        yield return c + new Vector3(+e.x, -e.y, +e.z);
        yield return c + new Vector3(+e.x, -e.y, -e.z);
        yield return c + new Vector3(-e.x, +e.y, +e.z);
        yield return c + new Vector3(-e.x, +e.y, -e.z);
        yield return c + new Vector3(-e.x, -e.y, +e.z);
        yield return c + new Vector3(-e.x, -e.y, -e.z);
    }

    //计算所有顶点在相机坐标系中的最大范围
    void ComputeExtents(List<Vector3> cornerList, Vector3 centerPosition,
                        Vector3 cameraRight, Vector3 cameraUp, Vector3 cameraForward,
                        out float maxHorizontalDistance, out float maxVerticalDistance,
                        out float nearestDepth, out float farthestDepth)
    {
        maxHorizontalDistance = 0f;
        maxVerticalDistance = 0f;
        nearestDepth = float.MaxValue;
        farthestDepth = float.MinValue;

        foreach (var cornerPosition in cornerList)
        {
            var offsetFromCenter = cornerPosition - centerPosition;
            var horizontalDistance = Vector3.Dot(offsetFromCenter, cameraRight);
            var verticalDistance = Vector3.Dot(offsetFromCenter, cameraUp);
            var depthDistance = Vector3.Dot(offsetFromCenter, cameraForward);

            if (Mathf.Abs(horizontalDistance) > maxHorizontalDistance)
            {
                maxHorizontalDistance = Mathf.Abs(horizontalDistance);
            }
            if (Mathf.Abs(verticalDistance) > maxVerticalDistance)
            {
                maxVerticalDistance = Mathf.Abs(verticalDistance);
            }
            if (depthDistance < nearestDepth)
            {
                nearestDepth = depthDistance;
            }
            if (depthDistance > farthestDepth)
            {
                farthestDepth = depthDistance;
            }
        }
    }

    // 临时相机的位置计算矩形的视口中心
    void ComputeViewportCenter(List<Vector3> cornerList, Vector3 cameraPosition,
                                Vector3 cameraRight, Vector3 cameraUp, Vector3 cameraForward,
                                float horizontalTangent, float verticalTangent,
                                out float viewportCenterX, out float viewportCenterY)
    {
        float minViewportX = 1e9f, maxViewportX = -1e9f;
        float minViewportY = 1e9f, maxViewportY = -1e9f;

        foreach (var cornerPosition in cornerList)
        {
            var offsetFromCamera = cornerPosition - cameraPosition;
            var cameraSpaceX = Vector3.Dot(offsetFromCamera, cameraRight);
            var cameraSpaceY = Vector3.Dot(offsetFromCamera, cameraUp);
            var cameraSpaceZ = Vector3.Dot(offsetFromCamera, cameraForward);

            // 跳过相机后方的点
            if (cameraSpaceZ <= 0f)
            {
                continue;
            }

            // 屏幕坐标 = 相机坐标 / 深度
            var viewportX = 0.5f + 0.5f * (cameraSpaceX / (cameraSpaceZ * horizontalTangent));
            var viewportY = 0.5f + 0.5f * (cameraSpaceY / (cameraSpaceZ * verticalTangent));

            // 安全限制
            viewportX = Mathf.Clamp(viewportX, -1f, 2f);
            viewportY = Mathf.Clamp(viewportY, -1f, 2f);

            if (viewportX < minViewportX)
            {
                minViewportX = viewportX;
            }
            if (viewportX > maxViewportX)
            {
                maxViewportX = viewportX;
            }
            if (viewportY < minViewportY)
            {
                minViewportY = viewportY;
            }
            if (viewportY > maxViewportY)
            {
                maxViewportY = viewportY;
            }
        }

        // 计算中心点
        if (minViewportX > maxViewportX || minViewportY > maxViewportY)
        {
            viewportCenterX = 0.5f;
            viewportCenterY = 0.5f;
        }
        else
        {
            viewportCenterX = (minViewportX + maxViewportX) * 0.5f;
            viewportCenterY = (minViewportY + maxViewportY) * 0.5f;
        }
    }
}
