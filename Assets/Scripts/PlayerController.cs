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

            // [추가] 뒤로 걷기 로직
            // vertical 값이 0보다 작으면(보통 -1) 뒤로 걷는 것으로 판단합니다.
            // 약간의 오차를 고려해 -0.1f보다 작을 때 true로 설정합니다.
            animator.SetBool("IsBackwards", vertical < -0.1f);
        }
        
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
    
    void HandleJump()
    {
        if (jumpBufferCounter > 0 && isGrounded)
        {
            animator.SetTrigger("OnJump");
            jumpBufferCounter = 0f;
        }
    }
}