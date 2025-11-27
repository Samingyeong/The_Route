using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    [Header("Recoil Speed (반동 속도)")]
    public float snappiness = 20f;  
    public float returnSpeed = 5f;   

    [Header("Recoil Recovery (반동 유지)")]
    public float recoveryDelay = 0.3f; 
    private float recoveryTimer = 0f;  

    [Header("Recoil Limits (각도 제한) - [추가됨]")]
    public float maxTotalRecoilAngle = 15f; // 화면이 위로 들리는 최대 각도 제한

    [Header("Hipfire (일반 사격 강도)")]
    public Vector3 RecoilRotation = new Vector3(10f, 5f, 3f); 

    [Header("Aiming (조준 사격 강도)")]
    public Vector3 AimRecoilRotation = new Vector3(3f, 1f, 1f); 

    [Header("Accumulation (반동 누적)")]
    public float accumulationPerShot = 5f;   
    public float maxAccumulation = 30f;      
    
    private float currentAccumulation = 0f; 
    private Vector3 currentRotation; 
    private Vector3 targetRotation;  
    
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
            // 2. 복귀 로직
            targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
            if (currentAccumulation > 0)
            {
                currentAccumulation -= Time.deltaTime * returnSpeed; 
            }
        }
        
        // 3. 부드러운 이동
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.deltaTime);
        
        // 4. 적용
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void RecoilFire(bool isAimingState)
    {
        isAiming = isAimingState;
        recoveryTimer = recoveryDelay;

        Vector3 baseRecoil = isAiming ? AimRecoilRotation : RecoilRotation;

        float finalRecoilX = baseRecoil.x + currentAccumulation;
        float finalRecoilY = Random.Range(-baseRecoil.y, baseRecoil.y);
        float finalRecoilZ = Random.Range(-baseRecoil.z, baseRecoil.z);

        // 반동 적용
        targetRotation += new Vector3(-finalRecoilX, finalRecoilY, finalRecoilZ);

        // =========================================================
        // [핵심 수정] 반동 각도 제한 (Clamp)
        // =========================================================
        // X축 회전이 너무 위로(-값) 솟구치지 않게 제한합니다.
        // -maxTotalRecoilAngle 보다 더 작아지지 않게(더 위로 안 가게) 막습니다.
        // 예를 들어 max가 20이면, -20도까지만 들리고 그 이상은 무시됩니다.
        targetRotation.x = Mathf.Clamp(targetRotation.x, -maxTotalRecoilAngle, 0f);

        // 다음 발사를 위해 누적치 증가
        currentAccumulation += accumulationPerShot;
        currentAccumulation = Mathf.Clamp(currentAccumulation, 0f, maxAccumulation);
    }
}