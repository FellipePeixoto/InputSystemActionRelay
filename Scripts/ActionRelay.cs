using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DevPeixoto.InputSystemUtils.InputSystemActionRelay
{
    [AddComponentMenu("DevPeixoto/Input System Utils/Action Relay")]
    public class ActionRelay : MonoBehaviour
    {
        public List<InputActionGroup> inputActionGroups = new List<InputActionGroup>();

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

        public void IgnoreDevice(InputDevice device)
        {
            foreach (var group in inputActionGroups)
            {
                group.IgnoreDevice(device);
            }
        }

        public void IncludeIgnoredDevice(InputDevice device)
        {
            foreach (var group in inputActionGroups)
            {
                group.IncludeIgnoreDevice(device);
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