const cp = require("child_process");
const fs = require("fs-extra");
const path = require("path");

const parse_qc = require("./lib/parse_qc.js")

const ASSET_BASE = path.join(__dirname,"../assets/")
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

if (true) {
    console.log("Import HL2 assets.");
    //cp.execFileSync(VPK,[GMOD_PATH("sourceengine/hl2_misc_dir.vpk")]).toString();

    let model_name = "weapons/w_pistol";
    let model_name_short = path.basename(model_name);

    cp.execFileSync(CROWBAR,[
        "-p",GMOD_PATH("sourceengine/hl2_misc_dir/models/"+model_name+".mdl"),
        "-o",ASSET_PATH("model_src/"+model_name)
    ]).toString();

    let qc = fs.readFileSync(ASSET_PATH("model_src/"+model_name+"/"+model_name_short+".qc")).toString();

    let model_info = parse_qc(qc);
    console.log(model_info);

    //fs.moveSync(GMOD_PATH("sourceengine/hl2_misc_dir"),ASSET_BASE,{overwrite: true});
}

console.log("DONE");
//let res = cp.execFileSync(VPK,["--help","butt"]).toString();
//console.log(res);


//let result = cp.execFileSync("Crowbar.exe",["--help","butt"],{}).toString();
//console.log(result);
