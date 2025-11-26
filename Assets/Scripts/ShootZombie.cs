using UnityEngine;

public class ShootZombie : MonoBehaviour
{
    [Header("Stats")]
    public int maxHp = 100;
    public int currentHp;
    private bool isDead = false;

    private Animator animator;
    private Collider mainCollider;
    
    // [추가] AI 스크립트 제어를 위한 참조
    private ZombieAI zombieAI;

    void Start()
    {
        currentHp = maxHp;
        animator = GetComponent<Animator>();
        mainCollider = GetComponent<Collider>();
        
        // [추가] 같은 오브젝트에 있는 ZombieAI 가져오기
        zombieAI = GetComponent<ZombieAI>();
    }

    public void TakeHit(int damage, ZombieBodyPart.PartType partType)
    {
        if (isDead) return;

        // 헤드샷 처리
        if (partType == ZombieBodyPart.PartType.Head)
        {
            Debug.Log("헤드샷! (즉사)");
            currentHp = 0;
            Die();
            return;
        }

        Debug.Log(partType + " 피격! 데미지: " + damage);
        currentHp -= damage;

        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            // [추가] 아직 살아있으면 피격(Hit) 모션 재생
            if (animator != null)
            {
                animator.SetTrigger("OnHit");
            }
            
            // [추가] AI에게 피격 사실 알림 (잠시 멈추게 하기 위함)
            if (zombieAI != null)
            {
                zombieAI.SetHit();
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHp -= damage;
        
        if (currentHp <= 0) 
        {
            Die();
        }
        else
        {
            // [추가] 호환용 함수에도 피격 모션 추가
            if (animator != null) animator.SetTrigger("OnHit");
            if (zombieAI != null) zombieAI.SetHit();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("좀비 사망");

        // 1. 사망 애니메이션 재생
        if (animator != null)
        {
            animator.SetTrigger("OnDeath");
        }

        // [추가] AI 완전히 정지시키기
        if (zombieAI != null)
        {
            zombieAI.SetDead();
        }

        // 2. 콜라이더 끄기 (시체 위로 지나갈 수 있게)
        if (mainCollider != null) mainCollider.enabled = false;
        
        // 자식 콜라이더들도 끄기
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach(var col in allColliders) col.enabled = false;

        // 3. 시체 삭제
        Destroy(gameObject, 5f);
    }
}