using UnityEngine;

public class GunAction : MonoBehaviour
{
    public Animator controller_gun_kriss;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // if (Input.GetKeyDown(KeyCode.A))
        // {
        //     // "OnReload" 트리거를 발동시켜 Reload 애니메이션 재생
        //     controller_gun_kriss.SetTrigger("OnShoot");
        // }
        if (Input.GetKeyDown(KeyCode.A)) controller_gun_kriss.SetBool("IsShooting", true);
        else if (Input.GetKeyUp(KeyCode.A)) controller_gun_kriss.SetBool("IsShooting", false);

        if (Input.GetKeyDown(KeyCode.B)) controller_gun_kriss.SetTrigger("OnReload");
        
        if (Input.GetKeyDown(KeyCode.C)) controller_gun_kriss.SetBool("IsHiding", true);
        else if (Input.GetKeyUp(KeyCode.C)) controller_gun_kriss.SetBool("IsHiding", false);

        if (Input.GetKeyDown(KeyCode.D)) controller_gun_kriss.SetTrigger("OnDraw");
        if (Input.GetKeyDown(KeyCode.Q)) controller_gun_kriss.SetTrigger("OnFire");
    }
}
