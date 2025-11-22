# Scripts 폴더 명세서

이 폴더에는 게임의 핵심 스크립트들이 포함되어 있습니다.

## 📁 파일 목록

### 1. `ZombieAI.cs`
**좀비 AI 시스템** - 거리 기반 상태 머신(FSM)을 사용한 좀비 행동 제어

#### 주요 기능
- **거리 기반 상태 전환**
  - `distance > 15m` → **Idle** (가만히 있음)
  - `5m < distance ≤ 15m` → **Walk** (천천히 따라옴)
  - `distance ≤ 5m` → **Run** (빠르게 추적)
  - `distance ≤ 1.5m` → **Attack** (공격)
- **랜덤성 시스템** (선택적)
  - Player가 멈춰있을 때 일정 확률로 멈춤
  - 추적 중 갑작스러운 속도 증가
- **애니메이션 연동**
  - Walk, Run, Attack, Search, Stumble 상태 지원

#### 적용 방법
1. `walking_zombie` prefab (또는 좀비 오브젝트) 선택
2. Inspector에서 `Add Component` → `Zombie AI` 추가
3. **필수 설정:**
   - `Player Target`: Player 오브젝트 할당 (자동 탐지 가능)
   - `Nav Agent`: NavMeshAgent 컴포넌트 할당 (자동 탐지 가능)
   - `Animator`: Animator 컴포넌트 할당 (자동 탐지 가능)
4. **선택 설정:**
   - 거리 값 조정 (Idle/Walk/Run/Attack Distance)
   - 속도 조정 (Walk Speed, Run Speed)
   - `Enable Randomness` 체크박스로 랜덤성 활성화

#### 주의사항
- **NavMesh가 베이크되어 있어야 함**
- Animator Controller에 다음 파라미터 필요:
  - `Speed` (Float)
  - `IsWalking` (Bool)
  - `IsRunning` (Bool)
  - `IsAttacking` (Bool)
  - `IsSearching` (Bool)
  - `IsStumbling` (Bool)

---

### 2. `PlayerController.cs`
**플레이어 이동 컨트롤러** - WASD 키보드 입력으로 플레이어 이동 제어

#### 주요 기능
- **WASD 이동**: 전후좌우 이동
- **Shift 달리기**: Left Shift 키로 달리기 속도 증가
- **Space 점프**: Space 키로 점프
- **카메라 기준 이동**: 카메라가 바라보는 방향 기준으로 이동

#### 적용 방법
1. Player 오브젝트 선택
2. Inspector에서 `Add Component` → `Player Controller` 추가
3. **설정 (선택적):**
   - `Move Speed`: 기본 걷기 속도 (기본값: 5)
   - `Run Speed`: 달리기 속도 (기본값: 8)
   - `Jump Force`: 점프 힘 (기본값: 5)
4. CharacterController는 자동으로 추가/탐지됨

#### 사용 키
- **W, A, S, D**: 이동
- **Left Shift**: 달리기
- **Space**: 점프

---

### 3. `CameraFollow.cs`
**카메라 추적 시스템** - 1인칭 시점 카메라가 플레이어를 따라다니며 마우스로 회전

#### 주요 기능
- **1인칭 시점**: 플레이어의 눈 높이에서 카메라 위치
- **플레이어 추적**: 카메라가 플레이어 위치를 따라다님
- **마우스 회전**: 마우스로 시점 회전 (좌우/상하)
- **부드러운 추적**: Lerp를 사용한 부드러운 카메라 이동
- **카메라 충돌 처리**: 벽이나 장애물에 카메라가 뚫지 않도록 처리
- **회전 제한**: 상하 회전 각도 제한 (-80° ~ 80°)

#### 적용 방법
1. **CameraRoot 생성**: Player의 자식으로 빈 오브젝트 생성, 이름을 "CameraRoot"로 설정
2. **Main Camera 설정**: Main Camera를 CameraRoot의 자식으로 설정 (Local Position: 0, 0, 0)
3. **CameraFollow 추가**: CameraRoot 오브젝트에 `Add Component` → `Camera Follow` 추가
4. **자동 설정**:
   - `Target`: Player 오브젝트 (자동 탐지)
   - `Camera Root`: CameraRoot 오브젝트 (자동 탐지)
5. **설정 조정 (선택적):**
   - `Eye Height`: 눈 높이 (기본값: 1.6m)
   - `Mouse Sensitivity`: 마우스 감도 (기본값: 2)
   - `Smooth Follow`: 부드러운 추적 활성화 여부 (기본값: true)

#### 카메라 구조
```
Player
  └ CameraRoot (CameraFollow 스크립트가 여기에 붙음)
      └ Main Camera
```

#### 주의사항
- CameraRoot가 Player의 자식이어야 함
- Main Camera가 CameraRoot의 자식이어야 함
- Play 모드에서 커서가 자동으로 잠김

자세한 내용은 `CameraFollow_README.md` 참고

---

### 4. `HeadBob.cs`
**헤드밥 시스템** - 걷기와 뛰기 시 카메라에 자연스러운 흔들림 효과 추가

#### 주요 기능
- **걷기 흔들림**: 걷을 때 부드러운 카메라 흔들림
- **뛰기 흔들림**: 뛸 때 더 강한 카메라 흔들림
- **자동 감지**: CharacterController와 CameraRoot 자동 탐지
- **부드러운 복귀**: 멈추면 원래 위치로 부드럽게 복귀

