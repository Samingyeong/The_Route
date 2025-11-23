using UnityEngine;

public class ShootZombie : MonoBehaviour
{
    public int maxHp = 100;
    private int currentHp;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHp = maxHp; 
    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        if (currentHp <= 0) Die();
    }
    void Die() // 지금은 걍 없어지게 해뒀고 디테일은 수정 필요
    {
        Destroy(gameObject); // 좀비 삭제 (큐브가 사라짐)
    }
    
    void Update()
    {
        
    }
}
