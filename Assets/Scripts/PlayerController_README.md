# PlayerController.cs

WASD 키보드 입력으로 플레이어 이동을 제어하는 스크립트

## 📋 개요

CharacterController를 사용하여 플레이어의 이동, 달리기, 점프를 처리합니다. 카메라 방향을 기준으로 이동하며, 중력과 지면 감지를 자동으로 처리합니다.

## 🎯 주요 기능

- **WASD 이동**: 전후좌우 이동
- **Shift 달리기**: Left Shift 키로 달리기 속도 증가
- **Space 점프**: Space 키로 점프
- **카메라 기준 이동**: 카메라가 바라보는 방향 기준으로 이동
- **자동 중력 처리**: 중력과 지면 감지 자동 처리

## 🔧 적용 방법

### 1단계: 컴포넌트 추가
1. Player 오브젝트 선택
2. Inspector에서 `Add Component` → `Player Controller` 추가

### 2단계: 자동 설정
- CharacterController가 없으면 자동으로 추가됨
- CharacterController 설정:
  - Height: 2
  - Radius: 0.5
  - Center: (0, 1, 0)

### 3단계: 설정 조정 (선택적)
- **Move Speed**: 기본 걷기 속도 (기본값: 5)
- **Run Speed**: 달리기 속도 (기본값: 8)
- **Jump Force**: 점프 힘 (기본값: 5)

## ⚙️ Inspector 파라미터

| 파라미터 | 타입 | 기본값 | 설명 |
|---------|------|--------|------|
| Move Speed | float | 5f | 기본 걷기 속도 |
| Run Speed | float | 8f | 달리기 속도 |
| Jump Force | float | 5f | 점프 힘 |
| Character Controller | CharacterController | - | CharacterController 컴포넌트 (자동 탐지) |

## 🎮 사용 키

| 키 | 기능 | 설명 |
|---|---|---|
| **W** | 앞으로 이동 | 플레이어가 앞으로 이동 |
| **A** | 왼쪽으로 이동 | 플레이어가 왼쪽으로 이동 |
| **S** | 뒤로 이동 | 플레이어가 뒤로 이동 |
| **D** | 오른쪽으로 이동 | 플레이어가 오른쪽으로 이동 |
| **Left Shift** | 달리기 | 누르고 있으면 달리기 속도로 이동 |
| **Space** | 점프 | 지면에 있을 때 점프 |

## 🔄 동작 원리

### 이동 방향 계산
1. 카메라의 forward와 right 방향 가져오기
2. Y축 제거 (수평 이동만)
3. W/S 입력으로 forward 방향, A/D 입력으로 right 방향 계산
4. 정규화하여 대각선 이동 시 속도 일정 유지

### 달리기
- Left Shift 키를 누르고 있으면 `runSpeed` 사용
- 키를 떼면 `moveSpeed`로 복귀

### 점프
- Space 키를 누르면 수직 속도 계산
- 중력에 의해 점점 떨어짐
- 지면에 닿으면 다시 점프 가능

### 중력 처리
- `gravity = -9.81f`로 설정
- 매 프레임 `velocity.y`에 중력 적용
- 지면에 닿으면 `velocity.y`를 -2로 리셋

## 🐛 문제 해결

### 플레이어가 움직이지 않을 때
1. **CharacterController 확인**
   - Inspector에서 CharacterController 컴포넌트가 있는지 확인
   - 자동으로 추가되지만, 수동으로 확인 필요

2. **지면 확인**
   - 지면에 Collider가 있는지 확인
   - CharacterController가 지면과 충돌하는지 확인

3. **입력 확인**
   - Unity Input Manager에서 Horizontal/Vertical 축이 설정되어 있는지 확인
   - 기본값: Horizontal (A/D), Vertical (W/S)

### 점프가 작동하지 않을 때
1. **지면 감지 확인**
   - `isGrounded`가 true인지 확인
   - CharacterController가 지면과 충돌하는지 확인

2. **중력 확인**
   - 중력 값이 올바른지 확인 (기본값: -9.81)

### 카메라 기준 이동이 안 될 때
1. **카메라 확인**
   - Main Camera가 Scene에 있는지 확인
   - `Camera.main`이 null이 아닌지 확인

2. **카메라 방향 확인**
   - 카메라가 올바른 방향을 바라보고 있는지 확인

## 📝 코드 예시

### 이동 속도 변경
```csharp
// Inspector에서 직접 변경하거나
playerController.moveSpeed = 7f;
```

### 점프 힘 변경
```csharp
playerController.jumpForce = 8f;
```

## 🔄 확장 가능성

### 추가 기능
- **크롤링**: Ctrl 키로 앉기/크롤링
- **대시**: 특정 키로 빠른 이동
- **벽 점프**: 벽에 닿았을 때 점프
- **사다리 오르기**: 사다리 감지 및 오르기

### 추가 입력
- **게임패드 지원**: Input.GetAxis 사용
- **터치 입력**: 모바일 지원

## 📚 관련 파일
- `CameraFollow.cs`: 카메라 추적 (이동 방향 기준)
- `CharacterController`: Unity 기본 컴포넌트

