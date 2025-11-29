using UnityEngine;

public class ZombieBodyPart : MonoBehaviour
{
    // 부위 종류 (머리만 구분하면 되지만, 나중을 위해 남겨둠)
    public enum PartType { Body, Head, Arm, Leg }
    public PartType partType = PartType.Body;

    private ShootZombie mainZombie;

    void Start()
    {
        // 부모에 있는 메인 스크립트 찾기
        mainZombie = GetComponentInParent<ShootZombie>();
    }

    // 총알이 맞으면 호출됨
    // ZombieBodyPart.cs의 OnHit 메서드에 추가
    public void OnHit(int damage)
    {
        if (mainZombie != null)
        {
            // 메인 좀비에게 "데미지랑 내 부위 정보"를 넘김
            mainZombie.TakeHit(damage, partType);
            
            // 튜토리얼 매니저에 알림
            TutorialManager tutorialManager = FindObjectOfType<TutorialManager>();
            if (tutorialManager != null)
            {
                bool isHeadshot = (partType == PartType.Head);
                tutorialManager.OnZombieHit(isHeadshot);
            }
        }
    }
}