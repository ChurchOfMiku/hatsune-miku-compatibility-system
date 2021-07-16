const parse_studio = require("./parse_studio");

function parse_qc(text) {
    let block = parse_studio(text);
    let model = {groups:{},models:{},bones:{},sequences:{},material_paths:[]};
    let i=0;
    function get_next_block() {
        let sub_block = block[i+1];
        if (sub_block.type == "block") {
            return sub_block;
        }
        return null;
    }
    let all_bones = {};
    for (;i<block.length;i++) {
        let line = block[i];
        if (line.type == "line") {
            let cmd = line[0];
            if (cmd == "$model") {
                if (typeof line[1] != "string" || typeof line[2] != "string") {
                    throw new Error("bad model = "+line[1]+", "+line[2]);
                }
                model.models[line[1]] = line[2];
            } else if (cmd == "$collisionmodel") {
                // we currently ignore the extra settings
                model.physics_hull = line[1];
            } else if (cmd == "$definebone") {
                let parent_name = line[2];
                let bone = {
                    name: line[1],
                    x: +line[3],
                    y: +line[4],
                    z: +line[5],
                    xr: +line[6],
                    yr: +line[7],
                    zr: +line[8],
                    children: {}
                };
                if (line[9] != 0 || line[10] != 0 || line[11] != 0 || line[12] != 0 || line[13] != 0 || line[14] != 0) {
                    console.log(line);
                    throw new Error("fixup");
                }
                all_bones[bone.name] = bone;
                if (parent_name == "") {
                    model.bones[bone.name] = bone;
                } else {
                    let parent = all_bones[parent_name];
                    if (parent == null) {
                        throw new Error("no parent for "+bone.name);
                    }
                    parent.children[bone.name] = bone;
                }
            } else if (cmd == "$sequence") {
                // note: this is very dumb
                let seq_name = line[1];
                let sub_block = get_next_block(cmd);
                let seq_path = sub_block[0][0];
                if (!seq_path.endsWith(".smd")) {
                    throw new Error("no path for sequence");
                }
                model.sequences[seq_name] = seq_path;
            } else if (cmd == "$bodygroup") {
                let sub_block = get_next_block(cmd);
                group_opts = sub_block.map(x=>{
                    if (x.type == "block") {
                        throw new Error("block in bodygroup");
                    }
                    if (x[0] == "studio") {
                        return x[1];
                    }
                    if (x[0] == "blank") {
                        return null;
                    }
                    throw new Error("bad bodygroup: "+x[0]);
                });
                model.groups[line[1]] = group_opts;
            } else if (cmd == "$cdmaterials") {
                model.material_paths.push(line[1]);
            } else if (cmd == "$texturegroup") {
                if (model.skins != null) {
                    throw new Error("multiple $texturegroup's");
                }
                let sub_block = get_next_block(cmd);
                for (let i=0;i<sub_block.length;i++) {
                    if (sub_block[i].length != 1 || sub_block[i][0].type != "line") {
                        throw new Error("bad skin block "+sub_block[i]);
                    }
                    let new_skins = {};
                    sub_block[i][0].forEach(x=>{
                        new_skins[x] = x;
                    });
                    sub_block[i] = new_skins;
                }
                delete sub_block.type;
                model.skins = sub_block;
            } else {
                console.log("ignoring",cmd);
            }
        }
    }
    return model;
}

module.exports = parse_qc;
