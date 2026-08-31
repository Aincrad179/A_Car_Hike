using UnityEngine;

/// <summary>
/// 3D 俯视角像素赛车控制器：包含基于侧滑速度的漂移烟雾触发逻辑。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PixelCarController : MonoBehaviour
{
    [Header("控制源")]
    public PoseSteeringManager inputManager; 

    [Header("移动设置")]
    public float acceleration = 60f;     // 加速力
    public float maxSpeed = 25f;         // 最大速度
    public float turnSpeed = 120f;       // 转向速度
    
    [Range(0, 1)]
    [Tooltip("侧向速度保留比例：1 = 完全侧滑，0 = 完全抓地")]
    public float driftFactor = 0.96f;    

    [Header("漂移视觉反馈")]
    [Tooltip("漂移时触发的粒子系统（如轮胎烟雾）")]
    public ParticleSystem driftParticles; 
    [Tooltip("侧滑速度超过此值时开启粒子发射")]
    public float driftThreshold = 2.0f;

    private Rigidbody _rb;
    private float _steeringValue;
    private float _throttleValue;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        
        // 物理配置
        _rb.useGravity = true;
        _rb.drag = 0.5f;
        _rb.angularDrag = 2.0f;
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (inputManager == null)
            inputManager = FindObjectOfType<PoseSteeringManager>();

        // 初始状态下关闭粒子发射
        if (driftParticles != null)
        {
            var emission = driftParticles.emission;
            emission.enabled = false;
        }
    }

    void Update()
    {
        if (inputManager != null)
        {
            _steeringValue = inputManager.steeringValue;
            _throttleValue = inputManager.throttleValue;
        }
    }

    void FixedUpdate()
    {
        ApplySteering();
        ApplyDriveForce();
        KillOrthogonalVelocity();
        HandleDriftVisuals();
    }

    void ApplyDriveForce()
    {
        float currentForwardSpeed = Vector3.Dot(_rb.velocity, transform.forward);

        // 基础动力逻辑
        if (_throttleValue > 0 && currentForwardSpeed < maxSpeed)
        {
            _rb.AddForce(transform.forward * acceleration * _throttleValue, ForceMode.Acceleration);
        }
        else if (_throttleValue < 0 && currentForwardSpeed > 2f) // 限制最小速度不退后
        {
            _rb.AddForce(transform.forward * acceleration * _throttleValue, ForceMode.Acceleration);
        }
    }

    void ApplySteering()
    {
        float rotation = _steeringValue * turnSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, rotation, 0f);
        _rb.MoveRotation(_rb.rotation * turnRotation);
    }

    void KillOrthogonalVelocity()
    {
        Vector3 forwardVelocity = transform.forward * Vector3.Dot(_rb.velocity, transform.forward);
        Vector3 rightVelocity = transform.right * Vector3.Dot(_rb.velocity, transform.right);
        _rb.velocity = forwardVelocity + (rightVelocity * driftFactor);
    }

    /// <summary>
    /// 根据侧滑程度控制粒子效果的开关
    /// </summary>
    void HandleDriftVisuals()
    {
        if (driftParticles == null) return;

        // 计算侧向速度分量 (Lateral Velocity)
        float sideSpeed = Mathf.Abs(Vector3.Dot(_rb.velocity, transform.right));

        // 动态开启或关闭粒子发射
        var emission = driftParticles.emission;
        emission.enabled = sideSpeed > driftThreshold;
    }
}
