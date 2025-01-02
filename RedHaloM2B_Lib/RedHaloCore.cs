using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Max;

namespace RedHaloM2B
{
    public class RedHaloCore
    {
        public static IGlobal Global
        {
            get { return GlobalInterface.Instance; }
        }

        public static IInterface14 Core
        {
            get { return Global.COREInterface14; }
        }

        public static IInterval Forever
        {
            get { return Global.Interval.Create(int.MinValue, int.MaxValue); }
        }
    }
}
