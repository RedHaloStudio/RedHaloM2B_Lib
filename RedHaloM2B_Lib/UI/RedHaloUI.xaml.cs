using Autodesk.Max;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            IIColorManager colorMan = RedHaloCore.Global.ColorManager;
            System.Drawing.Color bk_color = colorMan.GetColor(GuiColors.Background, Autodesk.Max.IColorManager.State.Normal);

            // due to a bug in the 3ds Max .NET API before 2017, you would have to reverse the R and B values to assign color properly. 
            // The Autodesk.Max assembly mapped them incorrectly
            // pre 2017: Color mcolorBack = SColor.FromRgb(dcolBack.B, dcolBack.G, dcolBack.R);

            // Get current background color and match our dialog to it
            Color mcolorBack = Color.FromRgb(bk_color.R, bk_color.G, bk_color.B);
            Brush colorBack = new SolidColorBrush(mcolorBack);
            //Note, if you want just a fixed color, you can comment this out and use the XAML defined value.
            RedHaloUILayout.Background = colorBack;

            // Get current text color and match our dialog to it.
            System.Drawing.Color dcolText = colorMan.GetColor(GuiColors.Text, Autodesk.Max.IColorManager.State.Normal);
            // pre 2017: Color mcolorText = Color.FromRgb(dcolText.B, dcolText.G, dcolText.R);
            Color mcolorText = Color.FromRgb(dcolText.R, dcolText.G, dcolText.B);
            Brush colorText = new SolidColorBrush(mcolorText);

            // To use pure white, we can just set a system brush.
            //Brush colorText = Brushes.White;
            //export_mode_lbl.Foreground = colorText;
            //export_option_lbl.Foreground = colorText;
            //export_btn.Foreground = colorText;

            // Set default states
            // 检测有没有安装USD插件，如果没有安装，就禁用USD选项
            export_mode_fast.IsEnabled = isUSDPluginInstalled;
            export_mode_slow.IsChecked = !isUSDPluginInstalled;
            
        }

        public bool isUSDPluginInstalled
        {
            get
            {
                return RedHaloTools.IsPluginInstalled("USD Importer");
            }
        }

        public bool exportMode
        {
            get 
            {
                return ((bool)export_mode_fast.IsChecked);
            }
        }

        public bool isSelected
        {
            get 
            { 
                return (bool)export_selected.IsChecked; 
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string exportFileFormat = "USD";

            if (!exportMode)
            {
                exportFileFormat = "FBX";
            }

            bool export_selected = isSelected;
            bool explodeGroup = false;
            bool scaleScene = true;
            bool convertToPoly = false;

            int result = ExportScene.SceneExporter(exportFileFormat, explodeGroup, convertToPoly);
            if (result == 0)
            {
                MessageBox.Show($"Export Success", "RedHalo Studio", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Export Failed, error code {result}", "RedHalo Studio", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
