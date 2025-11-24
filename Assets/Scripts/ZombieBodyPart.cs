using UnityEngine;

public class ZombieBodyPart : MonoBehaviour
{
    // 머리(Head)인지 몸통(Body)인지 구분만 하면 됩니다.
    public enum PartType { Body, Head }
    
    public PartType partType = PartType.Body;
    public ShootZombie mainScript; // 메인 스크립트 연결

    // 총알이 맞으면 이 함수가 실행됨
    public void OnHit(int damage)
    {
        if (mainScript != null)
        {
            // 메인 스크립트에게 "나(부위) 맞았어!" 하고 알려줌
            mainScript.TakeHit(damage, partType);
        }
    }
}