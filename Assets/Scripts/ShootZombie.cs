using UnityEngine;

public class ShootZombie : MonoBehaviour
{
    [Header("Stats")]
    public int maxHp = 100;
    public int currentHp;
    private bool isDead = false;

    // 애니메이터 (사망 모션용)
    private Animator animator;
    private Collider mainCollider; // (선택) 사망 후 시체 판정 끄기용

    void Start()
    {
        currentHp = maxHp;
        animator = GetComponent<Animator>();
        mainCollider = GetComponent<Collider>();
    }

    // 부위 정보를 받는 피격 함수
    public void TakeHit(int damage, ZombieBodyPart.PartType partType)
    {
        if (isDead) return;

        // 1. 헤드샷 판정 (머리면 무조건 즉사)
        if (partType == ZombieBodyPart.PartType.Head)
        {
            Debug.Log("헤드샷! (즉사)");
            currentHp = 0;
            Die();
            return;
        }

        // 2. 그 외 부위 (팔, 다리, 몸통) -> 그냥 데미지 적용
        Debug.Log(partType + " 피격! 데미지: " + damage);
        currentHp -= damage;

        if (currentHp <= 0)
        {
            Die();
        }
    }

    // (호환용) 부위 상관없는 데미지 함수
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHp -= damage;
        if (currentHp <= 0) Die();
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

        // 2. 죽은 뒤 총알 안 맞게 콜라이더 끄기 (선택사항)
        // (자식들에 있는 ZombieBodyPart 콜라이더들도 끄고 싶다면 아래 주석 해제)
        /*
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach(var col in allColliders) col.enabled = false;
        */

        // 3. 5초 뒤 시체 삭제
        Destroy(gameObject, 5f);
    }
}