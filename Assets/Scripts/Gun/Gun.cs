using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("총 기본 설정")]
    public string gunName;      
    public bool isAutomatic;    // 연사 여부
    public int maxAmmo = 30;    
    public int currentAmmo;     
    public float fireRate = 0.1f; 
    public int damage = 20;     
    public float range = 100f;

    [Header("애니메이션 컨트롤러 연결 (중요!)")]
    public Animator gunAnimator; // << 여기에 각 총의 애니메이터(controller_gun_xxx)를 넣으세요.

    [Header("반동 설정")]
    public Vector3 recoilRotation = new Vector3(10f, 5f, 3f); 
    public Vector3 aimRecoilRotation = new Vector3(3f, 1f, 1f);

    [Header("사운드 & 이펙트")]
    public Transform firePoint;     
    public AudioClip fireSound;
    public AudioClip reloadSound;

    void OnEnable()
    {
        // 총을 꺼낼 때(활성화될 때) Draw 애니메이션 재생
        if (gunAnimator != null) gunAnimator.SetTrigger("OnDraw");
    }

    public void Reload()
    {
        currentAmmo = maxAmmo;
    }
}