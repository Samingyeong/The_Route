using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    
    // [변경] 회전 속도 변수 삭제 (CameraFollow가 회전 담당함)

    [Header("컴포넌트")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator; 
    [SerializeField] private AudioSource audioSource; // 소리 재생기
    [SerializeField] private AudioClip stepSound;     // 발소리 파일 (.wav)
    
    private float verticalVelocity; 
    private bool isGrounded;
    private float gravity = -20f;   
    private bool isRunning = false;
    
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
    
    void Start()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
            // 없으면 자동 추가
            if (characterController == null)
            {
                characterController = gameObject.AddComponent<CharacterController>();
                characterController.height = 2f;
                characterController.center = new Vector3(0, 1f, 0); 
            }
        }
        if (animator == null) animator = GetComponent<Animator>(); 
    }
    
    void Update()
    {
        HandleMovement();
        
        // 점프
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            animator.SetTrigger("OnJump");
            verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    void HandleMovement()
    {
        isGrounded = characterController.isGrounded;
        
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; 
        }

        float horizontal = Input.GetAxis("Horizontal"); 
        float vertical = Input.GetAxis("Vertical");     
        isRunning = Input.GetKey(KeyCode.LeftShift);

        // [핵심 변경 사항]
        // CameraFollow 스크립트가 이미 플레이어의 몸통을 카메라가 보는 방향으로 돌려놓았습니다.
        // 그러므로 복잡한 카메라 계산 없이, 그냥 로컬 좌표(내 기준 앞/오른쪽)로 움직이면 됩니다.
        
        // transform.forward = 내 몸이 보는 앞쪽 (이미 카메라 방향)
        // transform.right = 내 몸의 오른쪽
        Vector3 moveDirection = transform.forward * vertical + transform.right * horizontal;

        // 대각선 이동 시 속도 일정하게
        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        // [삭제] 캐릭터 회전 로직 삭제 
        // (CameraFollow가 마우스에 따라 몸통을 돌려주므로 여기서 건드리면 안 됨)

        // 실제 이동 적용
        float currentSpeed = isRunning ? runSpeed : moveSpeed;
        Vector3 finalMove = moveDirection * currentSpeed;
        
        // 중력 적용
        verticalVelocity += gravity * Time.deltaTime;
        finalMove.y = verticalVelocity;

        characterController.Move(finalMove * Time.deltaTime);

        // 애니메이션
        if (animator != null)
        {
            // 입력이 있는지만 체크
            bool hasInput = new Vector2(horizontal, vertical).sqrMagnitude > 0.01f;
            
            animator.SetBool("IsWalking", hasInput);
            animator.SetBool("IsRunning", hasInput && isRunning);
            
            // 뒤로 걷기 애니메이션
            animator.SetBool("IsBackwards", vertical < -0.1f);
            
        }
    }
}