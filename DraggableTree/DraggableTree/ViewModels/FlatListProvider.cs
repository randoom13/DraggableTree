using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace DraggableTree.ViewModels
{
    public class LevelInfo
    {
        public List<IFlatListItem> Items { get; private set; }
        public LevelInfo(List<IFlatListItem> items)
        {
            Items = items;
        }
    }

    public class FlatListProvider<Node, Parent>
        where Node : class, IFlatListItem
         where Parent : class, ICompositeFlatListItem<Node>
    {
        protected readonly BindableCollection<Node> _treeItems;
        protected readonly List<LevelInfo> _levelInfos = new List<LevelInfo>();

        public FlatListProvider(BindableCollection<Node> treeItems)
        {
            _treeItems = treeItems;
            RefreshFlatList();
            _treeItems.CollectionChanged += CollectionChangedHandler;
        }

        private void CollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!_ignoreEvents)
                RefreshFlatList();
        }

        public void OnExpandedChanged(Node child)
        {
            if (!_ignoreEvents)
                RefreshFlatList();
        }

        public void OnSizeChanged(Node child, double actualWidth)
        {
            if (!_ignoreEvents)
                ModifyItemsSize(child, actualWidth);
        }

        private void ModifyItemsSize(Node child, double width)
        {
            if (child == null)
                return;
            var levelInfo = _levelInfos.ElementAtOrDefault(child.Level);
            if (levelInfo == null)
                return;

            if (child.SizeWidth != width)
                child.SizeWidth = width;

            var nextLevel = child.Level + 1;
            if (nextLevel < _levelInfos.Count)
            {
                var maxSizeWidth = levelInfo.Items.Max(it => it.SizeWidth);
                _levelInfos[nextLevel].Items.ForEach(
                    it =>
                    {
                        if (it.LeftMargin != maxSizeWidth)
                            it.LeftMargin = maxSizeWidth;
                    });
            }

        }

        private List<IFlatListItem> ParseTreeToFlatList()
        {
            var newItems = new List<IFlatListItem>();
            newItems.AddRange(_treeItems.SelectMany(ti => ti.ToFlatList()));
            return newItems;
        }

        protected void RefreshFlatList()
        {
            var newItems = ParseTreeToFlatList();
            Items.IsNotifying = false;
            foreach (var oldParent in Items.OfType<Parent>())
                oldParent.SubItems.CollectionChanged -= CollectionChangedHandler;

            Items.Clear();
            Items.AddRange(newItems);
            Items.IsNotifying = true;
            RefreshLevelInfos();
            foreach (var newParent in Items.OfType<Parent>())
                newParent.SubItems.CollectionChanged += CollectionChangedHandler;

            Items.Refresh();
        }

        private void RefreshLevelInfos()
        {
            _levelInfos.Clear();
            var newlevelInfos = Items.GroupBy(i => i.Level)
                                .Select(i => new 
                                {
                                    Items = i.Where(it => it.Level == i.Key).ToList(),
                                    Level = i.Key
                                }).OrderBy(it=>it.Level);

            _levelInfos.AddRange(newlevelInfos.Select(it=> new LevelInfo(it.Items)));
        }
        private readonly BindableCollection<IFlatListItem> _items =
            new BindableCollection<IFlatListItem>();
        public BindableCollection<IFlatListItem> Items
        {
            get { return _items; }
        }

        protected bool _ignoreEvents = false;
    }
}
