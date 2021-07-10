const cp = require("child_process");
const fs = require("fs-extra");
const path = require("path");

const parse_qc = require("./lib/parse_qc.js");
const generate_s2 = require("./lib/generate_s2.js");

const ASSET_BASE = path.join(__dirname,"../assets/");
const GMOD_BASE = "C:/Program Files (x86)/Steam/steamapps/common/GarrysMod/";

function ASSET_PATH(name) {
    return path.join(ASSET_BASE,name)
}

function GMOD_PATH(name) {
    return path.join(GMOD_BASE,name)
}

const VPK = GMOD_PATH("bin/vpk.exe");
const CROWBAR = path.join(__dirname,"CrowbarCMD.exe");

// Clear asset directory
fs.removeSync(ASSET_BASE);
fs.mkdirSync(ASSET_BASE);

if (false) {
    console.log("Import HL2 misc sounds.");
    cp.execFileSync(VPK,[GMOD_PATH("sourceengine/hl2_sound_misc_dir.vpk")]).toString();
    fs.moveSync(GMOD_PATH("sourceengine/hl2_sound_misc_dir"),ASSET_BASE,{overwrite: true});
}

function import_model(content_path,model_name) {
    let model_name_short = path.basename(model_name);

    cp.execFileSync(CROWBAR,[
        "-p",content_path+"/models/"+model_name+".mdl",
        "-o",ASSET_PATH("model_src/"+model_name)
    ]).toString();

    let qc = fs.readFileSync(ASSET_PATH("model_src/"+model_name+"/"+model_name_short+".qc")).toString();

    let model_info = parse_qc(qc);
    console.log(model_info);
    let s2_text = generate_s2(model_info,"assets/model_src/"+model_name);
    console.log(s2_text);

    let out_file = ASSET_PATH("models/"+model_name+".vmdl");
    fs.ensureDirSync(path.dirname(out_file));
    fs.writeFileSync(out_file,s2_text);
}

if (true) {
    console.log("Import HL2 assets.");
    if (false) {
        cp.execFileSync(VPK,[GMOD_PATH("sourceengine/hl2_misc_dir.vpk")]).toString();
    }

    //import_model(GMOD_PATH("sourceengine/hl2_misc_dir"),"weapons/w_pistol");
    import_model(GMOD_PATH("sourceengine/hl2_misc_dir"),"gman");
}

console.log("DONE");
//let res = cp.execFileSync(VPK,["--help","butt"]).toString();
//console.log(res);


//let result = cp.execFileSync("Crowbar.exe",["--help","butt"],{}).toString();
//console.log(result);
