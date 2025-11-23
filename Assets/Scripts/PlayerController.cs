using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpForce = 3f; 
    private float jumpBufferCounter = 0f;
    private float jumpBufferTime = 0.2f;
    
    [Header("컴포넌트")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator; 
    
    [Header("오디오")]
    [SerializeField] private AudioSource audioSource; 
    [SerializeField] private AudioClip stepSound;    

    private Vector3 velocity;
    private bool isGrounded;
    private float gravity = -100f;
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
        HandleJump();
    }

    public void OnFootstep() 
    {
        if (stepSound != null && audioSource != null && characterController.isGrounded)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f); 
            audioSource.volume = Random.Range(0.8f, 1.0f);
            audioSource.PlayOneShot(stepSound);
        }
    }

    // [추가] 애니메이션 이벤트에서 호출될 "진짜 점프" 함수
    public void OnJumpEvent()
    {
        velocity.y = Mathf.Sqrt(jumpForce * -1f * gravity);
        characterController.Move(Vector3.up * 0.01f);
    }

    void HandleMovement()
    {
        isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }
        
        float horizontal = Input.GetAxis("Horizontal"); 
        float vertical = Input.GetAxis("Vertical");     
        
        isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : moveSpeed;
        
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
        
        if (moveDirection.magnitude > 0.1f)
        {
            moveDirection.Normalize();
            characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
        }

        if (animator != null)
        {
            bool isMoving = moveDirection.magnitude > 0.1f; 
            animator.SetBool("IsWalking", isMoving);
            animator.SetBool("IsRunning", isMoving && isRunning);
        }
        
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
    
    void HandleJump()
    {
        // [핵심 변경]
        // "키를 방금 눌렀고(Counter > 0)"  && "땅에 있다(isGrounded)"면 점프!
        if (jumpBufferCounter > 0 && isGrounded)
        {
            // 1. 점프 실행 (이벤트 방식 유지)
            animator.SetTrigger("OnJump");
            
            // 2. 점프 했으니 카운터 초기화 (중복 점프 방지)
            jumpBufferCounter = 0f;
        }
    }
}