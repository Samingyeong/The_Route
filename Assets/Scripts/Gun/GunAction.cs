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

        if (Input.GetMouseButtonDown(0) && currentGun.currentAmmo <= 0)
        {
            // 빈 격발 소리 재생
            if (currentGun.dryFireSound != null)
            {
                audioSource.PlayOneShot(currentGun.dryFireSound);
            }
            // 여기서 return을 해버리면 아래 발사 로직으로 넘어가지 않으므로
            // 애니메이션도 실행되지 않습니다.
            return; 
        }

        // B. 실제 발사 로직
        bool triggerPulled = false;

        if (currentGun.isAutomatic)
        {
            // 연사 모드: 마우스 누르고 있고 && 총알이 있어야 함
            bool isFiring = Input.GetMouseButton(0) && currentGun.currentAmmo > 0;
            triggerPulled = isFiring;

            // --- 연사 사운드 (Loop) ---
            if (isFiring)
            {
                if (!audioSource.isPlaying || audioSource.clip != currentGun.fireSound)
                {
                    audioSource.clip = currentGun.fireSound;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
            else
            {
                if (audioSource.isPlaying && audioSource.clip == currentGun.fireSound)
                {
                    audioSource.Stop();
                    audioSource.loop = false;
                    audioSource.clip = null;
                }
            }
        }
        else
        {
            // 단발 모드: 누르는 순간 && 총알이 있어야 함
            // (총알 없으면 위에서 이미 빈 격발 처리하고 return 되었음)
            triggerPulled = Input.GetMouseButtonDown(0) && currentGun.currentAmmo > 0;
        }

        // 4. 발사 실행 (Shoot 함수 호출)
        if (triggerPulled && Time.time >= nextFireTime)
        {
            Shoot();
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

            // 애니메이션 실행 (이제 소리는 여기서 안 냄!)
            if(currentGun.gunAnimator != null) currentGun.gunAnimator.SetTrigger("OnReload");
            
            // [삭제됨] 여기서 즉시 소리 재생하던 코드 삭제
            // if (currentGun.reloadSound != null) audioSource.PlayOneShot(currentGun.reloadSound);
            
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

        if (currentGun.muzzleFlashParticles != null)
        {
            // 빠른 연사 시 이전 파티클이 남아있으면 어색할 수 있으므로,
            // 강제로 멈추고 깨끗이 비운 뒤 다시 재생합니다.
            currentGun.muzzleFlashParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            currentGun.muzzleFlashParticles.Play();
        }
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

        // // 레이캐스트 (기존 동일)
        // Ray ray;
        // if (isSniperMode) ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        // else ray = new Ray(currentGun.firePoint.position, currentGun.firePoint.forward);

        // 1. 화면 정중앙(조준점)이 가리키는 실제 월드 좌표 찾기
        Ray ray = fpsCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint;

        // 화면 중앙에서 레이를 쏘아 부딪힌 곳이 목표지점
        // (사거리 제한 없이 멀리 체크하기 위해 큰 숫자 사용)
        if (Physics.Raycast(ray, out hit, 1000f)) 
        {
            targetPoint = hit.point;
        }
        else
        {
            // 허공을 보고 있다면 레이의 끝부분을 목표로 설정
            targetPoint = ray.GetPoint(1000f); 
        }

        // 2. 총구(FirePoint)에서 목표지점(TargetPoint)으로 가는 방향 계산
        Vector3 direction = (targetPoint - currentGun.firePoint.position).normalized;

        // 3. 총알 생성
        GameObject currentBullet = Instantiate(currentGun.bulletPrefab, currentGun.firePoint.position, Quaternion.identity);
        
        // 4. 총알 방향 정렬 (총알 모델이 앞을 보게)
        currentBullet.transform.forward = direction;

        // 5. 총알 스크립트에 데미지 정보 전달
        Bullet bulletScript = currentBullet.GetComponent<Bullet>();
        if(bulletScript != null)
        {
            bulletScript.damage = currentGun.damage;
        }

        // 6. 물리 힘 가하기 (발사!)
        Rigidbody bulletRb = currentBullet.GetComponent<Rigidbody>();
        if(bulletRb != null)
        {
            // ForceMode.Impulse는 순간적인 힘을 가할 때 적합
            // 탄속(muzzleVelocity)을 곱해줍니다.
            bulletRb.AddForce(direction * currentGun.muzzleVelocity, ForceMode.Impulse); 
        }
        /*
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
        */
    }
}