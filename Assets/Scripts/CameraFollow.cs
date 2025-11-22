using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("타겟 설정")]
    [SerializeField] private Transform target; // Player
    [SerializeField] private Transform cameraRoot; // CameraRoot (Player의 자식)
    
    [Header("카메라 위치 설정")]
    [SerializeField] private float eyeHeight = 1.8f; // 눈 높이 (플레이어 발 기준, 미터 단위)
    [SerializeField] private Vector3 offset = new Vector3(0, 0, 0); // 추가 오프셋 (필요시)
    
    [Header("회전 설정")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float maxVerticalAngle = 80f;
    
    [Header("부드러운 추적")]
    [SerializeField] private bool smoothFollow = true;
    [SerializeField] private float smoothSpeed = 10f;
    
    [Header("카메라 충돌 설정")]
    [SerializeField] private float cameraCollisionRadius = 0.2f; // 카메라 충돌 체크 반경
    [SerializeField] private LayerMask obstacleLayer = -1; // 충돌 체크할 레이어
    
    private float rotationX = 0f;
    private float rotationY = 0f;
    
    void Start()
    {
        // Player 자동 찾기
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = GameObject.Find("Player");
            }
            if (player == null)
            {
                player = GameObject.Find("player");
            }
            if (player != null)
            {
                target = player.transform;
            }
        }
        
        // CameraRoot 자동 찾기
        if (cameraRoot == null && target != null)
        {
            cameraRoot = FindChildTransform(target, "CameraRoot");
            
            // CameraRoot가 없으면 이 스크립트가 붙은 오브젝트를 사용
            if (cameraRoot == null)
            {
                cameraRoot = transform;
            }
        }
        
        // 초기 회전값 설정
        if (target != null)
        {
            rotationY = target.eulerAngles.y;
        }
        
        if (cameraRoot != null)
        {
            rotationX = cameraRoot.localEulerAngles.x;
            // 각도를 -180 ~ 180 범위로 정규화
            if (rotationX > 180f)
                rotationX -= 360f;
        }
        
        // 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    // 자식 Transform 재귀적으로 찾기
    Transform FindChildTransform(Transform parent, string name)
    {
        if (parent == null) return null;
        
        foreach (Transform child in parent)
        {
            if (child.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            {
                return child;
            }
            Transform found = FindChildTransform(child, name);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }
    
    void LateUpdate()
    {
        if (target == null || cameraRoot == null) return;
        
        // 마우스 입력으로 카메라 회전
        HandleMouseLook();
        
        // 카메라 위치 업데이트
        UpdateCameraPosition();
    }
    
    void HandleMouseLook()
    {
        // 마우스 입력 받기
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // 수평 회전 (Y축) - Player도 함께 회전
        rotationY += mouseX;
        target.rotation = Quaternion.Euler(0, rotationY, 0);
        
        // 수직 회전 (X축) - CameraRoot만 회전 (Main Camera는 자식이라 따라감)
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
    }
    
    void UpdateCameraPosition()
    {
        // CameraRoot의 localPosition 설정 (플레이어 발 기준으로 눈 높이만큼 위로)
        // X, Z는 offset 적용, Y는 기본값을 설정하되 HeadBob이 수정할 수 있도록 함
        Vector3 desiredLocalPos = new Vector3(offset.x, eyeHeight + offset.y, offset.z);
        
        // CameraRoot 회전 적용 (수직 회전만 - 1인칭 시점)
        cameraRoot.localRotation = Quaternion.Euler(rotationX, 0, 0);
        
        // CameraRoot의 localPosition 업데이트
        // Y는 HeadBob이 제어하므로 처음 설정 후에는 유지
        Vector3 currentLocalPos = cameraRoot.localPosition;
        
        // 처음 시작할 때나 Y 값이 0에 가까우면 기본값으로 설정
        if (Mathf.Abs(currentLocalPos.y) < 0.01f)
        {
            cameraRoot.localPosition = desiredLocalPos;
        }
        else
        {
            // X, Z만 업데이트하고 Y는 HeadBob이 제어하도록 유지
            if (!smoothFollow)
            {
                cameraRoot.localPosition = new Vector3(desiredLocalPos.x, currentLocalPos.y, desiredLocalPos.z);
            }
            else
            {
                float newX = Mathf.Lerp(currentLocalPos.x, desiredLocalPos.x, smoothSpeed * Time.deltaTime);
                float newZ = Mathf.Lerp(currentLocalPos.z, desiredLocalPos.z, smoothSpeed * Time.deltaTime);
                cameraRoot.localPosition = new Vector3(newX, currentLocalPos.y, newZ);
            }
        }
    }
    
    Vector3 CheckCameraCollision(Vector3 desiredPosition, Vector3 referencePosition)
    {
        // 기준 위치(플레이어)에서 원하는 카메라 위치로의 방향과 거리
        Vector3 direction = desiredPosition - referencePosition;
        float distance = direction.magnitude;
        
        if (distance < 0.01f)
        {
            return desiredPosition;
        }
        
        direction.Normalize();
        
        // Raycast로 충돌 체크 (기준 위치에서 카메라 위치로)
        RaycastHit hit;
        if (Physics.SphereCast(
            referencePosition, 
            cameraCollisionRadius, 
            direction, 
            out hit, 
            distance, 
            obstacleLayer))
        {
            // 충돌이 발생하면 충돌 지점에서 약간 앞쪽으로 카메라 배치
            float safeDistance = hit.distance - cameraCollisionRadius * 2f;
            if (safeDistance < 0.1f)
            {
                safeDistance = 0.1f; // 최소 거리 보장
            }
            return referencePosition + direction * safeDistance;
        }
        
        return desiredPosition;
    }
    
    void OnDisable()
    {
        // 스크립트 비활성화 시 커서 잠금 해제
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}


