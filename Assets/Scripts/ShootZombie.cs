using UnityEngine;

public class ShootZombie : MonoBehaviour
{
    [Header("Stats")]
    public int maxHp = 100;
    private int currentHp;
    
    [Header("Dismemberment Settings")]
    [Range(0, 1)] public float dismemberChance = 0.5f; // 50% 확률로 절단
    public GameObject bloodEffectPrefab; // (선택) 피 이펙트

    void Start()
    {
        currentHp = maxHp;
    }

    // 부위 정보를 포함한 피격 처리 함수
    public void TakeHit(int damage, ZombieBodyPart part)
    {
        // 이미 죽었으면 무시
        if (currentHp <= 0) return;

        // 1. 헤드샷 처리 (즉사)
        if (part.partType == ZombieBodyPart.PartType.Head)
        {
            Debug.Log("헤드샷! 즉사.");
            currentHp = 0;
            Die();
            return;
        }

        // 2. 일반 데미지 적용
        currentHp -= damage;

        // 3. 팔/다리 절단 로직 (체력이 남았을 때도 발생 가능)
        // 몸통(Body)은 잘리면 안 되므로 제외
        if (part.partType == ZombieBodyPart.PartType.Arm || part.partType == ZombieBodyPart.PartType.Leg)
        {
            // 랜덤 확률 체크 (0.0 ~ 1.0)
            if (Random.value < dismemberChance)
            {
                // 해당 부위 절단 명령
                part.Dismember();
                
                // (선택) 피 이펙트 생성
                if (bloodEffectPrefab != null)
                {
                    Instantiate(bloodEffectPrefab, part.transform.position, Quaternion.identity);
                }
            }
        }

        // 4. 사망 체크
        if (currentHp <= 0) Die();
    }

    // (기존 호환용) 그냥 데미지만 들어올 때
    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        if (currentHp <= 0) Die();
    }

    void Die()
    {
        // 여기에 사망 애니메이션이나 랙돌 로직 추가 가능
        Debug.Log("좀비 사망");
        Destroy(gameObject); 
    }
}