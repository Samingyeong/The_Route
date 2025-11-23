ehicleController.cs 스크립트는 유니티의 WheelCollider 시스템과 Old Input Manager를 사용하여 기본적인 4륜 구동(4WD) 차량의 주행 로직을 구현합니다. 이 스크립트는 사용자의 키 입력을 물리적인 차량 움직임으로 변환하고, 그 결과를 3D 모델에 시각적으로 반영하는 핵심적인 역할을 합니다.

1. 스크립트의 기능 및 물리 처리
이 스크립트는 차량의 움직임에 필요한 물리 계산을 FixedUpdate 함수에서 처리합니다.

입력 처리: 유니티의 Old Input Manager를 통해 "Horizontal" (A/D 또는 좌/우 화살표) 입력값과 "Vertical" (W/S 또는 상/하 화살표) 입력값을 실시간으로 받아옵니다.

조향 (Steering): Horizontal 입력값을 Max Steer Angle과 곱하여 조향 각도를 계산한 뒤, 이 각도를 앞바퀴 두 개의 steerAngle 속성에 적용하여 차량의 방향을 전환합니다.

가속/토크 (Motor Torque): Vertical 입력값에 Motor Force를 곱하여 엔진 토크를 계산합니다. 이 토크는 네 바퀴 모두의 motorTorque 속성에 할당되어, 차량을 가속하거나 후진시키게 됩니다.

2. 시각적인 바퀴 모델 동기화
차량의 물리적인 움직임이 시각적으로 자연스럽게 보이도록, 스크립트는 Update 함수를 통해 물리적인 Wheel Collider의 상태를 3D 모델 바퀴에 동기화합니다.

UpdateWheelVisual 함수는 Wheel Collider의 현재 월드 위치와 회전값(GetWorldPose)을 정확하게 받아옵니다.

받아온 위치와 회전 정보를 실제 화면에 보이는 **3D 바퀴 모델(Wheel Models)**의 Transform에 적용함으로써, 모델 바퀴가 조향각에 맞춰 회전하고, 노면 상태에 따라 상하로 움직이는 물리적인 바퀴의 움직임을 완벽하게 따라하도록 만듭니다.

3. 사용을 위한 필수 요구 사항 및 설정
이 스크립트가 올바르게 작동하려면 다음 요소들이 차량 오브젝트에 할당되어야 합니다.

필수 컴포넌트: 차량의 루트 오브젝트에는 Rigidbody가, 바퀴 위치에는 4개의 Wheel Collider 컴포넌트가 각각 부착되어야 합니다.

Inspector 할당: 인스펙터 창의 스크립트 필드에 4개의 물리 Wheel Collider와 **4개의 시각적인 3D 바퀴 모델(Transform)**이 정확하게 연결되어야 합니다. 또한, 차량의 성능 특성에 맞춰 Motor Force와 Max Steer Angle 값을 적절히 설정해야 합니다.

이 스크립트는 모든 제어 로직을 컴포넌트 참조로 처리하므로, 프리팹으로 저장하거나 프로젝트에 공유할 때 할당된 Wheel Collider와 Transform 참조가 깨지지 않도록 Hierarchy 구조를 유지하는 것이 중요합니다.