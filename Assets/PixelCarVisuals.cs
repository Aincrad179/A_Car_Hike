using UnityEngine;

/// <summary>
/// 像素赛车视觉增强脚本：处理转弯时的侧倾 (Roll) 与车头视觉偏转 (Yaw)。
/// 该脚本直接作用于包含 PixelStacker 的 Visuals 子物体。
/// </summary>
public class PixelCarVisuals : MonoBehaviour
{
    [Header("控制源")]
    public PoseSteeringManager inputManager;

    [Header("侧倾设置 (Roll Tilt)")]
    [Tooltip("转向时车身绕 Z 轴侧倾的强度（角度）")]
    public float tiltAmount = 15f;
    [Tooltip("侧倾平滑速度")]
    public float tiltSmoothSpeed = 8f;

    [Header("车头偏转设置 (Visual Yaw)")]
    [Tooltip("转向时车头相对于行进方向额外偏转的角度（产生漂移感）")]
    public float yawAmount = 15f;
    [Tooltip("偏转平滑速度")]
    public float yawSmoothSpeed = 10f;

    private Quaternion _baseRotation;
    private float _currentTilt;
    private float _currentYaw;

    void Start()
    {
        if (inputManager == null)
            inputManager = FindObjectOfType<PoseSteeringManager>();

        // 记录物体在场景中原本的局部旋转（作为偏移基准）
        _baseRotation = transform.localRotation;
    }

    void Update()
    {
        if (inputManager == null) return;

        // 1. 计算目标侧倾角度 (Roll - 绕车头轴)
        // 向左转时，车身向右倾斜 (反向偏移)
        float targetTilt = -inputManager.steeringValue * tiltAmount;
        _currentTilt = Mathf.Lerp(_currentTilt, targetTilt, Time.deltaTime * tiltSmoothSpeed);

        // 2. 计算目标车头偏转角度 (Yaw - 绕垂直轴)
        // 向左转时，车头向左进一步偏转 (正向偏移)
        float targetYaw = inputManager.steeringValue * yawAmount;
        _currentYaw = Mathf.Lerp(_currentYaw, targetYaw, Time.deltaTime * yawSmoothSpeed);

        // 3. 将视觉偏移叠加到原始旋转上
        // 在 Unity 3D 坐标系中：
        // 假设 Y 是 Up（偏转 Yaw），Z 是 Forward（侧倾 Roll）
        Quaternion visualOffset = Quaternion.Euler(0, _currentYaw, _currentTilt);
        
        transform.localRotation = _baseRotation * visualOffset;
    }
}
