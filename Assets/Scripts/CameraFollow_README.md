# CameraFollow.cs

1인칭 시점 카메라가 플레이어를 따라다니며 마우스로 회전하는 시스템

## 📋 개요

플레이어의 머리 위치를 따라다니며, 마우스 입력으로 시점을 회전시킵니다. 부드러운 추적과 회전 각도 제한을 지원합니다.

## 🎯 주요 기능

- **플레이어 추적**: 카메라가 플레이어 위치를 따라다님
- **마우스 회전**: 마우스로 시점 회전 (좌우/상하)
- **부드러운 추적**: Lerp를 사용한 부드러운 카메라 이동
- **회전 제한**: 상하 회전 각도 제한 (-80° ~ 80°)
- **자동 Player 탐지**: Tag 또는 이름으로 Player 자동 찾기

## 🔧 적용 방법

### 1단계: 컴포넌트 추가
1. Main Camera 오브젝트 선택
2. Inspector에서 `Add Component` → `Camera Follow` 추가

### 2단계: 설정 (선택적)
- **Target**: Player 오브젝트 할당
  - 자동 탐지: Tag "Player" 또는 이름 "Player"
- **Offset**: 카메라 위치 오프셋 (기본값: Y=1.8, 머리 높이)
- **Mouse Sensitivity**: 마우스 감도 (기본값: 2)
- **Smooth Follow**: 부드러운 추적 활성화 여부
- **Smooth Speed**: 추적 속도 (기본값: 10)

### 3단계: 카메라 위치
- 카메라는 Player의 **자식이 아니어야 함** (독립적으로 동작)
- 카메라가 Scene 루트에 있거나 별도 오브젝트로 배치

## ⚙️ Inspector 파라미터

| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| Target | Transform | - | 추적할 플레이어 오브젝트 |
| Offset | Vector3 | (0, 1.8, 0) | 카메라 위치 오프셋 (머리 높이) |
| Mouse Sensitivity | float | 2f | 마우스 감도 |
| Min Vertical Angle | float | -80f | 최소 상하 회전 각도 |
| Max Vertical Angle | float | 80f | 최대 상하 회전 각도 |
| Smooth Follow | bool | true | 부드러운 추적 활성화 |
| Smooth Speed | float | 10f | 추적 속도 |

## 🎮 사용 방법

### 마우스 조작
- **마우스 좌우 이동**: 플레이어가 좌우로 회전
- **마우스 상하 이동**: 카메라가 위아래로 회전 (각도 제한 있음)

### 커서 잠금
- Play 모드 진입 시 커서가 자동으로 잠김
- ESC 키로 잠금 해제 (별도 구현 필요)

## 🔄 동작 원리

### 위치 추적
1. Player 위치 + Offset 계산
2. Smooth Follow가 활성화되어 있으면 Lerp로 부드럽게 이동
3. 비활성화되어 있으면 즉시 이동

### 회전 처리
1. **수평 회전 (Y축)**
   - 마우스 X축 입력 받기
   - `rotationY`에 누적
   - Player 오브젝트 회전 적용

2. **수직 회전 (X축)**
   - 마우스 Y축 입력 받기
   - `rotationX`에 누적 (반전)
   - 각도 제한 적용 (-80° ~ 80°)
   - 카메라만 회전

### LateUpdate 사용
- `LateUpdate()`에서 실행하여 Player 이동 후 카메라 위치 업데이트
- 부드러운 추적 보장

## 🐛 문제 해결

### 카메라가 움직이지 않을 때
1. **Target 확인**
   - Inspector에서 `Target` 필드에 Player 오브젝트 할당
   - Console에서 자동 탐지 메시지 확인

2. **카메라 위치 확인**
   - 카메라가 Player의 자식이 아닌지 확인
   - 카메라가 Scene에 활성화되어 있는지 확인

3. **스크립트 활성화 확인**
   - Inspector에서 스크립트가 활성화되어 있는지 확인

### 시점이 회전하지 않을 때
1. **마우스 입력 확인**
   - Unity Input Manager에서 Mouse X, Mouse Y 축 확인
   - Play 모드에서 마우스 움직임 확인

2. **커서 잠금 확인**
   - 커서가 잠겨있어야 마우스 입력이 정상 작동
   - `Cursor.lockState = CursorLockMode.Locked` 확인

3. **감도 확인**
   - `Mouse Sensitivity` 값이 너무 낮은지 확인
   - 기본값: 2

### 카메라 위치가 이상할 때
1. **Offset 확인**
   - `Offset` 값이 적절한지 확인
   - 기본값: Y=1.8 (머리 높이)
   - Rigidbody 사용 시 높이 조정 필요

2. **Player 위치 확인**
   - Player 오브젝트의 위치가 올바른지 확인

## 📝 코드 예시

### 감도 변경
```csharp
// Inspector에서 직접 변경하거나
cameraFollow.mouseSensitivity = 3f;
```

### Offset 조정
```csharp
// 머리 높이 조정
cameraFollow.offset = new Vector3(0, 2f, 0);
```

### 부드러운 추적 비활성화
```csharp
cameraFollow.smoothFollow = false;
```

## 🔄 확장 가능성

### 추가 기능
- **FOV 변경**: 달리기 시 FOV 증가
- **카메라 흔들림**: 피격 시 카메라 흔들림
- **줌 기능**: 마우스 휠로 줌 인/아웃
- **3인칭 시점**: 거리 조정으로 3인칭 전환

### 추가 설정
- **최대 거리**: 카메라와 플레이어 최대 거리
- **장애물 감지**: 벽에 가려지면 카메라 위치 조정
- **부드러운 회전**: 회전도 Lerp 적용

## 📚 관련 파일
- `PlayerController.cs`: 추적 대상
- Main Camera: Unity 기본 컴포넌트

