# ZombieAI 애니메이션 문제 해결 가이드

좀비가 T-pose만 하고 애니메이션이 재생되지 않는 문제를 해결하는 방법입니다.

## 🔍 문제 진단 체크리스트

### 1단계: Animator Controller 기본 설정 확인

#### ✅ Animator Controller에 애니메이션 클립 할당 확인
1. `Assets/Controller/controller_zombie_A.controller` 더블클릭하여 Animator 창 열기
2. 각 상태(State)를 클릭하여 Inspector 확인:
   - **Idle** 상태 선택 → Inspector에서 **Motion** 필드 확인
     - `zombie idle` 또는 비슷한 이름이 할당되어 있어야 함
     - Motion 필드가 비어있거나 "None (Motion)"이면 애니메이션 클립을 드래그 & 드롭
   - **Walk** 상태 선택 → Motion 필드에 `walking` 할당 확인
   - **Run** 상태 선택 → Motion 필드에 `zombie running` 할당 확인
   - **Attack** 상태 선택 → Motion 필드에 `zombie attack` 할당 확인

#### ✅ 기본 상태(Default State) 확인
1. Animator 창에서 **Idle** 상태 확인
   - Idle 상태가 주황색으로 표시되어야 함 (Entry에서 연결됨)
   - 주황색이 아니면 → Idle 상태 우클릭 → `Set as Layer Default State` 선택

#### ✅ Entry → Idle 연결 확인
1. **Entry** 노드(주황색 원형)에서 **Idle** 상태로 화살표가 연결되어 있는지 확인
2. 연결이 없으면:
   - Entry 노드 우클릭 → `Make Transition` → Idle까지 드래그
   - 화살표 선택 → Inspector에서 **Has Exit Time** 체크 → **Exit Time: 1**

### 2단계: 파라미터(Parameters) 확인

#### ✅ Parameters 탭 확인
1. Animator 창 왼쪽 하단 **Parameters** 탭 클릭
2. 다음 파라미터들이 있는지 확인:
   - `Speed` (Float) ✅
   - `IsWalking` (Bool) ✅
   - `IsRunning` (Bool) ✅
   - `IsAttacking` (Bool) ✅

3. **파라미터가 없으면 추가**:
   - `+` 버튼 클릭 → 타입 선택 → 이름 입력

### 3단계: 애니메이션 임포트 설정 확인

#### ✅ .fbx 파일 임포트 설정 확인
1. Project 창에서 `Assets/Animation/Not So Scary Zombie Pack/` 폴더 열기
2. 각 애니메이션 .fbx 파일 선택 (예: `zombie idle.fbx`)
3. Inspector에서 확인:

**Rig 탭:**
- **Animation Type**: `Humanoid` 또는 `Generic` 설정 확인
- **Avatar Definition**: `Create From This Model` 선택 확인

**Animation 탭:**
- **Import Animation**: ✅ 체크되어 있는지 확인
- **Root Transform Rotation**: `Bake Into Pose` 체크
- **Root Transform Position (Y)**: `Bake Into Pose` 체크
- **Root Transform Position (XZ)**: `Bake Into Pose` 체크

**Apply** 버튼 클릭하여 저장

4. 같은 방식으로 모든 애니메이션 파일 확인:
   - `walking.fbx`
   - `zombie running.fbx`
   - `zombie attack.fbx`

### 4단계: 좀비 오브젝트 설정 확인

#### ✅ Animator 컴포넌트 확인
이미지에서 확인된 설정:
- **Controller**: `controller_zombie_A` ✅ (할당됨)
- **Avatar**: `BotAvatar` ✅ (할당됨)
- **Apply Root Motion**: 체크됨 ✅

#### ✅ ZombieAI 스크립트 확인
1. Hierarchy에서 `zombie` 오브젝트 선택
2. Inspector에서 **Zombie AI (Script)** 컴포넌트 확인:
   - **Animator**: Animator 컴포넌트가 할당되어 있는지 확인
   - **Player Target**: Player 오브젝트가 할당되어 있는지 확인
   - **Nav Agent**: NavMeshAgent가 할당되어 있는지 확인

### 5단계: Play 모드에서 실시간 확인

#### ✅ Animator 창에서 상태 확인
1. **Play 모드**로 전환
2. Animator 창을 열어두기 (`Window > Animation > Animator`)
3. `zombie` 오브젝트 선택
4. Animator 창에서 확인:
   - 현재 활성화된 상태가 **주황색**으로 표시됨
   - Idle → Walk → Run 상태로 전환되는지 확인

#### ✅ Parameters 값 확인
1. Play 모드에서 Animator 창의 **Parameters** 탭 확인
2. 실시간으로 값이 변하는지 확인:
   - 좀비가 멀리 있으면: `IsWalking = false`, `IsRunning = false`
   - 플레이어에 가까워지면: `IsWalking = true`
   - 더 가까워지면: `IsRunning = true`

**값이 변하지 않으면** → ZombieAI 스크립트가 파라미터를 설정하지 못하는 것

#### ✅ Console 창 확인
1. `Window > General > Console` 열기
2. 에러 메시지 확인:
   - "ZombieAI: Player를 찾았습니다" 메시지가 있는지 확인
   - 애니메이션 관련 에러가 있는지 확인

## 🔧 문제별 해결 방법

### 문제 1: 애니메이션이 재생되지 않음 (T-pose만 보임)

**원인**: 기본 상태가 설정되지 않았거나 애니메이션 클립이 할당되지 않음

