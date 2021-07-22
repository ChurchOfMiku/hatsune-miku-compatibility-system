
function generate(material) {

    return `
Layer0
{
    shader "simple.vfx"
    
    TextureColor "materials/${material.texture_base}.tga"
}`;
}
module.exports = generate;
