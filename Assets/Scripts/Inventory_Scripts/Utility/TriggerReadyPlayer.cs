using UnityEngine;

namespace StoreGame.Utility
{
    /// <summary>
    /// 플레이어 오브젝트가 트리거 이벤트 조건을 만족하도록 Rigidbody/Collider/Tag를 점검하는 유틸리티.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class TriggerReadyPlayer : MonoBehaviour
    {
        [Header("Tag 설정")]
        [SerializeField] private string requiredTag = "Player";
        [SerializeField] private bool enforceTagOnReset = true;

        [Header("Rigidbody 옵션")]
        [SerializeField] private bool addRigidbodyIfMissing = true;
        [SerializeField] private bool makeKinematic = true;
        [SerializeField] private bool disableGravity = true;

        [Header("Collider 옵션")]
        [SerializeField] private bool warnIfColliderIsTrigger;

        private Collider _collider;
        private Rigidbody _rigidbody;

        private void Reset()
        {
            _collider = GetComponent<Collider>();
            _rigidbody = GetComponent<Rigidbody>();

            EnsureColliderState();
            EnsureTag();
            EnsureRigidbody();
        }

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _rigidbody = GetComponent<Rigidbody>();

            EnsureColliderState();
            EnsureTag();
            EnsureRigidbody();
        }

        private void EnsureTag()
        {
            if (!enforceTagOnReset || string.IsNullOrEmpty(requiredTag))
            {
                return;
            }

            if (!gameObject.CompareTag(requiredTag))
            {
                Debug.LogWarning($"[TriggerReadyPlayer] GameObject '{name}'의 Tag가 '{requiredTag}'가 아닙니다. Player 관련 트리거가 동작하지 않을 수 있습니다.", this);
            }
        }

        private void EnsureRigidbody()
        {
            if (_rigidbody == null && addRigidbodyIfMissing)
            {
                _rigidbody = gameObject.AddComponent<Rigidbody>();
                Debug.Log("[TriggerReadyPlayer] Rigidbody가 없어 자동으로 추가했습니다.", this);
            }

            if (_rigidbody == null)
            {
                Debug.LogWarning("[TriggerReadyPlayer] Rigidbody가 없어 트리거 이벤트가 발생하지 않을 수 있습니다.", this);
                return;
            }

            _rigidbody.isKinematic = makeKinematic;
            _rigidbody.useGravity = !disableGravity;
        }

        private void EnsureColliderState()
        {
            if (_collider == null)
            {
                Debug.LogError("[TriggerReadyPlayer] Collider가 필요합니다.", this);
                return;
            }

            if (warnIfColliderIsTrigger && _collider.isTrigger)
            {
                Debug.LogWarning("[TriggerReadyPlayer] 플레이어 Collider가 Trigger로 설정되어 있습니다. 물리 충돌이 필요하면 해제하세요.", this);
            }
        }
    }
}


