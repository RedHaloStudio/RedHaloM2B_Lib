using Autodesk.Max;
using System.Diagnostics;

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

        static void Initialize()
        {
            Debug.Print("RedHaloCore initialized.");
        }

        public static void AssemblyMain()
        {
            Initialize();
        }

        public static void AssemblyInitializationCleanup()
        {
        }

        public static void AssemblyShutdown()
        {
        }
    }
}
