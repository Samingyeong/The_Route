# ZombieAI 애니메이션 문제 빠른 해결 방법

Console의 에러들은 Unity 에디터 내부 에러입니다. 실제 게임 실행과는 무관할 수 있지만, 다음을 확인하세요.

## 🔧 즉시 확인 사항

### 1. Console 창 에러 정리 및 확인
1. Console 창에서 **Clear** 버튼 클릭하여 에러 정리
2. **Play 모드로 전환**
3. Console에서 **ZombieAI 관련 에러** 확인:
   - "ZombieAI: Player를 찾았습니다" 메시지 확인
   - "ZombieAI 상태: ..." 디버그 메시지 확인 (5초마다)
   - 빨간색 에러 메시지가 있는지 확인

### 2. 각 상태에 Motion 할당 확인 (가장 중요!)

**이게 가장 흔한 문제입니다!**

1. `Assets/Controller/controller_zombie_A.controller` 더블클릭
2. Animator 창이 열립니다
3. **각 상태를 하나씩 클릭**하여 Inspector 확인:

#### Idle 상태 확인:
1. **idle** 상태(주황색 사각형) 클릭
2. **Inspector 창**(오른쪽) 확인
3. **Motion** 필드 확인:
   - 비어있거나 "None (Motion)"이면 → **문제!**
   - 해결: Project 창에서 `zombie idle` 클립을 Motion 필드로 **드래그 & 드롭**

#### Walking 상태 확인:
1. **walking** 상태 클릭
2. Inspector에서 **Motion** 필드 확인
3. 비어있으면 → `walking` 클립을 드래그 & 드롭

#### Running 상태 확인:
1. **running** 상태 클릭
2. Inspector에서 **Motion** 필드 확인
3. 비어있으면 → `zombie running` 클립을 드래그 & 드롭

#### Attack 상태 확인:
1. **attack** 상태 클릭
2. Inspector에서 **Motion** 필드 확인
3. 비어있으면 → `zombie attack` 클립을 드래그 & 드롭

### 3. Motion 할당하는 방법 (세부 단계)

#### 방법 1: 드래그 & 드롭
1. **Project 창**에서 `Assets/Animation/Not So Scary Zombie Pack/` 폴더 열기
2. **Animator 창**에서 상태 선택 (예: idle)
3. **Inspector**에서 **Motion** 필드 확인 (텍스트 입력란)
4. **Project 창**에서 애니메이션 클립(예: `zombie idle`)을 **Motion 필드로 드래그**
5. Motion 필드에 클립 이름이 표시되면 성공!

#### 방법 2: 클릭해서 선택
1. 상태 선택 → Inspector에서 Motion 필드의 **원형 아이콘** 클릭
2. 나타나는 창에서 애니메이션 클립 선택
3. 적용

### 4. Play 모드에서 확인

1. **Play 모드**로 전환
2. **Animator 창** 열기 (`Window > Animation > Animator`)
3. Hierarchy에서 **zombie** 오브젝트 선택
4. Animator 창에서 확인:
   - 현재 활성화된 상태가 **주황색**으로 표시됨
   - 플레이어에게 가까워지면 **idle → walking → running**으로 전환되는지 확인

### 5. 수동 테스트 (애니메이션 클립이 재생되는지 확인)

1. **Play 모드**로 전환
2. **Animator 창** 열기
3. Hierarchy에서 **zombie** 선택
4. **Animator 창**에서 상태를 직접 클릭:
   - **walking** 상태 클릭 → 걷기 애니메이션이 재생되는지 확인
   - **running** 상태 클릭 → 뛰기 애니메이션이 재생되는지 확인
   - **attack** 상태 클릭 → 공격 애니메이션이 재생되는지 확인

**애니메이션이 재생되면**: Transition 문제 (전환 조건 재설정)  
**애니메이션이 재생되지 않으면**: Motion 할당 문제 또는 애니메이션 클립 문제

### 6. 애니메이션 클립 자체 확인

Project 창에서 애니메이션 클립을 더블클릭하여 Preview 창에서 재생되는지 확인:

1. **Project 창**에서 `zombie idle` 클립 더블클릭
2. 하단에 **Animation Preview** 창이 열림
3. **Play 버튼** 클릭하여 애니메이션이 재생되는지 확인

**재생되지 않으면**: 애니메이션 임포트 설정 문제

## 🎯 체크리스트

다음을 모두 확인하세요:

- [ ] **idle** 상태의 Motion 필드에 `zombie idle` 할당됨 ✅
- [ ] **walking** 상태의 Motion 필드에 `walking` 할당됨 ✅
- [ ] **running** 상태의 Motion 필드에 `zombie running` 할당됨 ✅
- [ ] **attack** 상태의 Motion 필드에 `zombie attack` 할당됨 ✅
- [ ] Play 모드에서 Animator 창의 상태가 전환되는지 확인됨
- [ ] Console에 ZombieAI 관련 에러가 없음
- [ ] Animator 컴포넌트가 활성화되어 있음 (Inspector에서 체크박스 체크)

## 💡 가장 흔한 문제: Motion 필드가 비어있음

**확인 방법:**
- Animator 창에서 각 상태를 클릭
- Inspector 창의 Motion 필드 확인
- 비어있으면 → 애니메이션 클립 드래그 & 드롭

**Motion 필드가 비어있으면 애니메이션이 절대 재생되지 않습니다!**

## 🔍 디버그 정보 확인

ZombieAI 스크립트에 디버그 로그를 추가했습니다. Play 모드에서:

1. Console 창 열기 (`Window > General > Console`)
2. 5초마다 다음과 같은 메시지가 나타나야 함:
   ```
   ZombieAI 상태: Walk, 거리: 8.50m, IsWalking: True, IsRunning: False
   ```

이 메시지가 보이면 스크립트는 정상 작동 중입니다.  
이 메시지가 안 보이면 스크립트가 실행되지 않는 것입니다.

## ❓ 여전히 작동하지 않으면

1. **Console 창의 모든 메시지 스크린샷** 보내주세요
2. **Animator 창 스크린샷** (각 상태를 클릭했을 때 Inspector의 Motion 필드 포함)
3. **Play 모드에서 Animator 창의 상태 전환 스크린샷**

이 정보를 주시면 더 정확히 도와드릴 수 있습니다!

