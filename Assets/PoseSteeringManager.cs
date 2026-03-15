using UnityEngine;

/// <summary>
/// 将 BlazePose 提取的双手连线角度转换为赛车转向数值 (-1 到 1)，
/// 同时根据双手距离计算开关式油门/刹车数值（合拢加速，张开减速）。
/// </summary>
public class PoseSteeringManager : MonoBehaviour
{
    [Header("输入源")]
    public BlazePoseWristTracker tracker; 

    [Header("转向配置 (基于角度)")]
    [Tooltip("当双手连线倾斜达到此角度时，判定为 100% 转向")]
    public float maxSteeringAngle = 45f; 
    
    [Tooltip("忽略微小倾斜的死区角度")]
    public float deadZoneAngle = 5f;

    [Tooltip("数值平滑速度")]
    public float smoothSpeed = 10f;

    [Header("油门/速度控制配置")]
    [Tooltip("双手距离的阈值：小于此值加速，大于此值减速")]
    public float centerDistance = 0.4f;

    [Header("输出状态 (只读)")]
    [Range(-1f, 1f)]
    public float steeringValue; 
    [Range(-1f, 1f)]
    public float throttleValue;

    [Header("调试信息")]
    public bool isHandsDetected;
    public float currentAngle; // 当前双手连线的角度
    public float currentHandDistance; // 当前双手直线距离

    private float _targetSteering;
    private float _targetThrottle;

    void Update()
    {
        isHandsDetected = tracker != null && 
                          tracker.leftWristScore > 0.5f && 
                          tracker.rightWristScore > 0.5f;

        if (!isHandsDetected)
        {
            _targetSteering = 0f;
            _targetThrottle = 0f;
        }
        else
        {
            // 1. 转向逻辑 (基于角度机制)
            Vector2 handVector = (Vector2)tracker.leftWrist - (Vector2)tracker.rightWrist;
            currentAngle = Mathf.Atan2(handVector.y, handVector.x) * Mathf.Rad2Deg;

            if (Mathf.Abs(currentAngle) < deadZoneAngle)
            {
                _targetSteering = 0f;
            }
            else
            {
                _targetSteering = Mathf.Clamp(currentAngle / maxSteeringAngle, -1f, 1f);
            }

            // 2. 油门/加速减速逻辑 (开关式控制：合拢加速，张开减速)
            currentHandDistance = Vector3.Distance(tracker.leftWrist, tracker.rightWrist);
            
            if (currentHandDistance < centerDistance)
            {
                // 距离小于阈值：合拢加速
                _targetThrottle = 1f;
            }
            else
            {
                // 距离大于阈值：张开减速
                _targetThrottle = -1f;
            }
        }

        // 3. 平滑处理
        steeringValue = Mathf.Lerp(steeringValue, _targetSteering, Time.deltaTime * smoothSpeed);
        throttleValue = Mathf.Lerp(throttleValue, _targetThrottle, Time.deltaTime * smoothSpeed);
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = isHandsDetected ? Color.green : Color.red;

        if (!isHandsDetected && Application.isPlaying)
        {
            GUI.Label(new Rect(20, 20, 200, 30), "未检测到双手！", style);
        }
        else if (Application.isPlaying)
        {
            GUI.Label(new Rect(20, 20, 300, 30), $"当前角度: {currentAngle:F1}°", style);
            GUI.HorizontalSlider(new Rect(20, 50, 200, 30), steeringValue, -1f, 1f);

            GUI.Label(new Rect(20, 90, 300, 30), $"油门状态: {(throttleValue > 0 ? "加速" : "刹车")}", style);
            GUI.HorizontalSlider(new Rect(20, 120, 200, 30), throttleValue, -1f, 1f);
            
            GUI.Label(new Rect(20, 150, 300, 30), $"双手距离: {currentHandDistance:F2}", style);
        }
    }
}
