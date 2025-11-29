using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
    [Header("반동 물리 설정")]
    public float snappiness = 6f;  // 반동 반응 속도
    public float returnSpeed = 2f; // 제자리 복귀 속도

    [Header("회전 반동 설정 (Rotation)")]
    public Vector3 recoilRotation = new Vector3(10f, 2f, 2f); // 강도
    public float maxRecoilRotationX = 15f; // [핵심] 총이 위로 들리는 최대 각도 제한

    [Header("위치 반동 설정 (Kickback - 뒤로 밀림)")]
    public Vector3 recoilKickBack = new Vector3(0.01f, 0f, -0.05f); // 뒤로 밀리는 힘 (Z값 음수 중요)
    public float maxKickBackZ = -0.1f; // 뒤로 밀리는 최대 거리

    // 내부 계산용 변수
    private Vector3 currentRotation;
    private Vector3 targetRotation;
    
    private Vector3 currentPosition;
    private Vector3 targetPosition;
    private Vector3 initialPosition; // 총의 원래 위치 저장

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        // 1. 복귀 (Lerp) - 0으로 돌아옴
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        targetPosition = Vector3.Lerp(targetPosition, Vector3.zero, returnSpeed * Time.deltaTime);

        // 2. 적용 (Slerp) - 부드럽게 이동
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.deltaTime);
        currentPosition = Vector3.Lerp(currentPosition, targetPosition, snappiness * Time.deltaTime);

        // 3. 실제 트랜스폼 반영
        transform.localRotation = Quaternion.Euler(currentRotation);
        transform.localPosition = initialPosition + currentPosition; // 원래 위치 기준에서 더함
    }

    public void RecoilFire()
    {
        // A. 회전 반동 (Rotation)
        // 위로 들리는 힘(-x)과 랜덤한 좌우/기울기
        targetRotation += new Vector3(
            -recoilRotation.x, 
            Random.Range(-recoilRotation.y, recoilRotation.y), 
            Random.Range(-recoilRotation.z, recoilRotation.z)
        );

        // [핵심 해결] 회전 각도 제한 (Clamp)
        // 총이 너무 위로 들리지 않게 막음 (-max ~ 0)
        targetRotation.x = Mathf.Clamp(targetRotation.x, -maxRecoilRotationX, 0f);

        // B. 위치 반동 (Position Kickback)
        // 총을 뒤로(Z축 음수) 밀어줌 -> 팔이 들리는 대신 어깨로 밀리는 느낌
        targetPosition += new Vector3(
            Random.Range(-recoilKickBack.x, recoilKickBack.x),
            Random.Range(-recoilKickBack.y, recoilKickBack.y),
            recoilKickBack.z // 보통 음수 (뒤로)
        );
        
        // 위치 반동 제한
        // 앞으로 튀어나오지 않게 하고(Min), 너무 뒤로 가지 않게(Max) 제한
        // Z축이 뒤로 가는게 음수라면: maxKickBackZ(-0.1) ~ 0 사이
        targetPosition.z = Mathf.Clamp(targetPosition.z, maxKickBackZ, 0f);
    }
}