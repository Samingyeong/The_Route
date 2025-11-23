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
    [SerializeField] private float idleDistance = 51f;      // 25m 이상이면 Idle
    [SerializeField] private float walkDistance = 50f;      // 12m 이하면 Walk 시작
    [SerializeField] private float runDistance = 20f;        // 8m 이하면 Run
    [SerializeField] private float attackDistance = 2.5f;   // 2.5m 이하면 Attack
    
    [Header("속도 설정")]
    [SerializeField] private float walkSpeed = 2.5f;       // 걷기 속도 (2~3m/s)
    [SerializeField] private float runSpeed = 4.5f;         // 뛰기 속도 (4~5m/s)
    
    [Header("=== 2단계: 랜덤성 설정 ===")]
    [SerializeField] private bool enableRandomness = true;
    [SerializeField] [Range(0f, 1f)] private float pauseProbability = 0.3f;  // 멈춤 확률 30%
    [SerializeField] private float pauseDuration = 1.5f;    // 멈춤 시간 1~2초
    [SerializeField] private float speedBoostProbability = 0.2f;  // 속도 증가 확률
    [SerializeField] private float speedBoostDuration = 0.5f;     // 속도 증가 지속 시간
    [SerializeField] private float speedBoostMultiplier = 1.5f;   // 속도 증가 배율
    
    [Header("=== 탐색(Search) 설정 ===")]
    [SerializeField] [Range(0f, 1f)] private float searchProbability = 0.4f;  // 최장거리에서 탐색 확률 40%
    [SerializeField] private float searchDuration = 3f;      // 탐색 지속 시간 (초)
    [SerializeField] private float searchRotationSpeed = 60f; // 탐색 중 회전 속도 (도/초)
    [SerializeField] private float searchDetectionRange = 15f; // 탐색 중 플레이어 감지 범위
    
    [Header("=== 3단계: 애니메이션 파라미터 ===")]
    [SerializeField] private string animParamSpeed = "Speed";
    [SerializeField] private string animParamIsWalking = "IsWalking";
    [SerializeField] private string animParamIsRunning = "IsRunning";
    [SerializeField] private string animParamIsAttacking = "IsAttacking";
    [SerializeField] private string animParamAttackType = "AttackType";
    [SerializeField] private string animParamIdleType = "IdleType";
    // IsSearching과 IsStumbling은 제거됨:
    // - Search는 IdleType.Search로 처리됨
    // - Stumble은 현재 사용하지 않음
    
    [Header("=== 4단계: Idle 타입 랜덤 설정 ===")]
    [SerializeField] private bool useRandomIdleTypes = true;
    [SerializeField] [Range(1f, 30f)] private float idleChangeInterval = 5f; // Idle 타입 변경 간격 (초)
    [Tooltip("Idle 상태에서 랜덤으로 재생할 Idle 타입들. 원하는 것만 선택하세요.")]
    [SerializeField] private IdleType[] availableIdleTypes = { 
        IdleType.Idle, 
        IdleType.Agonizing, 
        IdleType.Search 
        // 필요시 Bite, ReactionHit, StandUp, Stumbling 등 추가 가능
    };
    
    [Header("=== 5단계: 공격 타입 랜덤 설정 ===")]
    [SerializeField] private bool useAttackTypeParameter = true; // 단일 Controller 사용 시 true
    [SerializeField] private bool useRandomAttacks = true;
    [SerializeField] private AttackType[] availableAttackTypes = { AttackType.Attack, AttackType.Kicking, AttackType.Punching, AttackType.Headbutt, AttackType.Scratch };
    
    // Idle 타입 열거형 (Idle 상태에서 랜덤 선택)
    // 필요에 따라 더 추가 가능
    public enum IdleType
    {
        Idle = 0,        // idle.fbx
        Agonizing = 1,   // agonizing.fbx
        Search = 2,      // turn.fbx (탐색)
        Bite = 3,        // bite.fbx (추가 예정)
        ReactionHit = 4, // reaction hit.fbx
        StandUp = 5,     // stand up.fbx
        Stumbling = 6   // stumbling.fbx
    }
    
    // 공격 타입 열거형 (AnyState에서 랜덤 선택)
    public enum AttackType
    {
        Attack = 0,      // attack.fbx
        Kicking = 1,    // kicking.fbx
        Punching = 2,   // punching.fbx
        Headbutt = 3,   // headbutt.fbx
        Scratch = 4     // scratch.fbx
    }
    
    // 내부 변수
    private ZombieState currentState = ZombieState.Idle;
    private AttackType currentAttackType = AttackType.Attack;
    private IdleType currentIdleType = IdleType.Idle;
    private float distanceToPlayer;
    private float lastPlayerMovementTime;
    private Vector3 lastPlayerPosition;
    private bool isPaused = false;
    private float pauseTimer = 0f;
    private bool isSpeedBoosted = false;
    private float speedBoostTimer = 0f;
    private float originalSpeed;
    
    // Search 관련 변수
    private float searchTimer = 0f;
    private bool isSearching = false;
    private Vector3 searchStartPosition;
    private float searchStartRotation;
    
    // Attack 관련 변수
    private float lastAttackTime = 0f;
    private float attackCooldown = 0.5f; // 공격 쿨다운 (초)
    
    // Idle 타입 변경 관련 변수
    private float lastIdleTypeChangeTime = 0f;
    
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
        
        // NavMeshAgent와 애니메이션 루트 모션 충돌 방지
        // updatePosition과 updateRotation을 false로 설정하여 OnAnimatorMove에서 수동 제어
        navAgent.updatePosition = false;
        navAgent.updateRotation = false;
        
        // 초기 Y 위치 저장 (땅에 박히는 문제 방지)
        initialYPosition = transform.position.y;
        
        // NavMeshAgent의 초기 위치를 현재 Transform 위치로 설정 (원래 설치한 위치 유지)
        // Y축은 원래 위치 그대로 유지 (하늘로 올라가는 문제 방지)
        if (navAgent.isOnNavMesh)
        {
            // NavMesh 위의 XZ 위치만 찾기 (Y는 원래 위치 유지)
            UnityEngine.AI.NavMeshHit hit;
            Vector3 checkPosition = transform.position;
            if (UnityEngine.AI.NavMesh.SamplePosition(checkPosition, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
            {
                // XZ 위치는 NavMesh 위로, Y는 원래 위치 유지
                Vector3 navMeshPosition = new Vector3(hit.position.x, initialYPosition, hit.position.z);
                transform.position = navMeshPosition;
                navAgent.Warp(navMeshPosition);
                navAgent.nextPosition = navMeshPosition;
            }
            else
            {
                // NavMesh를 찾을 수 없으면 현재 위치 그대로 유지
                navAgent.nextPosition = transform.position;
                navAgent.Warp(transform.position);
            }
        }
        
        // 초기 Idle 타입 랜덤 선택 (각 좀비마다 다른 시드 사용)
        if (useRandomIdleTypes && availableIdleTypes.Length > 0)
        {
            // 각 좀비마다 다른 랜덤 시드 사용 (같은 행동 방지)
            int randomOffset = Random.Range(0, 1000);
            currentIdleType = availableIdleTypes[(Random.Range(0, availableIdleTypes.Length) + randomOffset) % availableIdleTypes.Length];
            lastIdleTypeChangeTime = Time.time + Random.Range(0f, idleChangeInterval * 0.5f); // 시작 시간도 랜덤
        }
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
        
        // 거리 계산 (XZ 평면만, Y축 무시)
        Vector3 zombiePos = transform.position;
        Vector3 playerPos = playerTarget.position;
        zombiePos.y = 0; // Y축 무시
        playerPos.y = 0; // Y축 무시
        distanceToPlayer = Vector3.Distance(zombiePos, playerPos);
        
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
    
    void LateUpdate()
    {
        // NavMeshAgent.updatePosition = false로 설정했으므로
        // NavMeshAgent의 nextPosition을 현재 Transform 위치로 동기화
        // 단, Idle 상태일 때는 위치 변경하지 않음 (갑자기 나타나는 문제 방지)
        if (navAgent != null && navAgent.enabled && navAgent.isOnNavMesh)
        {
            // Walk/Run 상태일 때만 동기화 (Idle 상태에서는 위치 고정)
            if (currentState == ZombieState.Walk || currentState == ZombieState.Run)
            {
                if (Vector3.Distance(transform.position, navAgent.nextPosition) > navAgent.radius)
                {
                    navAgent.nextPosition = transform.position;
                }
            }
        }
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
        // 공격 중이면 공격 상태 유지 (쿨다운 체크)
        if (currentState == ZombieState.Attack)
        {
            // 공격 거리 내에 있고 쿨다운이 지나지 않았으면 공격 유지
            if (distanceToPlayer <= attackDistance * 1.2f && Time.time - lastAttackTime < attackCooldown)
            {
                return ZombieState.Attack;
            }
            // 공격 완료 후 거리 체크
        }
        
        // 탐색 중이면 탐색 상태 유지
        if (currentState == ZombieState.Search && isSearching)
        {
            // 탐색 중 플레이어가 가까워지면 추적 시작
            if (distanceToPlayer <= searchDetectionRange)
            {
                isSearching = false;
                searchTimer = 0f;
                // 거리에 따라 Walk 또는 Run으로 전환
                if (distanceToPlayer <= runDistance)
                {
                    return ZombieState.Run;
                }
                else if (distanceToPlayer <= walkDistance)
                {
                    return ZombieState.Walk;
                }
            }
            // 탐색 시간이 남아있으면 계속 탐색
            if (searchTimer > 0f)
            {
                return ZombieState.Search;
            }
            // 탐색 완료
            isSearching = false;
            searchTimer = 0f;
        }
        
        // 거리 기반 상태 결정 (가까운 순서대로 체크)
        if (distanceToPlayer <= attackDistance)
        {
            // 공격 거리 이하
            // 단일 Controller + AttackType 파라미터 사용 시에만 랜덤 공격 타입 선택
            if (useAttackTypeParameter && useRandomAttacks && availableAttackTypes.Length > 0)
            {
                // 랜덤으로 공격 타입 선택
                int randomIndex = Random.Range(0, availableAttackTypes.Length);
                currentAttackType = availableAttackTypes[randomIndex];
                Debug.Log($"ZombieAI: 공격 타입 랜덤 선택 - {currentAttackType} (인덱스: {randomIndex}, 사용 가능: {availableAttackTypes.Length}개)");
            }
            else
            {
                Debug.LogWarning($"ZombieAI: 공격 타입 랜덤 선택 실패 - useAttackTypeParameter: {useAttackTypeParameter}, useRandomAttacks: {useRandomAttacks}, availableAttackTypes.Length: {availableAttackTypes.Length}");
            }
            lastAttackTime = Time.time;
            return ZombieState.Attack;
        }
        else if (distanceToPlayer <= runDistance)
        {
            // 뛰기 거리 이하 (2.5m 초과 ~ 8m 이하)
            return ZombieState.Run;
        }
        else if (distanceToPlayer <= walkDistance)
        {
            // 걷기 거리 이하 (8m 초과 ~ 12m 이하)
            return ZombieState.Walk;
        }
        else if (distanceToPlayer > idleDistance)
        {
            // 최장거리 (25m 초과) - Idle 상태 (랜덤 Idle 타입 사용)
            // Idle 타입은 UpdateAnimations()에서 랜덤으로 변경
            return ZombieState.Idle;
        }
        else
        {
            // 중간 거리 (12m 초과 ~ 25m 이하) - Idle 상태
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
                // 탐색 시작 위치와 회전 저장
                if (!isSearching)
                {
                    searchStartPosition = transform.position;
                    searchStartRotation = transform.eulerAngles.y;
                }
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
                // 탐색 타이머 업데이트
                if (isSearching)
                {
                    searchTimer -= Time.deltaTime;
                    
                    // 주변을 둘러보는 동작 (부드러운 회전)
                    float rotationAmount = searchRotationSpeed * Time.deltaTime;
                    transform.Rotate(0, rotationAmount, 0);
                    
                    // 탐색 중 플레이어 감지 체크
                    if (distanceToPlayer <= searchDetectionRange)
                    {
                        // 플레이어 발견! 탐색 중단하고 추적 시작
                        isSearching = false;
                        searchTimer = 0f;
                    }
                    
                    // 탐색 시간 종료
                    if (searchTimer <= 0f)
                    {
                        isSearching = false;
                        searchTimer = 0f;
                    }
                }
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
        if (animator == null)
        {
            Debug.LogWarning("ZombieAI: Animator가 null입니다!");
            return;
        }
        
        if (!animator.enabled)
        {
            Debug.LogWarning("ZombieAI: Animator가 비활성화되어 있습니다!");
            return;
        }
        
        // 모든 애니메이션 파라미터 초기화
        animator.SetBool(animParamIsWalking, false);
        animator.SetBool(animParamIsRunning, false);
        animator.SetBool(animParamIsAttacking, false);
        
        // 속도 파라미터
        float speed = navAgent != null ? navAgent.velocity.magnitude : 0f;
        animator.SetFloat(animParamSpeed, speed);
        
        // 상태별 애니메이션 설정
        switch (currentState)
        {
            case ZombieState.Idle:
                // Idle 상태에서 랜덤으로 Idle 타입 변경
                if (useRandomIdleTypes && availableIdleTypes.Length > 0)
                {
                    // 일정 간격마다 Idle 타입 랜덤 변경
                    if (Time.time - lastIdleTypeChangeTime >= idleChangeInterval)
                    {
                        // 사용 가능한 Idle 타입 중 랜덤 선택 (각 좀비마다 다른 값)
                        int previousType = (int)currentIdleType;
                        int randomIndex = Random.Range(0, availableIdleTypes.Length);
                        currentIdleType = availableIdleTypes[randomIndex];
                        
                        // 같은 타입 연속 방지
                        if (availableIdleTypes.Length > 1 && (int)currentIdleType == previousType)
                        {
                            // 다른 타입 선택
                            int newIndex = (randomIndex + 1) % availableIdleTypes.Length;
                            currentIdleType = availableIdleTypes[newIndex];
                        }
                        
                        Debug.Log($"ZombieAI: Idle 타입 랜덤 변경 - {currentIdleType} (인덱스: {(int)currentIdleType}, 사용 가능: {availableIdleTypes.Length}개)");
                        lastIdleTypeChangeTime = Time.time;
                    }
                    
                    // IdleType 파라미터 설정 (Animator Controller에 전달)
                    // 매 프레임마다 설정하여 확실히 적용
                    if (animator.parameters != null)
                    {
                        bool paramFound = false;
                        foreach (AnimatorControllerParameter param in animator.parameters)
                        {
                            if (param.name == animParamIdleType && param.type == AnimatorControllerParameterType.Int)
                            {
                                animator.SetInteger(animParamIdleType, (int)currentIdleType);
                                paramFound = true;
                                break;
                            }
                        }
                        
                        // 파라미터가 없으면 경고 (디버깅용)
                        if (!paramFound && Time.frameCount % 300 == 0)
                        {
                            Debug.LogWarning($"ZombieAI: IdleType 파라미터 '{animParamIdleType}'를 찾을 수 없습니다. Animator Controller에 추가해주세요.");
                        }
                    }
                }
                // 모든 Bool 파라미터는 false로 유지
                break;
                
            case ZombieState.Walk:
                // 걷기 상태: IsWalking = true
                animator.SetBool(animParamIsWalking, true);
                break;
                
            case ZombieState.Run:
                // 뛰기 상태: IsRunning = true
                animator.SetBool(animParamIsRunning, true);
                break;
                
            case ZombieState.Attack:
                // 공격 상태: IsAttacking = true
                animator.SetBool(animParamIsAttacking, true);
                // 공격 타입 설정 (단일 Controller + AttackType 파라미터 사용 시에만)
                if (useAttackTypeParameter && animator.parameters != null)
                {
                    // AttackType 파라미터가 있으면 설정
                    bool paramFound = false;
                    foreach (AnimatorControllerParameter param in animator.parameters)
                    {
                        if (param.name == animParamAttackType && param.type == AnimatorControllerParameterType.Int)
                        {
                            animator.SetInteger(animParamAttackType, (int)currentAttackType);
                            paramFound = true;
                            break;
                        }
                    }
                    
                    // 파라미터가 없으면 경고 (디버깅용)
                    if (!paramFound && Time.frameCount % 300 == 0)
                    {
                        Debug.LogWarning($"ZombieAI: AttackType 파라미터 '{animParamAttackType}'를 찾을 수 없습니다. Animator Controller에 추가해주세요.");
                    }
                }
                // 여러 Controller 사용 시: 각 Controller에 이미 공격 애니메이션이 설정되어 있으므로
                // IsAttacking = true만 설정하면 Controller가 자동으로 해당 공격 재생
                break;
                
            case ZombieState.Search:
                // 탐색 상태: IdleType.Search로 처리됨 (Idle 상태에서 IdleType 파라미터로 제어)
                // 별도의 IsSearching 파라미터 불필요
                break;
                
            case ZombieState.Stumble:
                // 우당탕 상태: 현재 사용하지 않음
                break;
        }
        
        // 디버그: 현재 상태 로그 (10초마다 - 로그 줄이기)
        if (Time.frameCount % 600 == 0)
        {
            Debug.Log($"ZombieAI 상태: {currentState}, 거리: {distanceToPlayer:F2}m (Walk:{walkDistance}m, Run:{runDistance}m), IsWalking: {animator.GetBool(animParamIsWalking)}, IsRunning: {animator.GetBool(animParamIsRunning)}");
        }
    }
    
    // === 루트 모션 제어 ===
    // NavMeshAgent를 사용할 때 애니메이션의 루트 모션이 Y축 위치를 변경하지 않도록 제어
    void OnAnimatorMove()
    {
        if (animator == null || navAgent == null || !navAgent.enabled)
        {
            return;
        }
        
        // 초기 Y 위치 사용 (땅 아래로 들어가는 것을 방지)
        float initialY = transform.position.y;
        
        // NavMeshAgent를 사용하는 상태 (Walk, Run)
        if (navAgent.isOnNavMesh && (currentState == ZombieState.Walk || currentState == ZombieState.Run))
        {
            // NavMeshAgent의 다음 위치 가져오기 (XZ만)
            Vector3 nextPosition = navAgent.nextPosition;
            
            // Y 위치는 초기 위치 유지 (하늘로 올라가거나 땅에 박히는 문제 방지)
            nextPosition.y = initialY;
            
            // Transform 위치를 NavMeshAgent의 위치로 설정 (Y는 초기 위치 유지)
            transform.position = nextPosition;
            
            // 회전은 NavMeshAgent의 회전을 사용하거나 루트 모션 회전 사용
            if (navAgent.velocity.magnitude > 0.1f)
            {
                // 이동 방향으로 회전
                Vector3 direction = navAgent.velocity.normalized;
                direction.y = 0;
                if (direction.magnitude > 0.1f)
                {
                    transform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }
        else if (currentState == ZombieState.Idle)
        {
            // Idle 상태에서는 위치 변경하지 않음 (설치한 위치 유지)
            // Y 위치를 명시적으로 유지하여 땅에 박히는 문제 방지
            Vector3 currentPos = transform.position;
            currentPos.y = initialY;
            transform.position = currentPos;
            
            // 회전만 적용 (Idle 상태에서도 회전은 가능)
            if (animator.deltaRotation != Quaternion.identity)
            {
                transform.rotation = animator.rootRotation;
            }
        }
        else
        {
            // NavMeshAgent를 사용하지 않는 다른 상태 (Attack 등)
            // 루트 모션 적용 - XZ만 이동, Y는 유지
            Vector3 rootMotionDelta = animator.deltaPosition;
            rootMotionDelta.y = 0; // Y축 이동 무시
            
            Vector3 newPosition = transform.position + rootMotionDelta;
            newPosition.y = initialY; // Y 위치는 초기 위치 유지
            
            transform.position = newPosition;
            
            // 회전 적용
            if (animator.deltaRotation != Quaternion.identity)
            {
                transform.rotation = animator.rootRotation;
            }
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

