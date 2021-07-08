using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DialogueProgrammer.Views
{
    /// <summary>
    /// Interaction logic for HandleWindow.xaml
    /// </summary>
    public partial class HandleWindow : UserControl
    {
        public static readonly DependencyProperty AdditionalContentProperty =
            DependencyProperty.Register(
            "AdditionalContent", typeof(object),
            typeof(HandleWindow)
            );
        public object AdditionalContent
        {
            get { return (object)GetValue(AdditionalContentProperty); }
            set { SetValue(AdditionalContentProperty, value); }
        }

        public static readonly DependencyProperty CloseVisibilityProperty =
            DependencyProperty.Register(
            "CloseVisibility", typeof(bool),
            typeof(HandleWindow)
            );
        public bool CloseVisibility
        {
            get { return (bool)GetValue(AdditionalContentProperty); }
            set { SetValue(AdditionalContentProperty, value); }
        }


        public HandleWindow()
        {
            InitializeComponent();
        }
    }
}
