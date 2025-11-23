using UnityEngine;

namespace StoreGame.Utility
{
    /// <summary>
    /// 씬에서 트리거 충돌 감지를 디버깅하기 위한 보조 스크립트.
    /// Collider를 트리거로 만들어두고, 플레이어나 아이템이 진입/이탈하는 순간을 로그로 남깁니다.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class TriggerTester : MonoBehaviour
    {
        [Header("필터링")]
        [SerializeField] private string requiredTag = "";
        [SerializeField] private LayerMask layerMask = ~0;

        [Header("로그 옵션")]
        [SerializeField] private bool logStayEvents;
        [SerializeField] private bool warnIfNotTrigger = true;

        private Collider _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider>();

            if (warnIfNotTrigger && _collider != null && !_collider.isTrigger)
            {
                Debug.LogWarning($"[TriggerTester] {_collider.name} Collider가 Trigger가 아닙니다. Trigger로 설정해야 OnTrigger 이벤트가 발생합니다.", this);
            }
        }

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!PassesFilter(other))
            {
                return;
            }

            Debug.Log($"[TriggerTester] OnTriggerEnter → {Describe(other)}", this);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!PassesFilter(other))
            {
                return;
            }

            Debug.Log($"[TriggerTester] OnTriggerExit → {Describe(other)}", this);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!logStayEvents || !PassesFilter(other))
            {
                return;
            }

            Debug.Log($"[TriggerTester] OnTriggerStay → {Describe(other)}", this);
        }

        private bool PassesFilter(Collider other)
        {
            if (other == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
            {
                return false;
            }

            var otherLayer = other.gameObject.layer;
            return (layerMask.value & (1 << otherLayer)) != 0;
        }

        private static string Describe(Collider other)
        {
            var rb = other.attachedRigidbody;
            var hasRb = rb != null ? $"Rigidbody={(rb.isKinematic ? "Kinematic" : "Dynamic")}" : "Rigidbody=None";
            var layerName = LayerMask.LayerToName(other.gameObject.layer);
            if (string.IsNullOrEmpty(layerName))
            {
                layerName = $"Layer {other.gameObject.layer}";
            }

            return $"{other.name} (Tag={other.tag}, {hasRb}, {layerName})";
        }
    }
}


