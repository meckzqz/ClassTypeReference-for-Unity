﻿namespace TypeReferences.Editor.TypeDropdown
{
    using System.Linq;
    using SolidUtilities.Editor.Extensions;
    using UnityEngine;

    internal partial class SelectionTree
    {
        #region KeyboardEvents

        private void HandleKeyboardEvents()
        {
            if (Event.current.type != EventType.KeyDown)
                return;

            bool eventUsed = Event.current.keyCode switch
            {
                KeyCode.RightArrow => OnArrowRight(),
                KeyCode.LeftArrow => OnArrowLeft(),
                KeyCode.KeypadEnter => OnEnter(),
                KeyCode.Return => OnEnter(),
                KeyCode.DownArrow => OnArrowDown(),
                KeyCode.UpArrow => OnArrowUp(),
                _ => false
            };

            if (eventUsed)
                Event.current.Use();
        }

        private bool OnArrowRight()
        {
            if (SelectedNode == null || DrawInSearchMode || !SelectedNode.IsFolder || SelectedNode.Expanded)
                return false;

            SelectedNode.Expanded = true;
            return true;
        }

        private bool OnArrowLeft()
        {
            if (SelectedNode == null ||DrawInSearchMode || !SelectedNode.IsFolder || !SelectedNode.Expanded)
                return false;

            SelectedNode.Expanded = false;
            return true;
        }

        private bool OnEnter()
        {
            if (SelectedNode == null)
                return false;

            if (SelectedNode.IsFolder)
            {
                SelectedNode.Expanded = ! SelectedNode.Expanded;
            }
            else
            {
                FinalizeSelection();
            }

            return true;
        }

        private bool OnArrowDown()
        {
            if (_noneElement is { IsSelected: true })
                return OnArrowDownNone();

            if (DrawInSearchMode)
                return OnArrowDownSearch();

            return OnArrowDownRegular();
        }

        private bool OnArrowDownRegular()
        {
            if (SelectedNode == null)
            {
                if (_root.ChildNodes.Count == 0)
                {
                    return false;
                }

                SelectedNode = _root.ChildNodes[0];
                return true;
            }

            if (SelectedNode.IsFolder && SelectedNode.Expanded)
            {
                SelectedNode = SelectedNode.ChildNodes[0];
                return true;
            }

            if (SelectedNode.IsRoot)
                return false;

            SelectedNode = SelectedNode.ParentNode.GetNextChild(SelectedNode);

            if (!_visibleRect.Contains(SelectedNode.Rect))
            {
                _scrollbar.RequestScrollToNode(SelectedNode, Scrollbar.NodePosition.Bottom);
            }

            return true;
        }

        private bool OnArrowDownSearch()
        {
            if (_searchModeTree.Count == 0)
                return false;

            int indexOfSelected = _searchModeTree.IndexOf(SelectedNode);

            if (indexOfSelected == _searchModeTree.Count - 1)
                return false;

            if (indexOfSelected == -1)
            {
                SelectedNode = _searchModeTree[0];
                return true;
            }

            SelectedNode = _searchModeTree[indexOfSelected + 1];
            return true;
        }

        private bool OnArrowDownNone()
        {
            var firstItem = _root.ChildNodes.FirstOrDefault();

            if (firstItem == null)
                return false;

            SelectedNode = firstItem;
            return true;
        }

        private bool OnArrowUp()
        {
            if (DrawInSearchMode)
                return OnArrowUpSearch();

            if (SelectedNode == null || SelectedNode.IsRoot)
                return false;

            if (SelectedNode.ParentNode.IsRoot)
            {
                bool isFirst = SelectedNode.ParentNode.ChildNodes.IndexOf(SelectedNode) == 0;

                if (isFirst && _noneElement != null)
                {
                    SelectedNode = _noneElement;
                    return true;
                }
            }

            var previousNode = SelectedNode.ParentNode.GetPreviousChild(SelectedNode);

            if (IsExpandedFolder(previousNode) && !previousNode.ChildNodes.Contains(SelectedNode))
            {
                // choose last item of the previous folder instead.
                previousNode = previousNode.ChildNodes[previousNode.ChildNodes.Count - 1];
            }

            SelectedNode = previousNode;

            if (!_visibleRect.Contains(SelectedNode.Rect))
            {
                _scrollbar.RequestScrollToNode(SelectedNode, Scrollbar.NodePosition.Top);
            }

            return true;
        }

        private bool OnArrowUpSearch()
        {
            if (_searchModeTree.Count == 0)
                return false;

            int indexOfSelected = _searchModeTree.IndexOf(SelectedNode);

            if (indexOfSelected <= 0)
                return false;

            SelectedNode = _searchModeTree[indexOfSelected - 1];
            return true;
        }

        private bool IsExpandedFolder(SelectionNode previousNode)
        {
            return previousNode.IsFolder && previousNode.Expanded && previousNode.ChildNodes.Count != 0;
        }

        #endregion
    }
}