using UnityEngine;

public class GunAction : MonoBehaviour
{
    [Header("무기 목록")]
    public Gun[] allGuns;       
    private Gun currentGun;     

    [Header("컴포넌트 연결")]
    public Camera fpsCamera;
    
    [Header("Recoil System")]
    public WeaponRecoil weaponRecoil;
    public CameraRecoil cameraRecoil;

    [Header("Sound")]
    public AudioSource audioSource;

    [Header("UI & Scope")]
    public GameObject scopeOverlay;
    public float defaultFOV = 60f;
    public float scopeFOV = 30f;
    public bool isSniperMode = false;

    private float nextFireTime = 0f;

    void Start()
    {
        if (fpsCamera != null) defaultFOV = fpsCamera.fieldOfView;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        
        // 초기화
        foreach(var gun in allGuns) gun.currentAmmo = gun.maxAmmo;
        SwapWeapon(0);
    }

    void Update()
    {
        if (currentGun == null) return;

        // 1. 무기 교체
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwapWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwapWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwapWeapon(2);

        // 2. 조준
        if (Input.GetMouseButton(1)) 
        {
            isSniperMode = true;
            fpsCamera.fieldOfView = Mathf.Lerp(fpsCamera.fieldOfView, scopeFOV, Time.deltaTime * 10f);
        }
        else
        {
            isSniperMode = false;
            fpsCamera.fieldOfView = Mathf.Lerp(fpsCamera.fieldOfView, defaultFOV, Time.deltaTime * 10f);
        }
        if (scopeOverlay != null) scopeOverlay.SetActive(isSniperMode);

        // =========================================================
        // 3. 발사 입력 감지 및 사운드 처리 (여기가 핵심 수정!)
        // =========================================================
        bool triggerPulled = false;

        if (currentGun.isAutomatic)
        {
            // [연사 모드: Kriss]
            // 마우스를 누르고 있고 && 총알이 있을 때
            bool isFiring = Input.GetMouseButton(0) && currentGun.currentAmmo > 0;
            triggerPulled = isFiring;

            // --- 연사 사운드 로직 (Loop & Stop) ---
            if (isFiring)
            {
                // 소리가 안 나고 있거나, 다른 소리가 나고 있다면 -> 연사 소리 재생 시작
                if (!audioSource.isPlaying || audioSource.clip != currentGun.fireSound)
                {
                    audioSource.clip = currentGun.fireSound;
                    audioSource.loop = true; // 반복 재생 켜기
                    audioSource.Play();
                }
            }
            else
            {
                // 쏘고 있지 않은데, 지금 울리는 소리가 '총소리'라면 -> 뚝 끊기
                if (audioSource.isPlaying && audioSource.clip == currentGun.fireSound)
                {
                    audioSource.Stop();
                    audioSource.loop = false; // 반복 끄기
                    audioSource.clip = null;
                }
            }
        }
        else
        {
            // [단발 모드: Glock, Mark]
            // 누르는 순간 한 번만 true
            triggerPulled = Input.GetMouseButtonDown(0);
        }

        // 4. 실제 발사 (총알 감소, 반동, 데미지)
        if (triggerPulled && Time.time >= nextFireTime)
        {
            if (currentGun.currentAmmo > 0)
            {
                Shoot();
            }
        }

        // 5. 재장전
        if (Input.GetKeyDown(KeyCode.R))
        {
            // 연사 중에 재장전하면 총소리 끊어줘야 함
            if(audioSource.clip == currentGun.fireSound)
            {
                audioSource.Stop();
                audioSource.loop = false;
                audioSource.clip = null;
            }

            if(currentGun.gunAnimator != null) currentGun.gunAnimator.SetTrigger("OnReload");
            
            // 재장전 소리는 PlayOneShot으로 (끊기지 않게)
            if (currentGun.reloadSound != null) audioSource.PlayOneShot(currentGun.reloadSound);
            
            currentGun.Reload();
        }
        
        if (Input.GetKeyDown(KeyCode.C) && currentGun.gunAnimator != null) 
            currentGun.gunAnimator.SetTrigger("OnHiding");
    }

    void SwapWeapon(int index)
    {
        if (index < 0 || index >= allGuns.Length) return;
        if (currentGun == allGuns[index]) return;

        // 무기 바꿀 때 기존 총소리가 나고 있다면 끄기
        if (audioSource.isPlaying && currentGun != null && audioSource.clip == currentGun.fireSound)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
        }

        for (int i = 0; i < allGuns.Length; i++)
        {
            allGuns[i].gameObject.SetActive(false);
        }

        allGuns[index].gameObject.SetActive(true);
        currentGun = allGuns[index];
    }

    void Shoot()
    {
        nextFireTime = Time.time + currentGun.fireRate;
        currentGun.currentAmmo--;

        if (currentGun.gunAnimator != null) currentGun.gunAnimator.SetTrigger("OnShoot");

        // 사운드
        if (!currentGun.isAutomatic)
        {
            if (currentGun.fireSound != null) audioSource.PlayOneShot(currentGun.fireSound);
        }

        // 손 반동
        if (weaponRecoil != null) weaponRecoil.RecoilFire();

        // [카메라 반동 적용]
        if (cameraRecoil != null)
        {
            // 1. 반동 각도 랜덤 계산 (기존 동일)
            Vector3 randomRecoil = new Vector3(
                Random.Range(currentGun.minRecoilRotation.x, currentGun.maxRecoilRotation.x),
                Random.Range(currentGun.minRecoilRotation.y, currentGun.maxRecoilRotation.y),
                Random.Range(currentGun.minRecoilRotation.z, currentGun.maxRecoilRotation.z)
            );

            Vector3 randomAimRecoil = new Vector3(
                Random.Range(currentGun.minAimRecoilRotation.x, currentGun.maxAimRecoilRotation.x),
                Random.Range(currentGun.minAimRecoilRotation.y, currentGun.maxAimRecoilRotation.y),
                Random.Range(currentGun.minAimRecoilRotation.z, currentGun.maxAimRecoilRotation.z)
            );

            // 2. 값 적용 (각도)
            cameraRecoil.RecoilRotation = randomRecoil;
            cameraRecoil.AimRecoilRotation = randomAimRecoil;

            // =========================================================
            // [핵심 추가] 총의 속도(물리) 설정을 카메라에 덮어씌움!
            // =========================================================
            cameraRecoil.snappiness = currentGun.snappiness;
            cameraRecoil.returnSpeed = currentGun.returnSpeed;
            // =========================================================

            // 3. 발사 실행
            cameraRecoil.RecoilFire(isSniperMode);
        }

        // 레이캐스트 (기존 동일)
        Ray ray;
        if (isSniperMode) ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        else ray = new Ray(currentGun.firePoint.position, currentGun.firePoint.forward);

        RaycastHit hit;
        int layerMask = ~LayerMask.GetMask("Player");

        if (Physics.Raycast(ray, out hit, currentGun.range, layerMask))
        {
            ZombieBodyPart bodyPart = hit.collider.GetComponent<ZombieBodyPart>();
            if (bodyPart != null) bodyPart.OnHit(currentGun.damage);
            else
            {
                ShootZombie target = hit.transform.GetComponent<ShootZombie>();
                if (target == null) target = hit.transform.GetComponentInParent<ShootZombie>();
                if (target != null) target.TakeDamage(currentGun.damage);
            }
        }
    }
}