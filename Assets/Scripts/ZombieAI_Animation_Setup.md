# Zombie AI 애니메이션 설정 가이드

ZombieAI 스크립트에 애니메이션을 연결하는 방법입니다.

## 📋 필요한 애니메이션 파일

`Assets/Animation/Not So Scary Zombie Pack/` 폴더에 있는 애니메이션:
- **Idle**: `zombie idle.fbx` ✅
- **Walk**: `walking.fbx` ✅
- **Run**: `zombie running.fbx` ✅
- **Attack**: `zombie attack.fbx`, `zombie punching.fbx`, `zombie kicking.fbx`, `zombie headbutt.fbx` ✅
- **Stumble**: `zombie stumbling.fbx` (선택사항)
- **Search**: `zombie idle.fbx` 또는 `zombie scratch idle.fbx` (선택사항)

## 🎯 1단계: Animator Controller 설정

### Animator Controller 열기
1. Project 창에서 `Assets/Controller/controller-zombie-A.controller` (또는 B, C) 더블클릭
2. Animator 창이 열립니다 (없으면 `Window > Animation > Animator`)

### 파라미터(Parameters) 추가
**파라미터란?** Animator Controller에서 사용하는 변수입니다. ZombieAI 스크립트가 이 파라미터의 값을 변경하면 애니메이션 상태가 전환됩니다.

Animator 창 왼쪽 하단의 **"Parameters"** 탭에서 다음 파라미터들을 추가:

| 파라미터 이름 | 타입 | 설명 | Conditions에서 사용 |
|-------------|------|------|-------------------|
| **Speed** | Float | 좀비의 이동 속도 | 비교 (Greater, Less 등) |
| **IsWalking** | Bool | 걷기 상태 | true/false |
| **IsRunning** | Bool | 뛰기 상태 | true/false |
| **IsAttacking** | Bool | 공격 상태 | true/false |
| **IsSearching** | Bool | 탐색 상태 (선택사항) | true/false |
| **IsStumbling** | Bool | 우당탕 상태 (선택사항) | true/false |

**추가 방법 (단계별):**
1. **Animator 창** 왼쪽 하단에서 **"Parameters"** 탭 클릭
   - Layers, Parameters, Settings 탭 중 Parameters 탭 선택
2. `+` 버튼 클릭 (Parameters 탭 상단)
3. 드롭다운에서 타입 선택:
   - **Float**: 숫자 (속도 등)
   - **Bool**: true/false (상태 on/off)
4. 이름 입력창에 파라미터 이름 입력 (예: `IsWalking`)
5. Enter 키 누르거나 이름 입력창 외부 클릭
6. **반복**: 다른 파라미터들도 같은 방식으로 추가

**예시: IsWalking 추가하기**
1. `+` 버튼 클릭
2. `Bool` 선택
3. 이름 입력란에 `IsWalking` 입력
4. Enter 또는 외부 클릭
5. ✅ Parameters 목록에 `IsWalking (Bool)`이 추가됨

> **⚠️ 주의**: 
> - 파라미터 이름은 정확히 입력해야 합니다 (`IsWalking`, `IsRunning` 등)
> - 대소문자 구분합니다 (`isWalking` ≠ `IsWalking`)
> - 이 파라미터들을 나중에 Conditions에서 사용합니다

## 🎨 2단계: 애니메이션 상태(States) 생성

### 기본 상태 생성
Animator 창에서 우클릭 → `Create State > Empty` 로 다음 상태들을 생성:

1. **Idle** (기본 상태)
2. **Walk**
3. **Run**
4. **Attack**

### Idle 상태 설정
1. `Idle` 상태를 선택
2. Inspector에서:
   - **Motion**: `zombie idle` (애니메이션 클립 드래그 & 드롭)
   - **Speed**: 1
   - **Write Defaults**: 체크 해제 (권장)

### Walk 상태 설정
1. `Walk` 상태를 선택
2. Inspector에서:
   - **Motion**: `walking` 애니메이션 클립 (Project 창에서 드래그 & 드롭)
   - **Speed**: 1
   - **Write Defaults**: 체크 해제

