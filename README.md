# The Route

좀비 서바이벌 FPS 게임 프로젝트

## 📋 목차

- [게임 개요](#게임-개요)
- [주요 기능](#주요-기능)
- [시작하기](#시작하기)
- [조작 방법](#조작-방법)
- [씬 구조](#씬-구조)
- [프로젝트 구조](#프로젝트-구조)
- [주요 스크립트](#주요-스크립트)
- [개발 환경](#개발-환경)

## 🎮 게임 개요

The Route는 좀비 아포칼립스 배경의 1인칭 슈팅 서바이벌 게임입니다. 플레이어는 좀비들과 맞서 싸우며 생존해야 하며, 차량을 이용한 이동, 인벤토리 시스템, 튜토리얼 등 다양한 기능을 제공합니다.

## ✨ 주요 기능

### 🎯 핵심 게임플레이
- **FPS 전투 시스템**: 다양한 무기와 반동 시스템
- **좀비 AI**: 거리 기반 상태 머신을 사용한 지능형 좀비 AI
- **차량 시스템**: 차량 탑승/하차 및 운전 기능
- **인벤토리 시스템**: 아이템 수집 및 사용
- **체력 시스템**: 데미지 및 회복 메커니즘

### 🎨 UI/UX
- **메인 메뉴**: 게임 시작 및 설정
- **튜토리얼 시스템**: 9단계 게임플레이 가이드
- **총알 개수 표시**: 실시간 총알 개수 UI
- **체력바**: 플레이어 체력 표시
- **사망 화면**: 게임 오버 및 재시작 기능

### 🎬 시네마틱
- **게임 시작 시퀀스**: 카메라 시네마틱 및 페이드 효과
- **조준 시스템**: 스나이퍼 모드 및 스코프 오버레이

## 🚀 시작하기

### 필수 요구사항
- Unity 2021.3 이상
- Windows 10 이상

### 설치 방법

1. **프로젝트 클론**
   ```bash
   git clone <repository-url>
   cd The_Route
   ```

2. **Unity 에디터에서 열기**
   - Unity Hub에서 프로젝트 추가
   - `The_Route` 폴더 선택

3. **씬 열기**
   - Unity 에디터에서 `Assets/Scenes/MainMenu.unity` 씬을 엽니다
   - Play 버튼을 눌러 게임을 시작합니다

### 빌드 설정 확인

1. `File > Build Settings` 메뉴 열기
2. 씬 순서 확인:
   - **Index 0**: `MainMenu.unity` (필수)
   - **Index 1**: `Tutorial.unity`
   - **Index 2**: `[main]demo_city_night.unity`

## 🎮 조작 방법

### 플레이어 이동
| 키 | 기능 |
|---|---|
| **W, A, S, D** | 앞/왼쪽/뒤/오른쪽 이동 |
| **Left Shift** | 달리기 (누르고 있음) |
| **Space** | 점프 |
| **마우스 이동** | 시점 회전 |

### 총기 조작
| 키 | 기능 |
|---|---|
| **마우스 왼쪽 버튼** | 발사 |
| **마우스 오른쪽 버튼** | 조준 (누르고 있음) |
| **R** | 재장전 |
| **1, 2, 3** | 무기 교체 |

### 상호작용
| 키 | 기능 |
|---|---|
| **E** | 차량 탑승/하차 |
| **F** | 아이템 픽업 |
| **I** | 인벤토리 열기/닫기 |
| **ESC** | 메뉴 (향후 추가 예정) |

### 튜토리얼
튜토리얼은 9단계로 구성되어 있으며, 각 단계를 완료하면 다음 단계로 진행됩니다:
1. 걷기/뛰기
2. 데미지 받기
3. 총 장전/쏘기
4. 붕대 픽업
5. 인벤토리 열기
6. 붕대 사용
7. 차 탑승
8. 차 움직이기
9. 차에서 내리기

## 🗺️ 씬 구조

### 씬 전환 흐름
```
MainMenu → Tutorial → [main]demo_city_night
```

1. **MainMenu**: 게임 시작 화면
   - 시작 버튼 클릭 시 Tutorial 씬으로 이동

2. **Tutorial**: 튜토리얼 씬
   - 9단계 튜토리얼 진행
   - 완료 후 Start 버튼으로 메인 게임 씬 이동

3. **[main]demo_city_night**: 메인 게임 씬
   - 실제 게임플레이 진행
   - 좀비와의 전투, 차량 운전, 아이템 수집 등

## 📁 프로젝트 구조

```
The_Route/
├── Assets/
│   ├── Scripts/              # 게임 스크립트
│   │   ├── Gun/             # 총기 관련 스크립트
│   │   ├── UI/              # UI 관련 스크립트
│   │   ├── Zombie/          # 좀비 AI 스크립트
│   │   ├── HealthBar_Scripts/  # 체력 시스템
│   │   └── Inventory_Scripts/   # 인벤토리 시스템
│   ├── Scenes/              # 게임 씬 파일
│   ├── Prefabs/             # 프리팹
│   ├── Animation/           # 애니메이션
│   ├── Sounds/              # 사운드 파일
│   └── Material/            # 머티리얼
├── ProjectSettings/         # Unity 프로젝트 설정
└── README.md               # 이 파일
```

## 🔧 주요 스크립트

### 플레이어 관련
- **`PlayerController.cs`**: 플레이어 이동 및 조작
- **`CameraFollow.cs`**: 카메라 추적 시스템
- **`HeadBob.cs`**: 걷기 시 카메라 흔들림 효과

### 총기 시스템
- **`GunAction.cs`**: 총기 발사 및 관리
- **`Gun.cs`**: 총기 속성 및 설정
- **`WeaponRecoil.cs`**: 무기 반동 시스템
- **`CameraRecoil.cs`**: 카메라 반동 시스템
- **`AmmoUI.cs`**: 총알 개수 UI 표시

### 좀비 AI
- **`ZombieAI.cs`**: 좀비 AI 상태 머신
- **`ShootZombie.cs`**: 좀비 체력 및 데미지 처리
- **`ZombieSpawner.cs`**: 좀비 스폰 시스템

### 차량 시스템
- **`VehicleController.cs`**: 차량 물리 및 조작
- **`VehicleEntryExitManager.cs`**: 차량 탑승/하차 관리

### UI 시스템
- **`MainMenuController.cs`**: 메인 메뉴 제어
- **`TutorialManager.cs`**: 튜토리얼 진행 관리
- **`TutorialUI.cs`**: 튜토리얼 UI 표시
- **`DeathScreenController.cs`**: 사망 화면 제어
- **`GameStartSequence.cs`**: 게임 시작 시네마틱

### 인벤토리 시스템
- **`DevionInventoryBridge.cs`**: 인벤토리 시스템 통합
- **`BandageQuickUse.cs`**: 붕대 사용 기능

### 체력 시스템
- **`HealthSystem.cs`**: 플레이어 체력 관리
- **`SimpleHealthBar.cs`**: 체력바 UI

## 🛠️ 개발 환경

### 사용된 에셋
- **Devion Games Inventory System**: 인벤토리 시스템
- **TextMesh Pro**: 텍스트 렌더링
- **Input System**: 입력 시스템

### 주요 패키지
- Unity Input System
- TextMesh Pro
- NavMesh (좀비 AI용)

## 📝 참고 문서

- [게임 키 조작 가이드](Organize_Key/README.md)
- [스크립트 상세 설명](Assets/Scripts/README.md)
- [총알 UI 설정 가이드](Assets/Scripts/UI/AmmoUI_설정_가이드.md)

## 🐛 알려진 문제

- Unity 에디터에서 Play 버튼을 누를 때 현재 열려있는 씬부터 시작됩니다. MainMenu 씬을 열고 Play 버튼을 눌러야 정상적으로 시작됩니다.

## 🔄 업데이트 내역

### 최근 업데이트
- 총알 개수 UI 추가 (화면 오른쪽 아래)
- 씬 전환 흐름 수정 (MainMenu → Tutorial → Main)
- obsolete API 경고 수정 (`FindObjectOfType` → `FindFirstObjectByType`)

## 📄 라이선스

이 프로젝트는 교육 목적으로 제작되었습니다.

## 👥 기여자

프로젝트 개발팀

---

**참고**: 이 프로젝트는 Unity 게임 엔진을 사용하여 개발되었습니다.
