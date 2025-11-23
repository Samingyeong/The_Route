1. 스크립트 개요 및 역할
VehicleEntryExitManager는 플레이어와 차량 간의 상호작용을 관리하는 핵심 매니저 역할을 합니다. 이 스크립트는 플레이어의 입력, 특히 'E' 키 입력을 감지하여 차량 제어 스크립트(VehicleController)를 활성화하거나 비활성화하고, 게임 시점을 플레이어 카메라와 차량 카메라 사이에서 전환합니다. 이 시스템의 주요 목표는 단일 키로 탑승과 하차를 모두 처리하고, 이 과정에서 차량 제어권과 카메라 렌더링을 정확하게 전환하는 것입니다.

주요 기능으로는 'E' 키 하나로 탑승과 하차를 처리하는 단일 키 제어 기능이 있습니다. 또한, 탑승 시에는 VehicleController와 차량 카메라를 켜고, 하차 시에는 이들을 끄는 방식으로 제어의 활성화 상태를 관리합니다. 카메라 전환 측면에서는 탑승 시 플레이어 카메라를 끄고 차량 카메라를 켜며, 하차 시 이들을 역전환하여 렌더링 오류를 방지합니다. 마지막으로, 플레이어 오브젝트 전체를 비활성화하여 숨기고, 하차 시 지정된 위치(playerExitPoint)에 다시 활성화하여 플레이어 상태를 관리합니다.

2. Unity 설정 및 요구 사항
이 스크립트는 차량 오브젝트에 부착해야 하며, 인스펙터에서 필수 변수를 연결해야 정상적으로 작동합니다.

차량 오브젝트에 필요한 컴포넌트로는 차량의 물리 제어 및 카메라 Transform 제어를 담당하는 **VehicleController**가 필수이며, 본 스크립트인 VehicleEntryExitManager 역시 차량에 부착되어야 합니다. 또한, 카메라 오브젝트인 **Car_ThirdPerson_Camera**는 VehicleController의 Vehicle Camera 슬롯에 연결되어 있어야 하며, 이 오브젝트에 Camera 컴포넌트가 부착되어 있어야 합니다.

인스펙터 설정에서 Player Exit Point 변수에는 플레이어가 차량에서 내릴 때 안전하게 스폰될 위치와 방향을 지정하는 빈 게임 오브젝트의 Transform을 연결해야 합니다.

게임 시작 전 초기 상태로는 차량 오브젝트의 VehicleController 컴포넌트와 Car_ThirdPerson_Camera 오브젝트의 Camera 컴포넌트가 비활성화(체크 해제) 상태여야 합니다.

3. 스크립트 핵심 로직
void Awake() 함수는 게임 시작 시 실행되어 초기 설정을 담당합니다. 이 함수는 필요한 컴포넌트(예: VehicleController, 차량 카메라)를 참조하고, 차량 제어 및 차량 카메라 렌더링을 멈추기 위해 이들의 enabled 속성을 false로 설정합니다.

void Update() 함수는 프레임마다 실행되어 플레이어의 'E' 키 입력을 처리합니다. 현재 탑승 중일 경우('E' 입력 시) ExitVehicle() 함수를 호출하여 하차를 진행하고, 하차 상태일 경우('E' 입력 시) EnterVehicle() 함수를 호출하여 탑승을 시도합니다.

public void EnterVehicle(GameObject player) (탑승) 함수는 플레이어 오브젝트를 비활성화하여 플레이어 캐릭터와 카메라를 숨긴 후, **차량 카메라의 Camera.enabled**를 true로, **VehicleController.enabled**를 true로 설정하여 차량 시점 전환 및 제어를 활성화합니다.

public void ExitVehicle() (하차) 함수는 차량 카메라와 VehicleController를 비활성화하여 제어를 중지시킵니다. 이후, 플레이어를 playerExitPoint 위치로 이동시킨 후, **currentPlayer.SetActive(true)**를 호출하여 플레이어 캐릭터와 카메라를 다시 활성화하고 시점을 복구합니다. 최종적으로 isOccupied 상태를 false로 설정하여 다음 탑승을 준비합니다.