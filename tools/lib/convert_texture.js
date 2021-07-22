const vtflib = require("vtflib");
const TGA = require("tga");
const dxt = require("dxt-js");
const fs = require("fs");

function convert_texture(source_file_name, dest_file_name) {
    let vtf = new vtflib.VTFFile(fs.readFileSync(source_file_name));
    let images = vtf.getImages();
    let source_img = images[images.length-1];

    let width = source_img.Width;
    let height = source_img.Height;

    let pixel_buffer;
    
    switch (source_img.Format) {
        case "DXT1":
            pixel_buffer = Buffer.from(dxt.decompress(source_img.Data, width, height, dxt.flags.DXT1));
            break;
        case "BGR888":
            pixel_buffer = Buffer.alloc(width * height * 4);
            for (let i=0,j=0;i<pixel_buffer.length;i+=4,j+=3) {
                pixel_buffer[i] = source_img.Data[j+2];
                pixel_buffer[i+1] = source_img.Data[j+1];
                pixel_buffer[i+2] = source_img.Data[j+0];
                pixel_buffer[i+3] = 255;
            }
            break;
        default:
            pixel_buffer = source_img.toRGBA8888();
    }
    
    let tga_data = TGA.createTgaBuffer(width, height, pixel_buffer);

    fs.writeFileSync(dest_file_name,tga_data);
}

module.exports = convert_texture;
