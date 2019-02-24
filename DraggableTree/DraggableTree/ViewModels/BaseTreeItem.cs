using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
namespace DraggableTree.ViewModels
{
    public abstract class BaseTreeItem : PropertyChangedBase, IFlatListItem
    {
        private string _title;
        public string Title 
        {
            get { return _title; }
            set { _title = value; NotifyOfPropertyChange(() => Title); }
        }

        public BaseTreeItem(string title, IFlatListItem parent)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");

            Title = title;
            AttachTo(parent);
        }

        public BaseTreeItem(string title)
        {
            Title = title;
        }

        public IFlatListItem Parent { get; set; }
        public double SizeWidth { get; set; }
        public int Level
        {
            get { return Parent != null ? Parent.Level + 1 : 0; }
        }

        public double DropLeftPadding { get; set; }
        public double DropLeftMargin { get { return LeftMargin - DropLeftPadding; } }

        private double _leftMargin;
        public double LeftMargin
        {
            get { return _leftMargin; }
            set
            {
                _leftMargin = value;
                NotifyOfPropertyChange(() => LeftMargin);
            }
        }

        public virtual IEnumerable<IFlatListItem> ToFlatList()
        {
            yield return this;
        }

        public IEnumerable<IFlatListItem> Nodes
        {
            get
            {
                IFlatListItem item = this;
                while (item != null)
                {
                    yield return item;
                    item = item.Parent;
                }
            }
        }

        public void AttachTo(IFlatListItem parent)
        {
            Parent = parent;
            LeftMargin = parent == null ? 0 : parent.SizeWidth;
        }
    }
}
