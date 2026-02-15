using UnityEngine;
#if UNITY_UI_EXIST
using UnityEngine.EventSystems;
#endif

namespace FMODUnity
{
    public abstract class EventHandler : MonoBehaviour
#if UNITY_UI_EXIST
    , IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
#endif
    {
        public string CollisionTag = "";
        [SerializeField]
        protected string[] ExcludeTags = new string[0];

        private bool IsExcludedByTag(string tag)
        {
            if (ExcludeTags == null || ExcludeTags.Length == 0)
                return false;
            for (int i = 0; i < ExcludeTags.Length; i++)
            {
                var t = ExcludeTags[i];
                if (!string.IsNullOrEmpty(t) && t == tag)
                    return true;
            }
            return false;
        }

        protected virtual void Start()
        {
            HandleGameEvent(EmitterGameEvent.ObjectStart);
        }

        protected virtual void OnDestroy()
        {
            HandleGameEvent(EmitterGameEvent.ObjectDestroy);
        }

        private void OnEnable()
        {
            HandleGameEvent(EmitterGameEvent.ObjectEnable);
        }

        private void OnDisable()
        {
            HandleGameEvent(EmitterGameEvent.ObjectDisable);
        }

#if UNITY_PHYSICS_EXIST
        private void OnTriggerEnter(Collider other)
        {
            // Exclude by tag (collider or attached rigidbody)
            if (IsExcludedByTag(other.tag) || (other.attachedRigidbody && IsExcludedByTag(other.attachedRigidbody.tag)))
            {
                return;
            }

            if (string.IsNullOrEmpty(CollisionTag) || other.CompareTag(CollisionTag) || (other.attachedRigidbody && other.attachedRigidbody.CompareTag(CollisionTag)))
            {
                HandleGameEvent(EmitterGameEvent.TriggerEnter);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Exclude by tag (collider or attached rigidbody)
            if (IsExcludedByTag(other.tag) || (other.attachedRigidbody && IsExcludedByTag(other.attachedRigidbody.tag)))
            {
                return;
            }

            if (string.IsNullOrEmpty(CollisionTag) || other.CompareTag(CollisionTag) || (other.attachedRigidbody && other.attachedRigidbody.CompareTag(CollisionTag)))
            {
                HandleGameEvent(EmitterGameEvent.TriggerExit);
            }
        }
#endif

#if UNITY_PHYSICS2D_EXIST
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Exclude by tag
            if (IsExcludedByTag(other.tag))
            {
                return;
            }

            if (string.IsNullOrEmpty(CollisionTag) || other.CompareTag(CollisionTag))
            {
                HandleGameEvent(EmitterGameEvent.TriggerEnter2D);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // Exclude by tag
            if (IsExcludedByTag(other.tag))
            {
                return;
            }

            if (string.IsNullOrEmpty(CollisionTag) || other.CompareTag(CollisionTag))
            {
                HandleGameEvent(EmitterGameEvent.TriggerExit2D);
            }
        }
#endif

        private void OnCollisionEnter()
        {
            HandleGameEvent(EmitterGameEvent.CollisionEnter);
        }

        private void OnCollisionExit()
        {
            HandleGameEvent(EmitterGameEvent.CollisionExit);
        }

        private void OnCollisionEnter2D()
        {
            HandleGameEvent(EmitterGameEvent.CollisionEnter2D);
        }

        private void OnCollisionExit2D()
        {
            HandleGameEvent(EmitterGameEvent.CollisionExit2D);
        }

#if UNITY_UI_EXIST
        private void OnMouseEnter()
        {
            HandleGameEvent(EmitterGameEvent.ObjectMouseEnter);
        }

        private void OnMouseExit()
        {
            HandleGameEvent(EmitterGameEvent.ObjectMouseExit);
        }

        private void OnMouseDown()
        {
            HandleGameEvent(EmitterGameEvent.ObjectMouseDown);
        }

        private void OnMouseUp()
        {
            HandleGameEvent(EmitterGameEvent.ObjectMouseUp);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            HandleGameEvent(EmitterGameEvent.UIMouseEnter);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HandleGameEvent(EmitterGameEvent.UIMouseExit);
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            HandleGameEvent(EmitterGameEvent.UIMouseDown);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            HandleGameEvent(EmitterGameEvent.UIMouseUp);
        }
#endif
        protected abstract void HandleGameEvent(EmitterGameEvent gameEvent);
    }
}
