# HeadBob.cs

걷기와 뛰기 시 카메라에 자연스러운 흔들림 효과를 추가하는 스크립트입니다.

## 📋 개요

FPS 게임에서 플레이어가 걷거나 뛸 때 카메라가 위아래로 흔들리는 효과를 구현합니다. CharacterController의 속도를 감지하여 움직임에 따라 다른 강도의 흔들림을 적용합니다. Outlast, COD, Battlefield 같은 상용 FPS 게임에서 사용하는 방식과 동일한 구조입니다.

## 🎯 주요 기능

- **걷기 흔들림**: 걷을 때 부드러운 카메라 흔들림
- **뛰기 흔들림**: 뛸 때 더 강한 카메라 흔들림
- **자동 감지**: CharacterController를 자동으로 찾아 연결
- **부드러운 복귀**: 멈추면 원래 위치로 부드럽게 복귀
- **속도별 차등**: 걷기와 뛰기에 다른 파라미터 적용

## 🔧 적용 방법

### 1단계: 스크립트 추가
1. Hierarchy에서 `player` 오브젝트 선택
2. Inspector에서 `Add Component` → `Head Bob` 추가

### 2단계: 자동 설정 (수동 설정 불필요)
- **Controller**: CharacterController 자동 탐지
- **Camera Holder**: CameraRoot 자동 탐지

### 3단계: 설정 조정 (선택적)
- **Walk Bob Speed**: 걷기 흔들림 속도 (기본값: 14)
- **Walk Bob Amount**: 걷기 흔들림 강도 (기본값: 0.05)
- **Run Bob Speed**: 뛰기 흔들림 속도 (기본값: 18)
- **Run Bob Amount**: 뛰기 흔들림 강도 (기본값: 0.1)

## ⚙️ Inspector 파라미터

| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| **Controller** | CharacterController | - | 플레이어의 CharacterController (자동 탐지) |
| **Camera Holder** | Transform | - | CameraRoot Transform (자동 탐지) |
| **Walk Bob Speed** | float | 14f | 걷기 시 흔들림 속도 |
| **Walk Bob Amount** | float | 0.05f | 걷기 시 흔들림 강도 (미터 단위) |
| **Run Bob Speed** | float | 18f | 뛰기 시 흔들림 속도 |
| **Run Bob Amount** | float | 0.1f | 뛰기 시 흔들림 강도 (미터 단위) |

## 🎮 동작 원리

### 흔들림 계산
1. CharacterController의 속도(`velocity.magnitude`) 감지
2. 속도가 0.1 이상이고 지면에 닿아있으면 흔들림 활성화
3. Left Shift 키로 걷기/뛰기 구분
4. Sin 함수를 사용하여 자연스러운 파동 생성
5. CameraRoot의 `localPosition.y`를 수정하여 흔들림 적용

### 수식
```csharp
timer += Time.deltaTime * speed;
newY = defaultYPos + Mathf.Sin(timer) * amount;
```

### 상태별 동작
- **Idle (정지)**: 흔들림 없음, 원래 위치로 부드럽게 복귀
- **Walk (걷기)**: `walkBobSpeed`와 `walkBobAmount` 적용
- **Run (뛰기)**: `runBobSpeed`와 `runBobAmount` 적용

## 🎨 권장 설정값

### 기본 설정 (자연스러움)
| 구분 | Bob Speed | Bob Amount |
|------|-----------|------------|
| **걷기** | 12 ~ 16 | 0.04 ~ 0.07 |
| **뛰기** | 18 ~ 22 | 0.08 ~ 0.12 |

### 강한 흔들림 (리얼리즘)
| 구분 | Bob Speed | Bob Amount |
|------|-----------|------------|
| **걷기** | 14 ~ 18 | 0.06 ~ 0.10 |
| **뛰기** | 20 ~ 24 | 0.12 ~ 0.18 |

### 약한 흔들림 (편안함)
| 구분 | Bob Speed | Bob Amount |
|------|-----------|------------|
| **걷기** | 10 ~ 14 | 0.03 ~ 0.05 |
| **뛰기** | 16 ~ 20 | 0.06 ~ 0.09 |

## 🐛 문제 해결

### Controller 필드가 비어있을 때
1. **자동 탐지 확인**
   - 스크립트가 Player에 붙어있는지 확인
   - Player에 CharacterController 컴포넌트가 있는지 확인
   - Play 모드로 전환하면 자동으로 찾음

