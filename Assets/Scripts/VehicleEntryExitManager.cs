using UnityEngine;

// 이 스크립트는 차량(Jeep)에 부착됩니다.
public class VehicleEntryExitManager : MonoBehaviour
{
    // 차량 제어 스크립트
    private VehicleController vehicleController;

    [Header("차량 탑승 설정")]
    // 하차 시 플레이어가 스폰될 위치의 Transform을 여기에 연결합니다.
    public Transform playerExitPoint;

    // 차량에 부착된 카메라 컴포넌트를 직접 제어하기 위한 변수
    private Camera vehicleCam;

    // 플레이어의 메인 카메라 컴포넌트 (하차 후 활성화를 위해 참조 필요)
    private Camera playerCam;

    private GameObject currentPlayer; // 현재 탑승 중인 플레이어 객체
    private bool isOccupied = false; // 현재 탑승 여부

    void Awake()
    {
        // 1. VehicleController 컴포넌트 참조
        vehicleController = GetComponent<VehicleController>();

        // 2. 차량 카메라 컴포넌트 참조
        // VehicleController의 public 변수 vehicleCamera (Transform)에서 Camera 컴포넌트를 가져옵니다.
        if (vehicleController != null && vehicleController.vehicleCamera != null)
        {
            vehicleCam = vehicleController.vehicleCamera.GetComponent<Camera>();
        }

        // 시작 시 VehicleController 비활성화
        if (vehicleController != null)
        {
            vehicleController.enabled = false;
        }

        // 시작 시 차량 카메라 비활성화 (플레이어 카메라가 렌더링하도록)
        if (vehicleCam != null)
        {
            vehicleCam.enabled = false;
        }
    }

    void Update()
    {
        // 'E' 키 입력 감지
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isOccupied)
            {
                // **1. 현재 탑승 중:** 'E' 키로 하차
                ExitVehicle();
            }
            else
            {
                // **2. 탑승 중이 아님:** 'E' 키로 탑승 시도

                // 예시: 태그가 "Player"인 객체를 찾습니다. (실제 게임에서는 근접 감지 로직으로 대체)
                GameObject player = GameObject.FindGameObjectWithTag("Player");

                if (player != null /* && IsPlayerNear(player) */)
                {
                    EnterVehicle(player);
                }
            }
        }
    }

    // 외부에서 호출될 탑승 함수
    public void EnterVehicle(GameObject player)
    {
        if (isOccupied) return;

        currentPlayer = player;
        isOccupied = true;

        // 플레이어 카메라 참조 (탑승 시 딱 한 번만 가져옵니다.)
        playerCam = currentPlayer.GetComponentInChildren<Camera>(true);

        // 1. 플레이어 제어 비활성화 및 위치 설정
        // 플레이어 캐릭터와 카메라를 비활성화합니다.
        currentPlayer.SetActive(false);

        // 2. 차량 카메라 컴포넌트 활성화
        if (vehicleCam != null)
        {
            vehicleCam.enabled = true;
        }

        // 3. VehicleController 활성화 (차량 제어 시작)
        vehicleController.enabled = true;

        Debug.Log("차량에 탑승했습니다. (E 키)");
    }

    // 외부에서 호출될 하차 함수
    public void ExitVehicle()
    {
        if (!isOccupied || currentPlayer == null) return;

        // 1. VehicleController 비활성화 (차량 제어 중지)
        vehicleController.enabled = false;

        // 2. 차량 카메라 컴포넌트 비활성화
        if (vehicleCam != null)
        {
            vehicleCam.enabled = false;
        }

        // 3. 플레이어의 물리/제어 활성화 및 위치 설정
        if (playerExitPoint != null)
        {
            currentPlayer.transform.position = playerExitPoint.position;
            currentPlayer.transform.rotation = playerExitPoint.rotation;
        }

        // 플레이어 캐릭터와 카메라를 다시 활성화합니다.
        currentPlayer.SetActive(true);

        // 4. 상태 초기화
        currentPlayer = null;
        playerCam = null;
        isOccupied = false;

        Debug.Log("차량에서 하차했습니다. (E 키)");
    }
}