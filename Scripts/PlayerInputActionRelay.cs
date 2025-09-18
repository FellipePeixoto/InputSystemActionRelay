using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DevPeixoto.InputSystemUtils.InputSystemActionRelay
{
    [RequireComponent(typeof(PlayerInput))]
    [AddComponentMenu("DevPeixoto/Input System Utils/Player Input Action Relay")]
    public class PlayerInputActionRelay : MonoBehaviour
    {
        public List<PlayerInputActionGroup> inputActionGroups = new List<PlayerInputActionGroup>();
        
        PlayerInput playerInput;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            foreach (var group in inputActionGroups)
            {
                group.inputAction = playerInput.actions.FindAction(group.InputActionRef.action.id);
            }
        }

        void OnEnable()
        {
            foreach (var group in inputActionGroups)
            {
                group.coroutineRunner = this;
                group.OnEnable();
            }
        }

        void OnDisable()
        {
            foreach (var group in inputActionGroups)
            {
                group.OnDisable();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            foreach (var group in inputActionGroups)
            {
                group.OnValidate();
            }
        }
#endif
    }
}