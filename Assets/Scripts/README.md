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
- **플레이어 추적**: 카메라가 플레이어 위치를 따라다님
- **마우스 회전**: 마우스로 시점 회전 (좌우/상하)
- **부드러운 추적**: Lerp를 사용한 부드러운 카메라 이동
- **회전 제한**: 상하 회전 각도 제한 (-80° ~ 80°)

#### 적용 방법
1. Main Camera 오브젝트 선택
2. Inspector에서 `Add Component` → `Camera Follow` 추가
3. **설정 (선택적):**
   - `Target`: Player 오브젝트 할당 (자동 탐지 가능)
   - `Offset`: 카메라 위치 오프셋 (기본값: Y=1.8, 머리 높이)
   - `Mouse Sensitivity`: 마우스 감도 (기본값: 2)
   - `Smooth Follow`: 부드러운 추적 활성화 여부
   - `Smooth Speed`: 추적 속도 (기본값: 10)

#### 주의사항
- 카메라가 Player의 자식이 아니어야 함 (독립적으로 동작)
- Play 모드에서 커서가 자동으로 잠김

---

### 4. `GunAction.cs`
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
1. Main Camera에 `CameraFollow` 스크립트 추가
2. Camera의 `Target` 필드에 Player 오브젝트 할당

### 좀비 설정
1. 좀비 오브젝트에 `NavMeshAgent` 컴포넌트 추가
2. 좀비 오브젝트에 `ZombieAI` 스크립트 추가
3. Scene에 NavMesh 베이크 필요

---

## 📝 개발 단계별 사용 가이드

### 0단계: 기본 설정
- `PlayerController` + `CameraFollow`로 기본 이동/시점 구현

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
1. 카메라에 `CameraFollow` 스크립트가 있는지 확인
2. `Target` 필드에 Player가 할당되었는지 확인
3. Play 모드에서 마우스 움직임 확인

### 플레이어가 움직이지 않을 때
1. `CharacterController` 컴포넌트가 있는지 확인
2. 지면에 Collider가 있는지 확인

