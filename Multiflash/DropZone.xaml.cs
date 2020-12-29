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

namespace JBlam.Multiflash
{
    /// <summary>
    /// Interaction logic for DropZone.xaml
    /// </summary>
    public partial class DropZone : UserControl
    {
        public DropZone()
        {
            InitializeComponent();
        }

        public interface IDropZoneViewModel
        {
            DragDropEffects OnDragOver(DragEventArgs args);
            DragDropEffects OnDrop(DragEventArgs args);
        }
    }
}
