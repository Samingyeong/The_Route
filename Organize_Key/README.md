# 게임 키 조작 가이드

이 문서는 게임에서 사용하는 모든 키 입력을 정리한 것입니다.

---

## 🎮 플레이어 이동 및 조작

### 기본 이동
| 키 | 기능 | 설명 |
|---|---|---|
| **W** | 앞으로 이동 | 플레이어가 앞으로 이동합니다 |
| **A** | 왼쪽으로 이동 | 플레이어가 왼쪽으로 이동합니다 |
| **S** | 뒤로 이동 | 플레이어가 뒤로 이동합니다 |
| **D** | 오른쪽으로 이동 | 플레이어가 오른쪽으로 이동합니다 |

### 이동 보조
| 키 | 기능 | 설명 |
|---|---|---|
| **Left Shift** | 달리기 | 누르고 있으면 달리기 속도로 이동합니다 |
| **Space** | 점프 | 지면에 있을 때 점프합니다 |

### 시점 조작
| 입력 | 기능 | 설명 |
|---|---|---|
| **마우스 이동** | 시점 회전 | 마우스를 움직여 카메라 시점을 회전시킵니다 |
| - | 좌우 회전 | 마우스를 좌우로 움직이면 플레이어가 회전합니다 |
| - | 상하 회전 | 마우스를 상하로 움직이면 카메라가 위아래로 회전합니다 |

---

## 🔫 총 조작 (GunAction)

### 발사 및 전투
| 키 | 기능 | 설명 |
|---|---|---|
| **A** | 발사 | 누르고 있으면 계속 발사합니다 (IsShooting = true) |
| **Q** | 발사 트리거 | 한 번 누르면 발사 트리거가 실행됩니다 (OnFire) |

### 총 관리
| 키 | 기능 | 설명 |
|---|---|---|
| **B** | 재장전 | 재장전 애니메이션을 실행합니다 (OnReload) |
| **D** | 무기 꺼내기 | 무기를 꺼내는 애니메이션을 실행합니다 (OnDraw) |

### 전술 행동
| 키 | 기능 | 설명 |
|---|---|---|
| **C** | 은폐 | 누르고 있으면 은폐 모션을 취합니다 (IsHiding = true) |

---

## 📋 키 매핑 요약

### PlayerController.cs
```
W, A, S, D      → 이동
Left Shift      → 달리기
Space           → 점프
```

### CameraFollow.cs
```
마우스 이동     → 시점 회전
```

### GunAction.cs
```
A               → 발사 (누르고 있음)
B               → 재장전
C               → 은폐 (누르고 있음)
D               → 무기 꺼내기
Q               → 발사 트리거
```

### ZombieAI.cs
```
(키 없음)       → 자동 AI 동작
```

---

## 🎯 키 변경 방법

각 스크립트의 `Update()` 메서드에서 키 입력을 처리하고 있습니다.

### PlayerController.cs
```csharp
// 이동
float horizontal = Input.GetAxis("Horizontal"); // A, D
float vertical = Input.GetAxis("Vertical");     // W, S

// 달리기
isRunning = Input.GetKey(KeyCode.LeftShift);

// 점프
if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
```

### CameraFollow.cs
```csharp
// 마우스 회전
float mouseX = Input.GetAxis("Mouse X");
float mouseY = Input.GetAxis("Mouse Y");
```

### GunAction.cs
```csharp
// 발사
if (Input.GetKeyDown(KeyCode.A)) ...
if (Input.GetKeyUp(KeyCode.A)) ...

// 재장전
if (Input.GetKeyDown(KeyCode.B)) ...

// 은폐
if (Input.GetKeyDown(KeyCode.C)) ...
if (Input.GetKeyUp(KeyCode.C)) ...

// 무기 꺼내기
if (Input.GetKeyDown(KeyCode.D)) ...

// 발사 트리거
if (Input.GetKeyDown(KeyCode.Q)) ...
```

키를 변경하려면 위 코드에서 `KeyCode.XXX` 부분을 수정하면 됩니다.

---

## 📝 참고사항

- **누르고 있는 키**: A (발사), C (은폐), Left Shift (달리기)
- **한 번 누르는 키**: B (재장전), D (무기 꺼내기), Q (발사 트리거), Space (점프)
- **연속 입력**: W, A, S, D (이동), 마우스 이동 (시점 회전)

---

## 🔄 향후 추가 예정 키

- **E**: 상호작용 (Interact)
- **R**: 재장전 (대체)
- **Tab**: 인벤토리
- **ESC**: 메뉴

