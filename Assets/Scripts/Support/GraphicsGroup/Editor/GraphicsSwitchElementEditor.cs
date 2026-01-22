using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Support.GraphicsGroup.Editor
{
    [CustomEditor(typeof(GraphicsSwitchElement), true)]
    [CanEditMultipleObjects]
    public class GraphicsSwitchElementEditor : UnityEditor.Editor
    {
        private GraphicsSwitchElement appliedTarget;
        private List<GraphicsSwitchGroup> usedGroups;

        private void OnEnable()
        {
            appliedTarget = target as GraphicsSwitchElement;

            usedGroups = GetGroups(appliedTarget);
        }

        private List<GraphicsSwitchGroup> GetGroups(GraphicsSwitchElement castedTarget)
        {
            return castedTarget.GetComponentsInParent<GraphicsSwitchGroup>().ToList();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            DrawGroups();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGroups()
        {
            foreach (var group in usedGroups)
            {
                var state = group.Contains(appliedTarget);

                var previousColor = GUI.backgroundColor;

                GUI.backgroundColor = state ? Color.green : Color.gray;

                if (GUILayout.Button($"{group.Id}"))
                {
                    if (state)
                        group.Remove(appliedTarget);
                    else
                        group.Add(appliedTarget);
                }

                GUI.backgroundColor = previousColor;
            }
        }
    }
}