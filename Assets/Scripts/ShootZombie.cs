using UnityEngine;

public class ShootZombie : MonoBehaviour
{
    public int maxHp = 100;
    private int currentHp;

    void Start()
    {
        currentHp = maxHp;
    }

    // 부위 정보를 받아서 처리하는 새로운 데미지 함수
    public void TakeHit(int damage, ZombieBodyPart.PartType part)
    {
        // 1. 머리를 맞았을 경우 -> 즉사
        if (part == ZombieBodyPart.PartType.Head)
        {
            Debug.Log("헤드샷! 즉사합니다.");
            currentHp = 0;
            Die();
            return; // 아래 데미지 계산 안 하고 바로 종료
        }

        // 2. 그 외(몸통 등)를 맞았을 경우 -> 체력 감소
        currentHp -= damage;
        Debug.Log("데미지!.");
        if (currentHp <= 0) Die();
    }

    // (참고) 부위 상관없이 그냥 데미지 줄 때 쓰는 함수 (폭탄 등)
    public void TakeDamage(int damage)
    {
        TakeHit(damage, ZombieBodyPart.PartType.Body);
    }

    void Die()
    {
        Destroy(gameObject);
    }
}