using UnityEngine;

public class ZombieBodyPart : MonoBehaviour
{
    public enum PartType { Body, Head, Arm, Leg }
    public PartType partType = PartType.Body;

    // 메인 좀비 스크립트 (부모 찾아서 자동 연결할 예정)
    private ShootZombie mainZombie;
    private Collider myCollider; // 내 콜라이더 (MeshCollider 등)

    void Start()
    {
        // 부모 쪽에서 메인 스크립트 찾기
        mainZombie = GetComponentInParent<ShootZombie>();
        myCollider = GetComponent<Collider>();
    }

    // 총알이 맞았을 때 호출되는 함수
    public void OnHit(int damage)
    {
        if (mainZombie != null)
        {
            // 메인 좀비에게 "나(이 부위) 맞았어!"라고 보고
            mainZombie.TakeHit(damage, this);
        }
    }

    // 부위 절단 (숨기기) 함수
    public void Dismember()
    {
        // 1. 시각적으로 안 보이게 크기를 0으로 (자식 오브젝트들도 같이 안 보임)
        transform.localScale = Vector3.zero;

        // 2. 더 이상 총알에 맞지 않게 콜라이더 끄기
        if (myCollider != null) myCollider.enabled = false;

        Debug.Log(name + " 부위가 절단되었습니다!");
    }
}