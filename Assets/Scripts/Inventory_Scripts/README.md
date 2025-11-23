# 인벤토리 & 파밍 시스템 사용 가이드

## 개요
Devion Games Inventory System을 사용한 인벤토리 및 아이템 파밍 시스템입니다.

## 파일 구조
```
Assets/Scripts/Inventory_Scripts/
├── Integration/
│   └── DevionInventoryBridge.cs    # 인벤토리 연동 헬퍼
├── Loot/
│   └── SimplePickup.cs              # 아이템 파밍 스크립트
└── README.md                        # 이 문서

Assets/Prefabs/Inventory_Prefabs/
├── Canvas.prefab                    # 인벤토리 UI Canvas
├── Inventory Manager.prefab         # 인벤토리 관리자
├── EventSystem.prefab               # UI 이벤트 시스템
└── README.md                        # 프리팹 설정 가이드
```

## 빠른 시작

### 1. 인벤토리 시스템 추가 (새 씬에)

1. **프리팹 추가**
   - Project 창에서 `Assets/Prefabs/Inventory_Prefabs/` 폴더 열기
   - 다음 프리팹들을 Hierarchy로 드래그:
     - `Inventory Manager.prefab`
     - `Canvas.prefab`
     - `EventSystem.prefab` (씬에 EventSystem이 없을 경우만)

2. **Database 설정**
   - Hierarchy에서 Inventory Manager 선택
   - Inspector에서 **Database** 필드 확인
   - Database가 할당되어 있지 않으면 ItemDatabase ScriptableObject 할당

3. **완료!**
   - **I 키**를 눌러 인벤토리 열기/닫기

### 2. 아이템 파밍 설정

1. **아이템 오브젝트 생성**
   - Hierarchy에서 빈 GameObject 생성 (이름: "Health Potion" 등)
   - `Add Component` → `SimplePickup` 검색 → 추가

2. **SimplePickup 설정**
   - Inspector에서 **Item Data** 필드에 ItemData ScriptableObject 드래그
   - **Amount**: 아이템 개수 (기본값: 1)
   - **Player Tag**: "Player" (기본값, 플레이어 태그가 다르면 변경)
   - **Prompt Canvas**: F 키 프롬프트 UI (선택사항)

3. **Collider 추가** (필수)
   - `Add Component` → `Collider` 추가
   - **Is Trigger**: 체크 (필수)
   - 플레이어가 범위 안에 들어가면 F 키 프롬프트 표시

4. **완료!**
   - 플레이어가 아이템 범위 안에 들어가면 F 키 프롬프트 표시
   - **F 키**를 누르면 인벤토리에 추가되고 아이템 오브젝트가 사라짐

## 스크립트 사용법

### DevionInventoryBridge (인벤토리 연동)

```csharp
using StoreGame.Integration;
using StoreGame.Data;

// 아이템 추가
ItemData healthPotion = ...; // ItemData 참조
bool success = DevionInventoryBridge.TryAddItem(healthPotion, 1, "Inventory");

if (success)
{
    Debug.Log("아이템이 인벤토리에 추가되었습니다!");
}
```

**메서드:**
- `TryAddItem(ItemData itemData, int amount, string windowName = "Inventory")`
  - 아이템을 인벤토리에 추가
  - 반환값: 성공 여부 (bool)

### SimplePickup (아이템 파밍)

**Inspector 설정:**
- **Item Data**: 추가할 아이템의 ItemData ScriptableObject
- **Amount**: 아이템 개수

**동작 방식:**
- 플레이어가 오브젝트의 Collider 범위 안에 들어가면 "F 키" 프롬프트 표시
- **F 키**를 누르면 인벤토리에 추가되고 오브젝트가 사라짐
- `DevionInventoryBridge.TryAddItem()`을 내부적으로 사용

**커스터마이징:**
- `SimplePickup.cs`를 수정하여 파밍 조건, 애니메이션 등을 추가 가능

## 인벤토리 UI 조작

### 기본 조작
- **I 키**: 인벤토리 열기/닫기
- **마우스 드래그**: 아이템 이동
- **우클릭**: 아이템 사용/정보 확인

### 인벤토리 위치 조정
- Hierarchy에서 Canvas → Inventory 선택
- Inspector에서 RectTransform 조정
- 기본적으로 화면 중앙에 위치

## 아이템 추가 방법

### 1. ItemData ScriptableObject 생성
1. Project 창에서 우클릭
2. `Create` → `StoreGame` → `ItemData`
3. 이름 설정 (예: "Health Potion")

### 2. ItemData 설정
- **Display Name**: 아이템 이름
- **Icon**: 아이템 아이콘 스프라이트
- **Description**: 아이템 설명
- **Devion Item Template**: Devion Games Item 템플릿 할당

### 3. Devion Games Item 템플릿 설정
1. Devion Games Inventory System의 ItemDatabase 열기
2. 새 Item 생성 또는 기존 Item 수정
3. ItemData의 **Devion Item Template** 필드에 할당

## 다른 씬으로 옮기기

### 방법 1: 프리팹 사용 (권장)
1. `Assets/Prefabs/Inventory_Prefabs/` 폴더의 프리팹들을 새 씬으로 드래그
2. Inventory Manager의 Database 필드 확인

### 방법 2: 수동 설정
1. 새 씬에서:
   - Canvas 생성
   - Inventory Manager GameObject 생성 → InventoryManager 컴포넌트 추가
   - EventSystem 생성 (없으면)
2. Inventory Manager 설정:
   - Database 할당
   - Dont Destroy On Load 체크 (선택사항)

## 문제 해결

### 인벤토리가 열리지 않음
- Canvas가 있는지 확인
- EventSystem이 있는지 확인
- Inventory Manager의 Database가 할당되었는지 확인

### 아이템이 인벤토리에 추가되지 않음
- Inventory Manager가 씬에 있는지 확인
- ItemData의 Devion Item Template이 할당되었는지 확인
- 콘솔 로그 확인 (에러 메시지 확인)

### 아이템 아이콘이 보이지 않음
- ItemData의 Icon 필드에 스프라이트가 할당되었는지 확인
- Devion Item Template의 Icon도 확인

### "InventoryManager를 찾을 수 없습니다" 오류
- 씬에 Inventory Manager가 있는지 확인
- Inventory Manager GameObject가 활성화되어 있는지 확인

## 고급 사용법

### 커스텀 파밍 로직
`SimplePickup.cs`를 상속하거나 수정하여:
- 특정 조건에서만 파밍 가능하게 만들기
- 파밍 애니메이션 추가
- 파밍 사운드 추가
- 파밍 확률 시스템 추가

### 인벤토리 이벤트 구독
```csharp
using DevionGames.InventorySystem;

// 아이템 추가 이벤트
ItemContainer.onAddItem += (item, slot) => {
    Debug.Log($"아이템 추가: {item.DisplayName}");
};

// 아이템 제거 이벤트
ItemContainer.onRemoveItem += (item, slot) => {
    Debug.Log($"아이템 제거: {item.DisplayName}");
};
```

## 참고 자료
- Devion Games Inventory System 공식 문서
- `Assets/Prefabs/Inventory_Prefabs/README.md` - 프리팹 설정 상세 가이드

