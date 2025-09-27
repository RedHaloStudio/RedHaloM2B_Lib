using Autodesk.Max;
using Autodesk.Max.Plugins;
using System;
using System.Diagnostics;

namespace RedHaloM2B
{
    /// <summary>
    /// Public managed c# class to load assembly 
    /// </summary>
    public static class Loader
    {
        /// <summary>
        /// Init point for assembly loader
        /// </summary>
        public static void AssemblyMain()
        {
            //RedHaloMenuUtilities.SetupMenuHandlers();
        }
    }

    /// <summary>
    /// Utilitiy class to handle menus at global level.
    /// </summary>
    public class RedHaloMenuUtilities
    {
        static IActionItem m_mouseAction;

        /// <summary>
        /// Note, this method is iterating all action tbales and actions (for this example to also be able to find built-in actions).
        /// Normally, if you only need your own, you can use actionManager.FindTable(context); but when using the 
        /// CuiActionCommandAdapter, the context is not available, so this method is still the best option. 
        /// To find only your actions, if you make all your CuiActionCommandAdapter actions use the same category(s), 
        /// then you can use the table name to find your table (category), then iterate just your the commands.
        /// </summary>
        static void LookupActions()
        {
            var actionManager = RedHaloCore.Core.ActionManager;
            //actionManager.FindTable();
            for (var actionTableIndex = 0; actionTableIndex < actionManager.NumActionTables; ++actionTableIndex)
            {
                var actionTable = actionManager.GetTable(actionTableIndex);
                //actionTable.
                for (var actionIndex = 0; actionIndex < actionTable.Count; ++actionIndex)
                {
                    var action = actionTable[actionIndex];
                    // finds our known cui action.
                    if (action != null && action.DescriptionText == RedHaloMenuStrings.menuActionName)
                    {
                        m_mouseAction = action;
                        uint n = actionTable.ContextId;
                    }
                }
            }
        }

        static GlobalDelegates.Delegate5 m_SystemStartupDelegate;

        private static void MenuSystemStartupHandler(IntPtr objPtr, INotifyInfo infoPtr)
        {
            IIMenuManager manager = RedHaloCore.Core.MenuManager;
            IIMenu menu = manager.FindMenu(RedHaloMenuStrings.menuName);
            if (menu == null)
                InstallMenu();
        }

        /// <summary>
        /// This method setups all possible callbacks to handle menus add/remove in various user actions. For example, 
        /// if the user changes workspaces, the entire menu bar is updated, so this handles adding it in all workspaces as switched.
        /// The drawback is that 3ds Max calls some more than once, so you get some seemingly unnecessary add/remove. But it's safer 
        /// if you always want your menu present.
        /// Of course you could also call the add/remove in other conexts and callbacks depending on the 3ds max state where 
        /// you need your menu to display.
        /// </summary>
        public static void SetupMenuHandlers()
        {
            m_SystemStartupDelegate = new GlobalDelegates.Delegate5(MenuSystemStartupHandler);

            // this will add it at startup and for some scenerios is enough. But a commercial app shuold consider above for workspace switching.
            RedHaloCore.Global.RegisterNotification(m_SystemStartupDelegate, null, SystemNotificationCode.SystemStartup);
        }

        /// <summary>
        /// Installs the menu from scratch
        /// </summary>
        /// <returns>1 when successfully installed, or 0 in error state</returns>
        private static uint InstallMenu()
        {

            LookupActions();

            IIActionManager actionManager = RedHaloCore.Core.ActionManager;
            IIMenuManager menuManager = RedHaloCore.Core.MenuManager;

            IIMenu mainMenuBar = menuManager.MainMenuBar;
            IIMenu rhExportMenu = RedHaloCore.Global.IMenu;
            rhExportMenu.Title = RedHaloMenuStrings.menuName;
            menuManager.RegisterMenu(rhExportMenu, 0);

            // Launch option
            {
                IIMenuItem menuItem1 = RedHaloCore.Global.IMenuItem;
                menuItem1.ActionItem = m_mouseAction; // uses text from ActionItem.DescriptionText
                rhExportMenu.AddItem(menuItem1, -1);
            }
            // }
            IIMenuItem adnMenu = RedHaloCore.Global.IMenuItem;
            adnMenu.Title = RedHaloMenuStrings.menuName;
            adnMenu.SubMenu = rhExportMenu;
            menuManager.MainMenuBar.AddItem(adnMenu, -1);
            RedHaloCore.Core.MenuManager.UpdateMenuBar();

            return 1;
        }

        /// <summary>
        /// removes the menu
        /// </summary>
        /// <param name="menuName"></param>
        private static void RemoveMenu(string menuName)
        {
            IIActionManager actionManager = RedHaloCore.Core.ActionManager;
            IIMenuManager menuManager = RedHaloCore.Core.MenuManager;
            IIMenu customMenu = menuManager.FindMenu(RedHaloMenuStrings.menuName);

            menuManager.UnRegisterMenu(customMenu);
            RedHaloCore.Global.ReleaseIMenu(customMenu);
            customMenu = null;
        }
    }
}
