using UnityEngine;

// **********************************************
// 이 스크립트는 Old Input Manager (Input.GetAxis) 방식입니다.
// **********************************************
public class VehicleController : MonoBehaviour
{
    // 이 스크립트는 차량(Jeep)에 부착됩니다.

    [Header("차량 설정")]
    public float motorForce = 1500f; // 가속 힘
    public float maxSteerAngle = 30f; // 최대 조향 각도
    public float brakeForce = 3000f; // 제동 힘 (OnDisable 시 사용)

    [Header("카메라 설정 (차량 자식으로 연결된 카메라 오브젝트)")]
    // Car_ThirdPerson_Camera 오브젝트의 Transform을 여기에 연결합니다.
    public Transform vehicleCamera;
    public float cameraRotationSpeed = 3f; // 카메라 마우스 회전 속도
    public float cameraSmoothSpeed = 5f; // 차량 움직임에 따른 카메라 회전 부드러움

    private float _yaw = 0f; // Yaw (수평 회전)
    private float _pitch = 0f; // Pitch (수직 회전)

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

    // Start() 대신 OnEnable() 사용: 스크립트가 활성화될 때 (탑승 시) 호출
    void OnEnable()
    {
        // 마우스 커서를 숨기고 중앙에 잠급니다.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 초기 카메라 Yaw 값을 차량의 현재 회전과 일치시킵니다.
        if (vehicleCamera != null)
        {
            // 차량 회전값을 기준으로 초기 Yaw 설정 (월드 기준 Y축 회전)
            _yaw = transform.rotation.eulerAngles.y;
        }

        // 제동 토크를 0으로 초기화합니다.
        ApplyBrake(0f);
    }

    // OnDisable() 함수 추가: 스크립트가 비활성화될 때 (하차 시) 호출
    void OnDisable()
    {
        // 커서 잠금을 해제하고 보이게 합니다.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 하차 시 차량의 움직임을 멈추기 위해 제동 토크를 적용합니다.
        ApplyBrake(brakeForce); // 정의된 brakeForce를 사용

        // 모든 Motor Torque를 0으로 설정하여 즉시 가속을 멈춥니다.
        wc_FrontLeft.motorTorque = 0f;
        wc_FrontRight.motorTorque = 0f;
        wc_BackLeft.motorTorque = 0f;
        wc_BackRight.motorTorque = 0f;
    }

    private void FixedUpdate()
    {
        // 1. 입력값 직접 읽어오기 (Old Input Manager 방식)
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // 하차 시 적용된 브레이크를 해제합니다. (탑승 중에는 제동이 없어야 가속됨)
        // 수동 브레이크 구현을 위해 여기에 Input.GetAxis("Jump") 등을 사용할 수 있습니다.
        ApplyBrake(0f);

        // 2. 조향 (Steering) 적용
        float steer = maxSteerAngle * horizontalInput;
        wc_FrontLeft.steerAngle = steer;
        wc_FrontRight.steerAngle = steer;

        // 3. 가속/후진 (Motor Torque) 적용
        float motor = verticalInput * motorForce;

        wc_FrontLeft.motorTorque = motor;
        wc_FrontRight.motorTorque = motor;
        wc_BackLeft.motorTorque = motor;
        wc_BackRight.motorTorque = motor;
    }

    private void Update()
    {
        // 1. 바퀴 시각 업데이트
        UpdateWheelVisual(wc_FrontLeft, wheel_FrontLeft);
        UpdateWheelVisual(wc_FrontRight, wheel_FrontRight);
        UpdateWheelVisual(wc_BackLeft, wheel_BackLeft);
        UpdateWheelVisual(wc_BackRight, wheel_BackRight);

        // 2. 카메라 제어
        if (vehicleCamera != null)
        {
            HandleCameraControl();
        }

        // 참고: ESC 키로 커서 잠금 해제하는 로직은 OnDisable()로 이동했으므로 제거합니다.
    }

    // Wheel Collider의 상태를 3D 모델에 적용하는 함수
    void UpdateWheelVisual(WheelCollider collider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);

        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }

    // 카메라 회전 제어 함수
    void HandleCameraControl()
    {
        // 마우스 입력으로 카메라 회전 각도 계산
        float mouseX = Input.GetAxis("Mouse X") * cameraRotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * cameraRotationSpeed;

        _yaw += mouseX;
        _pitch -= mouseY; // Y축은 반전

        // Pitch (수직) 회전 제한
        _pitch = Mathf.Clamp(_pitch, -10f, 60f); // 필요에 따라 각도 조절 가능

        // 차량 회전각을 _yaw에 부드럽게 보간하여 카메라가 차량의 움직임을 따라가도록 합니다.
        float targetYaw = transform.rotation.eulerAngles.y;
        _yaw = Mathf.LerpAngle(_yaw, targetYaw, Time.deltaTime * cameraSmoothSpeed);

        // 카메라의 최종 회전 적용:
        Quaternion targetWorldRotation = Quaternion.Euler(_pitch, _yaw, 0f);

        // vehicleCamera (Car_ThirdPerson_Camera)의 월드 회전을 최종 회전으로 설정합니다.
        vehicleCamera.rotation = targetWorldRotation;
    }

    // 모든 바퀴에 제동 토크를 적용하는 헬퍼 함수
    void ApplyBrake(float torque)
    {
        wc_FrontLeft.brakeTorque = torque;
        wc_FrontRight.brakeTorque = torque;
        wc_BackLeft.brakeTorque = torque;
        wc_BackRight.brakeTorque = torque;
    }
}