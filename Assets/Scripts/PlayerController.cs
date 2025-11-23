using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpForce = 3f; 
    
    [Header("컴포넌트")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator; 
    
    [Header("오디오")]
    [SerializeField] private AudioSource audioSource; // 소리 재생기
    [SerializeField] private AudioClip stepSound;     // 발소리 파일 (.wav)

    private Vector3 velocity;
    private bool isGrounded;
    private float gravity = -9.81f;
    private bool isRunning = false;
    
    void Start()
    {
        if (characterController == null) characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.height = 2f;
            characterController.radius = 0.5f;
            characterController.center = new Vector3(0, 1, 0);
        }
        if (animator == null) animator = GetComponent<Animator>(); 
    }
    
    void Update()
    {
        HandleMovement();
        HandleJump();
    }

    public void OnFootstep() // 발소리
    {
        // 걷거나 뛰고 있을 때만 소리 재생
        if (stepSound != null && audioSource != null && characterController.isGrounded)
        {
            // (선택사항) 소리가 기계음처럼 들리지 않게 음정(Pitch)을 살짝 랜덤으로 바꿈
            audioSource.pitch = Random.Range(0.9f, 1.1f); 
            audioSource.volume = Random.Range(0.8f, 1.0f);

            // 소리 '한 번' 재생
            audioSource.PlayOneShot(stepSound);
        }
    }

    void HandleMovement()
    {
        // 지면 체크
        isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }
        
        // WASD 입력
        float horizontal = Input.GetAxis("Horizontal"); 
        float vertical = Input.GetAxis("Vertical");     
        
        // Left Shift로 달리기 상태 확인
        isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : moveSpeed;
        
        // 이동 방향 계산
        Camera mainCam = Camera.main;
        Vector3 moveDirection = Vector3.zero;
        
        if (mainCam != null)
        {
            Vector3 forward = mainCam.transform.forward;
            Vector3 right = mainCam.transform.right;
            forward.y = 0f; right.y = 0f;
            forward.Normalize(); right.Normalize();
            moveDirection = forward * vertical + right * horizontal;
        }
        else
        {
            moveDirection = transform.forward * vertical + transform.right * horizontal;
        }
        
        // 이동 적용
        if (moveDirection.magnitude > 0.1f)
        {
            moveDirection.Normalize();
            characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
        }

        // 애니메이션 처리 로직 (걷기 & 달리기)
        if (animator != null)
        {
            
            bool isMoving = moveDirection.magnitude > 0.1f; 
            
            animator.SetBool("IsWalking", isMoving);
            animator.SetBool("IsRunning", isMoving && isRunning);
        }
        
        // 중력 적용
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
    
    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }
}