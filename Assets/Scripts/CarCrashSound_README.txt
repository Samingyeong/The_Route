1. 스크립트의 기능 및 작동 원리
이 스크립트의 작동은 세 단계로 이루어집니다. 첫째, 스크립트가 시작될 때 Start() 함수를 통해 차량 오브젝트에 부착된 AudioSource 컴포넌트를 미리 가져와 변수(audioSource)에 저장합니다. 둘째, 유니티의 물리 시스템 내장 함수인 **OnCollisionEnter(Collision collision)**를 이용하여 차량이 다른 3D 오브젝트와 접촉(충돌)하는 순간을 감지합니다. 이 감지 함수가 정상적으로 호출되려면 오브젝트에 Rigidbody 컴포넌트와 Collider가 모두 있어야 합니다. 셋째, 충돌이 감지되면, 스크립트는 AudioSource 컴포넌트가 존재하고 충돌음 클립이 할당되어 있는지 확인한 후, AudioSource.PlayOneShot(crashSound) 함수를 호출하여 오디오 클립을 재생합니다. PlayOneShot을 사용하면 엔진 소리 등 다른 소리의 재생을 중단시키지 않고 충돌음만 독립적으로 한 번 출력할 수 있어 자연스러운 사운드 환경을 유지할 수 있습니다.

2. 사용을 위한 필수 설정
이 스크립트를 차량에 적용하고 정상 작동시키기 위해서는 다음 세 가지 필수 요구 사항을 충족해야 합니다.

Rigidbody 컴포넌트: 충돌 감지(OnCollisionEnter) 기능을 활성화하기 위해 차량 오브젝트에 Rigidbody 컴포넌트가 반드시 필요합니다.

AudioSource 컴포넌트: 소리를 출력하는 '스피커' 역할을 담당하며 차량 오브젝트에 부착되어야 합니다. 이때, 충돌 시에만 소리를 내기 위해 Play On Awake와 Loop 옵션은 반드시 체크를 해제해야 합니다.

인스펙터 할당: 스크립트를 차량에 부착한 후, 인스펙터 창의 Crash Sound 필드에 사용할 충돌음 오디오 파일(AudioClip)을 드래그하여 할당해야 합니다.

3. 디버깅 및 주의 사항
만약 차량이 충돌했는데도 소리가 나지 않는다면, 차량과 충돌 대상 오브젝트 모두에 Collider가 있는지, 그리고 차량 오브젝트에 Rigidbody가 부착되어 있는지 확인해야 합니다. 또한, 콜라이더의 Is Trigger 옵션이 체크되어 있으면 OnCollisionEnter가 아닌 OnTriggerEnter가 호출되므로, 충돌음을 재생하려면 Is Trigger가 체크 해제되어 있어야 합니다. 이 스크립트는 3D 충돌 함수(OnCollisionEnter)를 사용하므로, 2D 프로젝트에서는 void OnCollisionEnter2D(Collision2D collision) 함수를 사용하도록 스크립트를 수정해야 합니다.