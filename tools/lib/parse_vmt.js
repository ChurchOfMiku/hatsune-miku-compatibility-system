const parse_studio = require("./parse_studio");

const SHADER_WHITELIST = {
    VertexLitGeneric: true,
    Teeth: true,
    eyes: true
}

function parse_vmt(text) {
    let block = parse_studio(text);
    let material = {};
    let shader_line = block[0];
    let main_block = block[1];
    if (shader_line.type != "line" || main_block.type != "block") {
        throw new Error("bad vmt");
    }

    let shader = shader_line[0];
    if (!SHADER_WHITELIST[shader]) {
        throw new Error("bad shader: "+shader);
    }

    main_block.forEach(line=>{
        if (line.type != "line") {
            return;
        }
        let cmd = line[0].toLowerCase();
        if (cmd == "$basetexture") {
            material.texture_base = line[1];
        }
    });

    return material;
}

module.exports = parse_vmt;
