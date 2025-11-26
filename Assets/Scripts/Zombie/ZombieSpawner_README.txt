
사용 방법 및 설정
1. 준비 작업
좀비 프리팹: 씬에 있는 zombie_Random 오브젝트를 **프리팹(Prefab)**으로 만드세요.

스포너 오브젝트 생성: Hierarchy 창에서 빈 GameObject를 생성하고 이름을 **ZombieSpawner**로 변경합니다. (이 오브젝트는 좀비 인스턴스에 넣으면 안 됩니다.)

스크립트 추가: ZombieSpawner.cs 스크립트를 새로 만든 ZombieSpawner 오브젝트에 추가합니다.

2. Inspector 설정
ZombieSpawner 오브젝트를 선택하고 Inspector 창에서 다음 변수들을 설정하세요.

Zombie Prefab: 프로젝트 창에서 만든 zombie_Random 프리팹을 여기에 드래그하여 연결합니다.

Total Zombies To Spawn: 원하는 좀비 개수인 **30**으로 설정합니다.

Spawn Radius: 좀비가 생성될 반경을 설정합니다. 맵 크기에 맞게 적절히 조절하세요 (예: 50).

위치 조정: ZombieSpawner 오브젝트의 Transform 위치를 좀비를 생성할 맵의 중심 근처로 이동시킵니다.

3. 실행 환경 조건
씬에는 좀비가 이동할 수 있도록 NavMesh Surface가 **Bake(베이크)**되어 있어야 합니다.