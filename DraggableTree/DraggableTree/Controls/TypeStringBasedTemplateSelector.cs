using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DraggableTree.Controls
{
    public class TypeStringBasedTemplateSelector : DataTemplateSelector
    {
        private readonly Dictionary<string, DataTemplate> _templates = new Dictionary<string, DataTemplate>();
        public Dictionary<string, DataTemplate> Templates
        {
            get { return this._templates; }
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
                return base.SelectTemplate(item, container);

            var key = item.GetType().Name;
            DataTemplate template = null;

            if (this.Templates.TryGetValue(key, out template))
                return template;

            return base.SelectTemplate(item, container);
        }
    }
}
