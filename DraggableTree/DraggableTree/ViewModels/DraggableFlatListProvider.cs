using Caliburn.Micro;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace DraggableTree.ViewModels
{
    public class DragInfo<Node>
        where Node : class, IFlatListItem
    {
        public IEnumerable<IEnumerable<IFlatListItem>> DraggingItems { get; private set; }
        public IEnumerable<Node> DroppingItems { get; private set; }
        public DragInfo(IEnumerable<IEnumerable<IFlatListItem>> draggingItems, IEnumerable<Node> droppingItems)
        {
            DraggingItems = draggingItems;
            DroppingItems = droppingItems;
        }
    }

    public class DraggableFlatListProvider<Node, Parent> : FlatListProvider<Node, Parent>
        where Node : class, IFlatListItem
        where Parent : class, ICompositeFlatListItem<Node>
    {
        public DraggableFlatListProvider(BindableCollection<Node> treeItems)
            : base(treeItems)
        {
        }

        private static IEnumerable<Node> GetBaseNodes(object obj)
        {
            if (obj == null)
                return Enumerable.Empty<Node>();

            var child = obj as Node;
            if (child != null)
                return new List<Node> { child };

            var enumer = obj as IEnumerable;
            if (enumer == null)
                return Enumerable.Empty<Node>();

            var items = enumer.OfType<Node>();
            return items;
        }

        public DragInfo<Node> PrepareDropInfo(object obj)
        {
            var initiatlItems = GetBaseNodes(obj).ToList();
            var draggingInfo = new Dictionary<Node, List<IFlatListItem>>();
            foreach (var item in initiatlItems.OrderByDescending(it => it.Level))
            {
                var draggedSequences = draggingInfo.Where(it => it.Key.Level > item.Level).
                                             SelectMany(it => it.Value).ToList();
                //Remove created higher sequences
                var sequence = item.ToFlatList().Except(draggedSequences).ToList();

                draggingInfo[item] = sequence;
            }
            //Restore initial selected order
            var draggingItems = initiatlItems.Select(it => draggingInfo[it]).ToList();
            var droppingItems = initiatlItems.ToList();
            var dragInfo = new DragInfo<Node>(draggingItems, droppingItems);
            return dragInfo;
        }

        public void PrepareDragLayout(IEnumerable<IEnumerable<IFlatListItem>> draggingItems)
        {
            var draggingItemInfos = draggingItems.
                Select(it => new
                {
                    Items = it,
                    Level = it.First().Level
                }).
                GroupBy(it => it.Level).
                Select(it => new
                {
                    Items = it.Where(i => i.Level == it.Key).SelectMany(i => i.Items).ToList(),
                    DropLeftPadding = _levelInfos[it.Key].Items.Max(i => i.LeftMargin)
                }).ToList();

            draggingItemInfos.ForEach(it =>
                it.Items.ForEach(i => i.DropLeftPadding = it.DropLeftPadding));
        }

        public bool CanDragOver(int index, object obj)
        {
            var items = GetBaseNodes(obj);
            return !AnyItemInsideOwnChildren(index, items);
        }

        private bool AnyItemInsideOwnChildren(int index, IEnumerable<Node> items)
        {
            var item = Items.ElementAtOrDefault(index - 1);
            return item != null && item.Nodes.Any(it => items.Contains(it));
        }

        private Parent GetTargetPlace(int index, bool isDirectOrder)
        {
            var item = Items.ElementAtOrDefault(index - 1);
            if (item == null)
                return null;

            var nodes = item.Nodes.Take(2);
            return isDirectOrder ? nodes.OfType<Parent>().FirstOrDefault() :
                                   nodes.Reverse().OfType<Parent>().FirstOrDefault();
        }

        private Parent _lastHightlightedParent;
        public void HightlightParent(int index, bool isDirectOrder)
        {
            RemoveHightlighting();

            var highlightedParent = GetTargetPlace(index, isDirectOrder);

            _lastHightlightedParent = highlightedParent;
            if (highlightedParent != null)
                highlightedParent.IsHighLighted = true;
        }

        public void RemoveHightlighting()
        {
            if (_lastHightlightedParent != null)
                _lastHightlightedParent.IsHighLighted = false;
        }

        public void Drop(int index, bool isDirectOrder, IEnumerable<Node> items)
        {
            var droppingItems = items.ToList();

            BindableCollection<Node> newParentCollection = null;
            var newParent = GetTargetPlace(index, isDirectOrder);
            if (newParent != null)
                newParentCollection = newParent.SubItems;

            newParentCollection = newParentCollection ?? _treeItems;
            var insertPlace = Items.ElementAtOrDefault(index);

            _ignoreEvents = true;
            RemoveItemsFromOldParents(droppingItems);
            AddItemsTo(newParentCollection, insertPlace, droppingItems);
            droppingItems.ForEach(it => it.AttachTo(newParent));
            _ignoreEvents = false;

            RefreshFlatList();
        }

        private void RemoveItemsFromOldParents(IEnumerable<Node> items)
        {
            var groupChildren = items.GroupBy(i => i.Parent)
                                .Select(i => new
                                {
                                    Children = i.Where(it => it.Parent == i.Key).ToList(),
                                    Parent = i.Key as Parent,
                                }).
                                OrderByDescending(it => it.Children.First().Level).ToList();

            groupChildren.ForEach(gc =>
            {
                var OldParentItems = gc.Parent == null ? _treeItems : gc.Parent.SubItems;
                OldParentItems.RemoveRange(gc.Children);
            });
        }

        private static void AddItemsTo(BindableCollection<Node> newParentCollection, IFlatListItem insertPlace, IEnumerable<Node> items)
        {
            var newItems = new List<Node>();
            var position = newParentCollection.IndexOf(insertPlace as Node);
            if (position != -1)
            {
                newItems.AddRange(newParentCollection.Take(position));
                newItems.AddRange(items);
                newItems.AddRange(newParentCollection.Skip(position));
            }
            else
            {
                newItems.AddRange(newParentCollection);
                newItems.AddRange(items);
            }
            newParentCollection.IsNotifying = false;
            newParentCollection.Clear();
            newParentCollection.AddRange(newItems);
            newParentCollection.IsNotifying = true;
            newParentCollection.Refresh();
        }
    }
}
