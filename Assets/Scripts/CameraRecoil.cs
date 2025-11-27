using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    [Header("Recoil Speed (반동 물리 설정)")]
    public float snappiness = 0.1f;  // [중요] 낮을수록 빠릿함 (0.1 ~ 0.2 추천) -> SmoothDamp 시간
    public float returnSpeed = 5f;   // 제자리로 돌아오는 속도 (크면 빨리 돌아옴)

    [Header("Recoil Recovery (반동 유지)")]
    public float recoveryDelay = 0.2f; 
    private float recoveryTimer = 0f;  

    [Header("Recoil Limits (각도 제한)")]
    public float maxTotalRecoilAngle = 25f; 

    [Header("Hipfire (일반 사격 강도)")]
    // Z축 값을 키워보세요! (기울기 효과)
    public Vector3 RecoilRotation = new Vector3(10f, 4f, 5f); 

    [Header("Aiming (조준 사격 강도)")]
    public Vector3 AimRecoilRotation = new Vector3(3f, 1f, 1.5f); 

    [Header("Accumulation (반동 누적)")]
    public float accumulationPerShot = 5f;   
    public float maxAccumulation = 30f;      
    
    private float currentAccumulation = 0f; 
    
    // SmoothDamp를 위한 물리 변수들
    private Vector3 currentRotation; 
    private Vector3 targetRotation;  
    private Vector3 rotationVelocity; // 가속도(관성) 저장용 변수
    
    private bool isAiming = false;

    void Update()
    {
        // 1. 회복 타이머 체크
        if (recoveryTimer > 0)
        {
            recoveryTimer -= Time.deltaTime;
        }
        else
        {
            // 2. 타겟값 복귀 (Lerp: 타겟은 천천히 줄어듦)
            targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
            
            // 누적치도 천천히 해소
            if (currentAccumulation > 0)
            {
                currentAccumulation -= Time.deltaTime * returnSpeed; 
            }
        }
        
        // =========================================================
        // [핵심 변경] Slerp -> SmoothDamp (관성 적용)
        // =========================================================
        // 목표 지점까지 스프링처럼 부드럽게 따라갑니다. 
        // snappiness가 작을수록 스프링이 강해서 팍! 튀고, 클수록 물속에 있는 듯 부드럽습니다.
        currentRotation = Vector3.SmoothDamp(currentRotation, targetRotation, ref rotationVelocity, snappiness);
        
        // 4. 적용
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void RecoilFire(bool isAimingState)
    {
        isAiming = isAimingState;
        recoveryTimer = recoveryDelay;

        // 건네받은 값이 있다면 그걸 쓰고, 없다면 기본값 사용 (GunAction에서 덮어씌워짐)
        Vector3 baseRecoil = isAiming ? AimRecoilRotation : RecoilRotation;
        
        // 외부(GunAction)에서 값을 안 덮어씌웠을 경우를 대비한 안전장치
        // GunAction에서 RecoilRotation을 주입하고 있다면 이 로직은 무시됩니다.

        // 누적 반동 계산
        float finalRecoilX = baseRecoil.x + currentAccumulation;
        float finalRecoilY = Random.Range(-baseRecoil.y, baseRecoil.y);
        
        // [중요] Z축(기울기) 랜덤 적용 -> 이게 자연스러움의 핵심!
        float finalRecoilZ = Random.Range(-baseRecoil.z, baseRecoil.z);

        // 타겟 회전값에 더하기 (-X가 위로 들림)
        targetRotation += new Vector3(-finalRecoilX, finalRecoilY, finalRecoilZ);

        // 각도 제한
        targetRotation.x = Mathf.Clamp(targetRotation.x, -maxTotalRecoilAngle, 0f);

        // 누적치 증가
        currentAccumulation += accumulationPerShot;
        currentAccumulation = Mathf.Clamp(currentAccumulation, 0f, maxAccumulation);
    }
}