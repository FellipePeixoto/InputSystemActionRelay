using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

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

        void Clear()
        {
            if (holdCoroutine != null)
                coroutineRunner.StopCoroutine(holdCoroutine);

            holdCoroutine = null;
            onStarted.AddListener(HandleOnStarted);
            onPerformed.AddListener(HandleOnPerformed);
            onCanceled.AddListener(HandleOnCanceled);
        }

        void HandleOnStarted(InputAction.CallbackContext ctx)
        {
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

        public void OnEnable()
        {
            if (InputActionRef == null)
                return;

            inputAction = InputActionRef.ToInputAction();
            inputAction.Enable();
            onStarted.AddListener(HandleOnStarted);
            onPerformed.AddListener(HandleOnPerformed);
            onCanceled.AddListener(HandleOnCanceled);
            inputAction.started += onStarted.Invoke;
            inputAction.performed += onPerformed.Invoke;
            inputAction.canceled += onCanceled.Invoke;
        }

        public void OnDisable()
        {
            if (inputAction == null)
                return;

            inputAction.Disable();
            Clear();
            inputAction.started -= onStarted.Invoke;
            inputAction.performed -= onPerformed.Invoke;
            inputAction.canceled -= onCanceled.Invoke;
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
    }
}