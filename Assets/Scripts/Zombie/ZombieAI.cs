using UnityEngine;
using UnityEngine.AI;
using StoreGame; // HealthSystem이 있는 네임스페이스 추가

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

    [Header("=== 전투 설정 ===")]
    [SerializeField] public float attackDamage = 10f;
    
    [Header("=== 거리 기반 상태 전환 ===")]
    [SerializeField] private float idleDistance = 51f;
    [SerializeField] private float walkDistance = 50f;
    [SerializeField] private float runDistance = 20f;
    [SerializeField] private float attackDistance = 2.5f;
    [SerializeField] private float attackMaintainDistance = 3.0f;
    
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
    
    // [추가] 공격 애니메이션 추적용
    private AttackType lastSetAttackType = AttackType.Attack;
    private bool hasTriggeredAttack = false; // 이번 공격 사이클에서 트리거했는지
    private ZombieState lastAttackState = ZombieState.Idle; // 이전 프레임의 상태
    private HealthSystem targetHealth;

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

            if (playerTarget != null)
            {
                targetHealth = playerTarget.GetComponent<HealthSystem>();
            }
        }
        else
        {
            // 이미 인스펙터에 playerTarget이 할당되어 있다면 바로 가져오기
            targetHealth = playerTarget.GetComponent<HealthSystem>();
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
            enabled = false;
            return;
        }
        
        // 초기 설정
        navAgent.stoppingDistance = attackDistance * 0.8f;
        
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
                targetHealth = playerTarget.GetComponent<HealthSystem>();
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
        
        // [추가] 상태 변경 추적 (애니메이션 트리거를 위해)
        lastAttackState = currentState;
        
        UpdateAnimations();
    }
    public void OnAttackHit()
    {
        // 1. 타겟이나 체력 시스템이 없으면 무시
        if (playerTarget == null || targetHealth == null) return;

        // 2. 좀비가 죽었거나 피격 중이면 공격 판정 무시
        if (isDead || isHit) return;

        // 3. 거리 체크: 애니메이션이 시작됐어도 플레이어가 도망갔으면 데미지를 주지 않음
        // attackDistance에 약간의 여유값(+0.5f)을 주어 판정
        float currentDist = Vector3.Distance(transform.position, playerTarget.position);
        if (currentDist <= attackDistance + 0.5f)
        {
            // 플레이어에게 데미지 전달
            targetHealth.TakeDamage(attackDamage);
            Debug.Log($"[Zombie] 플레이어 타격! 데미지: {attackDamage}");
        }
    }
    
    void RandomizeAttack()
    {
        if (!isCrawlingMode && useRandomAttacks && availableAttackTypes.Length > 0)
        {
            // 랜덤 뽑기
            AttackType previousAttackType = currentAttackType;
            currentAttackType = availableAttackTypes[Random.Range(0, availableAttackTypes.Length)];
                       
        }
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
            if (distanceToPlayer <= attackMaintainDistance)
            {
                // 쿨타임이 찼으면 새로운 공격 준비
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    RandomizeAttack(); // [중요] 쿨타임 찰 때마다 새로 뽑기
                    lastAttackTime = Time.time; // 시간 갱신
                    if (zombieAudio != null) zombieAudio.PlayAttack();
                    // [추가] 새로운 공격 타입이 선택되었으므로 트리거 리셋
                    hasTriggeredAttack = false;
                }
                
                // 쿨타임이 안 찼어도(공격 후 대기 중) 상태는 Attack 유지
                return ZombieState.Attack;
            }
            // 거리가 너무 멀어지면 추격(Run/Walk)으로 전환
        }

        // 4. 공격 시작 로직 (거리 안에 들어왔을 때)
        else if (distanceToPlayer <= attackDistance)
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                RandomizeAttack(); // [중요] 첫 진입 시 랜덤 뽑기
                lastAttackTime = Time.time;
                if (zombieAudio != null) zombieAudio.PlayAttack();
                hasTriggeredAttack = false; // [추가] 새로운 공격 시작
                return ZombieState.Attack;
            }
            else
            {
                // 쿨타임 중이지만 사거리 안에 들어왔으므로 공격 태세(대기)로 진입
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
                // [추가] Attack 상태 진입 시 공격 트리거 플래그 리셋 및 AttackType 설정
                if (state == ZombieState.Attack && !isCrawlingMode)
                {
                    hasTriggeredAttack = false;
                    // AttackType을 상태 진입 시에만 설정
                    if (useRandomAttacks && hasAttackTypeParameter && animator != null)
                    {
                        int attackTypeValue = (int)currentAttackType;
                        animator.SetInteger("AttackType", attackTypeValue);
                        lastSetAttackType = currentAttackType;
                    }
                }
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
                // NavMesh 위에 올라가지 않은 에이전트에서 isStopped를 읽지 않도록 순서/조건 정리
                if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh && !navAgent.isStopped)
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
                    animator.SetInteger("AttackType", (int)currentIdleType);
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
                if (isCrawlingMode)
                {
                    // 기어가는 상태 유지
                    animator.SetBool(animParamIsCrawling, true);
                    // 공격 트리거 (Crawl -> CrawlBite 전환용)
                    animator.SetBool(animParamIsAttacking, true);
                    
                }
                else
                {
                    // 서서 하는 공격
                    // [수정] AttackType을 항상 설정 (모든 타입이 작동하도록)
                    if (useRandomAttacks && hasAttackTypeParameter)
                    {
                        // AttackType을 매 프레임 설정하여 확실히 반영되도록 함
                        
                        
                        int attackTypeValue = (int)currentAttackType;
                        animator.SetInteger("AttackType", attackTypeValue);
                        lastSetAttackType = currentAttackType;
                    }
                    
                    // [수정] 공격 애니메이션 상태 확인
                    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                    bool isInAttackAnimation = stateInfo.IsName("attack") || 
                                              stateInfo.IsName("kicking") ||
                                              stateInfo.IsName("punching") ||
                                              stateInfo.IsName("headbutt") ||
                                              stateInfo.IsName("scratch");
                    
                    // [디버깅] 애니메이션 상태 정보
                    string currentStateName = stateInfo.IsName("attack") ? "attack" :
                                             stateInfo.IsName("kicking") ? "kicking" :
                                             stateInfo.IsName("punching") ? "punching" :
                                             stateInfo.IsName("headbutt") ? "headbutt" :
                                             stateInfo.IsName("scratch") ? "scratch" : "기타";
                    
                    // [수정] 공격 애니메이션이 재생 중일 때는 IsAttacking을 false로 유지
                    if (isInAttackAnimation)
                    {
                        // 애니메이션이 끝났는지 확인
                        if (stateInfo.normalizedTime >= 0.95f)
                        {
                            // 애니메이션이 거의 끝났으므로 다음 공격을 트리거할 수 있도록 준비
                            hasTriggeredAttack = false;
                            // IsAttacking을 false로 설정하여 애니메이션이 완전히 끝나도록 함
                            animator.SetBool(animParamIsAttacking, false);
                        }
                        else
                        {
                            // 애니메이션이 재생 중이면 IsAttacking을 false로 유지 (중단 방지)
                            animator.SetBool(animParamIsAttacking, false);
                        }
                    }
                    // 공격 애니메이션이 재생 중이 아니고, 트리거하지 않았으면 트리거
                    else if (!hasTriggeredAttack)
                    {
                        // [중요] AttackType을 먼저 설정한 후 IsAttacking을 true로 설정
                        // 이렇게 하면 애니메이션 컨트롤러가 올바른 AttackType 값을 읽을 수 있음
                        animator.SetBool(animParamIsAttacking, true);
                        hasTriggeredAttack = true;
                        
                    }
                    
                }
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
    
}
