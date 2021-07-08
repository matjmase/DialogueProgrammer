using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DialogueProgrammer.Common.Extensions
{
    public static class UiExtensions
    {
        public static DependencyObject GetParentOfType<T>(this DependencyObject fElement)
        {
            var parent = fElement;
            while (parent != null && !(parent is T))
            {
                var feParent = parent;
                parent = VisualTreeHelper.GetParent(feParent);
            }

            if (parent == null)
                return null;
            else
                return parent;
        }

        public static Point GetLocalCanvasPoint(this FrameworkElement element)
        {
            var adjusted = element.TranslatePoint(new Point(), (FrameworkElement)((DependencyObject)element).GetParentOfType<Canvas>());
            var dim = new Point(element.ActualWidth, element.ActualHeight);

            adjusted = new Point(adjusted.X + dim.X / 2, adjusted.Y + dim.Y / 2);
            return adjusted;
        }

        public static DependencyObject BredthGetFirstChildOfType<T>(this DependencyObject fElement)
        {
            Queue<DependencyObject> children = new Queue<DependencyObject>();
            children.Enqueue(fElement);

            do
            {
                var thisChild = children.Dequeue();
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(thisChild); i++)
                {
                    var newChild = VisualTreeHelper.GetChild(thisChild, i);
                    if ((newChild) is T)
                        return (newChild);
                    else
                        children.Enqueue(newChild);
                }
            } while (children.Count > 0);

            throw null;
        }
    }
}
