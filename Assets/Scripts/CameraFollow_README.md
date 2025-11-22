# CameraFollow.cs

1인칭 시점 카메라를 제어하는 스크립트. 플레이어를 따라가며 마우스 입력으로 회전합니다.

## 📋 개요

CameraRoot 구조를 사용하여 1인칭 FPS 시점을 구현합니다. Player 오브젝트를 타겟으로 따라가고, 마우스 입력으로 수평/수직 회전을 처리합니다. HeadBob 스크립트와 함께 사용하여 자연스러운 카메라 움직임을 제공합니다.

## 🎯 주요 기능

- **1인칭 시점**: 플레이어의 눈 높이에서 카메라 위치
- **마우스 회전**: 마우스로 시점 회전 (좌우/상하)
- **플레이어 추적**: 플레이어 위치를 자동으로 따라감
- **카메라 충돌 처리**: 벽이나 장애물에 카메라가 뚫지 않도록 처리
- **부드러운 추적**: Lerp를 사용한 부드러운 카메라 이동
- **회전 제한**: 상하 회전 각도 제한 (-80° ~ 80°)

## 🔧 적용 방법

### 1단계: CameraRoot 오브젝트 생성
1. Hierarchy에서 `player` 오브젝트 선택
2. 우클릭 → `Create Empty` → 이름을 `CameraRoot`로 변경
3. `CameraRoot`의 Transform 설정:
   - **Position**: `(0, 1.8, 0)` (눈 높이, Inspector에서 조정 가능)
   - **Rotation**: `(0, 0, 0)`

### 2단계: Main Camera를 CameraRoot의 자식으로
1. Hierarchy에서 `Main Camera` 선택
2. `CameraRoot`로 드래그 & 드롭 (자식으로 설정)
3. `Main Camera`의 Transform 설정:
   - **Local Position**: `(0, 0, 0)`
   - **Local Rotation**: `(0, 0, 0)`

### 3단계: CameraFollow 스크립트 추가
1. `CameraRoot` 오브젝트 선택
2. Inspector에서 `Add Component` → `Camera Follow` 추가
3. **자동 설정**:
   - `Target`: Player 오브젝트 (자동으로 찾음)
   - `Camera Root`: CameraRoot 오브젝트 (자동으로 찾음)

### 4단계: 설정 조정 (선택적)
- **Eye Height**: 눈 높이 (기본값: 1.6m)
- **Mouse Sensitivity**: 마우스 감도 (기본값: 2)
- **Min/Max Vertical Angle**: 상하 회전 제한 (기본값: -80° ~ 80°)
- **Smooth Follow**: 부드러운 추적 활성화 (기본값: true)
- **Smooth Speed**: 추적 속도 (기본값: 10)

## ⚙️ Inspector 파라미터

| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| **Target** | Transform | - | 추적할 플레이어 오브젝트 (자동 탐지) |
| **Camera Root** | Transform | - | CameraRoot Transform (자동 탐지) |
| **Eye Height** | float | 1.6f | 눈 높이 (플레이어 발 기준, 미터 단위) |
| **Offset** | Vector3 | (0, 0, 0) | 추가 위치 오프셋 |
| **Mouse Sensitivity** | float | 2f | 마우스 감도 |
| **Min Vertical Angle** | float | -80f | 최소 수직 회전 각도 |
| **Max Vertical Angle** | float | 80f | 최대 수직 회전 각도 |
| **Smooth Follow** | bool | true | 부드러운 추적 활성화 |
| **Smooth Speed** | float | 10f | 추적 속도 |
| **Camera Collision Radius** | float | 0.2f | 카메라 충돌 체크 반경 |
| **Obstacle Layer** | LayerMask | -1 | 충돌 체크할 레이어 |

## 🎮 동작 원리

### 카메라 구조
```
Player
  └ CameraRoot (CameraFollow 스크립트가 여기에 붙음)
      └ Main Camera
```

### 회전 시스템
- **수평 회전 (Y축)**: Player 오브젝트가 회전 (마우스 좌우)
- **수직 회전 (X축)**: CameraRoot만 회전 (마우스 상하)
- **Main Camera**: CameraRoot의 자식이므로 자동으로 회전을 따라감

