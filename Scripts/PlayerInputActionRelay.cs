using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DevPeixoto.InputSystemUtils.InputSystemActionRelay
{
    [AddComponentMenu("DevPeixoto/Input System Utils/Player Input Action Relay")]
    public class PlayerInputActionRelay : MonoBehaviour
    {
        [SerializeField] PlayerInput playerInput;

        public List<PlayerInputActionGroup> inputActionGroups = new List<PlayerInputActionGroup>();        

        private void Awake()
        {
            if (playerInput == null)
            {
                Debug.LogError("No Player Input set");
            }

            foreach (var group in inputActionGroups)
            {
                group.inputAction = playerInput.actions.FindAction(group.InputActionRef.action.id);
            }
        }

        void OnEnable()
        {
            if (playerInput == null)
                Debug.LogError("No Player Input set");

            foreach (var group in inputActionGroups)
            {
                group.coroutineRunner = this;
                group.OnEnable();
            }
        }

        void OnDisable()
        {
            if (playerInput == null)
                Debug.LogError("No Player Input set");

            foreach (var group in inputActionGroups)
            {
                group.OnDisable();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (playerInput == null)
                Debug.LogError("No Player Input set");

            foreach (var group in inputActionGroups)
            {
                group.OnValidate();
            }
        }
#endif
    }
}