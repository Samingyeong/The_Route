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

    private Animator animator;
    private Collider mainCollider;
    private ZombieAI zombieAI;

    void Start()
    {
        currentHp = maxHp;
        animator = GetComponent<Animator>();
        mainCollider = GetComponent<Collider>();
        zombieAI = GetComponent<ZombieAI>();
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

        ApplyDamage(damage);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        ApplyDamage(damage);
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
                hasDecidedCrawl = true; // 결정 완료 표시 (다시 실행 안 되게)

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
        foreach(var col in allColliders) col.enabled = false;

        Destroy(gameObject, 5f);
    }
}