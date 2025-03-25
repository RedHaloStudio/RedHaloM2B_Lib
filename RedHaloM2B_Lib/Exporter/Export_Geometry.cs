using Autodesk.Max;
using RedHaloM2B.Nodes;

namespace RedHaloM2B
{
    partial class Exporter
    {
        public static RedHaloGeometry ExportGeometry(IINode maxNode, int index)
        {
            IMatrix3 tm = maxNode.GetNodeTM(0, RedHaloCore.Forever);
            IPoint3 offset_object = RedHaloCore.Global.Point3.Create();

            // Group pivot offset
            if (maxNode.IsGroupHead)
            {
                var obj_pivot = tm.Trans; // Get pivot position 
                var obj_center = maxNode.EvalWorldState(0, true).Obj.GetWorldBoundBox(0, maxNode, RedHaloCore.Core.GetViewExp(0)).Center;
                offset_object = obj_pivot.Subtract(obj_center);
            }

            string originalName = maxNode.Name;
            string newName = $"Mesh_{index:D5}";

            // Set object name to new name
            maxNode.Name = newName;

            RedHaloGeometry redHaloGeometry = new RedHaloGeometry
            {
                Name = newName,
                OriginalName = originalName,
                Transform = RedHaloTools.ConvertMatrixToString(tm),
                PivotOffset = $"{offset_object.X},{offset_object.Y},{offset_object.Z}",
                Renderable = maxNode.Renderable,
                VisibleCamera = maxNode.PrimaryVisibility,
                VisibleSpecular = maxNode.SecondaryVisibility,
                VisibleVolume = maxNode.ApplyAtmospherics,
                CastShadow = maxNode.CastShadows
            };

            return redHaloGeometry;
        }
    }
}