2. **수동 할당**
   - Inspector에서 Controller 필드에 Player의 CharacterController 드래그
   - 또는 Player를 선택한 후 CharacterController 컴포넌트 드래그

### Camera Holder 필드가 비어있을 때
1. **CameraRoot 확인**
   - Hierarchy에서 Player의 자식에 CameraRoot가 있는지 확인
   - CameraRoot의 이름이 정확히 "CameraRoot"인지 확인 (대소문자 구분 없음)

2. **수동 할당**
   - Inspector에서 Camera Holder 필드에 CameraRoot 드래그

### 흔들림이 너무 강할 때
1. **Bob Amount 줄이기**
   - Walk Bob Amount: 0.03 ~ 0.04
   - Run Bob Amount: 0.06 ~ 0.08

2. **Bob Speed 조정**
   - 값이 높을수록 빠르게 흔들림
   - 값이 낮을수록 느리게 흔들림

### 흔들림이 안 보일 때
1. **Bob Amount 늘리기**
   - Walk Bob Amount: 0.06 ~ 0.08
   - Run Bob Amount: 0.12 ~ 0.15

2. **속도 확인**
   - CharacterController의 속도가 0.1 이상인지 확인
   - PlayerController의 moveSpeed/runSpeed가 너무 낮은지 확인

### 멈췄을 때 흔들림이 사라지지 않을 때
1. **복귀 속도 확인**
   - 코드에서 `Time.deltaTime * 5f`가 적용되어 자동으로 복귀
   - Lerp 값을 높이면 더 빠르게 복귀

2. **Y 위치 확인**
   - CameraRoot의 Y 위치가 올바른지 확인
   - CameraFollow의 Eye Height와 일치하는지 확인

## 📝 코드 예시

### 파라미터 변경
```csharp
// Inspector에서 직접 변경하거나
headBob.walkBobSpeed = 16f;
headBob.walkBobAmount = 0.06f;
headBob.runBobSpeed = 20f;
headBob.runBobAmount = 0.12f;
```

### 런타임에서 흔들림 비활성화
```csharp
// HeadBob 스크립트 비활성화
headBob.enabled = false;
```

### 커스텀 흔들림 추가
```csharp
// X, Z축 흔들림 추가 가능
cameraHolder.localPosition = new Vector3(
    Mathf.Sin(timer * 0.5f) * sideAmount,  // X축 흔들림
    defaultYPos + Mathf.Sin(timer) * amount,
    cameraHolder.localPosition.z
);
```

## 🔄 확장 가능성

### 추가 기능
- **X, Z축 흔들림**: 좌우로도 흔들림 추가
- **대시 흔들림**: 대시 시 특별한 흔들림 효과
- **데미지 흔들림**: 피격 시 추가 흔들림
- **지형 흔들림**: 계단이나 경사면에서 다른 흔들림

### 추가 파라미터
- **걷기/뛰기 감지**: 속도 기반 자동 감지 대신 입력 기반 감지
- **부드러운 전환**: 걷기→뛰기 전환 시 부드러운 흔들림 변화
- **랜덤성 추가**: 약간의 랜덤성을 추가하여 자연스러움 향상

## 📚 관련 파일
- `CameraFollow.cs`: 카메라 위치 및 회전 제어
- `PlayerController.cs`: 플레이어 이동 제어
- `CharacterController`: Unity 기본 컴포넌트

## 💡 참고사항

### VR 멀미 주의
- 흔들림이 너무 강하면 VR 멀미처럼 어지러울 수 있음
- 최소한으로 주는 것이 베스트
- 개인 취향에 맞게 조정 필요

### 성능 최적화
- Update에서 실행되지만 매우 가벼운 계산
- Sin 함수 계산은 성능 영향 거의 없음
- 추가 오버헤드 최소

### 구조 설명
- CameraRoot의 `localPosition.y`만 수정
- CameraFollow와 충돌하지 않음
- X, Z축은 CameraFollow가 제어

## 🎮 사용 예시

### 일반적인 FPS 게임
```
Walk Bob Speed: 14
Walk Bob Amount: 0.05
Run Bob Speed: 18
Run Bob Amount: 0.1
```

### 시뮬레이션 게임 (현실적)
```
Walk Bob Speed: 16
Walk Bob Amount: 0.07
Run Bob Speed: 22
Run Bob Amount: 0.15
```

### 캐주얼 게임 (편안함)
```
Walk Bob Speed: 12
Walk Bob Amount: 0.03
Run Bob Speed: 16
Run Bob Amount: 0.06
```

