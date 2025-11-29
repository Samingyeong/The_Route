using UnityEngine;
using StoreGame;
using DevionGames.InventorySystem;
using DevionGames.UIWidgets; // WidgetUtility 네임스페이스
using System;

public enum TutorialStep
{
    WalkRun,           // 1. 걷기/뛰기
    TakeDamage,        // 2. 데미지 받기 (순서 변경됨)
    ReloadShoot,       // 3. 총 장전/쏘기 (순서 변경됨)
    PickupBandage,     // 4. 붕대 픽업
    OpenInventory,     // 5. 인벤토리 열기
    UseBandage,        // 6. 붕대 사용
    EnterVehicle,      // 7. 차 탑승
    DriveVehicle,      // 8. 차 움직이기
    ExitVehicle        // 9. 차에서 내리기
}

public class TutorialManager : MonoBehaviour
{
    // ... (참조 및 변수 선언 부분은 동일) ...
    [Header("참조 설정")]
    [SerializeField] private GameObject player;
    [SerializeField] private HealthSystem playerHealth;
    [SerializeField] private VehicleEntryExitManager vehicleManager;
    [SerializeField] private TutorialUI tutorialUI;
    
    [Header("튜토리얼 설정")]
    [SerializeField] private TutorialStep currentStep = TutorialStep.WalkRun;
    [SerializeField] private bool isTutorialActive = true;
    
    [Header("단계별 완료 조건")]
    [SerializeField] private float walkDistanceRequired = 5f;
    [SerializeField] private float driveDistanceRequired = 10f;
    
    // 상태 추적
    private Vector3 startPosition;
    private Vector3 vehicleStartPosition;
    private bool hasWalked = false;
    private bool hasRun = false;
    private bool hasHitZombie = false;
    private bool hasTakenDamage = false;
    private bool hasPickedUpBandage = false;
    private bool hasOpenedInventory = false;
    private bool hasUsedBandage = false;
    private bool hasEnteredVehicle = false;
    private bool hasDriven = false;
    private bool hasExitedVehicle = false;
    
    // 이벤트 구독 추적
    private bool isSubscribedToHealth = false;
    
    // ... (Start, Update, InitializeStep 동일) ...

    void Start()
    {
        // ... (기존 내용 동일) ...
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        if (playerHealth == null && player != null) playerHealth = player.GetComponent<HealthSystem>();
        if (vehicleManager == null) vehicleManager = FindObjectOfType<VehicleEntryExitManager>();
        if (tutorialUI == null) tutorialUI = FindObjectOfType<TutorialUI>();
        startPosition = player != null ? player.transform.position : Vector3.zero;
        
        if (isTutorialActive) InitializeStep(currentStep);
    }
    
    void Update() { if (!isTutorialActive) return; CheckStepCompletion(); }

    void InitializeStep(TutorialStep step)
    {
        UnsubscribeFromEvents();
        ResetStepStates(); // 단계가 바뀔 때마다 상태 초기화
        SubscribeToEvents(step);
        
        if (tutorialUI != null) tutorialUI.UpdateStep((int)step + 1, GetStepDescription(step));
        Debug.Log($"[Tutorial] Step {(int)step + 1}/9 Start: {step}");
    }
    
    void SubscribeToEvents(TutorialStep step)
    {
        switch (step)
        {
            case TutorialStep.TakeDamage: // 데미지 받기 단계
            case TutorialStep.UseBandage: // 붕대 사용 단계
                if (playerHealth != null && !isSubscribedToHealth)
                {
                    playerHealth.OnDamageTaken += OnPlayerDamaged;
                    playerHealth.OnHealed += OnPlayerHealed;
                    isSubscribedToHealth = true;
                }
                break;
            // ReloadShoot 단계는 이벤트 구독 필요 없음 (ZombieBodyPart에서 직접 호출)
        }
    }
    
    // ... (UnsubscribeFromEvents, ResetStepStates 동일) ...
    void UnsubscribeFromEvents()
    {
        if (playerHealth != null && isSubscribedToHealth)
        {
            playerHealth.OnDamageTaken -= OnPlayerDamaged;
            playerHealth.OnHealed -= OnPlayerHealed;
            isSubscribedToHealth = false;
        }
    }

    void ResetStepStates()
    {
        // ... (기존과 동일하게 모든 플래그 false로) ...
        hasWalked = false; hasRun = false; hasHitZombie = false; hasTakenDamage = false;
        hasPickedUpBandage = false; hasOpenedInventory = false; hasUsedBandage = false;
        hasEnteredVehicle = false; hasDriven = false; hasExitedVehicle = false;
    }
    
