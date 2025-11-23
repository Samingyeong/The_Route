# 체력바 시스템 사용 가이드

## 개요
Unity 기본 UI Slider를 사용한 간단하고 안정적인 체력바 시스템입니다.

## 파일 구조
```
Assets/Scripts/HealthBar_Scripts/
├── HealthSystem.cs          # 체력 관리 시스템 (필수)
├── SimpleHealthBar.cs       # 체력바 UI 스크립트
└── README.md                # 이 문서

Assets/Prefabs/HealthBar_Prefabs/
└── SimpleHealthBar.prefab   # 체력바 프리팹
```

## 빠른 시작

### 1. Player에 HealthSystem 추가
1. Hierarchy에서 Player GameObject 선택
2. `Add Component` → `HealthSystem` 검색 → 추가
3. Inspector에서 설정:
   - **Max Health**: 최대 체력 (기본값: 100)

### 2. 체력바 UI 추가
1. Hierarchy에서 Canvas 선택 (없으면 생성)
2. Project 창에서 `Assets/Prefabs/HealthBar_Prefabs/SimpleHealthBar.prefab` 찾기
3. Canvas로 드래그 앤 드롭
4. Inspector에서 SimpleHealthBar 컴포넌트 확인:
   - **Health System**: Player의 HealthSystem 컴포넌트 드래그
   - **Show On Start**: 체크 (시작 시 표시)
   - **Full Health Color**: 초록색 (체력이 많을 때)
   - **Low Health Color**: 빨간색 (체력이 적을 때)

### 3. 완료!
체력바가 화면 왼쪽 상단에 표시됩니다.

## 스크립트 사용법

### HealthSystem (체력 관리)

```csharp
using StoreGame;

// HealthSystem 참조 가져오기
HealthSystem health = GetComponent<HealthSystem>();

// 데미지 주기
health.TakeDamage(10f);

// 체력 회복
health.Heal(20f);

// 이벤트 구독
health.OnHealthChanged += (current, max) => {
    Debug.Log($"체력: {current}/{max}");
};

health.OnDeath += () => {
    Debug.Log("사망!");
};
```

**주요 메서드:**
- `TakeDamage(float damage)` - 데미지 받기
- `Heal(float amount)` - 체력 회복
- `SetHealth(float health)` - 체력 직접 설정
- `SetMaxHealth(float newMaxHealth)` - 최대 체력 변경
- `RestoreFullHealth()` - 체력 완전 회복

**이벤트:**
- `OnHealthChanged` - 체력이 변경될 때 (currentHealth, maxHealth)
- `OnDeath` - 체력이 0이 되었을 때
- `OnDamageTaken` - 데미지를 받았을 때 (damageAmount)
- `OnHealed` - 체력을 회복했을 때 (healAmount)

**프로퍼티:**
- `MaxHealth` - 최대 체력
- `CurrentHealth` - 현재 체력
- `HealthPercentage` - 체력 비율 (0~1)
- `IsDead` - 사망 여부

### SimpleHealthBar (UI)

자동으로 HealthSystem을 찾아 연결하므로 대부분의 경우 추가 코드 작성이 필요 없습니다.

**Inspector 설정:**
- **Health Slider**: 자동으로 찾음 (수동 설정 가능)
- **Fill Image**: 자동으로 찾음 (수동 설정 가능)
- **Health Text**: 체력 텍스트 표시 (선택사항)
- **Health System**: Player의 HealthSystem (자동으로 찾음)
- **Show On Start**: 시작 시 표시 여부
- **Full Health Color**: 체력이 많을 때 색상
- **Low Health Color**: 체력이 적을 때 색상

## 위치 조정

체력바 위치를 변경하려면:
1. Hierarchy에서 SimpleHealthBar 선택
2. Inspector에서 RectTransform 확인
3. **Pos X**, **Pos Y** 값 조정
   - 현재 위치: X=200, Y=-30 (왼쪽 상단)

## 문제 해결

### 체력바가 보이지 않음
- Canvas가 있는지 확인
- SimpleHealthBar의 **Show On Start**가 체크되어 있는지 확인
- HealthSystem이 제대로 연결되었는지 확인

### Slider를 찾을 수 없다는 오류
- 프리팹을 다시 열어서 Slider GameObject가 있는지 확인
- Inspector에서 Health Slider 필드에 Slider GameObject를 수동으로 드래그

### 체력이 업데이트되지 않음
- HealthSystem이 제대로 연결되었는지 확인
- HealthSystem의 이벤트가 제대로 발생하는지 확인 (Debug.Log 추가)

## 테스트

HealthSystem에는 테스트용 키 입력이 포함되어 있습니다:
- **H 키**: 데미지 10 받기
- **J 키**: 체력 10 회복

게임 실행 후 H, J 키를 눌러 체력바가 제대로 작동하는지 확인하세요.

