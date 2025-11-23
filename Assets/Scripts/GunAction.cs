using UnityEngine;

public class GunAction : MonoBehaviour
{
    // 총 설정
    public int maxAmmo = 25; 
    public int currentAmmo = 0; 
    public float fireRate = 0.7f;
    private float nextFireTime = 0f;
    public int damage = 20;      
    public float range = 100f;   

    // 스코프 모드 설정
    public bool isSniperMode = false; 
    public float defaultFOV = 60f;    
    public float scopeFOV = 20f;      
    public float zoomSpeed = 10f;     

    // 컴포넌트 연결
    public Camera fpsCamera;     
    public Transform firePoint;
    public Animator controller_gun_kriss;
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
        // 스코프 로직
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
        // 소리 반복 재생 로직
        // ==================================================================
        
        bool isTryingToShoot = Input.GetKey(KeyCode.A) && currentAmmo > 0;

        if (isTryingToShoot)
        {
            // 발사 소리 세팅 및 재생
            if (!audioSource.isPlaying || audioSource.clip != fireSound)
            {
                audioSource.clip = fireSound; 
                audioSource.loop = true;      
                audioSource.Play();           
            }

            // 실제 발사 로직
            if (Time.time >= nextFireTime) Shoot();
        }
        else
        {
            // 현재 재생 중이고, 그 소리가 '총 소리'라면 멈춥니다.
            if (audioSource.isPlaying && audioSource.clip == fireSound)
            {
                audioSource.loop = false; 
                audioSource.Stop();       
                
                audioSource.clip = null; 
            }
        }

        // 재장전
        if (Input.GetKeyDown(KeyCode.R)) {
            controller_gun_kriss.SetTrigger("OnReload");
            
            // ⭐ 장전 소리 재생
            if(reloadSound != null) 
            {
                // 혹시 총쏘던 중이면 확실히 끄고
                if(audioSource.clip == fireSound)
                {
                    audioSource.Stop();
                    audioSource.loop = false;
                    audioSource.clip = null; // 여기서도 클립 초기화
                }
                
                // 장전 소리 재생 (PlayOneShot은 clip 변수를 바꾸지 않음)
                audioSource.PlayOneShot(reloadSound);
            }

            currentAmmo = maxAmmo;
        }
        
        if (Input.GetKeyDown(KeyCode.C)) controller_gun_kriss.SetTrigger("OnHiding");
        if (Input.GetKeyDown(KeyCode.D)) controller_gun_kriss.SetTrigger("OnDraw");
    }

    void Shoot(){
        
        // 총 쏨
        nextFireTime = Time.time + fireRate;
        controller_gun_kriss.SetTrigger("OnShoot");
        currentAmmo --;

        // 총격 판정
        RaycastHit hit; 
        if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, range))
        {
            ShootZombie target = hit.transform.GetComponent<ShootZombie>();
            if (target != null) target.TakeDamage(damage);
        }
    }
}