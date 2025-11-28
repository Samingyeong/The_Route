using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;
    public float lifeTime = 3f; // 3초 뒤 자동 삭제

    void Start()
    {
        Destroy(gameObject, lifeTime); // 너무 오래된 총알 삭제
    }

    void OnCollisionEnter(Collision collision)
    {
        // 1. 무엇과 부딪혔는지 이름 출력
        Debug.Log($"[Bullet] 충돌 발생! 대상: {collision.gameObject.name}");

        ZombieBodyPart bodyPart = collision.collider.GetComponent<ZombieBodyPart>();
        
        // 2. 좀비 부위(BodyPart)를 찾았는지 확인
        if (bodyPart != null)
        {
            Debug.Log($" -> [Hit] 좀비 부위 명중! 데미지: {(int)damage}");
            bodyPart.OnHit((int)damage);
        }
        else
        {
            ShootZombie target = collision.transform.GetComponent<ShootZombie>();
            if (target == null) target = collision.transform.GetComponentInParent<ShootZombie>();
            
            if (target != null)
            {
                Debug.Log($" -> [Hit] 좀비 본체(또는 부모) 명중! 데미지: {(int)damage}");
                target.TakeDamage((int)damage);
            }
            else
            {
                // 3. 좀비가 아닌 벽이나 바닥에 맞은 경우
                Debug.Log(" -> [Miss] 좀비가 아님 (벽, 바닥 등)");
            }
        }

        Destroy(gameObject);
    }
}