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

    // 👇👇👇 수정된 부분: 드랍 확률이 0.1f (10%)로 재설정됨 👇👇👇
    [Header("Drop Item")]
    // 에디터에서 드랍할 열쇠 프리팹을 할당합니다.
    public GameObject keyPrefab;
    // 열쇠 드랍 확률 (0.1 = 10%)
    public float keyDropChance = 0.1f; // 10% 드랍으로 재설정
    // 👆👆👆 수정된 부분 👆👆👆

    private Animator animator;
    private Collider mainCollider;
    private ZombieAI zombieAI;
    private ZombieAudio zombieAudio;

    void Start()
    {
        currentHp = maxHp;
        animator = GetComponent<Animator>();
        mainCollider = GetComponent<Collider>();
        zombieAI = GetComponent<ZombieAI>();
        zombieAudio = GetComponent<ZombieAudio>();
    }

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

        if (partType == ZombieBodyPart.PartType.Head)
        {
            if (zombieAudio != null) zombieAudio.PlayAgony();
            // ...
            Die();
            return;
        }

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

        // 👇👇👇 열쇠 드랍 로직 (10% 확률) 👇👇👇
        // Random.value가 0.1f 미만일 때 (10% 확률) 열쇠를 드랍합니다.
        if (keyPrefab != null && Random.value < keyDropChance)
        {
            Instantiate(keyPrefab, transform.position, Quaternion.identity);
            Debug.Log("🎉 열쇠를 드랍했습니다! (확률: 10%)");
        }
        // 👆👆👆 열쇠 드랍 로직 👆👆👆

        // 5초 후 게임 오브젝트 파괴
        Destroy(gameObject, 5f);
    }
}