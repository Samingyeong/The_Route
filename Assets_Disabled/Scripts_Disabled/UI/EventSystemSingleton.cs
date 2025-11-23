using UnityEngine;
using UnityEngine.EventSystems;

namespace StoreGame.UI
{
    /// <summary>
    /// EventSystem 중복을 방지하는 싱글톤 컴포넌트.
    /// 씬에 이미 EventSystem이 있으면 자신을 제거합니다.
    /// </summary>
    [RequireComponent(typeof(EventSystem))]
    public class EventSystemSingleton : MonoBehaviour
    {
        private void Awake()
        {
            // 씬에 이미 EventSystem이 있는지 확인
            var existingEventSystems = FindObjectsOfType<EventSystem>();
            
            // 자신을 제외한 다른 EventSystem이 있으면
            if (existingEventSystems.Length > 1)
            {
                // 자신이 마지막에 생성된 것이면 제거
                // (먼저 생성된 것을 유지)
                bool shouldDestroy = false;
                foreach (var es in existingEventSystems)
                {
                    if (es != GetComponent<EventSystem>())
                    {
                        shouldDestroy = true;
                        break;
                    }
                }
                
                if (shouldDestroy)
                {
                    Debug.Log($"[EventSystemSingleton] Duplicate EventSystem detected. Destroying {gameObject.name}");
                    Destroy(gameObject);
                    return;
                }
            }
            
            // 유일한 EventSystem이면 유지
            Debug.Log($"[EventSystemSingleton] EventSystem initialized: {gameObject.name}");
        }
    }
}



