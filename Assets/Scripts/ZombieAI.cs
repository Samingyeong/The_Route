using UnityEngine;
using UnityEngine.AI;

public enum ZombieState
{
    Idle,
    Walk,
    Run,
    Attack
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
    
    [Header("=== 애니메이션 파라미터 ===")]
    [SerializeField] private string animParamIsWalking = "IsWalking";
    [SerializeField] private string animParamIsRunning = "IsRunning";
    [SerializeField] private string animParamIsAttacking = "IsAttacking";
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
        
        // 상태 머신 업데이트
        UpdateStateMachine();
        
        // 애니메이션 업데이트
        UpdateAnimations();
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
        // 공격 중이면 공격 상태 유지
        if (currentState == ZombieState.Attack)
        {
            if (distanceToPlayer <= attackDistance * 1.2f && Time.time - lastAttackTime < attackCooldown)
            {
                return ZombieState.Attack;
            }
        }
        
        // 거리 기반 상태 결정
        if (distanceToPlayer <= attackDistance)
        {
            // 공격 타입 랜덤 선택
            if (useRandomAttacks && availableAttackTypes.Length > 0)
            {
                currentAttackType = availableAttackTypes[Random.Range(0, availableAttackTypes.Length)];
            }
            lastAttackTime = Time.time;
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
        else
        {
            return ZombieState.Idle;
        }
    }
    
    void EnterState(ZombieState state)
    {
        if (navAgent == null || !navAgent.enabled || !navAgent.isOnNavMesh)
        {
            return;
        }
        
        switch (state)
        {
            case ZombieState.Idle:
                navAgent.isStopped = true;
                navAgent.nextPosition = transform.position;
                
                // Idle 상태로 진입할 때 한 번만 IdleType 선택
                if (useRandomIdleTypes && availableIdleTypes.Length > 0 && previousState != ZombieState.Idle)
                {
                    int previousType = (int)currentIdleType;
                    int randomIndex = Random.Range(0, availableIdleTypes.Length);
                    currentIdleType = availableIdleTypes[randomIndex];
                    
                    // 같은 타입 연속 방지
                    if (availableIdleTypes.Length > 1 && (int)currentIdleType == previousType)
                    {
                        int newIndex = (randomIndex + 1) % availableIdleTypes.Length;
                        currentIdleType = availableIdleTypes[newIndex];
                    }
                }
                break;
                
            case ZombieState.Walk:
                navAgent.isStopped = false;
                navAgent.speed = walkSpeed;
                break;
                
            case ZombieState.Run:
                navAgent.isStopped = false;
                navAgent.speed = runSpeed;
                break;
                
            case ZombieState.Attack:
                navAgent.isStopped = true;
                break;
        }
    }
    
    void UpdateState(ZombieState state)
    {
        switch (state)
        {
            case ZombieState.Walk:
            case ZombieState.Run:
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
        if (navAgent.isOnNavMesh && (currentState == ZombieState.Walk || currentState == ZombieState.Run))
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