    void CheckStepCompletion()
    {
        bool stepCompleted = false;
        
        switch (currentStep)
        {
            case TutorialStep.WalkRun:
                stepCompleted = CheckWalkRun();
                break;
            case TutorialStep.TakeDamage: // 순서 변경됨
                stepCompleted = hasTakenDamage;
                break;
            case TutorialStep.ReloadShoot: // 순서 변경됨
                stepCompleted = CheckReloadShoot();
                break;
            case TutorialStep.PickupBandage:
                stepCompleted = CheckBandagePickup();
                break;
            case TutorialStep.OpenInventory:
                stepCompleted = CheckInventoryOpen();
                break;
            case TutorialStep.UseBandage:
                stepCompleted = hasUsedBandage;
                break;
            case TutorialStep.EnterVehicle:
                stepCompleted = CheckVehicleEnter();
                break;
            case TutorialStep.DriveVehicle:
                stepCompleted = CheckVehicleDrive();
                break;
            case TutorialStep.ExitVehicle:
                stepCompleted = CheckVehicleExit();
                break;
        }
        
        if (stepCompleted) CompleteCurrentStep();
    }
    
    
    bool CheckWalkRun()
    {
        if (player == null) return false;
        float distanceMoved = Vector3.Distance(startPosition, player.transform.position);
        if (distanceMoved >= walkDistanceRequired) hasWalked = true;
        if (Input.GetKey(KeyCode.LeftShift) && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)) hasRun = true;
        return hasWalked && hasRun;
    }

    bool CheckReloadShoot() { return hasHitZombie; }

    bool CheckBandagePickup()
    {
        Item bandageItem = ItemContainer.GetItem("Inventory", "Bandage");
        if (bandageItem != null && ItemContainer.HasItem("Inventory", bandageItem, 1)) hasPickedUpBandage = true;
        return hasPickedUpBandage;
    }

    bool CheckInventoryOpen()
    {
        ItemContainer inventory = WidgetUtility.Find<ItemContainer>("Inventory");
        if (inventory != null && inventory.IsVisible) hasOpenedInventory = true;
        return hasOpenedInventory;
    }

    bool CheckVehicleEnter()
    {
        if (vehicleManager == null) return false;
        VehicleController vc = vehicleManager.GetComponent<VehicleController>();
        if (vc != null && vc.enabled) hasEnteredVehicle = true;
        return hasEnteredVehicle;
    }

    bool CheckVehicleDrive()
    {
        if (vehicleManager == null) return false;
        VehicleController vc = vehicleManager.GetComponent<VehicleController>();
        if (vc != null && vc.enabled)
        {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                if (vehicleStartPosition == Vector3.zero) vehicleStartPosition = vehicleManager.transform.position;
                if (Vector3.Distance(vehicleStartPosition, vehicleManager.transform.position) >= driveDistanceRequired) hasDriven = true;
            }
        }
        return hasDriven;
    }

    bool CheckVehicleExit()
    {
        if (vehicleManager == null) return false;
        
        VehicleController vc = vehicleManager.GetComponent<VehicleController>();
        
        // 수정된 로직:
        // 1. 차량 제어권이 비활성화됨 (!vc.enabled)
        // 2. 플레이어가 활성화됨 (player.activeSelf)
        // 3. (선택) 하차 로그가 찍힌 걸 보면 VehicleEntryExitManager의 isOccupied가 false일 것임.
        
        if (vc != null && !vc.enabled)
        {
            if (player != null && player.activeSelf)
            {
                hasExitedVehicle = true;
            }
        }
        return hasExitedVehicle;
    }

    // ... (CompleteCurrentStep, DelayedNextStep, CompleteTutorial 동일) ...
    void CompleteCurrentStep()
    {
        Debug.Log($"[Tutorial] Step {(int)currentStep + 1}/9 Complete!");
        
        if (tutorialUI != null)
        {
            // 체크 표시 보여주기
            tutorialUI.ShowStepComplete();
        }
        
        // 마지막 단계인지 확인
        if (currentStep == TutorialStep.ExitVehicle)
        {
            // 마지막 단계면 조금 더 기다렸다가 완료 처리 (체크 표시를 볼 시간 주기)
            StartCoroutine(DelayedCompleteTutorial());
        }
        else
        {
            // 다음 단계로 진행
            currentStep++;
            StartCoroutine(DelayedNextStep());
        }
    }

    System.Collections.IEnumerator DelayedCompleteTutorial()
    {
        yield return new WaitForSeconds(2.0f); // 2초 대기
        CompleteTutorial();
    }

    System.Collections.IEnumerator DelayedNextStep() { yield return new WaitForSeconds(1.5f); InitializeStep(currentStep); }
    
    void CompleteTutorial() { isTutorialActive = false; Debug.Log("[Tutorial] All Complete!"); if (tutorialUI != null) tutorialUI.ShowTutorialComplete(); UnsubscribeFromEvents(); }

    // 이벤트 핸들러
    void OnPlayerDamaged(float damage)
    {
        if (currentStep == TutorialStep.TakeDamage)
        {
            hasTakenDamage = true;
            Debug.Log("[Tutorial] Player took damage!");
        }
    }
    
    void OnPlayerHealed(float healAmount)
    {
        if (currentStep == TutorialStep.UseBandage)
        {
            hasUsedBandage = true;
            Debug.Log("[Tutorial] Player used bandage!");
        }
    }
    
    public void OnZombieHit(bool isHeadshot)
    {
        if (currentStep == TutorialStep.ReloadShoot)
        {
            hasHitZombie = true;
            Debug.Log(isHeadshot ? "[Tutorial] Headshot!" : "[Tutorial] Zombie Hit!");
        }
    }
    
    string GetStepDescription(TutorialStep step)
    {
        switch (step)
        {
            case TutorialStep.WalkRun:
                return "Use WASD to move & Hold Shift to run";
            case TutorialStep.TakeDamage: // 순서 변경됨
                return "Get hit by a zombie to take damage";
            case TutorialStep.ReloadShoot: // 순서 변경됨
                return "Reload & Shoot the zombie. Headshot is instant kill";
            case TutorialStep.PickupBandage:
                return "Pick up the Bandage dropped by zombie (Press F)";
            case TutorialStep.OpenInventory:
                return "Press I to open inventory & check Bandage";
            case TutorialStep.UseBandage:
                return "Use Bandage from inventory to heal (Press C)";
            case TutorialStep.EnterVehicle:
                return "Press E near the car to enter";
            case TutorialStep.DriveVehicle:
                return "Use WASD to drive the car";
            case TutorialStep.ExitVehicle:
                return "Press E to exit the car";
            default:
                return "";
        }
    }
}