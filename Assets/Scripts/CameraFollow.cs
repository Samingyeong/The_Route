using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("타겟 설정")]
    [SerializeField] private Transform target; // Player
    
    [Header("위치 오프셋")]
    [SerializeField] private Vector3 offset = new Vector3(0, 1.8f, 0); // 카메라 위치 (머리 높이, Rigidbody 기준)
    
    [Header("회전 설정")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float maxVerticalAngle = 80f;
    
    [Header("부드러운 추적")]
    [SerializeField] private bool smoothFollow = true;
    [SerializeField] private float smoothSpeed = 10f;
    
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
            if (player != null)
            {
                target = player.transform;
            }
        }
        
        // 초기 회전값 설정
        if (target != null)
        {
            rotationY = target.eulerAngles.y;
        }
        
        // 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
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
        
        // 수직 회전 (X축) - 카메라만 회전
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);
    }
    
    void UpdateCameraPosition()
    {
        // 목표 위치 계산 (Player 위치 + 오프셋)
        Vector3 targetPosition = target.position + offset;
        
        // 카메라 회전 적용
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
        
        // 카메라 위치 업데이트
        if (smoothFollow)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = targetPosition;
        }
    }
    
    void OnDisable()
    {
        // 스크립트 비활성화 시 커서 잠금 해제
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}

