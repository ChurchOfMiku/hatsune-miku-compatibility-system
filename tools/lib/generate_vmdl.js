function generate(model,base_path) {

    let meshes = "";
    for (let model_name in model.models) {
        let model_path = model.models[model_name];
        meshes += `
                    {
                        _class = "RenderMeshFile"
                        name = "${model_name}"
                        filename = "${base_path}/${model_path}"
                    },`;
    }
    for (let group_name in model.groups) {
        let group_meshes = model.groups[group_name];
        for (let i=0;i<group_meshes.length;i++) {
            if (group_meshes[i] == null) {
                continue;
            }
            // TODO use real body groups
            meshes += `
                    {
                        _class = "RenderMeshFile"
                        name = "bg_${group_name}_${i}"
                        filename = "${base_path}/${group_meshes[i]}"
                    },`;
        }
    }

    let material_fixups = "";
    for (let i=0;i<model.skins.length;i++) {
        for (let j=0;j<model.skins[i].length;j++) {
            let mat_name = model.skins[i][j];
            material_fixups += `
                            {
                                from = "${mat_name}.vmat"
                                to = "materials/default/white.vmat"
                            },`;
        }
    }


    return `<!-- kv3 encoding:text:version{e21c7f3c-8a33-41c5-9977-a76d3a32aa0d} format:modeldoc29:version{3cec427c-1b0e-4d48-a90a-0436f33a6041} -->
{
    rootNode = {
        _class = "RootNode"
        children = [
            {
                _class = "RenderMeshList"
                children = [${meshes}]
            },
            {
                _class = "MaterialGroupList"
                children = [
                    {
                        _class = "DefaultMaterialGroup"
                        remaps = [${material_fixups}]
                    }
                ]
            }
        ]
    }
}`;
}

/*    <!-- kv3 encoding:text:version{e21c7f3c-8a33-41c5-9977-a76d3a32aa0d} format:modeldoc29:version{3cec427c-1b0e-4d48-a90a-0436f33a6041} -->
CVModel CVModel_0
{
    CVmeshList m_meshList = CVmeshList
    {
        CVmesh[] m_meshList = [${meshes}]
    }
    CVMaterialRemapList m_pMaterialRemapList = CVMaterialRemapList{
        CVMaterialRemap[] m_vMaterialRemapList = [${material_fixups}]
    }
}`;
}*/



module.exports = generate;
