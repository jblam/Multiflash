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

namespace JBlam.Multiflash.App
{
    /// <summary>
    /// Interaction logic for InitView.xaml
    /// </summary>
    public partial class InitView : UserControl
    {
        public InitView()
        {
            InitializeComponent();
        }

        InitViewModel ViewModel => (InitViewModel)DataContext;

        private void DockPanel_DragEnter(object sender, DragEventArgs e)
        {
            ViewModel.OnDragEnter(e);
        }

        private void DockPanel_Drop(object sender, DragEventArgs e)
        {
            ViewModel.OnDrop(e);
        }

        private void Border_DragLeave(object sender, DragEventArgs e)
        {
            ViewModel.OnDragLeave(e);
        }

        private void Border_DragOver(object sender, DragEventArgs e)
        {
            ViewModel.OnDragOver(e);
        }
    }
}
