using ColossalFramework.UI;

namespace ExportPower
{
    public class Utils
    {
        private static readonly Logger logger = new Logger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void DumpHierarchy(UIComponent c, string prefix)
        {
            logger.Log($"{prefix}{c.name} ({c.GetType().Name}) {c.relativePosition}/{c.size}");
            foreach (var child in c.components)
            {
                DumpHierarchy(child, prefix + " ");
            }
        }
    }
}