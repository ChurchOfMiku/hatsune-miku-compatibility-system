const cp = require("child_process");
const fs = require("fs-extra");
const merge_dirs = require("@crokita/merge-dirs").default;
const path = require("path");

const parse_qc = require("./lib/parse_qc.js");
const generate_vmdl = require("./lib/generate_vmdl.js");

const parse_vmt = require("./lib/parse_vmt.js");
const generate_vmat = require("./lib/generate_vmat");

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
    //console.log(res);

    let qc = fs.readFileSync(MIKU_PATH("asset_src/model_src/"+model_name+"/"+model_name_short+".qc")).toString();

    let model_info = parse_qc(qc);
    console.log(model_info);
    let vmdl_text = generate_vmdl(model_info,"asset_src/model_src/"+model_name);
    console.log(vmdl_text);

    let out_file = MIKU_PATH("models/"+model_name+".vmdl");
    fs.ensureDirSync(path.dirname(out_file));
    fs.writeFileSync(out_file,vmdl_text);
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
}

if (true) {

    //import_model(GMOD_PATH("sourceengine/hl2_misc_dir"),"weapons/w_pistol");
    import_model("gman");
    //import_material("models/gman/gman_sheet");
}

console.log("DONE");
//let res = cp.execFileSync(VPK,["--help","butt"]).toString();
//console.log(res);


//let result = cp.execFileSync("Crowbar.exe",["--help","butt"],{}).toString();
//console.log(result);
