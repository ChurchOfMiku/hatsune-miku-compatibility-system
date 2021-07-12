function fix_path(path) {
    return path.replace(/\\/g,'/');
}

function generate(model,base_path) {

    let meshes = "";
    for (let model_name in model.models) {
        let model_path = model.models[model_name];
        meshes += `
                    {
                        _class = "RenderMeshFile"
                        name = "${model_name}"
                        filename = "${fix_path(base_path+'/'+model_path)}"
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
                        filename = "${fix_path(base_path+'/'+group_meshes[i])}"
                    },`;
        }
    }

    let material_fixups = "";
    for (let i=0;i<model.skins.length;i++) {
        for (let source in model.skins[i]) {
            let target = model.skins[i][source];
            material_fixups += `
                            {
                                from = "${source}.vmat"
                                to = "${fix_path(target+".vmat")}"
                            },`;
        }
    }

    let physics_shapes = "";
    if (model.physics_hull != null) {
        physics_shapes += `
                        {
                            _class = "PhysicsHullFile"
                            name = "physics"
                            parent_bone = ""
                            surface_prop = "default"
                            collision_prop = "default"
                            recenter_on_parent_bone = false
                            offset_origin = [ 0.0, 0.0, 0.0 ]
                            offset_angles = [ 0.0, 0.0, 0.0 ]
                            filename = "${fix_path(base_path+'/'+model.physics_hull)}"
                            import_scale = 1.0
                            faceMergeAngle = 10.0
                            maxHullVertices = 0
                            import_mode = "SingleHull"
                            optimization_algorithm = "QEM"
                            import_filter = 
                            {
                                exclude_by_default = false
                                exception_list = [  ]
                            }
                        },`;
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
            },
            {
                _class = "PhysicsShapeList"
                children = [${physics_shapes}]
            },
        ]
    }
}`;
}

module.exports = generate;
