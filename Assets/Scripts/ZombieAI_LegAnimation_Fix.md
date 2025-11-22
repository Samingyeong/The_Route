# 좀비 다리 애니메이션 문제 해결 가이드

달릴 때 다리가 안 움직이고 머리/몸통만 움직이는 문제를 해결하는 방법입니다.

## 🔍 문제 원인

다리가 안 움직이는 주요 원인:
1. **애니메이션 임포트 설정 문제** (Root Transform 설정)
2. **Avatar 설정 문제** (Humanoid 설정 불일치)
3. **애니메이션 클립 자체 문제** (다리 움직임이 없음)
4. **Apply Root Motion 설정 문제**

## 🔧 해결 방법

### 1단계: 애니메이션 클립 확인

먼저 애니메이션 클립 자체에 다리 움직임이 있는지 확인:

1. **Project 창**에서 `Assets/Animation/Not So Scary Zombie Pack/zombie running.fbx` 선택
2. Inspector에서 **Animation** 탭 클릭
3. 하단의 **Preview** 창에서 애니메이션 재생:
   - Play 버튼 클릭
   - **다리가 움직이는지 확인**
   - 다리가 안 움직이면 → 애니메이션 클립 문제

4. **walking.fbx**도 같은 방식으로 확인

### 2단계: 애니메이션 임포트 설정 수정

애니메이션 임포트 설정이 잘못되어 있으면 다리가 안 움직일 수 있습니다.

#### zombie running.fbx 설정:

1. **Project 창**에서 `zombie running.fbx` 선택
2. Inspector에서 **Rig** 탭:
   - **Animation Type**: `Humanoid` 선택 (또는 `Generic`)
   - **Avatar Definition**: `Create From This Model` 선택
   - **Apply** 버튼 클릭

3. Inspector에서 **Animation** 탭:
   - **Import Animation**: ✅ 체크
   - **Root Transform Rotation**:
     - ❌ **Bake Into Pose** 체크 해제 (체크 제거)
     - 또는 `Bake Into Pose`를 체크하고 **Offset** 설정 확인
   - **Root Transform Position (Y)**:
     - ❌ **Bake Into Pose** 체크 해제
     - 또는 `Bake Into Pose` 체크
   - **Root Transform Position (XZ)**:
     - ❌ **Bake Into Pose** 체크 해제 (다리 움직임에 중요!)
   - **Loop Time**: ✅ 체크 (달리기/걷기는 반복)
   - **Loop Pose**: ✅ 체크

4. **Apply** 버튼 클릭

#### walking.fbx도 같은 설정 적용:

1. **walking.fbx** 선택
2. 같은 설정 적용 (특히 Root Transform Position (XZ) Bake Into Pose 해제)

### 3단계: Avatar 설정 확인

Humanoid 타입을 사용하는 경우 Avatar 설정 확인:

1. **Project 창**에서 `X Bot.fbx` (또는 좀비 모델) 선택
2. Inspector에서 **Rig** 탭:
   - **Animation Type**: `Humanoid` 확인
   - **Avatar Definition**: `Create From This Model` 확인
3. **Configure** 버튼 클릭 (Avatar 설정 창 열림)
4. **Mapping** 탭에서:
   - **다리 본(Bone) 매핑 확인**:
     - LeftUpperLeg, LeftLowerLeg, LeftFoot
     - RightUpperLeg, RightLowerLeg, RightFoot
   - 본이 매핑되지 않으면 자동으로 매핑
5. **Apply** 버튼 클릭

### 4단계: Animator 컴포넌트 설정 확인

1. Hierarchy에서 **zombie** 오브젝트 선택
2. Inspector에서 **Animator** 컴포넌트 확인:
   - **Apply Root Motion**: 
     - ✅ 체크되어 있으면 → **체크 해제** 시도 (NavMeshAgent와 충돌할 수 있음)
     - ❌ 체크 해제되어 있으면 → 그대로 유지
   - **Avatar**: `BotAvatar` (또는 해당 Avatar) 할당 확인

### 5단계: 애니메이션 클립 미리보기

각 애니메이션 클립을 미리보기로 확인:

1. **zombie running.fbx** 더블클릭
2. 하단에 **Animation Preview** 창 열림
3. **Play** 버튼 클릭
4. 다리가 움직이는지 확인:
   - ✅ **다리가 움직임** → 애니메이션 클립은 정상, 설정 문제
   - ❌ **다리가 안 움직임** → 애니메이션 클립 문제 또는 모델 문제

### 6단계: Write Defaults 설정 확인

Animator Controller에서 각 상태의 Write Defaults 설정 확인:

1. `Assets/Controller/controller_zombie_A.controller` 더블클릭
2. **running** 상태 선택
3. Inspector에서:
   - **Write Defaults**: ❌ **체크 해제** (권장)
     - 체크되어 있으면 기본 포즈로 고정되어 다리가 안 움직일 수 있음

4. **walking** 상태도 같은 설정

### 7단계: 애니메이션 속도 확인

애니메이션이 너무 느리게 재생되면 다리 움직임이 안 보일 수 있음:

1. Animator Controller에서 **running** 상태 선택
2. Inspector에서 **Speed** 필드 확인:
   - 기본값: `1`
   - 너무 낮으면 (예: 0.1) → `1`로 설정
   - 너무 높으면 (예: 10) → `1`로 설정

## 🎯 가장 흔한 원인

### 1. Root Transform Position (XZ) Bake Into Pose 체크

**문제**: 이 옵션이 체크되어 있으면 다리 움직임이 제한될 수 있음

**해결**:
- 애니메이션 .fbx 파일 선택
- Inspector → Animation 탭
- **Root Transform Position (XZ)**: **Bake Into Pose** ❌ 체크 해제
- **Apply**

### 2. Write Defaults 체크

**문제**: Write Defaults가 체크되어 있으면 기본 포즈로 고정됨

**해결**:
- Animator Controller 열기
- running, walking 상태 선택
- Inspector에서 **Write Defaults** ❌ 체크 해제

### 3. Avatar 매핑 문제

**문제**: Humanoid Avatar에서 다리 본이 제대로 매핑되지 않음

**해결**:
- 모델 .fbx 파일 선택
- Inspector → Rig 탭 → **Configure** 버튼
- 다리 본이 제대로 매핑되었는지 확인

## ✅ 체크리스트

- [ ] 애니메이션 클립 Preview에서 다리 움직임 확인됨
- [ ] Root Transform Position (XZ) Bake Into Pose 체크 해제됨
- [ ] Write Defaults 체크 해제됨 (running, walking 상태)
- [ ] Avatar 매핑이 올바름 (다리 본 매핑됨)
- [ ] 애니메이션 Speed가 1로 설정됨
- [ ] Loop Time 체크됨 (running, walking)

## 💡 빠른 해결

**가장 빠른 방법:**

1. **zombie running.fbx** 선택
2. Inspector → Animation 탭
3. **Root Transform Position (XZ)** → **Bake Into Pose** ❌ 체크 해제
4. **Apply** 버튼 클릭
5. **walking.fbx**도 같은 설정
6. Play 모드에서 테스트

이렇게 하면 대부분 해결됩니다!

