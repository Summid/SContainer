using SContainer.Runtime;
using SContainer.Runtime.Diagnostics;
using SContainer.Runtime.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace SContainer.Editor.Diagnostics
{
    /// <summary> 包含被注册、解析类型的信息 </summary>
    public sealed class DiagnosticsInfoTreeViewItem : TreeViewItem
    {
        public string ScopeName { get; set; }
        public DiagnosticsInfo DiagnosticsInfo { get; }

        public RegistrationBuilder RegistrationBuilder => this.DiagnosticsInfo.RegisterInfo.RegistrationBuilder;
        public Registration Registration => this.DiagnosticsInfo.ResolveInfo.Registration;
        public int? RefCount => this.DiagnosticsInfo.ResolveInfo.RefCount;
        public long ResolveTime => this.DiagnosticsInfo.ResolveInfo.ResolveTime;

        public string TypeSummary => TypeNameHelper.GetTypeAlias(this.Registration.ImplementationType);

        public string ContractTypesSummary
        {
            get
            {
                if (this.Registration.InterfaceTypes != null)
                {
                    var values = this.Registration.InterfaceTypes.Select(TypeNameHelper.GetTypeAlias);
                    return string.Join(", ", values);
                }
                return "";
            }
        }

        public string RegisterSummary
        {
            get
            {
                if (this.RegistrationBuilder == null)
                    return "";

                var type = this.RegistrationBuilder.GetType();
                if (type == typeof(RegistrationBuilder))
                {
                    return "";
                }

                var typeName = type.Name;
                var suffixIndex = typeName.IndexOf("Builder");
                if (suffixIndex > 0)
                {
                    typeName = typeName.Substring(0, suffixIndex);
                }
                suffixIndex = typeName.IndexOf("Registration");
                if (suffixIndex > 0)
                {
                    typeName = typeName.Substring(0, suffixIndex);
                }

                if (typeName.StartsWith("Instance") && this.TypeSummary.StartsWith("Func<"))
                {
                    return "FuncFactory";
                }

                return typeName;
            }
        }

        public DiagnosticsInfoTreeViewItem(DiagnosticsInfo info)
        {
            this.ScopeName = info.ScopeName;
            this.DiagnosticsInfo = info;
            this.displayName = this.TypeSummary;
        }
    }
    
    /// <summary> 诊断主面板 </summary>
    public sealed class SContainerDiagnosticsTreeView : TreeView
    {
        private static readonly MultiColumnHeaderState.Column[] Columns =
        {
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("Type") },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("ContractTypes"), canSort = false },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("Lifetime"), width = 15f },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("Register"), width = 15f },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("RefCount"), width = 5f },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("Scope"), width = 20f },
            new MultiColumnHeaderState.Column { headerContent = new GUIContent("Time"), width = 20f },
        };

        private static int idSeed;
        private static int NextId() => ++idSeed;
        
        private const string SessionStateKeySortedColumnIndex = "SContainer.Editor.DiagnosticsInfoTreeView:sortedColumnIndex";

        public bool Flatten
        {
            get => this.flatten;
            set
            {
                this.flatten = value;
                this.multiColumnHeader.ResizeToFit();
            }
        }
        
        private bool flatten;

        public SContainerDiagnosticsTreeView(TreeViewState state)
            : this(new TreeViewState(), new MultiColumnHeader(new MultiColumnHeaderState(Columns)))
        {
        }

        public SContainerDiagnosticsTreeView(TreeViewState state, MultiColumnHeader header)
            : base(state, header)
        {
            this.rowHeight = 20;
            this.showAlternatingRowBackgrounds = true;
            this.showBorder = true;
            header.sortingChanged += this.OnSortedChanged;
            
            header.ResizeToFit();
            this.Reload();

            header.sortedColumnIndex = Math.Min(
                header.state.columns.Length - 1,
                SessionState.GetInt(SessionStateKeySortedColumnIndex, 0));
        }

        public DiagnosticsInfoTreeViewItem GetSelectedItem()
        {
            if (this.state.selectedIDs.Count <= 0) return null;

            var selectedId = this.state.selectedIDs[0];
            return this.GetRows().FirstOrDefault(x => x.id == selectedId) as DiagnosticsInfoTreeViewItem;
        }

        public void ReloadAndSort()
        {
            var currentSelected = this.state.selectedIDs;
            this.Reload();
            this.OnSortedChanged(this.multiColumnHeader);
            this.state.selectedIDs = currentSelected;
        }
        
        private void OnSortedChanged(MultiColumnHeader multiColumnHeader)
        {
            var columnIndex = multiColumnHeader.sortedColumnIndex;
            if (columnIndex < 0) return;

            SessionState.SetInt(SessionStateKeySortedColumnIndex, columnIndex);
            var ascending = multiColumnHeader.IsSortedAscending(columnIndex);

            if (this.Flatten)
            {
                var items = this.rootItem.children.Cast<DiagnosticsInfoTreeViewItem>();
                this.rootItem.children = new List<TreeViewItem>(this.Sort(items, columnIndex, ascending));
            }
            else
            {
                foreach (var sectionHeaderItem in this.rootItem.children)
                {
                    var items = sectionHeaderItem.children.Cast<DiagnosticsInfoTreeViewItem>();
                    sectionHeaderItem.children = new List<TreeViewItem>(this.Sort(items, columnIndex, ascending));
                }
            }
            this.BuildRows(this.rootItem);
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { depth = -1 };
            var children = new List<TreeViewItem>();

            if (SContainerSettings.DiagnosticsEnabled)
            {
                if (this.Flatten)
                {
                    var infos = DiagnosticsContext.GetDiagnosticsInfos();
                    foreach (var info in infos)
                    {
                        children.Add(new DiagnosticsInfoTreeViewItem(info)
                        {
                            id = NextId(),
                            depth = 0,
                            ScopeName = info.ScopeName,
                        });
                    }
                }
                else
                {
                    var grouped = DiagnosticsContext.GetGroupedDiagnosticsInfos();
                    foreach (var scope in grouped)
                    {
                        // 以 Scope 分类
                        var sectionHeaderItem = new TreeViewItem(NextId(), 0, scope.Key);
                        children.Add(sectionHeaderItem);
                        this.SetExpanded(sectionHeaderItem.id, true);
                        
                        // 给 Scope 节点添加属于它的子节点
                        foreach (var info in scope)
                        {
                            this.AddDependencyItemsRecursive(info, sectionHeaderItem);
                        }
                    }
                }
            }

            root.children = children;
            return root;
        }

        protected override bool CanMultiSelect(TreeViewItem item) => false;

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as DiagnosticsInfoTreeViewItem;
            if (item is null)
            {
                base.RowGUI(args);
                return;
            }

            for (var visibleColumnIndex = 0; visibleColumnIndex < args.GetNumVisibleColumns(); visibleColumnIndex++)
            {
                var cellRect = args.GetCellRect(visibleColumnIndex);
                // CenterRectUsingSingleLineHeight(ref cellRect);
                var columnIndex = args.GetColumn(visibleColumnIndex);

                var labelStyle = args.selected ? EditorStyles.whiteLabel : EditorStyles.label;
                labelStyle.alignment = TextAnchor.MiddleCenter;

                switch (columnIndex)
                {
                    case 0:
                        base.RowGUI(args);
                        break;
                    case 1:
                        EditorGUI.LabelField(cellRect, item.ContractTypesSummary, labelStyle);
                        break;
                    case 2:
                        EditorGUI.LabelField(cellRect, item.Registration.Lifetime.ToString(), labelStyle);
                        break;
                    case 3:
                        EditorGUI.LabelField(cellRect, item.RegisterSummary, labelStyle);
                        break;
                    case 4:
                        EditorGUI.LabelField(cellRect, item.RefCount.ToString(), labelStyle);
                        break;
                    case 5:
                        EditorGUI.LabelField(cellRect, item.ScopeName, labelStyle);
                        break;
                    case 6:
                        EditorGUI.LabelField(cellRect, item.ResolveTime.ToString(), labelStyle);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
                }
            }
        }

        private void AddDependencyItemsRecursive(DiagnosticsInfo info, TreeViewItem parent)
        {
            var item = new DiagnosticsInfoTreeViewItem(info)
            {
                id = NextId(),
                depth = parent.depth + 1
            };
            parent.AddChild(item);
            this.SetExpanded(item.id, item.depth <= 1);
            
            foreach (var dependency in info.Dependencies)
            {
                this.AddDependencyItemsRecursive(dependency, item);
            }
        }

        private IEnumerable<DiagnosticsInfoTreeViewItem> Sort(
            IEnumerable<DiagnosticsInfoTreeViewItem> items,
            int sortedColumnIndex,
            bool ascending)
        {
            switch (sortedColumnIndex)
            {
                case 0:
                    return ascending
                        ? items.OrderBy(x => x.TypeSummary)
                        : items.OrderByDescending(x => x.TypeSummary);
                case 2:
                    return ascending
                        ? items.OrderBy(x => x.Registration.Lifetime)
                        : items.OrderByDescending(x => x.Registration.Lifetime);
                case 3:
                    return ascending
                        ? items.OrderBy(x => x.RegisterSummary)
                        : items.OrderByDescending(x => x.RegisterSummary);
                case 4:
                    return ascending
                        ? items.OrderBy(x => x.RefCount)
                        : items.OrderByDescending(x => x.RefCount);
                case 5:
                    return ascending
                        ? items.OrderBy(x => x.ScopeName)
                        : items.OrderByDescending(x => x.ScopeName);
                case 6:
                    return ascending
                        ? items.OrderBy(x => x.ResolveTime)
                        : items.OrderByDescending(x => x.ResolveTime);
                default:
                    throw new ArgumentOutOfRangeException(nameof(sortedColumnIndex), sortedColumnIndex, null);
            }
        }
    }
}