const cheerio = require('cheerio');
const fs = require('fs');

const $ = cheerio.load(fs.readFileSync('index.html'));
const TEMPLATE = fs.readFileSync('template.html').toString();

function makeBar(counts,width,height) {
    let total = 0;
    let good = 0;
    let contents = "";
    for (let key in counts) {
        let c = counts[key];
        total += c;
    }

    ["MENU","CSHARP","LUA","FP-LUA","STUB","NYI"].forEach((key)=>{
        let c = counts[key];
        if (c == null) {
            return;
        }
        if (key == "MENU") {
            good += c;
        }
        let status_class = "st-"+key.toLowerCase();
        contents+=`<div style='width: ${c/total*100}%;' class='${status_class}'></div>`;
    });
    contents += `<span>${(good/total*100).toFixed(1)}%</span>`
    return `<div class='bar' style='width: ${width}px; height: ${height}px; font-size: ${height}px;'>${contents}</div>`;
}

let counts = {};
let table_contents = "";
$("#sidebar details:has(summary:contains(Globals)) li").each((i,elem)=>{
    /*$(elem).find("li").each((i,elem)=>{
        let name = $(elem).text().trim();
        console.log(name,i);
    });*/
    let a = $(elem).find("a");
    let realms = "";
    if (a.hasClass("rc")) realms += "rc ";
    if (a.hasClass("rs")) realms += "rs ";
    if (a.hasClass("rm")) realms += "rm ";

    let name = $(elem).text().trim();
    let status = "NYI";
    if (realms == "rm ") {
        status = "MENU";
    }
    let notes = "";

    counts[status]=(counts[status]||0)+1;

    let realms_pretty = realms.replace(/r/g,"").toUpperCase();
    let status_class = "st-"+status.toLowerCase();
    table_contents += `<tr><td class='${realms}'>${realms_pretty}</td><td>${name}</td><td class='${status_class}'>${status}</td><td>${notes}</td></tr>\n`;
});

let section_name_pretty = "Global";
let section_id = "lib.global";

let section = `
<h2>${section_name_pretty}</h2>
${makeBar(counts,400,30)}
<button onclick="showTable('${section_id}')">Details</button>
<table id="${section_id}" style="display: table;">
    ${table_contents}
</table>`;

fs.writeFileSync("hmcs_status.html",TEMPLATE.replace("@LIBRARIES@",section));