### Run 상태 설정
1. `Run` 상태를 선택
2. Inspector에서:
   - **Motion**: `zombie running` 애니메이션 클립
   - **Speed**: 1
   - **Write Defaults**: 체크 해제

### Attack 상태 설정
1. `Attack` 상태를 선택
2. Inspector에서:
   - **Motion**: `zombie attack` 애니메이션 클립 (또는 punching, kicking 등)
   - **Speed**: 1
   - **Write Defaults**: 체크 해제
   - **Has Exit Time**: **체크** (공격 애니메이션이 끝날 때까지 기다림)
   - **Exit Time**: 0.9 (애니메이션 90% 재생 후 전환 가능)

## 🔄 3단계: Transition (전환) 설정

### Entry → Idle 전환
1. **Animator 창**에서 `Entry` 노드(주황색)를 클릭하여 선택
2. `Entry` 노드에서 마우스 오른쪽 버튼 클릭 → `Make Transition` 선택
3. `Idle` 상태까지 드래그하여 화살표 생성
4. 생성된 **화살표(화살표 모양의 화살표)**를 클릭하여 선택
   - 화살표가 선택되면 파란색으로 하이라이트됩니다
5. **Unity 에디터의 오른쪽 패널인 Inspector 창**을 확인하세요
   - Inspector 창이 안 보이면: `Window > General > Inspector` 또는 단축키 `Ctrl+Shift+I` (Windows) / `Cmd+Shift+I` (Mac)
6. Inspector 창에서 다음 설정을 변경:
   - **Has Exit Time**: ✅ 체크박스 체크
   - **Exit Time**: `1` 입력
   - **Transition Duration**: `0` 입력

> **💡 팁**: 화살표가 선택되지 않으면 Animator 창을 클릭하고 화살표를 다시 클릭해보세요. Inspector는 현재 선택된 오브젝트의 설정을 보여줍니다.

### Idle → Walk 전환
1. **Animator 창**에서 `Idle` 상태(사각형 노드)를 클릭하여 선택
2. `Idle` 상태에서 마우스 오른쪽 버튼 클릭 → `Make Transition` 선택
3. `Walk` 상태까지 드래그하여 화살표 생성
4. 생성된 **화살표를 클릭**하여 선택 (파란색으로 하이라이트됨)
5. **Inspector 창**(Unity 에디터 오른쪽) 확인:
   - Inspector 창 하단에 "Conditions" 섹션이 보입니다
6. Inspector 창 하단의 **"Conditions"** 섹션 찾기:
   - Inspector 창을 아래로 스크롤하면 "Conditions" 섹션이 보입니다
   - 현재 비어있거나 "Blend > Greater > 0" 같은 기본 조건이 있을 수 있습니다

7. **Conditions 설정하기**:
   
   **먼저 기존 조건 삭제** (있다면):
   - 조건 옆의 `-` 버튼을 클릭하여 기존 조건 삭제
   
   **새 조건 추가**:
   - `+` 버튼 클릭 (Conditions 섹션 오른쪽)
   - 새로운 조건 행이 나타납니다
   
   **Parameter 선택**:
   - 왼쪽 첫 번째 드롭다운 클릭 → `IsWalking` 선택
     - 여기에 Animator Controller의 Parameters 목록이 나타납니다
     - `Speed`, `IsWalking`, `IsRunning`, `IsAttacking` 등이 보여야 합니다
     - 만약 목록이 비어있으면, 먼저 Animator Controller의 Parameters 탭에서 파라미터를 추가해야 합니다
   
   **조건 값 선택**:
   - 가운데 드롭다운 클릭 → `true` 선택
     - Bool 파라미터이면: `true` 또는 `false`
     - Float 파라미터이면: `Greater`, `Less`, `Equals` 등 비교 연산자
   
   **최종 결과**:
   - `IsWalking` (파라미터) → `true` (값) 이렇게 표시되면 성공!

