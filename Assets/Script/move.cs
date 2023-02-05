using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class move : MonoBehaviour
{
    [SerializeField]
    Transform Camera = default;

    [SerializeField, Range(0f, 100f)]
    float MaxSpeed = 5f;

    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f;
    [SerializeField, Range(0f, 10f)]
    float Jumpheight = 2f;
    [SerializeField, Range(0, 5)]//空中跳跃次数
    int MaxAirJumps = 2;
    int JumpPhase = 0;
    [SerializeField, Range(0f, 100f)]
    float maxAirAcceleration = 5f; //控制物体在空中的速度
    [SerializeField, Range(0f, 90f)]
    float maxGroundAngle = 25f;  //地面角度

/*     [SerializeField, Range(0f, 1f)]
    float Bounciness = 1f;

    [SerializeField]
    Rect allowArea = new Rect (-5f, -5f, 10f, 10f);
 */    Vector3 velocity;

    bool desiredJump;
    Rigidbody body;

    float minGroundDotProduct;
    Vector3 desiredVelocity;//理想速度或者说目标速度
    Vector3 constantNormal;
    int groundContactCount = 0;//与地面的接触点数
    bool onGround;
    bool OnGround => groundContactCount > 0;

    Vector3 upAxis, rightAxis, forwardAxis;
    private void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    Vector3 ProjectOnContactPlane(Vector3 vector3)
    {
        return vector3 - constantNormal * Vector3.Dot(vector3, constantNormal);
    }
    Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }


    void AdjustVelocity()
    {
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;
        float currentX = Vector3.Dot(velocity, xAxis);
        float currentZ = Vector3.Dot(velocity, zAxis);
        float maxSpeedChange = (onGround ? maxAcceleration : maxAirAcceleration) * Time.deltaTime;  //加速度（可调参）
        velocity = body.velocity;
        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);
        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }
    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        OnValidate();
    }

    private void Update()
    {
        Vector2 PlayerInput;
        PlayerInput.x = Input.GetAxis("Horizontal");
        PlayerInput.y = Input.GetAxis("Vertical");
        PlayerInput = Vector2.ClampMagnitude(PlayerInput, 1f);
        if(Camera)
        {
            Vector3 forward = Camera.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = Camera.right;
            right.y = 0f;
            right.Normalize();
            desiredVelocity = (forward * PlayerInput.y + right * PlayerInput.x) * MaxSpeed;//来控制相对运动
        }
        //desiredVelocity = new Vector3(PlayerInput.x, 0f, PlayerInput.y) * MaxSpeed;  //理想速度或者说是目标速度
        desiredJump |= Input.GetButtonDown("Jump");
    }

    void Jump()
    {
        if(OnGround || JumpPhase < MaxAirJumps)
        {
            JumpPhase += 1;
            float JumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * Jumpheight); 
            float alignSpeed = Vector3.Dot(velocity, constantNormal);
            if(alignSpeed > 0f)
            {
                //JumpSpeed = Mathf.Max(JumpSpeed - velocity.y, 0f); //为了防止在二段跳上升的过程中不会减速（有歧义）或者说限制向上速度
                JumpSpeed = Mathf.Max(JumpSpeed - alignSpeed, 0f);
            }
            velocity += constantNormal * JumpSpeed;
        }
    }
    void FixedUpdate()
    {
        AdjustVelocity();
        if(desiredJump)
        {
            desiredJump = false;
            if(OnGround)
            {
                JumpPhase = 0;
                if(groundContactCount > 1)constantNormal = constantNormal.normalized;
            } 
            else constantNormal = Vector3.up;  //不接触地面时跳跃沿着向上方向
            Jump();
        }
        body.velocity = velocity;
        ClearState();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //onGround = true;
        EvaluateCollision(collision);
    }

    private void OnCollisionStay(Collision collision) 
    {
        //onGround = true;    
        EvaluateCollision(collision);
    }

    void EvaluateCollision(Collision collision)
    {
        for(int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if(normal.y >= minGroundDotProduct)
            {
                groundContactCount += 1;
                constantNormal += normal;
            }
        }
    }

    void ClearState()
    {
        groundContactCount = 0;
        onGround = false;
        constantNormal = Vector3.zero;
    }
}