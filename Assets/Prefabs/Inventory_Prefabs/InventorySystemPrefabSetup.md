# 인벤토리 시스템 프리팹 설정 가이드

## 개요
Canvas, InventoryManager, EventSystem을 하나의 프리팹으로 만들어 다른 씬에 쉽게 옮길 수 있도록 설정합니다.

## ✅ 프리팹 폴더로 옮기기 가능 여부

**네, 가능합니다!** 프리팹 폴더를 만들어서 전체를 묶어 옮기면 그대로 사용할 수 있습니다.

### 주의사항
- **같은 프로젝트 내**: 프리팹만 옮겨도 GUID 참조가 유지되어 작동합니다
- **다른 프로젝트로 옮길 때**: 모든 의존성(Database, 스크립트 등)도 함께 옮겨야 합니다

## 프리팹 구조

```
InventorySystemPrefab (최상위)
├── Inventory Manager
│   └── InventoryManager 컴포넌트 (Database 참조 포함)
├── Canvas
│   ├── Canvas 컴포넌트
│   ├── CanvasScaler 컴포넌트
│   ├── GraphicRaycaster 컴포넌트
│   └── Inventory (프리팹 인스턴스)
│       └── Inventory UI 요소들
└── EventSystem
    ├── EventSystem 컴포넌트
    └── StandaloneInputModule 컴포넌트
```

## Unity에서 프리팹 생성 방법

### 1. 새 씬에서 프리팹 생성
1. 새 씬 생성 (File > New Scene)
2. Hierarchy에서 빈 GameObject 생성 (이름: "InventorySystemPrefab")
3. 하위에 다음 오브젝트들 생성:

#### Inventory Manager
- GameObject 생성: "Inventory Manager"
- InventoryManager 스크립트 추가
- Database 필드에 ItemDatabase 할당
- Dont Destroy On Load 체크

#### Canvas
- GameObject 생성: "Canvas"
- Canvas 컴포넌트 추가 (Render Mode: Screen Space - Overlay)
- CanvasScaler 컴포넌트 추가
- GraphicRaycaster 컴포넌트 추가
- Inventory 프리팹을 자식으로 드래그

#### EventSystem
- GameObject 생성: "EventSystem"
- EventSystem 컴포넌트 추가
- StandaloneInputModule 컴포넌트 추가

### 2. 중복 방지 스크립트 추가
EventSystem이 중복되지 않도록 초기화 스크립트를 추가합니다.

### 3. 프리팹으로 저장
1. InventorySystemPrefab을 선택
2. Project 창에서 프리팹 폴더로 드래그
3. 프리팹 이름: "InventorySystemPrefab"

## 사용 방법

### 새 씬에 추가
1. Project 창에서 InventorySystemPrefab을 찾기
2. Hierarchy로 드래그 앤 드롭
3. Inventory Manager의 Database 필드 확인

### 주의사항
- **EventSystem 중복**: 씬에 EventSystem이 이미 있으면 제거하거나 중복 방지 로직 필요
- **Database 참조**: Inventory Manager의 Database 필드가 올바르게 할당되었는지 확인
- **Dont Destroy On Load**: Inventory Manager는 씬 전환 시에도 유지됨

## 프리팹 폴더 구조 (권장)

프리팹 폴더를 만들어서 모든 관련 파일을 함께 관리하는 것을 권장합니다:

```
Assets/
└── Prefabs/
    └── InventorySystem/
        ├── InventorySystemPrefab.prefab (메인 프리팹)
        ├── README.md (사용 가이드)
        └── Dependencies/ (선택사항 - 다른 프로젝트로 옮길 때)
            └── Database.asset (ItemDatabase ScriptableObject)
```

### 프리팹 폴더 생성 방법

1. **프리팹 폴더 생성**
   - Project 창에서 `Assets/Prefabs/` 폴더 생성 (없으면)
   - `InventorySystem` 폴더 생성

2. **프리팹 생성**
   - Unity에서 새 씬 생성
   - Hierarchy에 InventorySystemPrefab 구성
   - Project 창의 `Assets/Prefabs/InventorySystem/` 폴더로 드래그하여 프리팹 저장

3. **사용 방법**
   - 다른 씬에서 `Assets/Prefabs/InventorySystem/InventorySystemPrefab.prefab`을 Hierarchy로 드래그
   - 즉시 사용 가능!

## SimplePickup 사용
SimplePickup은 정적 메서드를 사용하므로 추가 설정 없이 작동합니다:
- ItemData만 할당하면 됨
- DevionInventoryBridge가 자동으로 InventoryManager를 찾음

## 다른 프로젝트로 옮기기

다른 Unity 프로젝트로 옮길 때는 다음을 함께 옮겨야 합니다:

1. **프리팹 파일**
   - `InventorySystemPrefab.prefab`

2. **의존성 파일**
   - Database ScriptableObject (ItemDatabase)
   - Inventory 프리팹 (이미 존재하는 경우)
   - 스크립트 파일들 (DevionInventoryBridge, SimplePickup 등)

3. **Devion Games Inventory System**
   - 전체 인벤토리 시스템 패키지

### Unity Package로 내보내기 (권장)

1. Assets 폴더에서 관련 파일들 선택
2. Assets > Export Package
3. 모든 의존성 포함하여 Export
4. 다른 프로젝트에서 Import Package

