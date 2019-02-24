using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
namespace DraggableTree.ViewModels
{
    public class LeafViewModel : BaseTreeItem
    {
        public LeafViewModel(string title)
            : base(title)
        {
        }

        public LeafViewModel(string title, IFlatListItem parent)
            : base(title, parent)
        {
        }
    }
}
