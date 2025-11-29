# 총알 개수 UI 설정 가이드

## 개요
화면 오른쪽 아래에 총알 개수를 표시하는 UI를 설정하는 방법입니다.

## 설정 방법

### 1. Canvas 확인
- 씬에 Canvas가 있는지 확인합니다.
- 없으면 `GameObject > UI > Canvas`로 생성합니다.

### 2. 총알 표시 UI 생성
1. Canvas를 선택합니다.
2. `우클릭 > UI > Text - TextMeshPro`를 선택합니다.
   - 처음 사용하는 경우 TMP Essentials를 임포트하라는 창이 뜹니다. "Import TMP Essentials"를 클릭합니다.
3. 생성된 TextMeshPro 오브젝트 이름을 "AmmoText"로 변경합니다.

### 3. 위치 설정 (화면 오른쪽 아래)
1. AmmoText를 선택합니다.
2. Inspector에서 RectTransform 컴포넌트를 찾습니다.
3. Anchor Presets를 설정합니다:
   - `Alt + Shift`를 누른 상태에서 오른쪽 아래 모서리 아이콘을 클릭합니다.
   - 또는 수동으로 설정:
     - Anchor Min: (1, 0)
     - Anchor Max: (1, 0)
     - Pivot: (1, 0)
4. Position 설정:
   - Pos X: -50 (오른쪽에서 50픽셀 떨어진 위치)
   - Pos Y: 50 (아래에서 50픽셀 떨어진 위치)

### 4. 텍스트 스타일 설정
1. AmmoText를 선택합니다.
2. TextMeshProUGUI 컴포넌트에서:
   - Font Size: 24~36 (원하는 크기)
   - Alignment: 오른쪽 정렬 (우측 상단 아이콘)
   - Color: 흰색 또는 노란색 (가시성을 위해)
   - Text: "30 / 30" (임시로 표시)

### 5. AmmoUI 스크립트 연결
1. Canvas를 선택합니다 (또는 별도의 빈 GameObject 생성).
2. `Add Component`를 클릭합니다.
3. "AmmoUI"를 검색하여 추가합니다.
4. AmmoUI 컴포넌트에서:
   - Ammo Text: AmmoText 오브젝트를 드래그하여 할당
   - Gun Action: 씬에 있는 GunAction 컴포넌트가 있는 오브젝트를 드래그하여 할당
     - (자동으로 찾을 수도 있지만, 명시적으로 할당하는 것이 좋습니다)

### 6. 테스트
- Play 모드를 실행하여 총알 개수가 표시되는지 확인합니다.
- 발사하면 숫자가 줄어들고, 재장전(R 키)하면 다시 채워지는지 확인합니다.

## 추가 설정 (선택사항)

### 배경 추가
1. AmmoText를 선택합니다.
2. `우클릭 > UI > Image`를 선택합니다.
3. Image를 AmmoText의 부모로 만듭니다.
4. Image의 색상을 반투명 검은색으로 설정합니다.
5. Image의 크기를 텍스트보다 약간 크게 조정합니다.

### 폰트 크기 조정
- TextMeshProUGUI의 Font Size를 조정하여 원하는 크기로 설정합니다.

## 문제 해결

### 총알 개수가 표시되지 않는 경우
1. GunAction 컴포넌트가 씬에 있는지 확인합니다.
2. AmmoUI의 Gun Action 필드가 올바르게 할당되었는지 확인합니다.
3. AmmoUI의 Ammo Text 필드가 올바르게 할당되었는지 확인합니다.
4. Console 창에서 에러 메시지를 확인합니다.

### 위치가 맞지 않는 경우
1. RectTransform의 Anchor와 Pivot 설정을 확인합니다.
2. Canvas의 Canvas Scaler 설정을 확인합니다 (Screen Space - Overlay 모드 권장).