**해결**:
1. Animator Controller 열기
2. **Idle** 상태 우클릭 → `Set as Layer Default State` 선택
3. Idle 상태 선택 → Inspector에서 Motion 필드에 `zombie idle` 드래그 & 드롭
4. Play 모드에서 확인

### 문제 2: Parameters가 변하지 않음

**원인**: ZombieAI 스크립트가 Animator를 찾지 못하거나 파라미터 이름이 일치하지 않음

**해결**:
1. Hierarchy에서 `zombie` 오브젝트 선택
2. Inspector에서 **Zombie AI (Script)** 확인:
   - **Animator** 필드에 Animator 컴포넌트 수동으로 할당
   - **Anim Param Is Walking**: `IsWalking` 확인 (기본값과 일치하는지)
   - **Anim Param Is Running**: `IsRunning` 확인
   - **Anim Param Is Attacking**: `IsAttacking` 확인

3. Console 창에서 에러 메시지 확인

### 문제 3: Entry 상태에서 멈춤

**원인**: Entry → Idle 전환이 제대로 설정되지 않음

**해결**:
1. Animator Controller 열기
2. Entry 노드에서 Idle로 화살표가 연결되어 있는지 확인
3. 없으면:
   - Entry 우클릭 → `Make Transition` → Idle로 드래그
   - 화살표 선택 → Inspector에서 **Has Exit Time** 체크 → **Exit Time: 1**

### 문제 4: 애니메이션이 이상하게 재생됨

**원인**: 애니메이션 임포트 설정 문제

**해결**:
1. Project 창에서 `.fbx` 파일 선택
2. Inspector에서 **Animation** 탭 확인
3. **Loop Time** 체크 확인 (idle, walk, run 애니메이션)
4. **Root Transform** 설정 확인:
   - **Root Transform Rotation**: `Bake Into Pose` 체크
   - **Root Transform Position (Y)**: `Bake Into Pose` 체크

### 문제 5: 플레이어를 찾지 못함

**원인**: Player Target이 할당되지 않음

**해결**:
1. Hierarchy에서 `zombie` 오브젝트 선택
2. Inspector에서 **Zombie AI (Script)** 확인
3. **Player Target** 필드에 Player 오브젝트 드래그 & 드롭
4. 또는 Player 오브젝트의 Tag를 "Player"로 설정

## 🧪 디버깅 테스트

### 테스트 1: 수동으로 애니메이션 재생 확인
1. Play 모드로 전환
2. Hierarchy에서 `zombie` 오브젝트 선택
3. Inspector에서 **Animator** 컴포넌트 찾기
4. **Animator 창**에서 상태를 직접 클릭:
   - Walk 상태 클릭 → 걷기 애니메이션이 재생되는지 확인
   - Run 상태 클릭 → 뛰기 애니메이션이 재생되는지 확인

**애니메이션이 재생되면** → 전환(Transition) 설정 문제
**애니메이션이 재생되지 않으면** → 애니메이션 클립 할당 문제

### 테스트 2: Parameters 수동 테스트
1. Play 모드로 전환
2. Animator 창의 **Parameters** 탭 열기
3. `IsWalking` 옆의 체크박스를 수동으로 체크/해제:
   - 체크 → Walk 상태로 전환되는지 확인
   - 해제 → Idle 상태로 돌아가는지 확인

**전환이 되면** → ZombieAI 스크립트 문제
**전환이 안 되면** → Transition 설정 문제

### 테스트 3: Console 에러 확인
1. `Window > General > Console` 열기
2. Play 모드로 전환
3. 에러 메시지 확인:
   - "Animator.SetBool() called on inactive Animator"
     → Animator 컴포넌트가 비활성화되어 있음
   - "Animator parameter 'IsWalking' does not exist"
     → 파라미터 이름이 일치하지 않음

## ✅ 최종 체크리스트

다음을 모두 확인하세요:

- [ ] Animator Controller에 모든 상태에 애니메이션 클립 할당됨
- [ ] Idle 상태가 기본 상태(주황색)로 설정됨
- [ ] Entry → Idle 전환이 연결됨
- [ ] Parameters 탭에 모든 파라미터 추가됨 (IsWalking, IsRunning, IsAttacking, Speed)
- [ ] .fbx 파일의 Animation 탭에서 Import Animation 체크됨
- [ ] 좀비 오브젝트의 Animator 컴포넌트에 Controller 할당됨
- [ ] ZombieAI 스크립트의 Animator 필드에 Animator 컴포넌트 할당됨
- [ ] Player Target이 할당됨
- [ ] Play 모드에서 Animator 창의 상태가 전환되는지 확인됨

## 💡 빠른 해결 방법 (초기화 후 재설정)

위 방법들이 작동하지 않으면:

1. **새 Animator Controller 생성**:
   - Project 창에서 우클릭 → `Create > Animator Controller`
   - 이름: `controller_zombie_test`

2. **기본 설정부터 다시**:
   - Parameters 추가
   - Idle, Walk, Run, Attack 상태 생성
   - 각 상태에 애니메이션 클립 할당
   - 전환 설정

3. **좀비 오브젝트에 새 Controller 할당**:
   - Hierarchy에서 `zombie` 선택
   - Inspector에서 Animator 컴포넌트의 Controller 필드에 새 Controller 할당

## 📞 추가 도움

여전히 문제가 해결되지 않으면 다음 정보를 확인하세요:
1. Console 창의 에러 메시지
2. Animator 창 스크린샷 (Parameters 탭, 상태 다이어그램)
3. ZombieAI 스크립트의 Inspector 설정 스크린샷

