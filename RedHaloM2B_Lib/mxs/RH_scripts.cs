namespace RedHaloM2B
{
    class scripts
    {
        public static readonly string mxs = "function CleanupMaterial mat = (\n" +
            "    MAT_Type = classof mat\n " +
            "    case MAT_Type of (" +
            "            VrayMtlWrapper:(\n" +
            "            name = mat.name\n" +
            "            _t = VRayMtl name:name\n" +
            "            if mat.baseMtl != undefined do (\n" +
            "                _t = mat.baseMtl\n" +
            "                _t.name = name\n" +
            "            )\n" +
            "            replaceinstances mat _t\n" +
            "        )\n" +
            "        CoronaSelectMtl:(\n" +
            "            name = mat.name\n" +
            "            selected = mat.selected\n" +
            "            submtl = mat.materials[selected+1]\n" +
            "            if(submtl != undefined) do (\n" +
            "                submtl.name = name\n" +
            "                replaceinstances mat submtl\n" +
            "            )\n" +
            "        )\n" +
            "        VRayBumpMtl:(\n" +
            "            name = mat.name\n" +
            "            base_mtl = mat.base_mtl\n" +
            "            if base_mtl != undefined do (\n" +
            "                base_mtl.name = name\n" +
            "                replaceinstances mat base_mtl\n" +
            "            )\n" +
            "        )\n" +
            "        default:(\n" +
            "        )\n" +
            "    )\n" +
            ")\n" +
            "-- 处理不支持的材质\r\n" +
            "for cls in material.classes do (\n" +
            "    for m in getClassInstances cls do (\n" +
            "        try(\n" +
            "            CleanupMaterial m\n" +
            "        )catch(\n" +
            "        )\n" +
            "    )\n" +
            ")";

        public static readonly string mxs_delete_zero_face = "" +
            "sel = #()\n" +
            "for i in Geometry do (\n" +
            "    try(if getNumFaces i == 0 then append sel i)catch()\n" +
            ")\n" +
            "delete sel";


        public static readonly string mxs_cleanup_texture = "";
    }
}
