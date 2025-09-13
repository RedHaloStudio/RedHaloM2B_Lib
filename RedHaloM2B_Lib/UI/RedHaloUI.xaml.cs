using Autodesk.Max;
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

namespace RedHaloM2B.UI
{
    /// <summary>
    /// RedHaloUI.xaml 的交互逻辑
    /// </summary>
    public partial class RedHaloUI : UserControl
    {
        Window m_winParent;
        public RedHaloUI(Window winParent)
        {
            m_winParent = winParent;
            InitializeComponent();

            IIColorManager cm = RedHaloCore.Global.ColorManager;
            //System.Drawing.Color bk_color = cm.GetColor(GuiColors.Background, Autodesk.Max.IColorManager.State.Normal);

            // due to a bug in the 3ds Max .NET API before 2017, you would have to reverse the R and B values to assign color properly. 
            // The Autodesk.Max assembly mapped them incorrectly
            // pre 2017: Color mcolorBack = SColor.FromRgb(dcolBack.B, dcolBack.G, dcolBack.R);

            // Get current background color and match our dialog to it
            //Color mcolorBack = Color.FromRgb(bk_color.R, bk_color.G, bk_color.B);
            //Brush colorBack = new SolidColorBrush(mcolorBack);
             //Note, if you want just a fixed color, you can comment this out and use the XAML defined value.
            //RedHaloUI.Background = colorBack;

            // Get current text color and match our dialog to it.
            //System.Drawing.Color dcolText = cm.GetColor(GuiColors.Text, Autodesk.Max.IColorManager.State.Normal);
            // pre 2017: Color mcolorText = Color.FromRgb(dcolText.B, dcolText.G, dcolText.R);
            //Color mcolorText = Color.FromRgb(dcolText.R, dcolText.G, dcolText.B);
            //Brush colorText = new SolidColorBrush(mcolorText);

            // To use pure white, we can just set a system brush.
            //Brush colorText = Brushes.White;
            //export_mode_lbl.Foreground = colorText;
            //export_option_lbl.Foreground = colorText;
            //export_btn.Foreground = colorText;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This is a test message from RedHaloM2B.UI.RedHaloUI.");
        }
    }
}