> **💡 중요**: 
> - Parameter 드롭다운에 아무것도 안 보이면 → Animator Controller의 Parameters 탭에서 먼저 파라미터를 추가해야 합니다
> - `IsWalking`이 목록에 없으면 → 1단계 "파라미터 추가"로 돌아가서 확인하세요
> - 조건을 추가하면 Animator 창의 화살표 위에 `IsWalking = true` 같이 표시됩니다

### Walk → Idle 전환
1. **Animator 창**에서 `Walk` 상태를 클릭
2. `Walk` 상태에서 우클릭 → `Make Transition` → `Idle`로 드래그
3. 생성된 **화살표를 클릭**하여 선택
4. **Inspector 창**(오른쪽 패널)에서:
   - **Conditions**: 
     - `+` 버튼 클릭 → `IsWalking` 선택 → `false` 선택
   - **Has Exit Time**: ❌ 체크 해제
   - **Transition Duration**: `0.2` 입력

> **📍 위치 확인**: 
> - **Animator 창**: `Window > Animation > Animator` (일반적으로 왼쪽 하단 또는 별도 탭)
> - **Inspector 창**: Unity 에디터 오른쪽에 항상 보이는 패널

### Walk → Run 전환
1. `Walk` 상태를 선택
2. 우클릭 → `Make Transition` → `Run` 클릭
3. 화살표 선택 후 Inspector에서:
   - **Conditions**: `IsRunning` → `true`
   - **Has Exit Time**: 체크 해제
   - **Transition Duration**: 0.1

### Run → Walk 전환
1. `Run` 상태를 선택
2. 우클릭 → `Make Transition` → `Walk` 클릭
3. 화살표 선택 후 Inspector에서:
   - **Conditions**: `IsRunning` → `false`, `IsWalking` → `true`
   - **Has Exit Time**: 체크 해제
   - **Transition Duration**: 0.1

### Any State → Attack 전환
1. `Any State` 노드를 선택
2. 우클릭 → `Make Transition` → `Attack` 클릭
3. 화살표 선택 후 Inspector에서:
   - **Conditions**: `IsAttacking` → `true`
   - **Has Exit Time**: 체크 해제
   - **Transition Duration**: 0.1

### Attack → Idle/Walk/Run 전환
1. `Attack` 상태를 선택
2. 우클릭 → `Make Transition` → `Idle` 클릭
3. 화살표 선택 후 Inspector에서:
   - **Conditions**: `IsAttacking` → `false`
   - **Has Exit Time**: 체크 (애니메이션 끝날 때까지 기다림)
   - **Exit Time**: 0.9
   - **Transition Duration**: 0.2

4. `Walk`와 `Run`에도 같은 방식으로 전환 추가:
   - Attack → Walk: `IsAttacking` → `false`, `IsWalking` → `true`
   - Attack → Run: `IsAttacking` → `false`, `IsRunning` → `true`

## 📐 4단계: 애니메이션 클립 임포트 설정 확인

애니메이션 파일을 사용하기 전에 임포트 설정을 확인합니다:

### 각 .fbx 파일 설정
1. Project 창에서 애니메이션 .fbx 파일 선택 (예: `zombie idle.fbx`)
2. Inspector 창에서:
   - **Rig** 탭:
     - **Animation Type**: `Humanoid` 또는 `Generic` (모델에 맞게)
     - **Avatar Definition**: `Create From This Model`
   - **Animation** 탭:
     - **Import Animation**: 체크
     - **Root Transform Rotation**: `Bake Into Pose` 체크
     - **Root Transform Position (Y)**: `Bake Into Pose` 체크
     - **Root Transform Position (XZ)**: `Bake Into Pose` 체크
3. **Apply** 버튼 클릭

## 🎮 5단계: Zombie 오브젝트에 연결

### Animator 컴포넌트 확인
1. Hierarchy에서 좀비 오브젝트 선택 (예: `walking_zombie`)
2. Inspector에서 `Animator` 컴포넌트 확인:
   - **Controller**: `controller-zombie-A` (또는 설정한 Controller) 드래그 & 드롭
   - **Avatar**: 모델에 맞는 Avatar 설정

