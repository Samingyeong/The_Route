using UnityEngine;

public class HeadBob : MonoBehaviour
{
    [Header("Movement References")]
    public CharacterController controller;
    public Transform cameraHolder;

    [Header("Bob Settings")]
    public float walkBobSpeed = 14f;
    public float walkBobAmount = 0.05f;
    public float runBobSpeed = 18f;
    public float runBobAmount = 0.1f;

    private float defaultYPos;
    private float timer;

    void Start()
    {
        // CharacterController 자동 찾기 (Player에서)
        if (controller == null)
        {
            // 이 스크립트가 붙은 오브젝트에서 찾기
            controller = GetComponent<CharacterController>();
            
            // 없으면 부모(Player)에서 찾기
            if (controller == null && transform.parent != null)
            {
                controller = transform.parent.GetComponent<CharacterController>();
            }
            
            // 없으면 Player 태그를 가진 오브젝트에서 찾기
            if (controller == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player == null)
                {
                    player = GameObject.Find("Player");
                }
                if (player == null)
                {
                    player = GameObject.Find("player");
                }
                if (player != null)
                {
                    controller = player.GetComponent<CharacterController>();
                }
            }
        }
        
        // CameraHolder 자동 찾기 (CameraRoot)
        if (cameraHolder == null)
        {
            // Player의 자식에서 CameraRoot 찾기
            GameObject player = null;
            if (transform.parent != null)
            {
                player = transform.parent.gameObject;
            }
            else
            {
                player = GameObject.FindGameObjectWithTag("Player");
                if (player == null)
                {
                    player = GameObject.Find("Player");
                }
                if (player == null)
                {
                    player = GameObject.Find("player");
                }
            }
            
            if (player != null)
            {
                Transform cameraRoot = FindChildTransform(player.transform, "CameraRoot");
                if (cameraRoot != null)
                {
                    cameraHolder = cameraRoot;
                }
            }
            
            // 못 찾으면 이 오브젝트의 Transform 사용
            if (cameraHolder == null)
            {
                cameraHolder = transform;
            }
        }

        if (cameraHolder != null)
        {
            defaultYPos = cameraHolder.localPosition.y;
        }
    }
    
    // 자식 Transform 재귀적으로 찾기
    Transform FindChildTransform(Transform parent, string name)
    {
        if (parent == null) return null;
        
        foreach (Transform child in parent)
        {
            if (child.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            {
                return child;
            }
            Transform found = FindChildTransform(child, name);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    void Update()
    {
        if (controller == null) return;

        if (controller.velocity.magnitude > 0.1f && controller.isGrounded)
        {
            // 걸음 속도에 따라 다른 파라미터 적용
            bool isRunning = Input.GetKey(KeyCode.LeftShift);

            float speed = isRunning ? runBobSpeed : walkBobSpeed;
            float amount = isRunning ? runBobAmount : walkBobAmount;

            timer += Time.deltaTime * speed;
            float newY = defaultYPos + Mathf.Sin(timer) * amount;

            cameraHolder.localPosition = new Vector3(
                cameraHolder.localPosition.x,
                newY,
                cameraHolder.localPosition.z
            );
        }
        else
        {
            // 움직임 없으면 카메라 위치 원상 복귀
            timer = 0f;
            cameraHolder.localPosition = new Vector3(
                cameraHolder.localPosition.x,
                Mathf.Lerp(cameraHolder.localPosition.y, defaultYPos, Time.deltaTime * 5f),
                cameraHolder.localPosition.z
            );
        }
    }
}

