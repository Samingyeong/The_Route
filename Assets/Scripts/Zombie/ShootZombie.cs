using System.Collections;
using UnityEngine;
using UnityEngine.AI; // NavMeshAgent 제어를 위해 추가

public class ShootZombie : MonoBehaviour
{
    [Header("Stats")]
    public int maxHp = 100;
    public int currentHp;
    private bool isDead = false;

    // [설정] 기어가기 시작할 체력
    public int crawlHpThreshold = 30;
    private bool hasDecidedCrawl = false;

    [Header("Car Collision")]
    public int carHitDamage = 50;
    public float knockbackForce = 15f;
    public string carTag = "Car";

    [Header("Drop Item")]
    public GameObject keyPrefab;
    public float keyDropChance = 0.1f;
    public GameObject bandagePrefab;
    public float bandageDropChance = 0.3f;

    private Animator animator;
    private Collider mainCollider;
    private ZombieAI zombieAI;
    private ZombieAudio zombieAudio;
    private Rigidbody rb;
    private NavMeshAgent navAgent; // AI 이동 제어를 위해 추가

    void Start()
    {
        currentHp = maxHp;
        animator = GetComponent<Animator>();
        mainCollider = GetComponent<Collider>();
        zombieAI = GetComponent<ZombieAI>();
        zombieAudio = GetComponent<ZombieAudio>();
        rb = GetComponent<Rigidbody>();
        navAgent = GetComponent<NavMeshAgent>(); // NavMeshAgent 가져오기

        if (rb == null)
        {
            Debug.LogError("좀비 오브젝트에 Rigidbody가 없습니다!");
        }
        else
        {
            // [중요] 시작할 때 물리 힘에 영향을 받지 않도록 Kinematic을 켭니다.
            // 이렇게 하면 총을 맞아도 뒤로 밀리지 않습니다.
            rb.isKinematic = true;
        }
    }

    public void TakeHit(int damage, ZombieBodyPart.PartType partType)
    {
        if (isDead) return;

        if (partType == ZombieBodyPart.PartType.Head)
        {
            currentHp = 0;
            Die();
            return;
        }

        if (currentHp > 0 && zombieAudio != null)
        {
            zombieAudio.PlayHurt();
        }

        ApplyDamage(damage);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        if (currentHp > damage && zombieAudio != null)
        {
            zombieAudio.PlayHurt();
        }

        ApplyDamage(damage);
    }

    void ApplyDamage(int damage)
    {
        currentHp -= damage;

        if (currentHp <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null) animator.SetTrigger("OnHit");
            if (zombieAI != null) zombieAI.SetHit();

            if (currentHp <= crawlHpThreshold && !hasDecidedCrawl)
            {
                hasDecidedCrawl = true;
                if (Random.value < 0.5f)
                {
                    if (zombieAI != null) zombieAI.StartCrawling();
                    Debug.Log("다리가 부러졌다! (기어가기 시작)");
                }
            }
        }
    }

    // [수정됨] 차량 충돌 처리 로직
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(carTag))
        {
            if (isDead) return;

            Debug.Log($"좀비가 차량에 치였습니다! 데미지: {carHitDamage}");
            TakeDamage(carHitDamage);

            // 넉백 로직 (차량 충돌 시에만 물리 엔진 활성화)
            if (rb != null)
            {
                StartCoroutine(ApplyKnockback(collision));
            }
        }
    }

    // [추가됨] 넉백 처리를 위한 코루틴
    private IEnumerator ApplyKnockback(Collision collision)
    {
        // 1. AI 및 NavMesh 잠시 끄기 (물리 힘이 적용되도록)
        if (navAgent != null) navAgent.enabled = false;
        if (zombieAI != null) zombieAI.enabled = false;

        // 2. 물리 엔진 켜기 (Kinematic 끄기)
        rb.isKinematic = false;

        // 3. 힘 가하기
        Vector3 knockbackDirection = (transform.position - collision.contacts[0].point).normalized;
        knockbackDirection.y = 0.5f; 
        knockbackDirection = knockbackDirection.normalized;
        
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);

        // 4. 좀비가 날아가는 동안 잠시 대기 (예: 1.5초)
        yield return new WaitForSeconds(1.5f);

        // 5. 죽지 않았다면 다시 복구
        if (!isDead)
        {
            // 물리 엔진 다시 끄기 (총에 안 밀리게)
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero; // 남은 속도 제거

            // NavMeshAgent 다시 켜기 (다시 켤 때는 위치를 NavMesh 위로 보정해야 안전함)
            if (navAgent != null)
            {
                navAgent.Warp(transform.position); // 현재 위치로 Agent 이동
                navAgent.enabled = true;
            }
            if (zombieAI != null) zombieAI.enabled = true;
            
            // 일어나는 애니메이션 등이 필요하다면 여기서 처리
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (zombieAudio != null) zombieAudio.PlayAgony();
        if (animator != null) animator.SetTrigger("OnDeath");
        if (zombieAI != null) zombieAI.SetDead();

        // 사망 시에는 물리 충돌 완전히 끄기
        if (mainCollider != null) mainCollider.enabled = false;
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach (var col in allColliders) col.enabled = false;
        
        // 사망 시 Kinematic을 켜서 시체가 물리적으로 굴러다니지 않게 고정 (원하면 false로 해서 랙돌처럼 만들 수도 있음)
        if (rb != null) rb.isKinematic = true;

        if (keyPrefab != null && Random.value < keyDropChance)
        {
            Instantiate(keyPrefab, transform.position, Quaternion.identity);
        }

        if (bandagePrefab != null && Random.value < bandageDropChance)
        {
            Instantiate(bandagePrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject, 5f);
    }
}