using UnityEngine;

public class gunaction_glock : MonoBehaviour
{
    // 총 설정
    public int maxAmmo = 12;     // 권총이니까 탄창 용량 줄임 (예시)
    public int currentAmmo = 0; 
    public float fireRate = 0.2f; // 권총은 연사 속도가 빠름 (광클 가능하게)
    private float nextFireTime = 0f;
    public int damage = 15;      // 데미지는 기관단총보다 조금 낮거나 비슷하게
    public float range = 50f;    // 사거리는 조금 짧게

    // 스코프 모드 설정 (권총도 조준은 가능하므로 유지)
    public bool isSniperMode = false; 
    public float defaultFOV = 60f;    
    public float scopeFOV = 40f;      // 권총은 줌을 덜 당김
    public float zoomSpeed = 10f;     

    // 컴포넌트 연결
    public Camera fpsCamera;     
    public Transform firePoint;
    public Animator controller_gun_glock; // 애니메이터 변수명 변경
    public GameObject scopeOverlay;

    // 소리 설정
    [Header("Sound Settings")]
    public AudioSource audioSource; 
    public AudioClip fireSound;     
    public AudioClip reloadSound;   

    void Start()
    {
        if(fpsCamera != null) defaultFOV = fpsCamera.fieldOfView;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // ==================================================================
        // 스코프 (조준) 로직
        // ==================================================================
        if (Input.GetKey(KeyCode.Alpha2)) 
        {
            isSniperMode = true;
            fpsCamera.fieldOfView = Mathf.Lerp(fpsCamera.fieldOfView, scopeFOV, Time.deltaTime * zoomSpeed);
        }
        else
        {
            isSniperMode = false;
            fpsCamera.fieldOfView = Mathf.Lerp(fpsCamera.fieldOfView, defaultFOV, Time.deltaTime * zoomSpeed);
        }
        
        if (scopeOverlay != null) scopeOverlay.SetActive(isSniperMode);

        // ==================================================================
        // 발사 로직 (단발 - GetKeyDown 사용)
        // ==================================================================
        // ⭐ 차이점: GetKey -> GetKeyDown (누른 순간 한 번만 실행)
        if (Input.GetKeyDown(KeyCode.A) && Time.time >= nextFireTime && currentAmmo > 0) 
        {
            Shoot();
        }

        // ==================================================================
        // 재장전 로직
        // ==================================================================
        if (Input.GetKeyDown(KeyCode.R)) 
        {
            controller_gun_glock.SetTrigger("OnReload");
            
            // 장전 소리 재생 (PlayOneShot은 소리가 겹쳐도 자연스러움)
            if(reloadSound != null) audioSource.PlayOneShot(reloadSound);

            currentAmmo = maxAmmo;
        }
        
        if (Input.GetKeyDown(KeyCode.C)) controller_gun_glock.SetTrigger("OnHiding");
        if (Input.GetKeyDown(KeyCode.D)) controller_gun_glock.SetTrigger("OnDraw");
    }

    void Shoot()
    {
        // 다음 발사 가능 시간 설정
        nextFireTime = Time.time + fireRate;

        // ⭐ 소리 재생 (단발이므로 PlayOneShot 사용 - 루프 필요 없음)
        if(fireSound != null) audioSource.PlayOneShot(fireSound);

        // 애니메이션 실행
        controller_gun_glock.SetTrigger("OnShoot");
        
        // 총알 감소
        currentAmmo--;

        // 총격 판정 (Raycast)
        RaycastHit hit; 
        if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, range))
        {
            ShootZombie target = hit.transform.GetComponent<ShootZombie>();
            if (target != null) target.TakeDamage(damage);
        }
    }
}