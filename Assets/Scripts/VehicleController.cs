using UnityEngine;

// **********************************************
// 이 스크립트는 Old Input Manager (Input.GetAxis) 방식입니다.
// **********************************************
public class VehicleController : MonoBehaviour
{
    [Header("차량 설정")]
    public float motorForce = 1500f; // 가속 힘
    public float maxSteerAngle = 30f; // 최대 조향 각도

    [Header("Wheel Colliders (물리 바퀴)")]
    public WheelCollider wc_FrontLeft;
    public WheelCollider wc_FrontRight;
    public WheelCollider wc_BackLeft;
    public WheelCollider wc_BackRight;

    [Header("Wheel Models (시각 모델)")]
    public Transform wheel_FrontLeft;
    public Transform wheel_FrontRight;
    public Transform wheel_BackLeft;
    public Transform wheel_BackRight;

    private void FixedUpdate()
    {
        // 1. 입력값 직접 읽어오기 (Old Input Manager 방식)
        // Horizontal: A/D 또는 좌/우 화살표 키 (조향)
        // Vertical: W/S 또는 상/하 화살표 키 (가속/후진)
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // 2. 조향 (Steering) 적용
        // 앞바퀴 두 개에만 조향각을 적용합니다.
        float steer = maxSteerAngle * horizontalInput;
        wc_FrontLeft.steerAngle = steer;
        wc_FrontRight.steerAngle = steer;

        // 3. 가속/후진 (Motor Torque) 적용
        // 모든 바퀴에 힘을 전달합니다.
        wc_FrontLeft.motorTorque = verticalInput * motorForce;
        wc_FrontRight.motorTorque = verticalInput * motorForce;
        wc_BackLeft.motorTorque = verticalInput * motorForce;
        wc_BackRight.motorTorque = verticalInput * motorForce;
    }

    private void Update()
    {
        // 시각적 업데이트는 Update에서 처리합니다.
        // Wheel Collider의 상태를 실제 바퀴 모델에 반영합니다.
        UpdateWheelVisual(wc_FrontLeft, wheel_FrontLeft);
        UpdateWheelVisual(wc_FrontRight, wheel_FrontRight);
        UpdateWheelVisual(wc_BackLeft, wheel_BackLeft);
        UpdateWheelVisual(wc_BackRight, wheel_BackRight);
    }

    // Wheel Collider의 상태를 3D 모델에 적용하는 함수
    void UpdateWheelVisual(WheelCollider collider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        // 물리 바퀴의 현재 위치와 회전값을 가져옵니다.
        collider.GetWorldPose(out pos, out rot);

        // 3D 모델의 위치와 회전을 물리 바퀴와 동일하게 설정합니다.
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }
}