using UnityEngine;

/// <summary>
/// 3D 俯视角像素赛车控制器：XZ 平面移动，Y 轴为重力方向。
/// 包含基于双手距离的加速与减速逻辑。
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PixelCarController : MonoBehaviour
{
    [Header("控制源")]
    public PoseSteeringManager inputManager; 

    [Header("速度范围")]
    public float minSpeed = 5f;          // 最小巡航速度 (必须大于0)
    public float maxSpeed = 30f;         // 最大速度上限

    [Header("控制强度")]
    public float accelerationPower = 80f; // 加速时的推力
    public float brakePower = 100f;       // 减速时的制动力
    public float turnSpeed = 150f;       
    
    [Range(0, 1)]
    [Tooltip("漂移系数：1 = 抓地，0 = 完全侧滑")]
    public float driftFactor = 0.95f;    

    private Rigidbody _rb;
    private float _steeringValue;
    private float _throttleValue;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        
        // 1. 初始化物理属性
        _rb.useGravity = true;           
        _rb.drag = 0.8f;                 
        _rb.angularDrag = 2.0f;          

        // 2. 锁定 X 和 Z 轴旋转，防止赛车侧翻
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (inputManager == null)
            inputManager = FindObjectOfType<PoseSteeringManager>();
    }

    void Update()
    {
        if (inputManager != null)
        {
            // 获取转向和油门数值
            _steeringValue = inputManager.steeringValue;
            _throttleValue = inputManager.throttleValue;
        }
    }

    void FixedUpdate()
    {
        ApplySteering();
        ApplyDriveForce();
        KillOrthogonalVelocity();
    }

    void ApplyDriveForce()
    {
        // 计算当前在赛车正前方的速度分量
        float currentForwardSpeed = Vector3.Dot(_rb.velocity, transform.forward);

        if (_throttleValue > 0) 
        {
            // 加速阶段：只要没超过最高速
            if (currentForwardSpeed < maxSpeed)
            {
                _rb.AddForce(transform.forward * accelerationPower * _throttleValue, ForceMode.Acceleration);
            }
        }
        else if (_throttleValue < 0)
        {
            // 减速/刹车阶段：只要还没降到最低速
            if (currentForwardSpeed > minSpeed)
            {
                // 注意：_throttleValue 在此处为负值
                _rb.AddForce(transform.forward * brakePower * _throttleValue, ForceMode.Acceleration);
            }
        }

        // 最小速度保障逻辑：如果速度低于 minSpeed，自动补一点力
        if (currentForwardSpeed < minSpeed)
        {
            float boostForce = (minSpeed - currentForwardSpeed) * 5f; // 简单的比例补正
            _rb.AddForce(transform.forward * boostForce, ForceMode.Acceleration);
        }
    }

    void ApplySteering()
    {
        // 只有当赛车在移动时，转向才有意义（或者可以设定低速转向更慢）
        float rotation = _steeringValue * turnSpeed * Time.fixedDeltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, rotation, 0f);
        _rb.MoveRotation(_rb.rotation * turnRotation);
    }

    void KillOrthogonalVelocity()
    {
        // 侧滑逻辑
        Vector3 forwardVelocity = transform.forward * Vector3.Dot(_rb.velocity, transform.forward);
        Vector3 rightVelocity = transform.right * Vector3.Dot(_rb.velocity, transform.right);

        _rb.velocity = forwardVelocity + (rightVelocity * driftFactor);
    }
}
