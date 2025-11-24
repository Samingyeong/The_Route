using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Sway Settings")]
    public float amount = 0.02f;   // 흔들리는 정도 (클수록 많이 흔들림)
    public float maxAmount = 0.06f; // 최대 흔들림 제한 (너무 돌아가지 않게)
    public float smoothAmount = 6f; // 복귀 속도 (클수록 빨리 제자리로 옴)

    [Header("Tilt Settings (기울기)")]
    public float tiltAmount = 2f; // 회전할 때 총이 Z축으로 살짝 기우는 정도

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Start()
    {
        // 시작할 때의 총 위치와 회전값을 기억해둡니다.
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        // 1. 마우스 움직임 입력 받기
        float movementX = -Input.GetAxis("Mouse X") * amount;
        float movementY = -Input.GetAxis("Mouse Y") * amount;
        
        // 흔들림 범위 제한 (Clamp)
        movementX = Mathf.Clamp(movementX, -maxAmount, maxAmount);
        movementY = Mathf.Clamp(movementY, -maxAmount, maxAmount);

        // 2. 위치 스웨이 (Position Sway)
        // 마우스 반대 방향으로 총 위치를 살짝 이동
        Vector3 finalPosition = new Vector3(movementX, movementY, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + initialPosition, Time.deltaTime * smoothAmount);


        // 3. 회전 스웨이 (Rotation Tilt) - 선택사항
        // 좌우로 돌릴 때 총을 살짝 기울여줌 (Z축 회전)
        float tiltX = -Input.GetAxis("Mouse X") * tiltAmount;
        float tiltY = -Input.GetAxis("Mouse Y") * tiltAmount; // 위아래 틸트는 취향에 따라 제거 가능

        Quaternion targetRotation = Quaternion.Euler(initialRotation.eulerAngles.x + tiltY, initialRotation.eulerAngles.y + tiltX, initialRotation.eulerAngles.z - tiltX);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smoothAmount);
    }
}