using Autodesk.Max;
using System;
using System.Diagnostics;

namespace RedHaloM2B
{
    public class RedHaloMenuUtilities
    {
        static GlobalDelegates.Delegate5 m_MenuPostLoadDelegate;
        static GlobalDelegates.Delegate5 m_MenuPreSaveDelegate;
        static GlobalDelegates.Delegate5 m_MenuPostSaveDelegate;
        static GlobalDelegates.Delegate5 m_SystemStartupDelegate;

        static string menuName = "RedHalo...";

        private static void MenuPostLoadHandler(IntPtr objPtr, INotifyInfo infoPtr)
        {
            InstallMenu();
        }
        private static void MenuPreSaveHandler(IntPtr objPtr, INotifyInfo infoPtr)
        {
            var global = GlobalInterface.Instance;
            var ip = global.COREInterface14;
            IIMenuManager manager = ip.MenuManager;
            IIMenu menu = manager.FindMenu(menuName);
            if (menu != null)
                RemoveMenu(menuName);
        }
        private static void MenuPostSaveHandler(IntPtr objPtr, INotifyInfo infoPtr)
        {
            Debug.Print("Installing RedHalo menu...");
            InstallMenu();
        }
        private static void MenuSystemStartupHandler(IntPtr objPtr, INotifyInfo infoPtr)
        {
            Debug.Print("Installing RedHalo menu...");
            InstallMenu();
        }

        private static void InstallMenu()
        {
            Debug.Print("Installing RedHalo menu...");
            IIMenuManager menuManager = RedHaloCore.Core.MenuManager;

            var menu = menuManager.FindMenu(menuName);

            if (menu != null)
            {
                menuManager.UnRegisterMenu(menu);
                RedHaloCore.Global.ReleaseIMenu(menu);
                menu = null;
            }

            // Create a new menu
            menu = RedHaloCore.Global.IMenu;
            menu.Title = menuName;
            menuManager.RegisterMenu(menu, 0);

            menuManager.UpdateMenuBar();
        }

        /// <summary>
        /// removes the menu
        /// </summary>
        /// <param name="menuName"></param>
        private static void RemoveMenu(string menuName)
        {
            IGlobal global = GlobalInterface.Instance;
            IIActionManager actionManager = global.COREInterface.ActionManager;
            IIMenuManager menuManager = global.COREInterface.MenuManager;
            IIMenu customMenu = menuManager.FindMenu(menuName);

            menuManager.UnRegisterMenu(customMenu);
            global.ReleaseIMenu(customMenu);
            customMenu = null;
        }

    }
}
