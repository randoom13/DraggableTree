using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DraggableTree.ViewModels
{
    public class CompositeViewModel : BaseTreeItem, ICompositeFlatListItem<BaseTreeItem>
    {
        public CompositeViewModel(string title, IFlatListItem parent)
            : base(title, parent)
        {
            SubItems.CollectionChanged += CollectionChangedHandler;
        }

        public CompositeViewModel(string title)
            : base(title)
        {
            SubItems.CollectionChanged += CollectionChangedHandler;
        }

        private void CollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => HasItems);
            UpdateVisibleSubItems();
        }

        private readonly BindableCollection<BaseTreeItem> _items = new BindableCollection<BaseTreeItem>();
        public BindableCollection<BaseTreeItem> SubItems { get { return _items; } }
        public Boolean HasItems { get { return SubItems.Any(); } }

        public override IEnumerable<IFlatListItem> ToFlatList()
        {
            yield return this;
            foreach (var it in _visibleSubItems.SelectMany(si => si.ToFlatList()))
                yield return it;
        }

        private bool _isHighLighted;
        public bool IsHighLighted
        {
            get { return _isHighLighted; ; }
            set
            {
                _isHighLighted = value;
                NotifyOfPropertyChange(() => IsHighLighted);
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                UpdateVisibleSubItems();
                NotifyOfPropertyChange(() => IsExpanded);
            }
        }

        private void UpdateVisibleSubItems()
        {
            _visibleSubItems.Clear();
            if (IsExpanded)
                _visibleSubItems.AddRange(SubItems);
        }
        private List<BaseTreeItem> _visibleSubItems = new List<BaseTreeItem>();
    }
}
