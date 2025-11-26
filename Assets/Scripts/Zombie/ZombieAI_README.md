# ZombieAI.cs

거리 기반 상태 머신(FSM)을 사용한 좀비 AI 시스템

## 📋 개요

플레이어와의 거리에 따라 자동으로 상태를 전환하며 추적하고 공격하는 좀비 AI입니다. NavMeshAgent를 사용하여 지형을 따라 이동하며, Animator와 연동하여 애니메이션을 제어합니다.

## 🎯 주요 기능

### 거리 기반 상태 전환
- **Idle** (15m 이상): 플레이어가 멀리 있을 때 가만히 있음
- **Walk** (5m ~ 15m): 플레이어가 가까워지면 천천히 따라옴
- **Run** (5m 이하): 플레이어가 매우 가까우면 빠르게 추적
- **Attack** (1.5m 이하): 플레이어가 바로 앞에 있으면 공격

### 랜덤성 시스템 (선택적)
- 플레이어가 멈춰있을 때 일정 확률로 멈춤
- 추적 중 갑작스러운 속도 증가
- 주변 탐색 모션

### 애니메이션 연동
- Walk, Run, Attack, Search, Stumble 상태 지원
- Animator 파라미터 자동 설정

## 🔧 적용 방법

### 1단계: 컴포넌트 추가
1. `walking_zombie` prefab (또는 좀비 오브젝트) 선택
2. Inspector에서 `Add Component` → `Zombie AI` 추가

### 2단계: 필수 설정
- **Player Target**: Player 오브젝트 할당
  - 자동 탐지: Tag "Player" 또는 이름 "Player" 또는 PlayerController 컴포넌트
- **Nav Agent**: NavMeshAgent 컴포넌트 할당 (자동 탐지 가능)
- **Animator**: Animator 컴포넌트 할당 (자동 탐지 가능)

### 3단계: NavMesh 설정
1. Scene의 바닥 오브젝트에 `Navigation Static` 체크
2. Navigation 창에서 NavMesh Bake
3. 좀비 오브젝트가 NavMesh 영역 안에 있는지 확인

## ⚙️ Inspector 파라미터

### 0단계: 기본 설정
| 파라미터 | 타입 | 설명 |
|---------|------|------|
| Player Target | Transform | 추적할 플레이어 오브젝트 |
| Nav Agent | NavMeshAgent | NavMeshAgent 컴포넌트 |
| Animator | Animator | Animator 컴포넌트 |

### 1단계: 거리 기반 상태 전환
| 파라미터 | 기본값 | 설명 |
|---------|--------|------|
| Idle Distance | 15f | 이 거리 이상이면 Idle 상태 |
| Walk Distance | 5f | 이 거리 이하면 Walk 시작 |
| Run Distance | 5f | 이 거리 이하면 Run 시작 |
| Attack Distance | 1.5f | 이 거리 이하면 Attack 상태 |

### 속도 설정
| 파라미터 | 기본값 | 설명 |
|---------|--------|------|
| Walk Speed | 2.5f | 걷기 속도 (m/s) |
| Run Speed | 4.5f | 뛰기 속도 (m/s) |

### 2단계: 랜덤성 설정
| 파라미터 | 기본값 | 설명 |
|---------|--------|------|
| Enable Randomness | false | 랜덤성 활성화 여부 |
| Pause Probability | 0.3f | 멈춤 확률 (0~1) |
| Pause Duration | 1.5f | 멈춤 지속 시간 (초) |
| Speed Boost Probability | 0.2f | 속도 증가 확률 |
| Speed Boost Duration | 0.5f | 속도 증가 지속 시간 (초) |
| Speed Boost Multiplier | 1.5f | 속도 증가 배율 |

### 3단계: 애니메이션 파라미터
| 파라미터 | 기본값 | 설명 |
|---------|--------|------|
| Anim Param Speed | "Speed" | 속도 파라미터 이름 |
| Anim Param Is Walking | "IsWalking" | 걷기 Bool 파라미터 |
| Anim Param Is Running | "IsRunning" | 뛰기 Bool 파라미터 |
| Anim Param Is Attacking | "IsAttacking" | 공격 Bool 파라미터 |
| Anim Param Is Searching | "IsSearching" | 탐색 Bool 파라미터 |
| Anim Param Is Stumbling | "IsStumbling" | 우당탕 Bool 파라미터 |

## 🎬 Animator Controller 설정

다음 파라미터들을 Animator Controller에 추가해야 합니다:

### Float 파라미터
- **Speed**: 현재 이동 속도

### Bool 파라미터
- **IsWalking**: 걷기 상태
- **IsRunning**: 뛰기 상태
- **IsAttacking**: 공격 상태
- **IsSearching**: 탐색 상태
- **IsStumbling**: 우당탕 상태

### 상태 전환 예시
```
Idle → Walk (IsWalking = true)
Walk → Run (IsRunning = true)
Run → Attack (IsAttacking = true)
```

## 🐛 문제 해결

### 좀비가 반응하지 않을 때
1. **Console 창 확인**
   - "ZombieAI: Player를 찾았습니다" 메시지 확인
   - 오류 메시지 확인

2. **Player Target 확인**
   - Inspector에서 `Player Target` 필드에 Player 오브젝트 직접 할당
   - Player 오브젝트에 Tag "Player" 설정

3. **NavMesh 확인**
   - Scene에서 바닥에 `Navigation Static` 체크
   - Navigation 창에서 NavMesh Bake 실행
   - Scene 뷰에서 파란색 NavMesh 영역 확인
   - 좀비 오브젝트가 NavMesh 영역 안에 있는지 확인

### NavMeshAgent 오류 발생 시
- "can only be called on an active agent that has been placed on a NavMesh" 오류
- **해결**: NavMesh를 베이크하고 좀비를 NavMesh 영역 안에 배치

### 애니메이션이 재생되지 않을 때
1. Animator Controller에 필요한 파라미터가 있는지 확인
2. 파라미터 이름이 Inspector 설정과 일치하는지 확인
3. 상태 전환 조건이 올바르게 설정되었는지 확인

## 📝 코드 예시

### 상태 확인
```csharp
// 현재 상태 확인
ZombieState currentState = zombieAI.currentState;
```

### 거리 확인
```csharp
// 플레이어와의 거리
float distance = Vector3.Distance(zombie.transform.position, player.transform.position);
```

## 🔄 확장 가능성

### 추가 상태
- `Stunned`: 기절 상태
- `Patrol`: 순찰 상태
- `Chase`: 추격 상태

### 추가 기능
- 시야각 기반 감지
- 소리 기반 감지
- 그룹 AI (여러 좀비 협력)

## 📚 관련 파일
- `PlayerController.cs`: 추적 대상
- `NavMeshAgent`: 이동 시스템
- Animator Controller: 애니메이션 제어

