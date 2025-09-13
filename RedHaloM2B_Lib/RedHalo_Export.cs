using ManagedServices;
using RedHaloM2B.UI;
using System;
using System.Diagnostics;
using System.Windows;
using UiViewModels.Actions;

namespace RedHaloM2B
{
    public abstract class RedHalo_CuiActionCommandAdapter : CuiActionCommandAdapter
    {
        public override string ActionText
        {
            get { return InternalActionText; }
        }

        public override string Category
        {
            get { return InternalCategory; }
        }

        public override string InternalActionText
        {
            get { return CustomActionText; }
        }

        public override string InternalCategory
        {
            get { return "RedHaloTools C#"; }
        }

        public override void Execute(object parameter)
        {
            try
            {
                CustomExecute(parameter);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Exception Error: {e.Message}");
            }
        }

        public abstract string CustomActionText { get; }
        public abstract void CustomExecute(object parameter);
    }
    /// <summary>
    /// This is the specific action item that can be added to a UI element like the menus.
    /// </summary>
    public class AdnCui_ExplodeGeometry : RedHalo_CuiActionCommandAdapter
    {
        public override string CustomActionText
        {
            get { return "RedHalo Export Tools"; }
        }

        public override void CustomExecute(object parameter)
        {
            try
            {
                Window dialog = new Window();
                dialog.Title = "RedHalo Studio";
                dialog.SizeToContent = SizeToContent.Manual;
                dialog.Width = 300;
                dialog.Height = 350;
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dialog.ShowInTaskbar = false;
                dialog.ResizeMode = ResizeMode.NoResize;

                RedHaloUI redHaloUI = new RedHaloUI(dialog);

                dialog.Content = redHaloUI;

                System.Windows.Interop.WindowInteropHelper windowHandle = new System.Windows.Interop.WindowInteropHelper(dialog);
                windowHandle.Owner = AppSDK.GetMaxHWND();
                AppSDK.ConfigureWindowForMax(dialog);

                dialog.Show(); //modal version; this prevents changes being made to model while our dialog is running, etc.
                //dialog.ShowDialog(); //窗口前置，阻塞后续代码执行
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }
    }
}
