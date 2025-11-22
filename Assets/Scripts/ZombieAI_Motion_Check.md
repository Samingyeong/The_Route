# ZombieAI 애니메이션 Motion 할당 확인 가이드

로그는 정상적으로 작동하고 있지만 애니메이션이 재생되지 않는 문제입니다.

## 🎯 문제 원인

로그에서 상태는 전환되고 있지만 (`Idle` ↔ `Run`), **실제 애니메이션이 재생되지 않는** 이유는:

**각 상태(State)에 애니메이션 클립(Motion)이 할당되지 않았을 가능성이 99%입니다!**

## ✅ 확인 절차

### 1단계: Animator Controller에서 Motion 할당 확인

1. **`Assets/Controller/controller_zombie_A.controller` 더블클릭**
2. Animator 창이 열립니다
3. **각 상태를 하나씩 클릭**하여 Inspector 확인:

#### Idle 상태 확인:
1. **idle** 상태(주황색 사각형) 클릭
2. **Inspector 창**(오른쪽) 확인
3. **Motion** 필드 확인:
   - ❌ **비어있거나 "None (Motion)"이면** → **문제!**
   - ✅ **"zombie idle" 또는 클립 이름이 보이면** → 정상

#### Walking 상태 확인:
1. **walking** 상태 클릭
2. Inspector에서 **Motion** 필드 확인
3. 비어있으면 → 문제!

#### Running 상태 확인:
1. **running** 상태 클릭
2. Inspector에서 **Motion** 필드 확인
3. 비어있으면 → 문제!

#### Attack 상태 확인:
1. **attack** 상태 클릭
2. Inspector에서 **Motion** 필드 확인
3. 비어있으면 → 문제!

### 2단계: Motion 할당 방법

**가장 중요한 단계입니다!**

#### 방법 1: 드래그 & 드롭 (가장 쉬움)

1. **Project 창**에서 `Assets/Animation/Not So Scary Zombie Pack/` 폴더 열기
2. **Animator 창**에서 상태 선택 (예: **idle** 클릭)
3. **Inspector 창** 확인 (오른쪽 패널)
4. **Motion** 필드 찾기 (Inspector 중간 부분)
5. **Project 창**에서 애니메이션 클립을 **Motion 필드로 드래그**:

   **Idle 상태에 할당:**
   - Project 창에서 `zombie idle` 클립 찾기
   - 드래그하여 Inspector의 Motion 필드로 드롭
   - Motion 필드에 "zombie idle" 표시되면 성공!

   **Walking 상태에 할당:**
   - **walking** 상태 클릭
   - Project 창에서 `walking` 클립을 Motion 필드로 드래그

   **Running 상태에 할당:**
   - **running** 상태 클릭
   - Project 창에서 `zombie running` 클립을 Motion 필드로 드래그

   **Attack 상태에 할당:**
   - **attack** 상태 클릭
   - Project 창에서 `zombie attack` 클립을 Motion 필드로 드래그

#### 방법 2: 클릭해서 선택

1. 상태 선택 → Inspector에서 **Motion 필드의 원형 아이콘** 클릭
2. 나타나는 창에서 애니메이션 클립 선택
3. 적용

### 3단계: 할당 확인

각 상태를 다시 클릭하여 Motion 필드에 클립 이름이 표시되는지 확인:

- ✅ **idle** → Motion: `zombie idle`
- ✅ **walking** → Motion: `walking`
- ✅ **running** → Motion: `zombie running`
- ✅ **attack** → Motion: `zombie attack`

### 4단계: 즉시 테스트

1. **Play 모드**로 전환
2. **Animator 창** 열기 (`Window > Animation > Animator`)
3. Hierarchy에서 **zombie** 선택
4. Animator 창에서 **상태를 직접 클릭**:
   - **walking** 상태 클릭 → 걷기 애니메이션이 **즉시 재생**되어야 함!
   - **running** 상태 클릭 → 뛰기 애니메이션이 **즉시 재생**되어야 함!
   - **idle** 상태 클릭 → 대기 애니메이션이 **즉시 재생**되어야 함!

**애니메이션이 재생되면**: ✅ Motion 할당 성공!  
**애니메이션이 여전히 재생되지 않으면**: 다른 문제 (아래 확인)

### 5단계: 전환 조건 재확인 (Motion 할당 후)

Motion을 할당한 후에도 작동하지 않으면 전환 조건을 확인:

#### Idle → Walking 전환:
1. **idle → walking** 화살표 클릭
2. Inspector의 **Conditions** 확인:
   - `IsWalking` → `true` 조건이 있어야 함

#### Walking → Running 전환:
1. **walking → running** 화살표 클릭
2. Inspector의 **Conditions** 확인:
   - `IsRunning` → `true` 조건이 있어야 함

#### Running → Walking 전환:
1. **running → walking** 화살표 클릭
2. Inspector의 **Conditions** 확인:
   - `IsRunning` → `false`
   - `IsWalking` → `true` (또는 Speed 파라미터 사용)

### 6단계: 파라미터 이름 확인

로그에서 `IsRunning`은 `True`로 잘 설정되고 있습니다.

ZombieAI 스크립트의 파라미터 이름과 Animator Controller의 파라미터 이름이 일치하는지 확인:

1. **Animator 창**에서 **Parameters** 탭 클릭
2. 다음 파라미터들이 있는지 확인:
   - `IsWalking` (Bool) ✅
   - `IsRunning` (Bool) ✅
   - `IsAttacking` (Bool) ✅
   - `Speed` (Float) ✅

3. 이름이 정확히 일치하는지 확인 (대소문자 구분)

## 🔍 추가 확인 사항

### 애니메이션 클립 자체가 재생되는지 확인

1. **Project 창**에서 `zombie idle` 클립 더블클릭
2. 하단에 **Animation Preview** 창이 열림
3. **Play 버튼** 클릭하여 애니메이션이 재생되는지 확인

**재생되지 않으면**: 애니메이션 임포트 설정 문제

### Animator 컴포넌트 활성화 확인

1. Hierarchy에서 **zombie** 선택
2. Inspector에서 **Animator** 컴포넌트 확인:
   - 왼쪽 위 **체크박스**가 체크되어 있는지 확인
   - **Controller** 필드에 `controller_zombie_A`가 할당되어 있는지 확인

## 📝 체크리스트

다음을 모두 확인하세요:

- [ ] **idle** 상태의 Motion 필드에 `zombie idle` 할당됨 ✅
- [ ] **walking** 상태의 Motion 필드에 `walking` 할당됨 ✅
- [ ] **running** 상태의 Motion 필드에 `zombie running` 할당됨 ✅
- [ ] **attack** 상태의 Motion 필드에 `zombie attack` 할당됨 ✅
- [ ] Play 모드에서 상태를 직접 클릭했을 때 애니메이션이 즉시 재생됨
- [ ] Animator 컴포넌트가 활성화되어 있음

## ⚠️ 중요

**Motion 필드가 비어있으면 애니메이션이 절대 재생되지 않습니다!**

상태 다이어그램(전환 화살표)은 정상이어도, 각 상태에 실제 애니메이션 클립이 할당되어 있지 않으면 T-pose만 보입니다.

## 🎯 핵심 해결 방법

1. **Animator Controller 열기**
2. **각 상태를 클릭하여 Inspector 확인**
3. **Motion 필드가 비어있으면 → Project 창에서 애니메이션 클립 드래그 & 드롭**
4. **모든 상태에 Motion 할당**
5. **Play 모드에서 즉시 테스트**

이렇게 하면 애니메이션이 재생됩니다!

