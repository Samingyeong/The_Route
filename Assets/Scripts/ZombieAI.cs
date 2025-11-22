using UnityEngine;
using UnityEngine.AI;

public enum ZombieState
{
    Idle,
    Walk,
    Run,
    Attack,
    Search,
    Stumble
}

public class ZombieAI : MonoBehaviour
{
    [Header("=== 0단계: 기본 설정 ===")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private NavMeshAgent navAgent;
    [SerializeField] private Animator animator;
    
    [Header("=== 1단계: 거리 기반 상태 전환 ===")]
    [SerializeField] private float idleDistance = 15f;      // 15m 이상이면 Idle
    [SerializeField] private float walkDistance = 5f;       // 5m 이하면 Walk 시작
    [SerializeField] private float runDistance = 5f;        // 5m 이하면 Run
    [SerializeField] private float attackDistance = 1.5f;   // 1.5m 이하면 Attack
    
    [Header("속도 설정")]
    [SerializeField] private float walkSpeed = 2.5f;       // 걷기 속도 (2~3m/s)
    [SerializeField] private float runSpeed = 4.5f;         // 뛰기 속도 (4~5m/s)
    
    [Header("=== 2단계: 랜덤성 설정 ===")]
    [SerializeField] private bool enableRandomness = false;
    [SerializeField] [Range(0f, 1f)] private float pauseProbability = 0.3f;  // 멈춤 확률 30%
    [SerializeField] private float pauseDuration = 1.5f;    // 멈춤 시간 1~2초
    [SerializeField] private float speedBoostProbability = 0.2f;  // 속도 증가 확률
    [SerializeField] private float speedBoostDuration = 0.5f;     // 속도 증가 지속 시간
    [SerializeField] private float speedBoostMultiplier = 1.5f;   // 속도 증가 배율
    
    [Header("=== 3단계: 애니메이션 파라미터 ===")]
    [SerializeField] private string animParamSpeed = "Speed";
    [SerializeField] private string animParamIsWalking = "IsWalking";
    [SerializeField] private string animParamIsRunning = "IsRunning";
    [SerializeField] private string animParamIsAttacking = "IsAttacking";
    [SerializeField] private string animParamIsSearching = "IsSearching";
    [SerializeField] private string animParamIsStumbling = "IsStumbling";
    
    // 내부 변수
    private ZombieState currentState = ZombieState.Idle;
    private float distanceToPlayer;
    private float lastPlayerMovementTime;
    private Vector3 lastPlayerPosition;
    private bool isPaused = false;
    private float pauseTimer = 0f;
    private bool isSpeedBoosted = false;
    private float speedBoostTimer = 0f;
    private float originalSpeed;
    
    void Start()
    {
        // Player 자동 찾기 (Tag가 없으면 이름으로 찾기)
        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null)
            {
                playerObj = GameObject.Find("Player");
            }
            if (playerObj == null)
            {
                // PlayerController가 있는 오브젝트 찾기
                PlayerController playerController = FindObjectOfType<PlayerController>();
                if (playerController != null)
                {
                    playerObj = playerController.gameObject;
                }
            }
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
                Debug.Log($"ZombieAI: Player를 찾았습니다 - {playerObj.name}");
            }
        }
        
        // NavMeshAgent 자동 찾기
        if (navAgent == null)
        {
            navAgent = GetComponent<NavMeshAgent>();
        }
        
        // Animator 자동 찾기
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (navAgent == null)
        {
            Debug.LogError("ZombieAI: NavMeshAgent를 찾을 수 없습니다!");
            enabled = false;
            return;
        }
        
        if (playerTarget == null)
        {
            Debug.LogWarning("ZombieAI: Player를 찾을 수 없습니다. Inspector에서 직접 할당해주세요.");
        }
        else
        {
            Debug.Log($"ZombieAI: Player 타겟 설정 완료 - {playerTarget.name}");
        }
        
        // 초기 설정
        originalSpeed = navAgent.speed;
        navAgent.stoppingDistance = attackDistance;
        lastPlayerPosition = playerTarget != null ? playerTarget.position : transform.position;
    }
    
    void Update()
    {
        // Player 찾기 재시도 (Start에서 못 찾았을 경우)
        if (playerTarget == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null)
            {
                playerObj = GameObject.Find("Player");
            }
            if (playerObj == null)
            {
                PlayerController playerController = FindObjectOfType<PlayerController>();
                if (playerController != null)
                {
                    playerObj = playerController.gameObject;
                }
            }
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
            }
        }
        
        if (playerTarget == null)
        {
            // Player를 찾을 수 없으면 아무것도 하지 않음
            return;
        }
        
        if (navAgent == null || !navAgent.enabled)
        {
            return;
        }
        
        // NavMesh 위에 있는지 확인 (없어도 일단 거리 계산은 함)
        bool isOnNavMesh = navAgent.isOnNavMesh;
        
        // 거리 계산
        distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        
        // Player 움직임 감지
        if (Vector3.Distance(playerTarget.position, lastPlayerPosition) > 0.1f)
        {
            lastPlayerMovementTime = Time.time;
        }
        lastPlayerPosition = playerTarget.position;
        
        // === 2단계: 랜덤성 처리 ===
        if (enableRandomness)
        {
            HandleRandomness();
        }
        
        // === 1단계: 상태 머신 업데이트 ===
        UpdateStateMachine();
        
        // === 애니메이션 업데이트 ===
        UpdateAnimations();
    }
    
    void UpdateStateMachine()
    {
        ZombieState newState = DetermineState();
        
        if (newState != currentState)
        {
            ExitState(currentState);
            currentState = newState;
            EnterState(currentState);
        }
        
        UpdateState(currentState);
    }
    
    ZombieState DetermineState()
    {
        // 공격 중이면 공격 상태 유지
        if (currentState == ZombieState.Attack && distanceToPlayer <= attackDistance * 1.2f)
        {
            return ZombieState.Attack;
        }
        
        // 거리 기반 상태 결정
        if (distanceToPlayer <= attackDistance)
        {
            return ZombieState.Attack;
        }
        else if (distanceToPlayer <= runDistance)
        {
            return ZombieState.Run;
        }
        else if (distanceToPlayer <= walkDistance)
        {
            return ZombieState.Walk;
        }
        else if (distanceToPlayer > idleDistance)
        {
            // === 3단계: 멀리 있으면 Search 상태 가능 ===
            if (enableRandomness && Random.value < 0.1f)
            {
                return ZombieState.Search;
            }
            return ZombieState.Idle;
        }
        else
        {
            return ZombieState.Idle;
        }
    }
    
    void EnterState(ZombieState state)
    {
        // NavMeshAgent가 유효한지 확인
        if (navAgent == null || !navAgent.enabled || !navAgent.isOnNavMesh)
        {
            return;
        }
        
        switch (state)
        {
            case ZombieState.Idle:
                if (navAgent.isOnNavMesh)
                {
                    navAgent.isStopped = true;
                }
                break;
                
            case ZombieState.Walk:
                if (navAgent.isOnNavMesh)
                {
                    navAgent.isStopped = false;
                    navAgent.speed = walkSpeed;
                }
                break;
                
            case ZombieState.Run:
                if (navAgent.isOnNavMesh)
                {
                    navAgent.isStopped = false;
                    navAgent.speed = runSpeed;
                }
                break;
                
            case ZombieState.Attack:
                if (navAgent.isOnNavMesh)
                {
                    navAgent.isStopped = true;
                }
                // 공격 애니메이션은 Attack 상태 업데이트에서 처리
                break;
                
            case ZombieState.Search:
                if (navAgent.isOnNavMesh)
                {
                    navAgent.isStopped = true;
                }
                // 주변 탐색 로직
                break;
                
            case ZombieState.Stumble:
                if (navAgent.isOnNavMesh)
                {
                    navAgent.isStopped = true;
                }
                break;
        }
    }
    
    void UpdateState(ZombieState state)
    {
        switch (state)
        {
            case ZombieState.Idle:
                // 가만히 있음
                break;
                
            case ZombieState.Walk:
            case ZombieState.Run:
                if (!isPaused && navAgent != null && navAgent.enabled && !navAgent.isStopped)
                {
                    // NavMesh 위에 있으면 SetDestination 호출
                    if (navAgent.isOnNavMesh)
                    {
                        navAgent.SetDestination(playerTarget.position);
                    }
                    else
                    {
                        // NavMesh 위에 없으면 직접 이동 (임시 해결책)
                        Vector3 direction = (playerTarget.position - transform.position).normalized;
                        direction.y = 0;
                        if (direction.magnitude > 0.1f)
                        {
                            transform.position += direction * (currentState == ZombieState.Run ? runSpeed : walkSpeed) * Time.deltaTime;
                            transform.rotation = Quaternion.Slerp(transform.rotation, 
                                Quaternion.LookRotation(direction), Time.deltaTime * 5f);
                        }
                    }
                }
                break;
                
            case ZombieState.Attack:
                // 플레이어를 바라보기
                Vector3 lookDirection = (playerTarget.position - transform.position).normalized;
                lookDirection.y = 0;
                if (lookDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, 
                        Quaternion.LookRotation(lookDirection), Time.deltaTime * 5f);
                }
                break;
                
            case ZombieState.Search:
                // 주변을 둘러보는 동작
                transform.Rotate(0, 45f * Time.deltaTime, 0);
                break;
                
            case ZombieState.Stumble:
                // 우당탕 걸음 애니메이션 재생
                break;
        }
    }
    
    void ExitState(ZombieState state)
    {
        // 상태 종료 시 정리 작업
    }
    
    void HandleRandomness()
    {
        // 멈춤 처리
        if (isPaused)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                isPaused = false;
            }
            return;
        }
        
        // Player가 멈춰있을 때 멈춤 확률 체크
        if (Time.time - lastPlayerMovementTime > 2f && 
            (currentState == ZombieState.Walk || currentState == ZombieState.Run))
        {
            if (Random.value < pauseProbability * Time.deltaTime)
            {
                isPaused = true;
                pauseTimer = Random.Range(pauseDuration * 0.5f, pauseDuration * 1.5f);
            }
        }
        
        // 속도 증가 처리
        if (navAgent == null || !navAgent.enabled || !navAgent.isOnNavMesh)
        {
            return;
        }
        
        if (isSpeedBoosted)
        {
            speedBoostTimer -= Time.deltaTime;
            if (speedBoostTimer <= 0f)
            {
                isSpeedBoosted = false;
                if (navAgent.isOnNavMesh)
                {
                    navAgent.speed = originalSpeed;
                }
            }
        }
        else if (currentState == ZombieState.Run && Random.value < speedBoostProbability * Time.deltaTime)
        {
            isSpeedBoosted = true;
            speedBoostTimer = speedBoostDuration;
            if (navAgent.isOnNavMesh)
            {
                navAgent.speed = originalSpeed * speedBoostMultiplier;
            }
        }
    }
    
    void UpdateAnimations()
    {
        if (animator == null) return;
        
        // 모든 애니메이션 파라미터 초기화
        animator.SetBool(animParamIsWalking, false);
        animator.SetBool(animParamIsRunning, false);
        animator.SetBool(animParamIsAttacking, false);
        animator.SetBool(animParamIsSearching, false);
        animator.SetBool(animParamIsStumbling, false);
        
        // 속도 파라미터
        float speed = navAgent.velocity.magnitude;
        animator.SetFloat(animParamSpeed, speed);
        
        // 상태별 애니메이션 설정
        switch (currentState)
        {
            case ZombieState.Idle:
                // Idle은 기본 상태
                break;
                
            case ZombieState.Walk:
                animator.SetBool(animParamIsWalking, true);
                break;
                
            case ZombieState.Run:
                animator.SetBool(animParamIsRunning, true);
                break;
                
            case ZombieState.Attack:
                animator.SetBool(animParamIsAttacking, true);
                break;
                
            case ZombieState.Search:
                animator.SetBool(animParamIsSearching, true);
                break;
                
            case ZombieState.Stumble:
                animator.SetBool(animParamIsStumbling, true);
                break;
        }
    }
    
    // === 디버그용 Gizmos ===
    void OnDrawGizmosSelected()
    {
        if (playerTarget == null) return;
        
        // 거리 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, idleDistance);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, walkDistance);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, runDistance);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
        
        // Player로의 선
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, playerTarget.position);
    }
}

