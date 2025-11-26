# Zombie_Random 컨트롤러 랜덤 동작 문제 해결 가이드

## 🔍 발견된 문제점

### 1. Blend Tree 설정 문제 (수정 완료)
- ✅ `m_DirectBlendParameter` 제거됨 (Blend Tree에서는 사용하지 않음)
- ✅ `m_BlendParameter: IdleType` 정상 설정됨
- ✅ Threshold 값 정상 (0, 1, 2, 3, 4, 5, 6)

### 2. 확인 필요 사항

#### WriteDefaultValues 설정
모든 상태에서 `WriteDefaultValues: 1` (true)로 설정되어 있습니다.
- **문제**: WriteDefaultValues가 true이면 기본 포즈로 고정될 수 있음
- **해결**: Unity 에디터에서 각 상태의 `Write Defaults`를 false로 변경 권장

---

## ✅ 컨트롤러 설정 확인

### 파라미터 확인 (정상)
- ✅ `IdleType` (Int) - 존재함
- ✅ `AttackType` (Int) - 존재함
- ✅ `IsWalking`, `IsRunning`, `IsAttacking` (Bool) - 존재함

### AnyState 전환 확인 (정상)
- ✅ AnyState → attack (AttackType = 0)
- ✅ AnyState → kicking (AttackType = 1)
- ✅ AnyState → punching (AttackType = 2)
- ✅ AnyState → headbutt (AttackType = 3)
- ✅ AnyState → scratch (AttackType = 4)

### Blend Tree 확인 (수정 완료)
- ✅ Parameter: `IdleType`
- ✅ Threshold: 0, 1, 2, 3, 4, 5, 6
- ✅ `m_DirectBlendParameter` 제거됨

---

## 🔧 Unity 에디터에서 추가 수정 필요

### Step 1: Write Defaults 설정 변경

1. **Animator 창**에서 `Zombie_Random.controller` 열기
2. 각 상태를 선택하고 Inspector에서 확인:
   - **idle** 상태: `Write Defaults` ❌ **체크 해제**
   - **walking** 상태: `Write Defaults` ❌ **체크 해제**
   - **running** 상태: `Write Defaults` ❌ **체크 해제**
   - **attack** 상태: `Write Defaults` ❌ **체크 해제**
   - **kicking** 상태: `Write Defaults` ❌ **체크 해제**
   - **punching** 상태: `Write Defaults` ❌ **체크 해제**
   - **headbutt** 상태: `Write Defaults` ❌ **체크 해제**
   - **scratch** 상태: `Write Defaults` ❌ **체크 해제**

### Step 2: Inspector 설정 확인

**ZombieAI 스크립트** Inspector에서:

```
=== 4단계: Idle 타입 랜덤 설정 ===
✅ Use Random Idle Types: true
✅ Available Idle Types:
   - Size: 3 이상
   - Element 0: Idle
   - Element 1: Agonizing
   - Element 2: Search

=== 5단계: 공격 타입 랜덤 설정 ===
✅ Use Attack Type Parameter: true
✅ Use Random Attacks: true
✅ Available Attack Types:
   - Size: 5
   - Element 0: Attack
   - Element 1: Kicking
   - Element 2: Punching
   - Element 3: Headbutt
   - Element 4: Scratch
```

---

## 🐛 디버그 방법

### Console 창 확인

Play 모드에서 **Console 창** (`Window > General > Console`)을 열고 확인:

#### 정상 작동 시:
```
ZombieAI: Idle 타입 랜덤 변경 - Agonizing (인덱스: 1, 사용 가능: 3개)
ZombieAI: 공격 타입 랜덤 선택 - Kicking (인덱스: 1, 사용 가능: 5개)
```

#### 문제가 있을 시:
```
ZombieAI: IdleType 파라미터 'IdleType'를 찾을 수 없습니다.
ZombieAI: 공격 타입 랜덤 선택 실패 - useAttackTypeParameter: false, ...
```

### Animator 창에서 실시간 확인

1. **Play 모드**로 전환
2. **Animator 창** 열기 (`Window > Animation > Animator`)
3. 좀비 오브젝트 선택
4. **Parameters** 탭에서 확인:
   - `IdleType` 값이 변경되는지 (5초마다)
   - `AttackType` 값이 랜덤으로 설정되는지

---

## ✅ 최종 체크리스트

### 컨트롤러 설정
- [ ] `IdleType` (Int) 파라미터 존재
- [ ] `AttackType` (Int) 파라미터 존재
- [ ] Blend Tree가 `IdleType` 파라미터 사용
- [ ] AnyState 전환이 `AttackType` 조건 사용
- [ ] 모든 상태의 `Write Defaults` = false

### Inspector 설정
- [ ] `Use Random Idle Types` = true
- [ ] `Available Idle Types` 배열에 2개 이상
- [ ] `Use Attack Type Parameter` = true
- [ ] `Use Random Attacks` = true
- [ ] `Available Attack Types` 배열에 2개 이상

### 테스트
- [ ] Play 모드에서 Idle 타입이 5초마다 변경되는지
- [ ] Play 모드에서 공격 시 다른 공격 타입이 나오는지
- [ ] Console 창에서 디버그 로그 확인

---

## 💡 빠른 해결 방법

1. **Unity 에디터에서**:
   - Animator 창 열기
   - 각 상태 선택 → `Write Defaults` ❌ 체크 해제

2. **Inspector에서**:
   - `Use Random Idle Types` ✅ 체크
   - `Use Attack Type Parameter` ✅ 체크
   - `Use Random Attacks` ✅ 체크
   - 배열에 여러 타입 추가

3. **Play 모드에서 테스트**:
   - Console 창 열기
   - 디버그 로그 확인
   - Animator 창에서 파라미터 값 확인

이제 랜덤 동작이 정상 작동할 것입니다! 🎮

