using UnityEngine;

public class GunAction : MonoBehaviour
{
    // 총 설정
    public int maxAmmo = 25; // 최대 총알 개수
    public int currentAmmo = 0; // 남은 총알 개수
    public float fireRate = 0.7f;// 총알 발사 시간 (총쏘는 애니메이션 시간이랑 함께 조정)
    private float nextFireTime = 0f;
    public int damage = 20;      // 총 데미지
    public float range = 100f;   // 사정거리

    // 스코프 모드 설정
    public bool isSniperMode = false; // 스코프 모드인지 확인
    public float defaultFOV = 60f;    // 평소 시야각
    public float scopeFOV = 20f;      // 줌 당겼을 때 시야각
    public float zoomSpeed = 10f;     // 줌 속도

    // 컴포넌트 연결
    public Camera fpsCamera;     // ⭐ 중요: 레이저를 쏠 기준점 (카메라)
    public Transform firePoint;
    public Animator controller_gun_kriss;
    public GameObject scopeOverlay;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // 게임 시작 시 현재 카메라의 FOV를 기본값으로 저장
        if(fpsCamera != null) defaultFOV = fpsCamera.fieldOfView;
    }
    // Update is called once per frame
    void Update()
    {

        if (Input.GetKey(KeyCode.Alpha2)) // 보통 우클릭이 Fire2입니다.
        {
            isSniperMode = true;
            // 줌 인 (FOV를 줄임)
            fpsCamera.fieldOfView = Mathf.Lerp(fpsCamera.fieldOfView, scopeFOV, Time.deltaTime * zoomSpeed);
        }
        else
        {
            isSniperMode = false;
            // 줌 아웃 (FOV를 원래대로)
            fpsCamera.fieldOfView = Mathf.Lerp(fpsCamera.fieldOfView, defaultFOV, Time.deltaTime * zoomSpeed);
        }
        if (scopeOverlay != null)
        {
            // isSniperMode가 true면 켜지고(SetActive(true)), false면 꺼집니다.
            scopeOverlay.SetActive(isSniperMode);
        }



        // 총 쏠때 
        if (Input.GetKey(KeyCode.A) && Time.time >= nextFireTime && currentAmmo > 0) Shoot();

        // 재장전
        if (Input.GetKeyDown(KeyCode.R)) {
            controller_gun_kriss.SetTrigger("OnReload");
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

        // ⭐⭐⭐ 여기서부터 레이캐스트(총격 판정) 로직 ⭐⭐⭐
        RaycastHit hit; // 레이저에 맞은 물체의 정보를 담을 변수

        // 카메라 위치에서, 카메라가 보는 앞 방향으로, range만큼 쏩니다.
        if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, range))
        {
            Debug.Log("총구에서 쏴서 맞은 것: " + hit.transform.name);

            // 좀비 찾기
            ShootZombie target = hit.transform.GetComponent<ShootZombie>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
            
            // (선택) 레이저가 어디로 나가는지 눈으로 확인하고 싶으면 아래 주석 해제
            // Debug.DrawRay(firePoint.position, firePoint.forward * range, Color.red, 1.0f);
        }
    }
}
