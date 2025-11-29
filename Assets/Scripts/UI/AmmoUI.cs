using UnityEngine;
using TMPro;

public class AmmoUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI ammoText;
    
    [Header("총 관리자 연결")]
    [SerializeField] private GunAction gunAction;
    
    private int lastAmmo = -1;
    private int lastMaxAmmo = -1;
    private Gun lastGun = null;
    
    private void Start()
    {
        // GunAction이 없으면 자동으로 찾기
        if (gunAction == null)
        {
            gunAction = Object.FindFirstObjectByType<GunAction>();
        }
        
        // AmmoText가 없으면 자동으로 찾기
        if (ammoText == null)
        {
            ammoText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        UpdateAmmoDisplay();
    }
    
    private void Update()
    {
        // 총알 개수가 변경되었을 때만 업데이트 (성능 최적화)
        if (gunAction == null || ammoText == null) return;
        
        Gun currentGun = gunAction.GetCurrentGun();
        
        // 총이 바뀌었거나, 총알 개수가 변경되었을 때만 업데이트
        if (currentGun != lastGun || 
            (currentGun != null && (currentGun.currentAmmo != lastAmmo || currentGun.maxAmmo != lastMaxAmmo)))
        {
            UpdateAmmoDisplay();
        }
    }
    
    private void UpdateAmmoDisplay()
    {
        if (gunAction == null || ammoText == null) return;
        
        // GunAction에서 현재 총 정보 가져오기
        Gun currentGun = gunAction.GetCurrentGun();
        
        if (currentGun != null)
        {
            // 총알 개수 표시 (예: "30 / 30" 또는 "15 / 30")
            ammoText.text = $"{currentGun.currentAmmo} / {currentGun.maxAmmo}";
            
            // 마지막 값 저장
            lastAmmo = currentGun.currentAmmo;
            lastMaxAmmo = currentGun.maxAmmo;
            lastGun = currentGun;
        }
        else
        {
            ammoText.text = "-- / --";
            lastGun = null;
        }
    }
}