#### 적용 방법
1. Player 오브젝트 선택
2. Inspector에서 `Add Component` → `Head Bob` 추가
3. **자동 설정** (수동 설정 불필요):
   - `Controller`: CharacterController (자동 탐지)
   - `Camera Holder`: CameraRoot (자동 탐지)
4. **설정 조정 (선택적):**
   - `Walk Bob Speed`: 걷기 흔들림 속도 (기본값: 14)
   - `Walk Bob Amount`: 걷기 흔들림 강도 (기본값: 0.05)
   - `Run Bob Speed`: 뛰기 흔들림 속도 (기본값: 18)
   - `Run Bob Amount`: 뛰기 흔들림 강도 (기본값: 0.1)

#### 권장 설정값
| 구분 | Bob Speed | Bob Amount |
|------|-----------|------------|
| **걷기** | 12 ~ 16 | 0.04 ~ 0.07 |
| **뛰기** | 18 ~ 22 | 0.08 ~ 0.12 |

자세한 내용은 `HeadBob_README.md` 참고

---

### 5. `GunAction.cs`
**총 애니메이션 제어** - 키 입력으로 총 애니메이션 트리거

#### 주요 기능
- **총 발사 애니메이션**: A 키로 발사 애니메이션 제어
- **재장전 애니메이션**: B 키로 재장전 트리거
- **은폐 애니메이션**: C 키로 은폐 모션 제어
- **무기 꺼내기**: D 키로 무기 꺼내기 애니메이션
- **발사 트리거**: Q 키로 발사 트리거

#### 적용 방법
1. 총 오브젝트 (또는 Gun_Kriss prefab) 선택
2. Inspector에서 `Add Component` → `Gun Action` 추가
3. **필수 설정:**
   - `Controller Gun Kriss`: Animator 컴포넌트 할당

#### 사용 키
- **A**: 발사 (누르고 있으면 계속 발사)
- **B**: 재장전
- **C**: 은폐 (누르고 있으면 계속 은폐)
- **D**: 무기 꺼내기
- **Q**: 발사 트리거

#### Animator 파라미터 필요
- `IsShooting` (Bool)
- `OnReload` (Trigger)
- `IsHiding` (Bool)
- `OnDraw` (Trigger)
- `OnFire` (Trigger)

---

## 🔧 공통 설정 사항

### Player 오브젝트 설정
1. Player 오브젝트에 `PlayerController` 스크립트 추가
2. Player 오브젝트에 `CharacterController` 컴포넌트 필요 (자동 추가됨)
3. Player 오브젝트에 Tag를 "Player"로 설정 (선택적, 자동 탐지 가능)

### 카메라 설정
1. **CameraRoot 생성**
   - Player의 자식으로 빈 오브젝트 생성
   - 이름을 "CameraRoot"로 설정
   - Transform Position: (0, 1.6, 0) (눈 높이)
2. **Main Camera 설정**
   - Main Camera를 CameraRoot의 자식으로 설정
   - Local Position: (0, 0, 0)
3. **CameraFollow 추가**
   - CameraRoot에 `CameraFollow` 스크립트 추가
   - Target과 Camera Root는 자동 탐지됨
4. **HeadBob 추가** (선택사항)
   - Player에 `HeadBob` 스크립트 추가
   - Controller와 Camera Holder는 자동 탐지됨

### 좀비 설정
1. 좀비 오브젝트에 `NavMeshAgent` 컴포넌트 추가
2. 좀비 오브젝트에 `ZombieAI` 스크립트 추가
3. Scene에 NavMesh 베이크 필요

---

## 📝 개발 단계별 사용 가이드

### 0단계: 기본 설정
- `PlayerController` + `CameraFollow` + `HeadBob`로 기본 이동/시점/헤드밥 구현
- CameraRoot 구조 설정 필요

### 1단계: 좀비 AI 기본
- `ZombieAI` 스크립트 추가
- 거리 기반 상태 전환 테스트

### 2단계: 랜덤성 추가
- `ZombieAI`의 `Enable Randomness` 체크박스 활성화
- 랜덤성 파라미터 조정

### 3단계: 애니메이션 연동
- Animator Controller에 필요한 파라미터 추가
- 상태별 애니메이션 연결

---

## ⚠️ 문제 해결

### 좀비가 반응하지 않을 때
1. Console 창에서 "ZombieAI: Player를 찾았습니다" 메시지 확인
2. NavMesh가 베이크되었는지 확인
3. Inspector에서 `Player Target` 직접 할당

### 카메라가 움직이지 않을 때
1. CameraRoot에 `CameraFollow` 스크립트가 있는지 확인
2. `Target` 필드에 Player가 할당되었는지 확인
3. Main Camera가 CameraRoot의 자식인지 확인
4. Play 모드에서 마우스 움직임 확인

### 카메라가 3인칭처럼 보일 때
1. Main Camera의 Local Position이 (0, 0, 0)인지 확인
2. Main Camera가 CameraRoot의 자식인지 확인
3. CameraRoot의 Local Position Y 값이 눈 높이인지 확인 (기본: 1.6)

### 헤드밥이 작동하지 않을 때
1. Player에 `HeadBob` 스크립트가 있는지 확인
2. Controller 필드에 CharacterController가 할당되었는지 확인 (자동 탐지)
3. Camera Holder 필드에 CameraRoot가 할당되었는지 확인 (자동 탐지)

### 플레이어가 움직이지 않을 때
1. `CharacterController` 컴포넌트가 있는지 확인
2. 지면에 Collider가 있는지 확인

