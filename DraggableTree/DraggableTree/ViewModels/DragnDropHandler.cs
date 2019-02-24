using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GongSolutions.Wpf.DragDrop;
using DragDrop = GongSolutions.Wpf.DragDrop.DragDrop;

namespace DraggableTree.ViewModels
{
    public class DragnDropHandler<Node, Parent> : IDragSource, IDropTarget
        where Node : class,IFlatListItem
        where Parent : class, ICompositeFlatListItem<Node>
    {
        private readonly DraggableFlatListProvider<Node, Parent> _itemsProvider;
        public DragnDropHandler(DraggableFlatListProvider<Node, Parent> itemsProvider)
        {
            _itemsProvider = itemsProvider;
        }

        public void StartDrag(IDragInfo dragInfo)
        {
            DragDrop.DefaultDragHandler.StartDrag(dragInfo);
            var dragDropInfo = _itemsProvider.PrepareDropInfo(dragInfo.SourceItems);
            _itemsProvider.PrepareDragLayout(dragDropInfo.DraggingItems);
            dragInfo.Data = dragDropInfo.DraggingItems.SelectMany(it => it).ToList();
            _droppingItems = dragDropInfo.DroppingItems;
        }

        private IEnumerable<Node> _droppingItems;

        public bool CanStartDrag(IDragInfo dragInfo)
        {
            return DragDrop.DefaultDragHandler.CanStartDrag(dragInfo);
        }

        public void Dropped(IDropInfo dropInfo)
        {
        }

        public void DragCancelled()
        {
            DragDrop.DefaultDragHandler.DragCancelled();
            _itemsProvider.RemoveHightlighting();
        }

        public void DragOver(IDropInfo dropInfo)
        {
            DragDrop.DefaultDropHandler.DragOver(dropInfo);
            if (_itemsProvider.CanDragOver(dropInfo.InsertIndex, dropInfo.Data))
            {
                dropInfo.Effects = System.Windows.DragDropEffects.Move;
                _itemsProvider.HightlightParent(dropInfo.InsertIndex, GetIsDirectOrder(dropInfo));
            }
            else
                dropInfo.Effects = System.Windows.DragDropEffects.None;
        }

        private static bool GetIsDirectOrder(IDropInfo dropInfo)
        {
            var isDirectOrder = dropInfo.InsertPosition.
                                HasFlag(RelativeInsertPosition.AfterTargetItem);

            if (isDirectOrder && dropInfo.TargetItem == null
                && dropInfo.TargetCollection.OfType<object>().Count() ==
                dropInfo.InsertIndex)
                isDirectOrder = false;

            return isDirectOrder;
        }

        public void Drop(IDropInfo dropInfo)
        {
            _itemsProvider.Drop(dropInfo.InsertIndex, GetIsDirectOrder(dropInfo), _droppingItems);
            _itemsProvider.RemoveHightlighting();
        }

        public bool TryCatchOccurredException(Exception exception) => exception != null;
    }
}
