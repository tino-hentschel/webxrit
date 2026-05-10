using haw.pd20.events;
using UnityEngine;
using UnityEngine.Events;


namespace haw.pd20.training_modules
{
    public class Item : MonoBehaviour
    {
        public static bool CompareTag(GameObject go, ItemTag tag)
        {
            var item = go.GetComponent<Item>();
            if (!item)
                return false;

            return item.Tag == tag;
        }

        public enum OnDropBehaviour
        {
            ResetPosition,
            Destroy,
            CustomCallback,
            None
        }

        [SerializeField] private ItemTag itemTag;
        [SerializeField] private Transform initialResetTransform;
        [SerializeField] private OnDropBehaviour onDrop;

        [SerializeField] private UnityEvent onDropCustomCallback;

        private Vector3 resetPosition;
        private Quaternion resetRotation;

        public ItemTag Tag => itemTag;

        private void OnEnable()
        {
            if (initialResetTransform)
                SetResetTransform(initialResetTransform);
            else
                SetResetTransform();
        }

        private void OnCollisionEnter(Collision collision) => TryTriggerOnDropBehaviour(collision.gameObject);

        private void OnTriggerEnter(Collider other) => TryTriggerOnDropBehaviour(other.gameObject);

        private void TryTriggerOnDropBehaviour(GameObject other)
        {
            if (other.gameObject.layer != LayerMask.NameToLayer("Ground"))
                return;

            switch (onDrop)
            {
                case OnDropBehaviour.ResetPosition:
                    ResetPositionAndRotation();
                    break;

                case OnDropBehaviour.Destroy:
                    Destroy(gameObject);
                    break;

                case OnDropBehaviour.CustomCallback:
                    onDropCustomCallback?.Invoke();
                    break;

                case OnDropBehaviour.None:
                    return;
            }

            EventManager.Instance.Raise(new DefaultErrorEvent("Arbeitsmittel " + Tag.Name + " fallen gelassen."));
        }

        public void SetResetTransform(Transform resetTransform)
        {
            resetPosition = resetTransform.position;
            resetRotation = resetTransform.rotation;
        }

        public void SetResetTransform()
        {
            resetPosition = transform.position;
            resetRotation = transform.rotation;
        }

        public void ResetPositionAndRotation()
        {
            transform.position = resetPosition;
            transform.rotation = resetRotation;
        }
        
        // TODO: Check for Removal ... this could be accomplished by using SetResetTransform and ResetPositionAndRotation
        public void ResetPositionAndRotation(Transform tempResetTransform)
        {
            transform.position = tempResetTransform.position;
            transform.rotation = tempResetTransform.rotation;
        }

        public void SetOnDropBehaviour(OnDropBehaviour onDropBehaviour)
        {
            onDrop = onDropBehaviour;
        }

        public void SetOnDropBehaviour(string onDropBehaviourStr)
        {
            switch (onDropBehaviourStr)
            {
                case "Reset":
                    SetOnDropBehaviour(OnDropBehaviour.ResetPosition);
                    return;
                case "Destroy":
                    SetOnDropBehaviour(OnDropBehaviour.Destroy);
                    return;
                case "Custom":
                    SetOnDropBehaviour(OnDropBehaviour.CustomCallback);
                    return;
                case "None":
                    SetOnDropBehaviour(OnDropBehaviour.None);
                    return;
            }
        }
    }
}