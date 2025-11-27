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

    [Header("Recoil System")]
    public WeaponRecoil weaponRecoil;
    public CameraRecoil cameraRecoil;

    // 소리 설정
    [Header("Sound Settings")]
    public AudioSource audioSource; 
    public AudioClip fireSound;     
    public AudioClip reloadSound;   

    void Start()
    {
        if(fpsCamera != null) defaultFOV = fpsCamera.fieldOfView;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        currentAmmo = maxAmmo;
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
        
        //bool isTryingToShoot = Input.GetKey(KeyCode.A) && currentAmmo > 0;
        bool isTryingToShoot = Input.GetMouseButton(0) && currentAmmo > 0;
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
            
            // 장전 소리 재생
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

    void Shoot()
    {
        // 1. 발사 처리
        nextFireTime = Time.time + fireRate;
        if(controller_gun_kriss) controller_gun_kriss.SetTrigger("OnShoot");
        if (weaponRecoil != null)
        {
            weaponRecoil.RecoilFire();
        }
        if (cameraRecoil != null)
        {
            cameraRecoil.RecoilFire(isSniperMode);
        }
        currentAmmo--;

        // 2. 레이(Ray) 생성
        Ray ray;
        if (isSniperMode)
        {
            // 스코프 모드: 카메라 중앙
            ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        }
        else
        {
            // 일반 모드: 총구 방향
            ray = new Ray(firePoint.position, firePoint.forward);
        }

        RaycastHit hit;
        
        // 3. 레이캐스트 발사 (핵심 수정!)
        // ~0 : 모든 레이어 충돌 허용
        // LayerMask.GetMask("Player") : "Player"라는 이름의 레이어만 가져옴
        // ~LayerMask.GetMask("Player") : "Player" 레이어만 빼고 다 충돌
        // ※ 주의: 플레이어 오브젝트의 Layer가 반드시 "Player"로 설정되어 있어야 함!
        int layerMask = ~LayerMask.GetMask("Player"); 

        if (Physics.Raycast(ray, out hit, range, layerMask))
        {
            // [디버그] 맞은 곳까지 빨간 선 그리기 (여기서 끊겨야 정상)
            Debug.DrawLine(ray.origin, hit.point, Color.red, 2.0f);
            Debug.Log("🎯 맞은 물체: " + hit.collider.name + " / 태그: " + hit.collider.tag);

            // 부위별 판정
            ZombieBodyPart bodyPart = hit.collider.GetComponent<ZombieBodyPart>();
            if (bodyPart != null)
            {
                bodyPart.OnHit(damage);
            }
            else
            {
                ShootZombie target = hit.transform.GetComponent<ShootZombie>();
                if (target == null) target = hit.transform.GetComponentInParent<ShootZombie>();
                if (target != null) target.TakeDamage(damage);
            }
        }
        else
        {
            // [디버그] 허공을 갈랐을 때 (최대 사거리까지 선 그리기)
            Debug.DrawRay(ray.origin, ray.direction * range, Color.yellow, 2.0f);
            Debug.Log("❌ 허공을 쏨 (또는 충돌체 인식 실패)");
        }
    }
}