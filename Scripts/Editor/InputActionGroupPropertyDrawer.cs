#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace DevPeixoto.InputSystemUtils.InputSystemActionRelay
{
    [CustomPropertyDrawer(typeof(InputActionGroup))]
    public class InputActionGroupPropertyDrawer : PropertyDrawer
    {
        int holdCurrentrraySize = 0;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            var inputActionRefPropField = new PropertyField(property.FindPropertyRelative("InputActionRef"), "Input Action Reference");
            root.Add(new PropertyField(property.FindPropertyRelative("InputActionRef"), "Input Action Reference"));

            var eventsUnfolded = property.FindPropertyRelative("eventsUnfolded");
            var eventsFoldout = new Foldout() { text = "Input Action Events", value = eventsUnfolded.boolValue };
            eventsFoldout.BindProperty(eventsUnfolded);
            eventsFoldout.Add(new PropertyField(property.FindPropertyRelative("onStarted")));
            eventsFoldout.Add(new PropertyField(property.FindPropertyRelative("onPerformed")));
            eventsFoldout.Add(new PropertyField(property.FindPropertyRelative("onCanceled")));
            root.Add(eventsFoldout);

            var hasHoldInteractionProp = property.FindPropertyRelative("inputActionHasHoldInteraction");
            var holdEventsWraper = property.FindPropertyRelative("holdInteractionWraper");
            var holdInteractionFoldout = SetupHoldInteractionWraper(holdEventsWraper);
            holdInteractionFoldout.style.display = hasHoldInteractionProp.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
            root.Add(holdInteractionFoldout);

            //property.serializedObject.Update();
            holdInteractionFoldout.TrackPropertyValue(hasHoldInteractionProp, prop =>
            {
                holdInteractionFoldout.style.display = prop.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
            });

            return root;
        }

        VisualElement SetupHoldInteractionWraper(SerializedProperty wraper)
        {
            var holdInteractionFoldout = new Foldout();
            var nameProp = wraper.FindPropertyRelative("Name");
            holdInteractionFoldout.text = nameProp.stringValue;
            holdInteractionFoldout.TrackPropertyValue(nameProp, prop =>
            {
                holdInteractionFoldout.text = prop.stringValue;
            });

            var holdEventsUnfolded = wraper.FindPropertyRelative("unfolded");
            holdInteractionFoldout.value = holdEventsUnfolded.boolValue;
            holdInteractionFoldout.BindProperty(holdEventsUnfolded);

            holdInteractionFoldout.Add(new PropertyField(wraper.FindPropertyRelative("onStarted")));
            holdInteractionFoldout.Add(new PropertyField(wraper.FindPropertyRelative("onPerformed")));
            holdInteractionFoldout.Add(new PropertyField(wraper.FindPropertyRelative("onCanceled")));
            holdInteractionFoldout.Add(new PropertyField(wraper.FindPropertyRelative("onProgress")));
            holdInteractionFoldout.Add(new PropertyField(wraper.FindPropertyRelative("onTime")));

            return holdInteractionFoldout;
        }
    }
}
#endif