### 위치 추적
- CameraRoot의 `localPosition`을 Player의 눈 높이로 설정
- HeadBob 스크립트가 Y축을 제어하여 흔들림 효과 추가
- X, Z축은 offset으로 조정 가능

### 충돌 처리
- SphereCast를 사용하여 플레이어에서 카메라 위치까지의 충돌 체크
- 장애물이 있으면 카메라를 안전한 위치로 이동
- 배틀그라운드 스타일의 충돌 처리

## 🐛 문제 해결

### 카메라가 3인칭처럼 보일 때
1. **Main Camera 위치 확인**
   - Main Camera의 `Local Position`이 `(0, 0, 0)`인지 확인
   - Main Camera가 CameraRoot의 자식인지 확인

2. **CameraRoot 위치 확인**
   - CameraRoot의 `Local Position` Y 값이 눈 높이인지 확인 (기본: 1.6)
   - CameraRoot가 Player의 직접 자식인지 확인

3. **카메라 회전 확인**
   - Play 모드에서 마우스를 움직여 회전이 작동하는지 확인
   - CameraRoot의 회전이 올바르게 적용되는지 확인

### 마우스 회전이 안 될 때
1. **커서 잠금 확인**
   - Play 모드에서 커서가 잠겨있는지 확인
   - Esc 키를 누르면 커서 잠금 해제됨

2. **Input 설정 확인**
   - Unity Input Manager에서 "Mouse X", "Mouse Y" 축이 있는지 확인
   - 기본값으로 자동 설정되어 있음

3. **스크립트 확인**
   - CameraFollow 스크립트가 CameraRoot에 붙어있는지 확인
   - Target과 Camera Root가 올바르게 할당되었는지 확인

### 카메라가 플레이어를 안 따라갈 때
1. **Target 확인**
   - Inspector에서 Target 필드에 Player가 할당되었는지 확인
   - 자동으로 찾지 못하면 수동으로 할당

2. **CameraRoot 확인**
   - CameraRoot가 Player의 자식인지 확인
   - Camera Root 필드에 CameraRoot가 할당되었는지 확인

### 카메라가 벽을 뚫을 때
1. **충돌 레이어 확인**
   - Obstacle Layer에 벽 레이어가 포함되어 있는지 확인
   - 벽 오브젝트의 Layer 설정 확인

2. **충돌 반경 조정**
   - Camera Collision Radius 값을 조정
   - 너무 작으면 충돌을 감지하지 못할 수 있음

## 📝 코드 예시

### 눈 높이 변경
```csharp
// Inspector에서 직접 변경하거나
cameraFollow.eyeHeight = 1.75f;
```

### 마우스 감도 변경
```csharp
cameraFollow.mouseSensitivity = 3f;
```

### 회전 제한 변경
```csharp
cameraFollow.minVerticalAngle = -90f;
cameraFollow.maxVerticalAngle = 90f;
```

## 🔄 확장 가능성

### 추가 기능
- **FOV 조정**: 거리에 따라 FOV 변경
- **카메라 흔들림**: 데미지 받을 때 흔들림 효과
- **줌 기능**: 우클릭으로 줌 인/아웃
- **카메라 효과**: 후처리 효과 추가

### 추가 입력
- **게임패드 지원**: 우측 스틱으로 회전
- **터치 지원**: 모바일 터치 회전

## 📚 관련 파일
- `HeadBob.cs`: 걷기/뛰기 시 카메라 흔들림 효과
- `PlayerController.cs`: 플레이어 이동 제어
- `CharacterController`: Unity 기본 컴포넌트

## 💡 참고사항

### CameraRoot 구조의 장점
- HeadBob과의 분리된 제어로 자연스러운 움직임
- 카메라 위치와 회전의 독립적 관리
- 확장 가능한 구조 (추가 효과 추가 용이)

### 권장 설정
- **Eye Height**: 1.6 ~ 1.75m (평균 눈 높이)
- **Mouse Sensitivity**: 2 ~ 3 (개인 취향에 맞게 조정)
- **Smooth Speed**: 10 ~ 15 (부드러운 움직임)

