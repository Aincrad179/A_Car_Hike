using UnityEngine;

/// <summary>
/// 将 BlazePose 提取的手腕高度差转换为赛车转向数值 (-1 到 1)
/// </summary>
public class PoseSteeringManager : MonoBehaviour
{
    [Header("输入源")]
    public BlazePoseWristTracker tracker; 

    [Header("转向配置")]
    [Tooltip("手腕高度差达到多少时判定为 100% 转向")]
    public float sensitivity = 0.25f; 
    
    [Tooltip("忽略微小动作的死区范围")]
    public float deadZone = 0.03f;

    [Tooltip("数值平滑速度 (越小越平滑，但也越有延迟感)")]
    public float smoothSpeed = 10f;

    [Header("输出状态 (只读)")]
    [Range(-1f, 1f)]
    public float steeringValue; // 最终输出给赛车的数值

    [Header("调试信息")]
    public bool isHandsDetected;
    public float rawDifference; // 原始高度差

    private float _targetSteering;

    void Update()
    {
        // 1. 检查是否检测到双手 (置信度阈值建议 0.5)
        isHandsDetected = tracker != null && 
                          tracker.leftWristScore > 0.5f && 
                          tracker.rightWristScore > 0.5f;

        if (!isHandsDetected)
        {
            // 如果没检测到手，目标设为 0 (自动回正)
            _targetSteering = 0f;
        }
        else
        {
            // 2. 计算高度差
            // 由于摄像头通常是镜像的，且在 BlazePose 中 y 轴向下，
            // 我们通过 (左手y - 右手y) 来实现镜像后的直觉转向：
            // 玩家左手下压 -> 左手 y 变大 -> 得到正值 -> 向右转 (镜像视觉)
            rawDifference = tracker.leftWrist.y - tracker.rightWrist.y;

            // 3. 应用死区处理
            if (Mathf.Abs(rawDifference) < deadZone)
            {
                _targetSteering = 0f;
            }
            else
            {
                // 4. 映射到 -1 到 1 之间，并限制范围
                // 除以灵敏度，例如差值 0.25 达到满打方向盘
                _targetSteering = Mathf.Clamp(rawDifference / sensitivity, -1f, 1f);
            }
        }

        // 5. 平滑处理 (Lerp)，消除摄像头画面跳动带来的“打摆子”现象
        steeringValue = Mathf.Lerp(steeringValue, _targetSteering, Time.deltaTime * smoothSpeed);
    }

    // 在屏幕上画一个简单的状态显示，方便调试
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
            GUI.Label(new Rect(20, 20, 300, 30), $"当前转向强度: {steeringValue:F2}", style);
            // 画一个简单的进度条预览
            GUI.HorizontalSlider(new Rect(20, 60, 200, 30), steeringValue, -1f, 1f);
        }
    }
}
