const cp = require("child_process");
const fs = require("fs-extra");
const merge_dirs = require("@crokita/merge-dirs").default;
const path = require("path");

const parse_qc = require("./lib/parse_qc.js");
const generate_vmdl = require("./lib/generate_vmdl.js");

const parse_vmt = require("./lib/parse_vmt.js");
const generate_vmat = require("./lib/generate_vmat.js");

const convert_texture = require("./lib/convert_texture.js");

const ASSET_SOURCE = path.join(__dirname,"../asset_src/");
const GMOD_BASE = "C:/Program Files (x86)/Steam/steamapps/common/GarrysMod/";

function GMOD_PATH(name) {
    return path.join(GMOD_BASE,name)
}

function MIKU_PATH(name) {
    return path.join(__dirname,"..",name)
}

const VPK = GMOD_PATH("bin/vpk.exe");
const CROWBAR = path.join(__dirname,"CrowbarCMD.exe");

function extractVPK(file) {
    console.log(" - "+path.basename(file));
    cp.execFileSync(VPK,[file]);
    let dir = file.replace(/\.vpk$/,"");
    if (dir == file) {
        throw new Error("Failed to trim extension from: "+file);
    }
    merge_dirs(dir,ASSET_SOURCE,"overwrite");
}

if (false) {
    console.log("Extracting packages...");
    fs.removeSync(ASSET_SOURCE);
    extractVPK(GMOD_PATH("sourceengine/hl2_misc_dir.vpk"));
    extractVPK(GMOD_PATH("sourceengine/hl2_sound_misc_dir.vpk"));
    extractVPK(GMOD_PATH("sourceengine/hl2_textures_dir.vpk"));
}

if (false) {
    console.log("Copy sounds...");
    fs.copySync(MIKU_PATH("asset_src/sound"),MIKU_PATH("sounds"));
    throw "x";
}

function import_model(model_name) {
    let model_name_short = path.basename(model_name);

    let res = cp.execFileSync(CROWBAR,[
        "-p",MIKU_PATH("asset_src/models/"+model_name+".mdl"),
        "-o",MIKU_PATH("asset_src/model_src/"+model_name)
    ]).toString();

    let qc = fs.readFileSync(MIKU_PATH("asset_src/model_src/"+model_name+"/"+model_name_short+".qc")).toString();

    let materials = [];

    let model_info = parse_qc(qc);
    // Resolve material paths:
    model_info.skins.forEach(skin => {
        for (let key in skin) {
            for (let i=0;i<model_info.material_paths.length;i++) {
                let mat_path = path.join(model_info.material_paths[i],key);
                if (fs.existsSync(MIKU_PATH("asset_src/materials/"+mat_path+".vmt"))) {
                    skin[key] = "materials/"+mat_path;
                    materials.push(mat_path);
                    break;
                }
            }
        }
    });
    console.log(model_info);
    let vmdl_text = generate_vmdl(model_info,"asset_src/model_src/"+model_name);
    console.log(vmdl_text);

    let out_file = MIKU_PATH("models/"+model_name+".vmdl");
    fs.ensureDirSync(path.dirname(out_file));
    fs.writeFileSync(out_file,vmdl_text);

    return materials;
}

function import_material(material_name) {
    let mat_src = fs.readFileSync(MIKU_PATH("asset_src/materials/"+material_name+".vmt")).toString();
    
    let material = parse_vmt(mat_src);
    console.log(material);
    let mat_text = generate_vmat(material);
    console.log(mat_text);

    let out_file = MIKU_PATH("materials/"+material_name+".vmat");
    fs.ensureDirSync(path.dirname(out_file));
    fs.writeFileSync(out_file,mat_text);

    let textures = [];
    if (material.texture_base != null) {
        textures.push(material.texture_base);
    }
    return textures;
}

function import_texture(texture_name) {
    convert_texture(
        MIKU_PATH("asset_src/materials/"+texture_name+".vtf"),
        MIKU_PATH("materials/"+texture_name+".tga"));
}

function import_model_and_materials(name) {
    let materials = import_model(name);

    materials.forEach(mat=>{
        import_material(mat).forEach(import_texture);
    });
}

if (true) {

    import_model_and_materials("weapons/w_pistol");
}

console.log("DONE");
//let res = cp.execFileSync(VPK,["--help","butt"]).toString();
//console.log(res);


//let result = cp.execFileSync("Crowbar.exe",["--help","butt"],{}).toString();
//console.log(result);
