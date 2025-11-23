# 인벤토리 & 파밍 시스템

이 폴더에는 게임 캐릭터에 인벤토리와 파밍 시스템을 구현하기 위한 스크립트들이 포함되어 있습니다.

## 📁 폴더 구조

```
Sungju_Exception/
├── Scripts/
│   ├── Data/              # 아이템 데이터 정의
│   │   ├── ItemData.cs   # 아이템 데이터 ScriptableObject
│   │   ├── LootTable.cs  # 루트 테이블 ScriptableObject
│   │   └── Editor/       # 에디터 확장
│   ├── Loot/             # 파밍 시스템
│   │   ├── LootSpawner.cs      # 루트 스폰 관리
│   │   ├── WorldItemPickup.cs  # 월드 아이템 픽업 (자동)
│   │   └── SimplePickup.cs     # 간단한 픽업 (F키 상호작용)
│   ├── Integration/      # Devion Games 연동
│   │   └── DevionInventoryBridge.cs
│   └── Utility/          # 트리거/플레이어 디버깅
│       ├── TriggerReadyPlayer.cs
│       └── TriggerTester.cs
└── README.md
```

## 🚀 사용 방법

### 1. 필수 의존성 확인

이 시스템은 **Devion Games Inventory System**이 필요합니다.
- `Assets/Devion Games/Inventory System/` 폴더가 있어야 합니다.
- 씬에 `InventoryManager`가 설정되어 있어야 합니다.

### 2. 게임 캐릭터에 연결하기

#### 방법 1: LootSpawner 사용 (적 처치 후 루트 드롭)

1. 적 오브젝트 또는 적 스폰 위치에 빈 GameObject 생성
2. `LootSpawner` 컴포넌트 추가
3. Inspector에서 설정:
   - **Loot Table**: 드롭할 아이템 테이블 할당
   - **World Item Pickup Prefab**: `WorldItemPickup` Prefab 할당 (아래 참고)
   - **Current Wave**: 현재 웨이브 번호
   - **Scatter Radius**: 아이템이 흩어질 반경

4. 적 처치 시 `lootSpawner.SpawnLoot()` 호출

```csharp
// 예시: 적 처치 시
LootSpawner lootSpawner = GetComponent<LootSpawner>();
if (lootSpawner != null)
{
    lootSpawner.SpawnLoot();
}
```

#### 방법 2: WorldItemPickup Prefab 생성

1. Unity 에디터에서:
   - 빈 GameObject 생성
   - `WorldItemPickup` 컴포넌트 추가
   - `SphereCollider` 또는 `BoxCollider` 추가 → **Is Trigger** 체크
   - `Prefabs/WorldItemPickup.prefab`으로 저장

2. `LootSpawner`의 **World Item Pickup Prefab** 필드에 할당

#### 방법 3: SimplePickup 사용 (고정 아이템)

1. 씬에 아이템 오브젝트 생성
2. `SimplePickup` 컴포넌트 추가
3. Collider 추가 (Is Trigger 체크)
4. Inspector에서 설정:
   - **Item Data**: 픽업할 아이템 데이터
   - **Amount**: 개수
   - **Player Tag**: 플레이어 태그 (기본값: "Player")
   - **Prompt Canvas**: (선택) F키 프롬프트 UI

플레이어가 범위 안에서 **F 키**를 누르면 인벤토리에 추가됩니다.

### 3. 아이템 데이터 생성

1. Project 창에서 우클릭 → `Create → StoreGame → Item Data`
2. Inspector에서 설정:
   - **Display Name**: 아이템 이름
   - **Icon**: 아이템 아이콘
   - **Rarity**: 희귀도
   - **Max Stack Size**: 최대 스택 개수
   - **Devion Item Template**: Devion Games의 Item 템플릿 연결
     - 또는 **"이름으로 Devion Item 찾아서 연결"** 버튼 사용

### 4. 루트 테이블 생성

1. Project 창에서 우클릭 → `Create → StoreGame → Loot Table`
2. Inspector에서 **Entries** 리스트에 아이템 추가:
   - **Item**: 드롭할 아이템
   - **Drop Chance**: 드롭 확률 (0~1)
   - **Amount Range**: 드롭 개수 범위
   - **Scale With Wave**: 웨이브에 따라 확률/개수 조정

## 📝 주요 스크립트 설명

### ItemData
- 아이템의 기본 정보를 담는 ScriptableObject
- Devion Games Inventory System과 연동

### LootTable
- 여러 아이템의 드롭 확률과 개수를 관리
- 웨이브 시스템과 연동 가능

### LootSpawner
- 루트 테이블을 기반으로 아이템을 드롭
- 월드에 아이템을 스폰하거나 직접 인벤토리에 추가

### WorldItemPickup
- 월드에 떨어진 아이템을 자동으로 픽업
- 플레이어가 트리거에 닿으면 자동으로 인벤토리에 추가

### SimplePickup
- 고정된 위치의 아이템 픽업
- F키를 눌러 상호작용

### DevionInventoryBridge
- ItemData와 Devion Games Inventory System을 연결하는 헬퍼
- `TryAddItem()` 메서드로 아이템을 인벤토리에 추가

### TriggerReadyPlayer
- 플레이어가 트리거 조건(태그, Collider, Rigidbody)을 갖추도록 검사
- Rigidbody가 없으면 자동으로 추가하고, kinematic/중력 옵션을 제어

### TriggerTester
- 트리거 진입/이탈 로그를 출력하는 디버그 도구
- Tag, Layer 필터와 `OnTriggerStay` 로그 옵션 제공

## ⚠️ 주의사항

1. **Devion Games Inventory System 필수**: 이 시스템이 없으면 작동하지 않습니다.
2. **InventoryManager 설정**: 씬에 `InventoryManager`가 있어야 합니다.
3. **ItemData의 Devion Item Template**: 각 ItemData에 Devion Games의 Item 템플릿을 연결해야 인벤토리에 추가됩니다.
4. **WorldItemPickup Prefab**: LootSpawner를 사용하려면 Prefab을 먼저 만들어야 합니다.

## 🔧 문제 해결

### 아이템이 인벤토리에 추가되지 않을 때
1. `InventoryManager`가 씬에 있는지 확인
2. `ItemData`의 `Devion Item Template`이 설정되어 있는지 확인
3. Devion Inventory Window 이름이 올바른지 확인 (기본값: "Inventory")

### Prefab 참조가 깨질 때
- Unity 에디터에서 `.meta` 파일이 제대로 생성되었는지 확인
- 프로젝트를 다시 열어보세요

## 🧪 트리거 디버그 빠른 가이드

1. **플레이어 세팅**
   - `TriggerReadyPlayer` 컴포넌트를 플레이어 루트에 추가
   - 필요 시 Tag를 `Player`로 맞추고, 자동 추가된 Rigidbody를 확인
2. **트리거 확인**
   - 문제가 생긴 Collider에 `TriggerTester`를 붙이고 Tag/Layer 필터를 설정
   - 콘솔 로그로 `OnTriggerEnter/Exit`가 찍히는지 확인
3. **SimplePickup 전개**
   - Cube(아이템)에는 `BoxCollider (Is Trigger)` + `SimplePickup` + `TriggerTester`
   - 플레이어에는 `Collider + Rigidbody (Kinematic)` 조합이 유지되도록 합니다.


