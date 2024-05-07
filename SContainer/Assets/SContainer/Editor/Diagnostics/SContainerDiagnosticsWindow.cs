using SContainer.Runtime;
using SContainer.Runtime.Diagnostics;
using SContainer.Runtime.Unity;
using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SContainer.Editor.Diagnostics
{
    public sealed class SContainerDiagnosticsWindow : EditorWindow
    {
        private static SContainerDiagnosticsWindow window;

        private static readonly GUIContent FlattenHeadContent = EditorGUIUtility.TrTextContent("Flatten", "Flatten dependencies");
        private static readonly GUIContent ReloadHeadContent = EditorGUIUtility.TrTextContent("Reload", "Reload View");
        
        internal static bool EnableAutoReload;
        internal static bool EnableCaptureStackTrace;

        [MenuItem("Window/SContainer Diagnostics")]
        public static void OpenWindow()
        {
            if (window != null)
            {
                window.Close();
            }
            GetWindow<SContainerDiagnosticsWindow>("SContainer Diagnostics").Show();
        }

        private GUIStyle TableListStyle
        {
            get
            {
                var style = new GUIStyle("CN Box");
                style.margin.top = 0;
                style.padding.left = 3;
                return style;
            }
        }

        private GUIStyle DetailsStyle
        {
            get
            {
                var detailsStyle = new GUIStyle("CN Message");
                detailsStyle.wordWrap = false;
                detailsStyle.stretchHeight = true;
                detailsStyle.margin.right = 15;
                return detailsStyle;
            }
        }

        private SContainerDiagnosticsTreeView treeView;
        private SContainerInstanceTreeView instanceTreeView;
        private SearchField searchField;

        private object verticalSplitterState;
        private object horizontalSplitterState;
        private Vector2 tableScrollPosition;
        private Vector2 detailsScrollPosition;
        private Vector2 instanceScrollPosition;

        public void Reload(IObjectResolver resolver)
        {
            this.treeView.ReloadAndSort();
            this.Repaint();
        }

        private void OnPlayModeStateChange(PlayModeStateChange state)
        {
            this.treeView.ReloadAndSort();
            this.Repaint();
        }

        private void OnEnable()
        {
            window = this;
            this.verticalSplitterState = SplitterGUILayout.CreateSplitterState(new [] { 75f, 25f }, new [] { 32, 32 }, null);
            this.horizontalSplitterState = SplitterGUILayout.CreateSplitterState(new[] { 75, 25f }, new[] { 32, 32 }, null);
            this.treeView = new SContainerDiagnosticsTreeView();
            this.instanceTreeView = new SContainerInstanceTreeView();
            this.searchField = new SearchField();

            DiagnosticsContext.OnContainerBuilt += this.Reload;
            EditorApplication.playModeStateChanged += this.OnPlayModeStateChange;
        }

        private void OnDisable()
        {
            DiagnosticsContext.OnContainerBuilt -= this.Reload;
            EditorApplication.playModeStateChanged -= this.OnPlayModeStateChange;
        }

        private void OnGUI()
        {
            this.RenderHeadPanel();
            
            SplitterGUILayout.BeginVerticalSplit(this.verticalSplitterState,Array.Empty<GUILayoutOption>());
            {
                SplitterGUILayout.BeginHorizontalSplit(this.horizontalSplitterState);
                {
                    this.RenderBuildPanel();
                    this.RenderInstancePanel();
                }
                SplitterGUILayout.EndHorizontalSplit();
                
                this.RenderStackTracePanel();
            }
            SplitterGUILayout.EndVerticalSplit();
        }

        private void RenderHeadPanel()
        {
            using (new EditorGUILayout.VerticalScope())
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                var flattenOn = GUILayout.Toggle(this.treeView.Flatten, FlattenHeadContent, EditorStyles.toolbarButton);
                if (flattenOn != this.treeView.Flatten)
                {
                    this.treeView.Flatten = flattenOn;
                    this.treeView.ReloadAndSort();
                    this.Repaint();
                }
                
                GUILayout.FlexibleSpace();

                this.treeView.searchString = this.searchField.OnToolbarGUI(this.treeView.searchString);

                if (GUILayout.Button(ReloadHeadContent, EditorStyles.toolbarButton))
                {
                    this.treeView.ReloadAndSort();
                    this.Repaint();
                }
            }
        }

        private void RenderBuildPanel()
        {
            using (new EditorGUILayout.VerticalScope(this.TableListStyle))
            using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(this.tableScrollPosition,
                       // true,
                       // true,
                       GUILayout.ExpandWidth(true)
                       , GUILayout.MaxWidth(2000f)))
            {
                this.tableScrollPosition = scrollViewScope.scrollPosition;

                var controlRect = EditorGUILayout.GetControlRect(
                    GUILayout.ExpandHeight(true),
                    GUILayout.ExpandHeight(true));
                this.treeView?.OnGUI(controlRect);
            }
        }

        private void RenderInstancePanel()
        {
            if (!SContainerSettings.DiagnosticsEnabled)
            {
                return;
            }

            var selectedItem = this.treeView.GetSelectedItem();
            if (selectedItem?.DiagnosticsInfo.ResolveInfo is ResolveInfo resolveInfo)
            {
                if (resolveInfo.Instances.Count > 0)
                {
                    this.instanceTreeView.CurrentDiagnosticsInfo = selectedItem.DiagnosticsInfo;
                    this.instanceTreeView.Reload();

                    using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(this.instanceScrollPosition, GUILayout.ExpandHeight(true)))
                    {
                        this.instanceScrollPosition = scrollViewScope.scrollPosition;
                        var controlRect = EditorGUILayout.GetControlRect(
                            GUILayout.ExpandHeight(true),
                            GUILayout.ExpandWidth(true));
                        this.instanceTreeView?.OnGUI(controlRect);
                    }
                }
                else
                {
                    EditorGUILayout.SelectableLabel("No instance reference");
                }
            }
        }

        private void RenderStackTracePanel()
        {
            var message = "";
            if (SContainerSettings.DiagnosticsEnabled)
            {
                var selectedItem = this.treeView.GetSelectedItem();
                if (selectedItem?.DiagnosticsInfo?.RegisterInfo is RegisterInfo registerInfo)
                {
                    message = $"<a href=\"{registerInfo.GetScriptAssetPath()}\" line=\"{registerInfo.GetFileLineNumber()}\">Register at {registerInfo.GetHeadline()}</a>" +
                        Environment.NewLine +
                        Environment.NewLine +
                        selectedItem.DiagnosticsInfo.RegisterInfo.StackTrace;
                }
            }
            else
            {
                message = "SContainer Diagnostics collector is disabled. To enable, please check SContainerSettings.";
            }
            using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(this.detailsScrollPosition))
            {
                this.detailsScrollPosition = scrollViewScope.scrollPosition;
                var vector = this.DetailsStyle.CalcSize(new GUIContent(message));
                EditorGUILayout.SelectableLabel(message, this.DetailsStyle,
                    GUILayout.ExpandHeight(true),
                    GUILayout.ExpandWidth(true),
                    GUILayout.MinWidth(vector.x),
                    GUILayout.MinHeight(vector.y));
            }
        }
    }
}