using UnityEngine;
using StoreGame;
using System.Collections; // 코루틴 사용을 위해 필요

public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    
    [Header("컴포넌트")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator; 
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip stepSound;

    [Header("무기 컨트롤")]
    [SerializeField] private GunAction gunAction;      // 총 발사 제어용

    [Header("UI")]
    [SerializeField] private GameObject crosshairUI; // 화면 중앙 십자가(조준점) UI
    [SerializeField] private GameObject ammoUI;      // 화면 총알 수 UI
    
    // ================= [Death Cam 추가 변수] =================
    [Header("죽음 연출 설정")]
    [SerializeField] private GameObject weaponHolder; // 손에 든 무기 (숨김 처리용)
    [SerializeField] private Transform headBone;      // ★ 캐릭터의 머리(Head) 뼈
    // =======================================================

    private HealthSystem healthSystem;
    private float verticalVelocity; 
    private bool isGrounded;
    private float gravity = -20f;   
    private bool isRunning = false;
    private bool isDead = false;
    
    void Start()
    {
        if (characterController == null) characterController = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponent<Animator>(); 

        // GunAction 자동 할당 시도 (없으면 인스펙터에서 직접 넣어도 됨)
        if (gunAction == null)
        {
            gunAction = FindObjectOfType<GunAction>();
        }

        healthSystem = GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.OnDeath += HandleDeath;
        }
    }

    void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDeath -= HandleDeath;
        }
    }

    // [추가됨] 죽은 후 시체가 바닥에 닿을 때까지 중력을 적용하는 코루틴
    private IEnumerator ApplyGravityAfterDeath()
    {
        // 약간의 지연을 주어 애니메이션이 시작된 후 떨어지게 함 (선택사항)
        yield return new WaitForSeconds(0.1f);

        // 캐릭터 컨트롤러가 존재하고, 아직 공중에 떠 있다면 계속 반복
        while (characterController != null && characterController.enabled && !characterController.isGrounded)
        {
            // 중력 가속도 계산
            verticalVelocity += gravity * Time.deltaTime;
            
            // 아래 방향으로만 이동 적용
            Vector3 gravityMove = new Vector3(0, verticalVelocity, 0);
            characterController.Move(gravityMove * Time.deltaTime);

            // 다음 프레임까지 대기
            yield return null;
        }
        
        // 바닥에 닿았으면 속도 초기화 (안전장치)
        verticalVelocity = 0f;
    }

    void HandleDeath()
    {
        if (isDead) return;
        isDead = true; 
        
        // 1. 애니메이션 실행
        if (animator != null) animator.SetTrigger("Die");

        // 2. 무기 숨기기
        if (weaponHolder != null) weaponHolder.SetActive(false);

        // 2-0. 총 발사 완전 비활성화
        if (gunAction != null)
        {
            gunAction.SetGunEnabled(false);
        }

        // 2-1. 십자가(조준점) UI 숨기기
        if (crosshairUI != null) crosshairUI.SetActive(false);

        // 2-2. 총알 수 UI 숨기기
        if (ammoUI != null) ammoUI.SetActive(false);

        // ================= [1인칭 시점 유지 로직] =================
        Transform cameraTransform = Camera.main.transform;
        if (cameraTransform != null && headBone != null)
        {
            cameraTransform.SetParent(headBone);
            cameraTransform.localPosition = new Vector3(0, 0.15f, 0.1f); 
            cameraTransform.localRotation = Quaternion.identity; 
        }
        
        // 3. 마우스 커서 잠금 해제
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // [추가됨] ★ 죽은 후에도 몸이 바닥으로 떨어지도록 중력 적용 시작
        StartCoroutine(ApplyGravityAfterDeath());

        Debug.Log("플레이어 사망 - 1인칭 시점 유지 및 중력 적용 시작");
    }
    
    public void OnFootstep() 
    {
        if (isDead) return;
        if (stepSound != null && audioSource != null && characterController.isGrounded)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f); 
            audioSource.volume = Random.Range(0.8f, 1.0f);
            audioSource.PlayOneShot(stepSound);
        }
    }
    
    void Update()
    {
        // 죽으면 더 이상 플레이어 입력을 받지 않음
        if (isDead) return;

        HandleMovement();
        
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

        Vector3 moveDirection = transform.forward * vertical + transform.right * horizontal;

        if (moveDirection.magnitude > 1f)
        {
            moveDirection.Normalize();
        }

        float currentSpeed = isRunning ? runSpeed : moveSpeed;
        Vector3 finalMove = moveDirection * currentSpeed;
        
        verticalVelocity += gravity * Time.deltaTime;
        finalMove.y = verticalVelocity;

        characterController.Move(finalMove * Time.deltaTime);

        if (animator != null)
        {
            bool hasInput = new Vector2(horizontal, vertical).sqrMagnitude > 0.01f;
            
            animator.SetBool("IsWalking", hasInput);
            animator.SetBool("IsRunning", hasInput && isRunning);
            animator.SetBool("IsBackwards", vertical < -0.1f);
        }
    }
}