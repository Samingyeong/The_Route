using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    
    [Header("컴포넌트")]
    [SerializeField] private CharacterController characterController;
    
    private Vector3 velocity;
    private bool isGrounded;
    private float gravity = -9.81f;
    private bool isRunning = false;
    
    void Start()
    {
        // CharacterController 자동 찾기
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }
        
        // CharacterController가 없으면 추가
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.height = 2f;
            characterController.radius = 0.5f;
            characterController.center = new Vector3(0, 1, 0);
        }
    }
    
    void Update()
    {
        HandleMovement();
        HandleJump();
    }
    
    void HandleMovement()
    {
        // 지면 체크
        isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // 작은 값으로 지면에 붙이기
        }
        
        // WASD 입력 받기
        float horizontal = Input.GetAxis("Horizontal"); // A, D
        float vertical = Input.GetAxis("Vertical");       // W, S
        
        // Shift로 달리기
        isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : moveSpeed;
        
        // 이동 방향 계산 (카메라 기준으로 이동)
        Camera mainCam = Camera.main;
        Vector3 moveDirection = Vector3.zero;
        
        if (mainCam != null)
        {
            // 카메라의 앞방향과 오른쪽 방향 가져오기 (Y축 제외)
            Vector3 forward = mainCam.transform.forward;
            Vector3 right = mainCam.transform.right;
            
            forward.y = 0f;
            right.y = 0f;
            
            forward.Normalize();
            right.Normalize();
            
            // 카메라 기준으로 이동 방향 계산
            moveDirection = forward * vertical + right * horizontal;
        }
        else
        {
            // 카메라가 없으면 Player 회전 기준
            moveDirection = transform.forward * vertical + transform.right * horizontal;
        }
        
        // 이동 적용
        if (moveDirection.magnitude > 0.1f)
        {
            moveDirection.Normalize();
            characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
        }
        
        // 중력 적용
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
    
    void HandleJump()
    {
        // Space로 점프
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }
    
}

