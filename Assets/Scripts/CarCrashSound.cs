using UnityEngine;

public class CarCrashSound : MonoBehaviour
{
    // 인스펙터 창에서 충돌음 파일을 할당할 변수
    public AudioClip crashSound;

    // 오디오 소스 컴포넌트를 참조할 변수
    private AudioSource audioSource;

    void Start()
    {
        // 오브젝트에 부착된 AudioSource 컴포넌트를 가져옵니다.
        audioSource = GetComponent<AudioSource>();
    }

    // 다른 Collider와 충돌이 시작될 때 호출되는 유니티 내장 함수 (3D용)
    void OnCollisionEnter(Collision collision)
    {
        // AudioSource 컴포넌트가 존재하고 충돌음 클립이 할당되었는지 확인
        if (audioSource != null && crashSound != null)
        {
            // PlayOneShot을 사용하여 현재 재생 중인 다른 오디오에 영향을 주지 않고 클립을 한 번 재생합니다.
            audioSource.PlayOneShot(crashSound);

            // 필요하다면 충돌한 오브젝트의 이름 등을 콘솔에 출력하여 디버깅할 수 있습니다.
            // Debug.Log("충돌 발생! 충돌 상대: " + collision.gameObject.name);
        }
    }

    // 2D 환경에서는 void OnCollisionEnter2D(Collision2D collision) 함수를 사용해야 합니다.
}