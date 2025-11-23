# Unity 에디터에서 컨트롤러 수정 방법 (파일 직접 수정 없이)

## ⚠️ 중요: 파일을 직접 수정하지 마세요!

이 가이드는 Unity 에디터에서만 수정하는 방법입니다. 파일을 직접 수정하면 설정이 사라질 수 있습니다.

---

## 🎯 문제 해결: 랜덤 애니메이션이 작동하지 않음

로그는 정상적으로 나오지만 실제 애니메이션이 재생되지 않는 경우, Unity 에디터에서 직접 설정해야 합니다.

---

## 📋 Step 1: Animator 창 열기

1. **Window > Animation > Animator** 메뉴 선택
2. **Zombie_Random.controller** 파일 선택
3. 좀비 오브젝트를 선택하여 Animator 창에서 확인

---

## 🔧 Step 2: Blend Tree 설정 확인 및 수정

### 2-1. Idle 상태 선택

1. Animator 창에서 **"idle"** 상태를 클릭
2. Inspector 창에서 확인:
   - **Motion**: "Blend Tree"로 설정되어 있어야 함
   - **Write Defaults**: ❌ **체크 해제** (중요!)

### 2-2. Blend Tree 열기

1. **"idle"** 상태를 더블클릭하거나
2. Inspector에서 **"Blend Tree"**를 클릭하여 Blend Tree 창으로 이동

### 2-3. Blend Tree 타입 변경

1. Blend Tree 창 상단에서 **"Blend Type"** 확인
2. 드롭다운에서 **"Direct"** 선택
   - 현재 "Simple 1D"로 되어 있을 수 있음
   - **"Direct"**로 변경해야 Int 파라미터 사용 가능

### 2-4. Blend Parameter 설정

1. Blend Tree 창에서 **"Parameters"** 섹션 확인
2. **"Blend Parameter"** 드롭다운에서 **"IdleType"** 선택

### 2-5. 각 모션의 Threshold 확인

Blend Tree 창에서 각 모션의 **Threshold** 값이 다음과 같이 설정되어 있어야 합니다:

| 모션 (애니메이션) | Threshold 값 | IdleType 인덱스 |
|-----------------|-------------|----------------|
| idle | **0** | 0 |
| agonizing | **1** | 1 |
| turn (Search) | **2** | 2 |
| bite | **3** | 3 |
| reaction hit | **4** | 4 |
| stand up | **5** | 5 |
| stumbling | **6** | 6 |

**확인 방법**:
1. 각 모션을 클릭
2. Inspector에서 **Threshold** 값 확인
3. 위 표와 일치하지 않으면 수정

### 2-6. Direct Blend Parameter 설정 (Direct 타입일 때만)

Direct 타입을 사용할 때는 각 모션에 **Direct Blend Parameter**를 설정해야 합니다:

1. 각 모션을 하나씩 선택
2. Inspector에서 **"Direct Blend Parameter"** 드롭다운 확인
3. **"IdleType"**으로 설정되어 있는지 확인
4. 없으면 **"IdleType"** 선택

---

## ⚙️ Step 3: AnyState 전환 확인

### 3-1. AnyState 전환 확인

1. Animator 창으로 돌아가기
2. **"AnyState"** 노드를 확인
3. 다음 전환들이 있어야 함:
   - AnyState → attack (AttackType = 0)
   - AnyState → kicking (AttackType = 1)
   - AnyState → punching (AttackType = 2)
   - AnyState → headbutt (AttackType = 3)
   - AnyState → scratch (AttackType = 4)

### 3-2. 전환 조건 확인

각 전환을 클릭하고 Inspector에서 확인:

**조건 1**:
- **Parameter**: `IsAttacking`
- **Condition**: `true` (체크됨)

**조건 2**:
- **Parameter**: `AttackType`
- **Condition**: `Equals`
- **Value**: 각 공격 타입에 맞는 값 (0, 1, 2, 3, 4)

**중요 설정**:
- **Has Exit Time**: ❌ **체크 해제** (즉시 전환)
- **Transition Duration**: `0.1` (빠른 전환)

---

## 🎨 Step 4: Write Defaults 설정

모든 상태에서 **Write Defaults**를 체크 해제해야 합니다:

1. 각 상태를 하나씩 선택:
   - idle
   - walking
   - running
   - attack
   - kicking
   - punching
   - headbutt
   - scratch

2. Inspector에서 **Write Defaults** ❌ **체크 해제**

---

## ✅ Step 5: 파라미터 확인

**Parameters** 탭에서 다음 파라미터들이 있는지 확인:

- ✅ `IsWalking` (Bool)
- ✅ `IsRunning` (Bool)
- ✅ `IsAttacking` (Bool)
- ✅ `AttackType` (Int)
- ✅ `IdleType` (Int)
- ✅ `Speed` (Float)

---

## 🧪 Step 6: 테스트

1. **Play 모드**로 전환
2. **Animator 창** 열기 (Play 모드에서도 확인 가능)
3. 좀비 오브젝트 선택
4. **Parameters** 탭에서 확인:
   - `IdleType` 값이 변경되는지 (5초마다)
   - `AttackType` 값이 랜덤으로 설정되는지
5. **States** 탭에서 현재 재생 중인 애니메이션 확인

---

## 🐛 문제 해결 체크리스트

### 랜덤 애니메이션이 작동하지 않는 경우:

- [ ] Blend Tree 타입이 **"Direct"**인지 확인
- [ ] Blend Parameter가 **"IdleType"**인지 확인
- [ ] 각 모션의 **Threshold** 값이 0, 1, 2, 3, 4, 5, 6인지 확인
- [ ] Direct 타입일 때 각 모션의 **Direct Blend Parameter**가 **"IdleType"**인지 확인
- [ ] **Write Defaults**가 모든 상태에서 체크 해제되어 있는지 확인
- [ ] `IdleType` 파라미터가 **Int** 타입인지 확인
- [ ] `AttackType` 파라미터가 **Int** 타입인지 확인
- [ ] AnyState 전환의 **Has Exit Time**이 체크 해제되어 있는지 확인

### Idle 상태에서 땅에 박히는 경우:

- [ ] 스크립트의 `OnAnimatorMove()`가 수정되었는지 확인
- [ ] 초기 Y 위치가 제대로 유지되는지 확인
- [ ] NavMeshAgent의 `updatePosition`과 `updateRotation`이 false인지 확인

---

## 💡 추가 팁

### Direct Blend Tree 작동 원리

Direct Blend Tree는 Int 파라미터 값을 사용하여 **정확히 일치하는 Threshold 값을 가진 모션**을 재생합니다.

예를 들어:
- `IdleType = 0` → Threshold가 0인 모션 재생 (idle)
- `IdleType = 1` → Threshold가 1인 모션 재생 (agonizing)
- `IdleType = 2` → Threshold가 2인 모션 재생 (turn/Search)

따라서 Threshold 값이 파라미터 값과 **정확히 일치**해야 합니다!

### Simple 1D vs Direct

- **Simple 1D**: Float 파라미터 사용, Threshold 값 사이를 블렌딩
- **Direct**: Int 파라미터 사용, Threshold 값과 정확히 일치하는 모션만 재생

랜덤 선택을 위해서는 **Direct** 타입이 필요합니다!

---

이제 Unity 에디터에서 위 단계를 따라 설정하면 랜덤 애니메이션이 정상 작동할 것입니다! 🎮

**중요**: 파일을 직접 수정하지 말고, Unity 에디터에서만 수정하세요!

