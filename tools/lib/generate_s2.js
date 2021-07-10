/*

    CVMaterialRemapList m_pMaterialRemapList = CVMaterialRemapList{
        CVMaterialRemap[] m_vMaterialRemapList = [
            CVMaterialRemap
            {
                string m_sSearchMaterial = "pistol.vmat"
                string m_sReplaceMaterial = "materials/default/white.vmat"
            }
        ]
    }

*/



function generate(model,base_path) {

    let meshes = "";
    for (let model_name in model.models) {
        let model_path = model.models[model_name];
        meshes += `
            CVmesh
            {
                string m_meshName = "mdl_${model_name}"
                string m_meshFile = "${base_path}/${model_path}"
            },`;
    }
    for (let group_name in model.groups) {
        let group_meshes = model.groups[group_name];
        for (let i=0;i<group_meshes.length;i++) {
            if (group_meshes[i] == null) {
                continue;
            }
            meshes += `
            CVmesh
            {
                string m_meshName = "bg_${group_name}_${i}"
                string m_meshFile = "${base_path}/${group_meshes[i]}"
            },`;
        }
    }

    let material_fixups = "";
    for (let i=0;i<model.skins.length;i++) {
        for (let j=0;j<model.skins[i].length;j++) {
            let mat_name = model.skins[i][j];
            material_fixups += `
            CVMaterialRemap
            {
                string m_sSearchMaterial = "${mat_name}.vmat"
                string m_sReplaceMaterial = "materials/default/white.vmat"
            },`
        }
    }


    return `<!-- schema text {7e125a45-3d83-4043-b292-9e24f8ef27b4} generic {198980d8-3a93-4919-b4c6-dd1fb07a3a4b} -->
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
}



module.exports = generate;
