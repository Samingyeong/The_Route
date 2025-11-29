using UnityEngine;
using StoreGame; // HealthSystem 네임스페이스 추가

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
    
    [Header("카메라 쉐이크 설정")]
    [SerializeField] private float shakeIntensity = 2f; // 쉐이크 강도
    [SerializeField] private float shakeDuration = 0.3f; // 쉐이크 지속 시간
    
    private float rotationX = 0f;
    private float rotationY = 0f;
    
    // 카메라 쉐이크 변수
    private float currentShakeDuration = 0f;
    private float currentShakeIntensity = 0f;
    private Vector3 originalLocalPosition;
    private HealthSystem playerHealthSystem;
    
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
            
            // 원래 로컬 위치 저장
            originalLocalPosition = cameraRoot.localPosition;
        }
        
        // 플레이어 HealthSystem 찾기 및 이벤트 구독
        if (target != null)
        {
            playerHealthSystem = target.GetComponent<HealthSystem>();
            if (playerHealthSystem != null)
            {
                playerHealthSystem.OnDamageTaken += OnPlayerDamaged;
            }
        }
        
        // 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    // 플레이어가 데미지를 받았을 때 호출
    void OnPlayerDamaged(float damageAmount)
    {
        // 카메라 쉐이크 시작
        StartShake();
    }
    
    // 카메라 쉐이크 시작
    void StartShake()
    {
        currentShakeDuration = shakeDuration;
        currentShakeIntensity = shakeIntensity;
    }
    
    // 카메라 쉐이크 업데이트
    void UpdateShake()
    {
        if (currentShakeDuration > 0f)
        {
            // 랜덤한 방향으로 쉐이크 적용
            Vector3 shakeOffset = Random.insideUnitSphere * currentShakeIntensity;
            // X, Y축만 쉐이크 (Z축은 깊이감 때문에 제외하는 것이 좋음)
            shakeOffset.z = 0f;
            
            // 쉐이크를 카메라 로컬 위치에 추가
            cameraRoot.localPosition = originalLocalPosition + shakeOffset;
            
            // 쉐이크 감쇠
            currentShakeDuration -= Time.deltaTime;
            currentShakeIntensity = Mathf.Lerp(shakeIntensity, 0f, 1f - (currentShakeDuration / shakeDuration));
        }
        else
        {
            // 쉐이크가 끝나면 원래 위치로 복귀
            if (cameraRoot != null)
            {
                cameraRoot.localPosition = originalLocalPosition;
            }
        }
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
        
        // 카메라 쉐이크 업데이트
        UpdateShake();
        
        // 카메라 위치 업데이트
        UpdateCameraPosition();
    }
    
    void HandleMouseLook()
    {
        // 쉐이크 중일 때는 마우스 입력 감소 (자연스러운 느낌)
        float shakeMultiplier = currentShakeDuration > 0f ? 0.5f : 1f;
        
        // 마우스 입력 받기
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * shakeMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * shakeMultiplier;
        
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
        
        // 쉐이크 중이 아닐 때만 원래 위치 업데이트
        if (currentShakeDuration <= 0f)
        {
            originalLocalPosition = desiredLocalPos;
        }
        
        // CameraRoot 회전 적용 (수직 회전만 - 1인칭 시점)
        cameraRoot.localRotation = Quaternion.Euler(rotationX, 0, 0);
        
        // CameraRoot의 localPosition 업데이트
        // Y는 HeadBob이 제어하므로 처음 설정 후에는 유지
        Vector3 currentLocalPos = cameraRoot.localPosition;
        
        // 쉐이크 중이 아닐 때만 기본 위치 업데이트
        if (currentShakeDuration <= 0f)
        {
            // 처음 시작할 때나 Y 값이 0에 가까우면 기본값으로 설정
            if (Mathf.Abs(currentLocalPos.y) < 0.01f)
            {
                cameraRoot.localPosition = desiredLocalPos;
                originalLocalPosition = desiredLocalPos;
            }
            else
            {
                // X, Z만 업데이트하고 Y는 HeadBob이 제어하도록 유지
                if (!smoothFollow)
                {
                    Vector3 newPos = new Vector3(desiredLocalPos.x, currentLocalPos.y, desiredLocalPos.z);
                    cameraRoot.localPosition = newPos;
                    originalLocalPosition = newPos;
                }
                else
                {
                    float newX = Mathf.Lerp(currentLocalPos.x, desiredLocalPos.x, smoothSpeed * Time.deltaTime);
                    float newZ = Mathf.Lerp(currentLocalPos.z, desiredLocalPos.z, smoothSpeed * Time.deltaTime);
                    Vector3 newPos = new Vector3(newX, currentLocalPos.y, newZ);
                    cameraRoot.localPosition = newPos;
                    originalLocalPosition = newPos;
                }
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
        // 이벤트 구독 해제
        if (playerHealthSystem != null)
        {
            playerHealthSystem.OnDamageTaken -= OnPlayerDamaged;
        }
        
        // 스크립트 비활성화 시 커서 잠금 해제
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}


