using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
    [Header("Recoil Settings")]
    // 반동의 강도 (X: 위로 들리는 힘, Y/Z: 좌우/회전 흔들림)
    public Vector3 recoilParams = new Vector3(10f, 2f, 2f); 
    
    // 반동이 적용되는 속도 (클수록 빠릿함)
    public float snappiness = 6f;
    
    // 원래 위치로 돌아오는 속도 (클수록 빨리 제자리로 옴)
    public float returnSpeed = 2f;

    private Vector3 currentRotation;
    private Vector3 targetRotation;

    void Update()
    {
        // 1. 목표 회전값(targetRotation)을 0(원점)으로 서서히 복귀시킴
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);

        // 2. 현재 회전값(currentRotation)을 목표 회전값으로 부드럽게 이동 (Slerp)
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.deltaTime);

        // 3. 실제 트랜스폼에 적용
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    // 총을 쏠 때 외부(GunAction)에서 호출할 함수
    public void RecoilFire()
    {
        // 랜덤성을 섞어서 반동 생성
        float x = recoilParams.x; // 위로 들리는 힘은 항상 양수
        float y = Random.Range(-recoilParams.y, recoilParams.y); // 좌우는 랜덤
        float z = Random.Range(-recoilParams.z, recoilParams.z); // 기울기도 랜덤

        // 목표 회전값에 반동 추가 (-x인 이유는 유니티 각도상 뒤로 젖혀지는게 -값인 경우가 많음, 반대면 +로 변경)
        targetRotation += new Vector3(-x, y, z);
    }
}