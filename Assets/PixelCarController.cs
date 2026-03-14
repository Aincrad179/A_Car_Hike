using UnityEngine;

/// <summary>
/// 3D 俯视角像素赛车控制器：XZ 平面移动，Y 轴为重力方向
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PixelCarController : MonoBehaviour
{
    [Header("控制源")]
    public PoseSteeringManager inputManager; 

    [Header("移动设置")]
    public float acceleration = 60f;     // 加速力
    public float maxSpeed = 25f;         // 最大速度
    public float turnSpeed = 150f;       // 转向速度
    
    [Range(0, 1)]
    [Tooltip("漂移系数：1 = 抓地，0 = 完全侧滑")]
    public float driftFactor = 0.95f;    

    private Rigidbody _rb;
    private float _steeringValue;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        
        // 1. 初始化物理属性
        _rb.useGravity = true;           // 启用重力
        _rb.drag = 0.8f;                 // 空气阻力
        _rb.angularDrag = 2.0f;          // 转向阻力

        // 2. 锁定 X 和 Z 轴旋转，防止赛车侧翻
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        // 自动查找输入管理器
        if (inputManager == null)
            inputManager = FindObjectOfType<PoseSteeringManager>();
    }

    void Update()
    {
        if (inputManager != null)
        {
            // 获取镜像后的体感转向数值
            _steeringValue = inputManager.steeringValue;
        }
    }

    void FixedUpdate()
    {
        ApplySteering();
        ApplyEngineForce();
        KillOrthogonalVelocity();
    }

    void ApplyEngineForce()
    {
        // 只有当前进速度小于最大速度时才加力
        if (_rb.velocity.magnitude < maxSpeed)
        {
            // 沿着赛车正前方 (XZ 平面) 施加力
            _rb.AddForce(transform.forward * acceleration, ForceMode.Acceleration);
        }
    }

    void ApplySteering()
    {
        // 绕着 Y 轴（垂直轴）进行左右转向
        float rotation = _steeringValue * turnSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, rotation, 0f);
        _rb.MoveRotation(_rb.rotation * turnRotation);
    }

    void KillOrthogonalVelocity()
    {
        // 实现赛车物理的核心：通过消除侧向速度来实现侧滑/漂移感
        // 分解当前速度为：前进速度 + 侧向速度
        Vector3 forwardVelocity = transform.forward * Vector3.Dot(_rb.velocity, transform.forward);
        Vector3 rightVelocity = transform.right * Vector3.Dot(_rb.velocity, transform.right);

        // 削减侧向速度分量，模拟轮胎抓地力
        _rb.velocity = forwardVelocity + (rightVelocity * driftFactor);
    }
}
