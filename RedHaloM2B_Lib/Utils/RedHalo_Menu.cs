using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;
using Autodesk.Max.Plugins;

#if MAX2025 || MAX2026
using UiViewModels.Actions;
#else
using Autodesk.Max.IQuadMenuContext;
#endif

namespace RedHaloM2B
{
    public class RedHalo_Menu
    {
        class GlobalUtility : GUP
        {
            public const string ActionTableName = "Redhalo Actions";

            IIMenu menu;
            IIMenuItem menuItem;
            IIMenuItem menuItemRedHalo;

            uint idActionTable;
            IActionTable actionTable;
            IActionCallback actionCallback;

            public override uint Start
            {
                get
                {
                    IIActionManager actionManager = RedHaloCore.Core.ActionManager;

                    // Set up global actions
                    idActionTable = (uint)actionManager.NumActionTables;
                    Debug.Print($"{idActionTable}");
                    return 0;
                }
            }

            public override void Stop()
            {
                throw new NotImplementedException();
            }
        }

    }
}
