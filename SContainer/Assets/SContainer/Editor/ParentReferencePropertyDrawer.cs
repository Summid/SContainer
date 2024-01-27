using SContainer.Runtime.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SContainer.Editor
{
    [CustomPropertyDrawer(typeof(ParentReference))]
    public class ParentReferencePropertyDrawer : PropertyDrawer
    {
        private static string[] GetAllTypeNames()
        {
            return new List<string> { "None" }
                .Concat(TypeCache.GetTypesDerivedFrom<LifetimeScope>()
                    .Select(type => type.FullName))
                .ToArray();
        }

        private string[] names;

        public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
        {
            if (this.names == null)
            {
                this.names = GetAllTypeNames();
                if (prop.serializedObject.targetObject is LifetimeScope lifetimeScope)
                {
                    var lifetimeScopeName = lifetimeScope.GetType().FullName;
                    this.names = this.names.Where(name => name != lifetimeScopeName).ToArray();
                }
            }

            var typeNameProp = prop.FindPropertyRelative("TypeName");

            using (new EditorGUI.PropertyScope(rect, label, prop))
            {
                var labelRect = new Rect(rect.x, rect.y, rect.width, 18f);
                var popupRect = new Rect(rect.x, rect.y + labelRect.height, rect.width, 18f);

                var index = Array.IndexOf(this.names, typeNameProp.stringValue);
                if (index < 0) index = 0;

                EditorGUI.LabelField(labelRect, "Parent");
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    index = EditorGUI.Popup(popupRect, index, this.names);
                    if (check.changed)
                    {
                        typeNameProp.stringValue = this.names[index];
                    }
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 18f + 18f + 4f;
        }
    }
}