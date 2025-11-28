using UnityEngine;

public class ShootZombie : MonoBehaviour
{
    [Header("Stats")]
    public int maxHp = 100;
    public int currentHp;
    private bool isDead = false;

    // [설정] 기어가기 시작할 체력
    public int crawlHpThreshold = 30;

    // [내부변수] 이미 기어갈지 말지 결정했는지 체크하는 플래그
    private bool hasDecidedCrawl = false;

    // 👇👇👇 새로 추가된 부분: 차량 충돌 설정 👇👇👇
    [Header("Car Collision")]
    // 차량 충돌 시 입는 데미지
    public int carHitDamage = 50;
    // 차량 충돌 시 밀려나는 힘의 세기
    public float knockbackForce = 15f;
    // 차량의 태그를 설정 (예: "Car")
    public string carTag = "Car";
    // 👆👆👆 새로 추가된 부분 👆👆👆

    [Header("Drop Item")]
    // 에디터에서 드랍할 열쇠 프리팹을 할당합니다.
    public GameObject keyPrefab;
    // 열쇠 드랍 확률 (0.1 = 10%)
    public float keyDropChance = 0.1f; // 10% 드랍으로 재설정
    
    // 붕대 프리팹 및 드랍 확률
    public GameObject bandagePrefab;
    public float bandageDropChance = 0.3f; // 30% 드랍 확률

    private Animator animator;
    private Collider mainCollider;
    private ZombieAI zombieAI;
    private ZombieAudio zombieAudio;
    // 👇👇👇 새로 추가된 부분: Rigidbody 참조 👇👇👇
    private Rigidbody rb;
    // 👆👆👆 새로 추가된 부분 👆👆👆

    void Start()
    {
        currentHp = maxHp;
        animator = GetComponent<Animator>();
        mainCollider = GetComponent<Collider>();
        zombieAI = GetComponent<ZombieAI>();
        zombieAudio = GetComponent<ZombieAudio>();
        // 👇👇👇 새로 추가된 부분: Rigidbody 가져오기 👇👇👇
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("좀비 오브젝트에 Rigidbody 컴포넌트가 없습니다! 차량 충돌 밀림이 작동하지 않을 수 있습니다.");
        }
        // 👆👆👆 새로 추가된 부분 👆👆👆
    }

    // ... (TakeHit, TakeDamage, ApplyDamage 함수는 변경 없음) ...

    public void TakeHit(int damage, ZombieBodyPart.PartType partType)
    {
        if (isDead) return;

        // 헤드샷: 무조건 즉사 (기어가는 로직 무시)
        if (partType == ZombieBodyPart.PartType.Head)
        {
            currentHp = 0;
            Die();
            return;
        }

        // 중복된 헤드샷 로직 제거 또는 수정
        // if (partType == ZombieBodyPart.PartType.Head) { ... } 이 부분은 위에서 이미 처리되었습니다.

        if (currentHp > 0 && zombieAudio != null)
        {
            zombieAudio.PlayHurt();
        }

        ApplyDamage(damage);
    }

    // (호환용) 부위 상관없는 데미지 함수
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        // 1. 피격 사운드 재생
        if (currentHp > damage && zombieAudio != null)
        {
            zombieAudio.PlayHurt();
        }

        currentHp -= damage;

        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            // 2. 피격 애니메이션 및 AI 일시 정지
            if (animator != null) animator.SetTrigger("OnHit");
            if (zombieAI != null) zombieAI.SetHit();

            // 3. 기어가기(Crawl) 확률 로직
            if (currentHp <= crawlHpThreshold && !hasDecidedCrawl)
            {
                hasDecidedCrawl = true;

                if (Random.value < 0.5f)
                {
                    if (zombieAI != null) zombieAI.StartCrawling();
                    Debug.Log("TakeDamage 피격으로 다리 부상! (기어가기 시작)");
                }
            }
        }
    }

    // 데미지 처리 통합 함수
    void ApplyDamage(int damage)
    {
        currentHp -= damage;

        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            // 피격 모션
            if (animator != null) animator.SetTrigger("OnHit");
            if (zombieAI != null) zombieAI.SetHit();

            // [핵심 로직] 체력이 낮아졌고, 아직 결정하지 않았다면?
            if (currentHp <= crawlHpThreshold && !hasDecidedCrawl)
            {
                hasDecidedCrawl = true;

                // 50% 확률 (Random.value는 0.0 ~ 1.0 사이 난수)
                if (Random.value < 0.5f)
                {
                    // 당첨! -> 기어가기 시작
                    if (zombieAI != null) zombieAI.StartCrawling();
                    Debug.Log("다리가 부러졌다! (기어가기 시작)");
                }
                else
                {
                    // 꽝! -> 그냥 서서 계속 덤빔
                    Debug.Log("아직 버틸만하다! (계속 걸음)");
                }
            }
        }
    }

    // 👇👇👇 새로 추가된 부분: 차량 충돌 감지 및 밀림 로직 👇👇👇
    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 오브젝트의 태그가 차량 태그와 일치하는지 확인
        if (collision.gameObject.CompareTag(carTag))
        {
            if (isDead) return;

            // 1. 데미지 적용
            Debug.Log($"좀비가 차량에 치였습니다! 데미지: {carHitDamage}");
            TakeDamage(carHitDamage); // ApplyDamage 대신 기존 TakeDamage 사용

            // 2. 밀어내기(넉백)
            if (rb != null)
            {
                // 충돌 지점에서 좀비의 위치로 향하는 벡터 (밀려나갈 방향)
                Vector3 knockbackDirection = (transform.position - collision.contacts[0].point).normalized;

                // Y축(수직) 방향의 힘은 줄이고 수평(XZ 평면) 방향으로 더 강하게 밀어냅니다.
                knockbackDirection.y = 0.5f; // 약간의 수직 방향 힘 추가
                knockbackDirection = knockbackDirection.normalized;

                // Rigidbody에 순간적인 힘을 가함
                rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);

                // 좀비 AI를 잠시 멈추거나 넉백 상태로 설정 (필요에 따라 ZombieAI 스크립트 수정 필요)
                // 예: if (zombieAI != null) zombieAI.SetKnockback(0.5f); 
            }
        }
    }
    // 👆👆👆 새로 추가된 부분 👆👆👆

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (zombieAudio != null)
        {
            zombieAudio.PlayAgony();
        }

        // 사망 트리거 발동
        if (animator != null)
        {
            animator.SetTrigger("OnDeath");
        }

        // AI 정지
        if (zombieAI != null) zombieAI.SetDead();

        // 콜라이더 끄기
        if (mainCollider != null) mainCollider.enabled = false;
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach (var col in allColliders) col.enabled = false;

        // 열쇠 드랍 로직 (10% 확률)
        // Random.value가 0.1f 미만일 때 (10% 확률) 열쇠를 드랍합니다.
        if (keyPrefab != null && Random.value < keyDropChance)
        {
            Instantiate(keyPrefab, transform.position, Quaternion.identity);
            Debug.Log("🎉 열쇠를 드랍했습니다! (확률: 10%)");
        }

        // 👇👇👇 붕대 드랍 로직 (30% 확률) 👇👇👇
        if (bandagePrefab != null && Random.value < bandageDropChance)
        {
            Instantiate(bandagePrefab, transform.position, Quaternion.identity);
            Debug.Log($"🎉 붕대를 드랍했습니다! (확률: {bandageDropChance * 100}%)");
        }
        // 👆👆👆 붕대 드랍 로직 👆👆👆

        // 5초 후 게임 오브젝트 파괴
        Destroy(gameObject, 5f);
    }
}