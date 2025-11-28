using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("총 기본 설정")]
    public string gunName;      
    public bool isAutomatic;    
    public int maxAmmo = 30;    
    public int currentAmmo;     
    public float fireRate = 0.1f; 
    public int damage = 20;     
    public float range = 100f;

    [Header("애니메이션 컨트롤러")]
    public Animator gunAnimator;

    [Header("반동 범위 설정 (랜덤)")]
    // 일반 사격 시 최소~최대 반동
    public Vector3 minRecoilRotation = new Vector3(8f, 4f, 2f); 
    public Vector3 maxRecoilRotation = new Vector3(12f, 6f, 4f);

    // 조준(Aim) 사격 시 최소~최대 반동
    public Vector3 minAimRecoilRotation = new Vector3(2f, 0.5f, 0.5f);
    public Vector3 maxAimRecoilRotation = new Vector3(4f, 1.5f, 1.5f);

    [Header("반동 물리 설정 (속도감)")]
    public float snappiness = 0.1f; // 작을수록 빠르고 강함 (Glock: 0.05, Kriss: 0.1)
    public float returnSpeed = 5f;  // 클수록 빨리 돌아옴 (5 ~ 10 추천)

    [Header("사운드 & 이펙트")]
    public Transform firePoint;     
    public AudioClip fireSound;
    public AudioClip reloadSound;

    public AudioClip dryFireSound;
    public ParticleSystem muzzleFlashParticles;

    void OnEnable()
    {
        if (gunAnimator != null) gunAnimator.SetTrigger("OnDraw");
    }

    public void Reload()
    {
        currentAmmo = maxAmmo;
    }
    public void PlayReloadSound()
    {
        // 플레이어(부모)에 있는 AudioSource를 찾아서 재생
        AudioSource audio = GetComponentInParent<AudioSource>();
        
        if (audio != null && reloadSound != null)
        {
            audio.PlayOneShot(reloadSound);
        }
    }
}