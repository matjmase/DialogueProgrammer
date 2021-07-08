using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;

namespace DialogueProgrammer.Behaviors
{
    public class RectangleClickBehavior : Behavior<Rectangle>
    {
        public static readonly DependencyProperty ClickProperty =
            DependencyProperty.Register(
            "Click", typeof(ICommand),
            typeof(RectangleClickBehavior)
            );
        public ICommand Click
        {
            get { return (ICommand)GetValue(ClickProperty); }
            set { SetValue(ClickProperty, value); }
        }

        public static readonly DependencyProperty LeftClickProperty =
            DependencyProperty.Register(
            "LeftClick", typeof(bool),
            typeof(RectangleClickBehavior), new PropertyMetadata(true)
            );
        public bool LeftClick
        {
            get { return (bool)GetValue(LeftClickProperty); }
            set { SetValue(ClickProperty, value); }
        }

        private bool _clickStaged;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
            AssociatedObject.MouseUp += AssociatedObject_MouseUp;
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
        }

        private void AssociatedObject_MouseLeave(object sender, MouseEventArgs e)
        {
            _clickStaged = false;
        }

        private void AssociatedObject_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_clickStaged && ((LeftClick && e.LeftButton == MouseButtonState.Released) || (!LeftClick && e.RightButton == MouseButtonState.Released)))
            {
                if (Click.CanExecute(e))
                {
                    Click.Execute(e);
                    _clickStaged = false;
                }
            }

        }

        private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((LeftClick && e.LeftButton == MouseButtonState.Pressed) || (!LeftClick && e.RightButton == MouseButtonState.Pressed))
                _clickStaged = true;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
            AssociatedObject.MouseUp -= AssociatedObject_MouseUp;
            AssociatedObject.MouseLeave -= AssociatedObject_MouseLeave;
            base.OnDetaching();
        }
    }
}
