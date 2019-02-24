using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
namespace DraggableTree.ViewModels
{
    public interface IFlatListItem
    {
        IFlatListItem Parent { get; set; }
        int Level { get; }
        double SizeWidth { get; set; }
        double LeftMargin { get; set; }
        double DropLeftPadding { get; set; }
        IEnumerable<IFlatListItem> ToFlatList();
        IEnumerable<IFlatListItem> Nodes { get; }
        void AttachTo(IFlatListItem parent);
    }

    public interface ICompositeFlatListItem<BaseNode> : IFlatListItem
                     where BaseNode : class, IFlatListItem
    {
        bool IsExpanded { get; }
        BindableCollection<BaseNode> SubItems { get; }
        bool IsHighLighted { get; set; }
    }

}
