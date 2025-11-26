using UnityEngine;
using UnityEngine.AI;

public enum ZombieState
{
    Idle,
    Walk,
    Run,
    Crawl,
    Attack,
    Hit, 
    Dead 
}

public class ZombieAI : MonoBehaviour
{
    [Header("=== 기본 설정 ===")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private NavMeshAgent navAgent;
    [SerializeField] private Animator animator;
    
    [Header("=== 거리 기반 상태 전환 ===")]
    [SerializeField] private float idleDistance = 51f;
    [SerializeField] private float walkDistance = 50f;
    [SerializeField] private float runDistance = 20f;
    [SerializeField] private float attackDistance = 2.5f;
    
    [Header("=== 속도 설정 ===")]
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float runSpeed = 4.5f;
    [SerializeField] private float crawlSpeed = 3.0f;
    
    [Header("=== 애니메이션 파라미터 ===")]
    [SerializeField] private string animParamIsWalking = "IsWalking";
    [SerializeField] private string animParamIsRunning = "IsRunning";
    [SerializeField] private string animParamIsAttacking = "IsAttacking";
    [SerializeField] private string animParamIsCrawling = "IsCrawling";
    [SerializeField] private string animParamAttackType = "AttackType";
    [SerializeField] private string animParamIdleType = "IdleType";
    
    [Header("=== Idle 타입 랜덤 설정 ===")]
    [SerializeField] private bool useRandomIdleTypes = true;
    [Tooltip("Idle 상태에서 랜덤으로 재생할 Idle 타입들")]
    [SerializeField] private IdleType[] availableIdleTypes = { 
        IdleType.Idle, 
        IdleType.Agonizing, 
        IdleType.Search 
    };
    
    [Header("=== 공격 타입 랜덤 설정 ===")]
    [SerializeField] private bool useRandomAttacks = true;
    [SerializeField] private AttackType[] availableAttackTypes = { 
        AttackType.Attack, 
        AttackType.Kicking, 
        AttackType.Punching, 
        AttackType.Headbutt, 
        AttackType.Scratch 
    };
    
    // Idle 타입 열거형 (Zombie_Random 컨트롤러의 Blend Tree와 일치)
    public enum IdleType
    {
        Idle = 0,
        Agonizing = 1,
        Search = 2,
        Bite = 3,
        ReactionHit = 4,
        StandUp = 5,
        Stumbling = 6
    }
    
    // 공격 타입 열거형 (Zombie_Random 컨트롤러의 AnyState 전환과 일치)
    public enum AttackType
    {
        Attack = 0,
        Kicking = 1,
        Punching = 2,
        Headbutt = 3,
        Scratch = 4
    }
    
    // 내부 변수
    private ZombieState currentState = ZombieState.Idle;
    private ZombieState previousState = ZombieState.Idle;
    private AttackType currentAttackType = AttackType.Attack;
    private IdleType currentIdleType = IdleType.Idle;
    private float distanceToPlayer;
    
    // 파라미터 타입 캐시
    private bool hasIdleTypeParameter = false;
    private bool isIdleTypeFloat = false;
    private bool hasAttackTypeParameter = false;
    private bool isAttackTypeFloat = false;
    
    // 공격 관련
    private float lastAttackTime = 0f;
    private float attackCooldown = 0.5f;

    // [추가] 외부에서 좀비가 죽었는지 다쳤는지 알리기 위한 변수
    private bool isDead = false;
    private bool isHit = false;
    private bool isCrawlingMode = false;
    private float hitRecoveryTime = 0.5f; // 피격 모션 길이만큼 멈춤 (필요시 조절)
    private float hitTimer = 0f;

    //sound
    private ZombieAudio zombieAudio;

    // [추가] Idle 사운드 랜덤 타이머
    private float idleSoundTimer;
    private float idleSoundInterval;
    
    void Start()
    {
        // Player 자동 찾기
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
        
        // 초기 설정
        navAgent.stoppingDistance = attackDistance;
        
        // NavMeshAgent와 애니메이션 루트 모션 충돌 방지
        navAgent.updatePosition = false;
        navAgent.updateRotation = false;
        
        // NavMesh 위의 초기 위치 설정
        if (navAgent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
                navAgent.Warp(hit.position);
                navAgent.nextPosition = hit.position;
            }
        }
        
        // 파라미터 타입 확인 및 캐시
        if (animator != null && animator.parameters != null)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == animParamIdleType)
                {
                    hasIdleTypeParameter = true;
                    isIdleTypeFloat = (param.type == AnimatorControllerParameterType.Float);
                }
                if (param.name == animParamAttackType)
                {
                    hasAttackTypeParameter = true;
                    isAttackTypeFloat = (param.type == AnimatorControllerParameterType.Float);
                }
            }
        }
        
        // 초기 Idle 타입 선택
        if (useRandomIdleTypes && availableIdleTypes.Length > 0)
        {
            currentIdleType = availableIdleTypes[Random.Range(0, availableIdleTypes.Length)];
        }

        zombieAudio = GetComponent<ZombieAudio>();
        ResetIdleTimer();
    }
    
    void Update()
    {
        // Player 찾기 재시도
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
                    playerTarget = playerController.transform;
                }
            }
            if (playerObj != null)
            {
                playerTarget = playerObj.transform;
            }
        }
        
        if (playerTarget == null || navAgent == null || !navAgent.enabled)
        {
            return;
        }
        
        // 거리 계산 (XZ 평면만)
        Vector3 zombiePos = transform.position;
        Vector3 playerPos = playerTarget.position;
        zombiePos.y = 0;
        playerPos.y = 0;
        distanceToPlayer = Vector3.Distance(zombiePos, playerPos);

        if (!isDead && currentState != ZombieState.Attack && currentState != ZombieState.Hit)
        {
            idleSoundTimer -= Time.deltaTime;
            if (idleSoundTimer <= 0)
            {
                if (zombieAudio != null) zombieAudio.PlayIdle();
                ResetIdleTimer();
            }
        }

        UpdateStateMachine();
        UpdateAnimations();
    }
    
    void ResetIdleTimer()
    {
        idleSoundInterval = Random.Range(3.0f, 7.0f); // 3~7초마다 울음소리
        idleSoundTimer = idleSoundInterval;
    }

    public void SetDead()
    {
        isDead = true;
        // AI 기능 정지
        if(navAgent != null) navAgent.isStopped = true;
        enabled = false; // 이 스크립트의 Update를 멈춤
    }

    public void SetHit()
    {
        if (isDead) return;
        isHit = true;
        hitTimer = Time.time;
    }

    public void StartCrawling()
    {
        if (isDead) return;
        isCrawlingMode = true;
    }

    void FixedUpdate()
    {
        // NavMeshAgent 위치 동기화
        if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
        {
            if (Vector3.Distance(transform.position, navAgent.nextPosition) > navAgent.radius * 0.5f)
            {
                navAgent.nextPosition = transform.position;
            }
        }
    }
    
    void UpdateStateMachine()
    {
        ZombieState newState = DetermineState();
        
        if (newState != currentState)
        {
            previousState = currentState;
            ExitState(currentState);
            currentState = newState;
            EnterState(currentState);
            
            // Idle 상태로 전환된 후 previousState를 Idle로 설정
            if (currentState == ZombieState.Idle)
            {
                previousState = ZombieState.Idle;
            }
        }
        
        UpdateState(currentState);
    }
    
    ZombieState DetermineState()
    {
        // 1. 사망 상태 최우선 체크
        if (isDead) return ZombieState.Dead;

        // 2. 피격 상태 체크 (잠시 멈춤)
        if (isHit)
        {
            // 설정된 회복 시간(hitRecoveryTime) 동안은 Hit 상태 유지
            if (Time.time - hitTimer < hitRecoveryTime)
            {
                return ZombieState.Hit;
            }
            else
            {
                isHit = false; // 시간이 지나면 피격 상태 해제
            }
        }

        // 3. 공격 중 유지 로직
        // (한번 공격 모션이 시작되면 플레이어가 살짝 멀어져도 끊기지 않게 함)
        if (currentState == ZombieState.Attack)
        {
             if (distanceToPlayer <= attackDistance * 1.2f && Time.time - lastAttackTime < attackCooldown)
             {
                return ZombieState.Attack;
             }
        }

        // 4. 공격 시작 로직 (거리 안에 들어왔을 때)
        if (distanceToPlayer <= attackDistance)
        {
            // 쿨타임이 지났는지 확인
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                // 공격 타입 랜덤 선택
                if (useRandomAttacks && availableAttackTypes.Length > 0)
                {
                    currentAttackType = availableAttackTypes[Random.Range(0, availableAttackTypes.Length)];
                }

                // [사운드] 공격 소리 재생
                if (zombieAudio != null)
                {
                    zombieAudio.PlayAttack();
                }

                lastAttackTime = Time.time;
                return ZombieState.Attack;
            }
            else
            {
                // 쿨타임 중이지만 거리가 가까우면 공격 상태(대기) 유지
                return ZombieState.Attack;
            }
        }
        
        // 5. 이동 상태 결정 (기어가기 vs 일반 이동)
        if (isCrawlingMode)
        {
            // [기어가기 모드]
            // 공격 범위 밖이면 무조건 기어서 추격 (Idle 없이 끈질기게 쫓아오게 설정)
            return ZombieState.Crawl;
        }
        else
        {
            // [일반 모드]
            if (distanceToPlayer <= runDistance)
            {
                return ZombieState.Run;
            }
            else if (distanceToPlayer <= walkDistance)
            {
                return ZombieState.Walk;
            }
            else
            {
                return ZombieState.Idle;
            }
        }
    }
    void EnterState(ZombieState state)
    {
        if (navAgent == null || !navAgent.enabled || !navAgent.isOnNavMesh) return;

        switch (state)
        {
            case ZombieState.Idle:
                navAgent.isStopped = true;
                navAgent.nextPosition = transform.position;
                // (기존 Idle 랜덤 로직 유지...)
                break;
                
            case ZombieState.Walk:
                navAgent.isStopped = false;
                navAgent.speed = walkSpeed;
                break;
                
            case ZombieState.Run:
                navAgent.isStopped = false;
                navAgent.speed = runSpeed;
                break;

            case ZombieState.Crawl: // [추가]
                navAgent.isStopped = false;
                navAgent.speed = crawlSpeed;
                break;
                
            case ZombieState.Attack:
            case ZombieState.Hit:
            case ZombieState.Dead:
                navAgent.isStopped = true;
                navAgent.velocity = Vector3.zero;
                break;
        }
    }
    
    void UpdateState(ZombieState state)
    {
        switch (state)
        {
            case ZombieState.Walk:
            case ZombieState.Run:
            case ZombieState.Crawl: // [추가] 이동 로직 공유
                if (navAgent != null && navAgent.enabled && !navAgent.isStopped && navAgent.isOnNavMesh)
                {
                    navAgent.SetDestination(playerTarget.position);
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
        }
    }
    
    void ExitState(ZombieState state)
    {
        // 상태 종료 시 정리 작업
    }
    
    void UpdateAnimations()
    {
        if (animator == null || !animator.enabled)
        {
            return;
        }
        
        // 모든 Bool 파라미터 초기화
        animator.SetBool(animParamIsWalking, false);
        animator.SetBool(animParamIsRunning, false);
        animator.SetBool(animParamIsAttacking, false);
        animator.SetBool(animParamIsCrawling, false);
        
        // 상태별 애니메이션 설정
        switch (currentState)
        {
            case ZombieState.Idle:
                // IdleType 파라미터 설정
                if (useRandomIdleTypes && hasIdleTypeParameter)
                {
                    if (isIdleTypeFloat)
                    {
                        animator.SetFloat(animParamIdleType, (float)(int)currentIdleType);
                    }
                    else
                    {
                        animator.SetInteger(animParamIdleType, (int)currentIdleType);
                    }
                }
                break;
                
            case ZombieState.Walk:
                animator.SetBool(animParamIsWalking, true);
                break;
                
            case ZombieState.Run:
                animator.SetBool(animParamIsRunning, true);
                break;

            case ZombieState.Crawl: // [추가]
                animator.SetBool(animParamIsCrawling, true);
                break;
                
            case ZombieState.Attack:
                // AttackType 파라미터 설정 (AnyState 전환 트리거)
                if (useRandomAttacks && hasAttackTypeParameter)
                {
                    if (isAttackTypeFloat)
                    {
                        animator.SetFloat(animParamAttackType, (float)(int)currentAttackType);
                    }
                    else
                    {
                        animator.SetInteger(animParamAttackType, (int)currentAttackType);
                    }
                }
                // IsAttacking = true 설정 (AnyState 전환 트리거)
                animator.SetBool(animParamIsAttacking, true);
                break;
        }
    }
    
    // 루트 모션 제어
    void OnAnimatorMove()
    {
        if (animator == null || navAgent == null || !navAgent.enabled)
        {
            return;
        }
        
        // Walk, Run 상태: NavMeshAgent 위치 사용
        if (navAgent.isOnNavMesh && (currentState == ZombieState.Walk || currentState == ZombieState.Run || currentState == ZombieState.Crawl))
        {
            Vector3 nextPosition = navAgent.nextPosition;
            
            // NavMesh 위의 실제 Y 위치 확인
            NavMeshHit hit;
            Vector3 checkPosition = new Vector3(nextPosition.x, nextPosition.y + 1f, nextPosition.z);
            if (NavMesh.SamplePosition(checkPosition, out hit, 2f, NavMesh.AllAreas))
            {
                nextPosition.y = hit.position.y;
            }
            
            transform.position = nextPosition;
            
            // 이동 방향으로 회전
            if (navAgent.velocity.magnitude > 0.1f)
            {
                Vector3 direction = navAgent.velocity.normalized;
                direction.y = 0;
                if (direction.magnitude > 0.1f)
                {
                    transform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }
        // Idle 상태: 루트 모션 허용, NavMesh 위에 맞춤
        else if (currentState == ZombieState.Idle)
        {
            Vector3 rootMotionDelta = animator.deltaPosition;
            
            // XZ 평면 이동 제한
            Vector3 xzDelta = new Vector3(rootMotionDelta.x, 0, rootMotionDelta.z);
            if (xzDelta.magnitude > 0.1f)
            {
                xzDelta = xzDelta.normalized * 0.1f;
            }
            
            // 먼저 NavMesh 위의 기준 Y 위치 찾기
            float navMeshY = transform.position.y;
            if (navAgent.isOnNavMesh)
            {
                NavMeshHit hit;
                // 현재 XZ 위치에서 NavMesh 위의 Y 위치 찾기
                Vector3 checkPosition = new Vector3(transform.position.x + xzDelta.x, transform.position.y + 1f, transform.position.z + xzDelta.z);
                
                // 위에서 아래로 검색 (최대 3m 아래까지)
                if (NavMesh.SamplePosition(checkPosition, out hit, 3f, NavMesh.AllAreas))
                {
                    navMeshY = hit.position.y;
                }
            }
            
            // 루트 모션 적용: XZ는 제한된 이동, Y는 NavMesh 기준으로 상대적 적용
            Vector3 newPosition = transform.position + xzDelta;
            
            // 루트 모션의 Y축을 NavMesh 기준으로 상대적으로 적용
            // 눕는 애니메이션의 경우 루트 모션이 아래로 가므로 NavMesh Y에서 빼기
            float rootMotionY = rootMotionDelta.y;
            newPosition.y = navMeshY + rootMotionY;
            
            // 땅 아래로 너무 깊이 들어가지 않도록 제한 (최대 0.3m 아래)
            if (newPosition.y < navMeshY - 0.3f)
            {
                newPosition.y = navMeshY - 0.3f;
            }
            // 공중에 떠있지 않도록 제한
            else if (newPosition.y > navMeshY + 0.1f)
            {
                newPosition.y = navMeshY;
            }
            
            transform.position = newPosition;
            
            // 회전 적용
            if (animator.deltaRotation != Quaternion.identity)
            {
                transform.rotation = animator.rootRotation;
            }
        }
        // Attack 상태: 루트 모션 XZ만, Y는 NavMesh 위
        else
        {
            Vector3 rootMotionDelta = animator.deltaPosition;
            rootMotionDelta.y = 0; // Y축 이동 무시
            
            Vector3 newPosition = transform.position + rootMotionDelta;
            
            // NavMesh 위의 실제 Y 위치 확인
            if (navAgent.isOnNavMesh)
            {
                NavMeshHit hit;
                Vector3 checkPosition = new Vector3(newPosition.x, newPosition.y + 1f, newPosition.z);
                if (NavMesh.SamplePosition(checkPosition, out hit, 2f, NavMesh.AllAreas))
                {
                    newPosition.y = hit.position.y;
                }
            }
            
            transform.position = newPosition;
            
            // 회전 적용
            if (animator.deltaRotation != Quaternion.identity)
            {
                transform.rotation = animator.rootRotation;
            }
        }
    }
    
    // 디버그용 Gizmos
    void OnDrawGizmosSelected()
    {
        if (playerTarget == null) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, idleDistance);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, walkDistance);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, runDistance);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
        
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, playerTarget.position);
    }
}