### ZombieAI 스크립트 확인
1. 좀비 오브젝트에 `Zombie AI` 컴포넌트가 있는지 확인
2. **Animator** 필드에 Animator 컴포넌트가 할당되어 있는지 확인 (자동 탐지됨)

## 🧪 6단계: 테스트

### Play 모드에서 테스트
1. Unity 에디터에서 **Play** 버튼 클릭
2. 좀비가 플레이어와의 거리에 따라 다른 애니메이션을 재생하는지 확인:
   - 멀리 있으면: **Idle**
   - 가까워지면: **Walk**
   - 더 가까워지면: **Run**
   - 공격 거리이면: **Attack**

### Animator 창에서 확인
1. Play 모드에서 Animator 창을 열어두기
2. 상태 전환이 실시간으로 보이는지 확인
3. 파라미터 값이 변하는지 확인

## ⚠️ 문제 해결

### 애니메이션이 재생되지 않을 때
1. **Animator Controller 확인**
   - 좀비 오브젝트의 Animator 컴포넌트에 Controller가 할당되었는지 확인

2. **파라미터 이름 확인**
   - ZombieAI 스크립트의 파라미터 이름과 Animator Controller의 파라미터 이름이 정확히 일치하는지 확인
   - 기본값:
     - `Speed` (Float)
     - `IsWalking` (Bool)
     - `IsRunning` (Bool)
     - `IsAttacking` (Bool)

3. **애니메이션 클립 확인**
   - 각 상태(State)에 애니메이션 클립이 할당되었는지 확인
   - Motion 필드가 비어있지 않은지 확인

### 전환이 너무 느릴 때
- **Transition Duration** 값을 줄이기 (0.1 ~ 0.2 권장)
- **Has Exit Time** 체크 해제 (특히 Walk, Run 전환)

### 공격 애니메이션이 반복되지 않을 때
- **Attack → Idle/Walk/Run** 전환에서:
  - **Has Exit Time**: 체크
  - **Exit Time**: 0.9 (애니메이션이 거의 끝날 때까지 기다림)

### 걷기/뛰기 애니메이션 속도가 안 맞을 때
- **Speed 파라미터** 사용:
  1. Walk/Run 상태 선택
  2. Inspector에서 **Speed** 필드를 `Speed` 파라미터로 연결
  3. ZombieAI가 자동으로 속도에 맞게 조정

## 💡 고급 설정

### 여러 공격 애니메이션 랜덤 재생
1. `Attack Punch`, `Attack Kick`, `Attack Headbutt` 등 여러 Attack 상태 생성
2. 각각 다른 애니메이션 클립 할당
3. `Any State`에서 각 Attack 상태로 전환 설정
4. ZombieAI 스크립트에서 랜덤하게 선택하도록 수정

### 공격 애니메이션 이벤트
1. Attack 애니메이션 클립에 Animation Event 추가
2. 공격 판정 시점에 이벤트 트리거
3. ZombieAI 스크립트에서 이벤트 수신하여 데미지 처리

## 📚 관련 파일
- `ZombieAI.cs`: 좀비 AI 스크립트
- `Assets/Controller/controller-zombie-A.controller`: Animator Controller
- `Assets/Animation/Not So Scary Zombie Pack/`: 애니메이션 파일

## ✅ 체크리스트

애니메이션 설정이 완료되었는지 확인:

- [ ] Animator Controller에 파라미터 추가 완료 (Speed, IsWalking, IsRunning, IsAttacking)
- [ ] Idle, Walk, Run, Attack 상태 생성 완료
- [ ] 각 상태에 애니메이션 클립 할당 완료
- [ ] 상태 간 전환(Transition) 설정 완료
- [ ] 좀비 오브젝트의 Animator 컴포넌트에 Controller 할당 완료
- [ ] ZombieAI 스크립트의 Animator 필드 할당 완료
- [ ] Play 모드에서 테스트 완료

