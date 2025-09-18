using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.InputSystem.InputAction;

namespace DevPeixoto.InputSystemUtils.InputSystemActionRelay
{
    [Serializable]
    public class InputActionGroup
    {
        public InputActionReference InputActionRef;
        public UnityEvent<InputAction.CallbackContext> onStarted;
        public UnityEvent<InputAction.CallbackContext> onPerformed;
        public UnityEvent<InputAction.CallbackContext> onCanceled;
        public HoldInteractionWraper holdInteractionWraper;
        public bool eventsUnfolded;
        [HideInInspector] public InputAction inputAction;
        [HideInInspector] public Coroutine holdCoroutine;
        [HideInInspector] public MonoBehaviour coroutineRunner;

        Coroutine holdInteractioCo;

        [SerializeField] bool inputActionHasHoldInteraction;

        List<InputCtxContainer> ignoredDevices = new List<InputCtxContainer>();

        void HandleOnStarted(InputAction.CallbackContext ctx)
        {
            var target = ignoredDevices.FirstOrDefault(x => ctx.control.device == x.device);
            if (target != null)
            {
                target.ctx = ctx;
                return;
            }

            onStarted?.Invoke(ctx);

            switch (ctx.interaction)
            {
                case HoldInteraction hold:
                    if (holdInteractioCo == null && holdInteractionWraper.CurrentHoldInteract == null)
                    {
                        holdInteractioCo = coroutineRunner.StartCoroutine(holdInteractionWraper.HoldInteractionCo(hold));
                        holdInteractionWraper.OnStarted(ctx);
                        holdInteractionWraper.CurrentHoldInteract = hold;
                    }
                    break;

                default:
                    break;
            }
        }

        void HandleOnPerformed(InputAction.CallbackContext ctx)
        {
            if (ignoredDevices.Select(x => x.device).Contains(ctx.control.device))
                return;

            onPerformed?.Invoke(ctx);

            switch (ctx.interaction)
            {
                case HoldInteraction hold:
                    // TODO: Investigate Unity Input System Bug
                    break;

                default:
                    break;
            }
        }

        void HandleOnCanceled(InputAction.CallbackContext ctx)
        {
            if (ignoredDevices.Select(x => x.device).Contains(ctx.control.device))
                return;

            onCanceled?.Invoke(ctx);

            switch (ctx.interaction)
            {
                case HoldInteraction hold:
                    if (holdInteractioCo != null && holdInteractionWraper.CurrentHoldInteract == hold)
                    {
                        holdInteractionWraper.OnCanceled(ctx);
                        holdInteractionWraper.CurrentHoldInteract = null;
                        coroutineRunner.StopCoroutine(holdInteractioCo);
                        holdInteractioCo = null;
                    }
                    break;

                default:
                    break;
            }
        }

        internal void IgnoreDevice(InputDevice device)
        {
            if (ignoredDevices.Select(x => x.device).Contains(device))
                return;

            ignoredDevices.Add(new InputCtxContainer() { device = device });
        }

        internal void IncludeIgnoreDevice(InputDevice device)
        {
            var target = ignoredDevices.FirstOrDefault(x => x.device == device);

            if (target != null && (
                target.ctx.phase == InputActionPhase.Waiting ||
                target.ctx.phase == InputActionPhase.Performed ||
                target.ctx.phase == InputActionPhase.Canceled))
            {
                ignoredDevices.Remove(target);
            }
            else if (target != null)
            {
                coroutineRunner.StartCoroutine(WaitToDeleteIgnoreCo(target.ctx, target));
            }
        }

        IEnumerator WaitToDeleteIgnoreCo(CallbackContext ctx, InputCtxContainer toRemove)
        {
            yield return new WaitUntil(() => ctx.phase == InputActionPhase.Waiting);
            ignoredDevices.Remove(toRemove);
        }

        public void OnEnable()
        {
            if (InputActionRef == null)
                return;

            inputAction = InputActionRef.action.Clone();
            inputAction.Enable();
            inputAction.started += HandleOnStarted;
            inputAction.performed += HandleOnPerformed;
            inputAction.canceled += HandleOnCanceled;
        }

        public void OnDisable()
        {
            if (inputAction == null)
                return;

            inputAction.Disable();
            Clear();
            inputAction.started -= HandleOnStarted;
            inputAction.performed -= HandleOnPerformed;
            inputAction.canceled -= HandleOnCanceled;
        }

        void Clear()
        {
            if (holdCoroutine != null)
                coroutineRunner.StopCoroutine(holdCoroutine);

            holdCoroutine = null;
        }

#if UNITY_EDITOR
        public void OnValidate()
        {
            if (InputActionRef == null || InputActionRef.action == null)
            {
                HandleHoldInteractionClean();
                return;
            }

            SetupHoldInteractions();
        }

        void SetupHoldInteractions()
        {
            inputActionHasHoldInteraction = InputActionRef.action.interactions.Contains(HoldInteractionWraper.k_hold);
            if (inputActionHasHoldInteraction)
                holdInteractionWraper.Name = "Input Action Hold Events";
        }

        void HandleHoldInteractionClean()
        {
            inputActionHasHoldInteraction = false;
        }
#endif

        [Serializable]
        public class HoldInteractionWraper
        {
            public HoldInteraction CurrentHoldInteract;
            public string Name;
            public bool crescent;
            public bool unfolded;
            public static readonly string k_hold = "Hold";

            [SerializeField] UnityEvent<InputAction.CallbackContext> onStarted;
            [SerializeField] UnityEvent onPerformed;
            [SerializeField] UnityEvent<InputAction.CallbackContext> onCanceled;
            [SerializeField] UnityEvent<float> onProgress;
            [SerializeField] UnityEvent<float> onTime;

            public void OnStarted(InputAction.CallbackContext ctx)
            {
                onStarted?.Invoke(ctx);
            }

            public void OnPerformed()
            {
                onPerformed?.Invoke();
            }

            public void OnCanceled(InputAction.CallbackContext ctx)
            {
                onCanceled?.Invoke(ctx);
            }

            public IEnumerator HoldInteractionCo(HoldInteraction ctx)
            {
                float duration = Convert.ToSingle(ctx.duration > 0.0f ? ctx.duration : InputSystem.settings.defaultHoldTime);
                float startTime = Time.realtimeSinceStartup;

                float diff = 0;
                onProgress.Invoke(crescent ? 0 : 1);
                onTime?.Invoke(crescent ? 0 : duration);
                while (diff <= duration)
                {
                    float progress = crescent ? diff / duration : 1 - (diff / duration);
                    onProgress.Invoke(progress);

                    float timeProgress = crescent ? diff : diff - duration;
                    onTime?.Invoke(timeProgress);

                    diff = Time.realtimeSinceStartup - startTime;
                    yield return null;
                }

                onProgress.Invoke(crescent ? 1 : 0);
                onTime?.Invoke(crescent ? duration : 0);

                OnPerformed();
            }
        }

        class InputCtxContainer
        {
            public InputDevice device;
            public InputAction.CallbackContext ctx;
        }
    }
}