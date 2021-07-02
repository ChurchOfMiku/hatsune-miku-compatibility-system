const cheerio = require('cheerio');
const fs = require('fs');

const $ = cheerio.load(fs.readFileSync('index.html'));
const TEMPLATE = fs.readFileSync('template.html').toString();

const CLIENT_DUMP = JSON.parse(fs.readFileSync(__dirname+"/../../../data/local/hatsune-miku-compatibility-system/client.json"));
const SERVER_DUMP = JSON.parse(fs.readFileSync(__dirname+"/../../../data/local/hatsune-miku-compatibility-system/server.json"));

let GLOBAL_COUNTS = {};

let STATUS_TYPES = ["MENU","CSHARP","LUA","FP-LUA","STUB","NYI","ERROR"];

function makeBar(counts,width,height) {
    let total = 0;
    let good = 0;
    let contents = "";
    for (let key in counts) {
        let c = counts[key];
        total += c;
    }

    STATUS_TYPES.forEach((key)=>{
        let c = counts[key];
        if (c == null) {
            return;
        }
        if (key != "ERROR" && key != "NYI" && key != "STUB") {
            good += c;
        }
        let status_class = "st-"+key.toLowerCase();
        contents+=`<div style='width: ${c/total*100}%;' class='${status_class}'></div>`;
    });
    contents += `<span>${(good/total*100).toFixed(1)}%</span>`
    return `<div class='bar' style='width: ${width}px; height: ${height}px; font-size: ${height}px;'>${contents}</div>`;
}

let section_id = 0;

function makeSection(section_name_pretty,prefix,elems,force_open) {
    let counts = {};
    let table_contents = "";

    function generateRow(elem,prefix) {
        let a = $(elem).find("a");
        let realms = "";
        if (a.hasClass("rc")) realms += "rc ";
        if (a.hasClass("rs")) realms += "rs ";
        if (a.hasClass("rm")) realms += "rm ";
        let realm_pretty = "";
        if (realms.includes("rc")) {
            if (realms.includes("rs")) {
                realm_pretty = "Shared";
            } else {
                realm_pretty = "Client";
            }
        } else if (realms.includes("rs")) {
            realm_pretty = "Server";
        } else if (realms == "rm ") {
            realm_pretty = "Menu";
        } else {
            realm_pretty = "Field?";
            realms += "ru ";
        }
    
        let name = prefix+$(elem).text().trim();
        let status = "NYI";
        let notes = "";

        let status_c = CLIENT_DUMP[name];
        let status_s = SERVER_DUMP[name];
        if (realm_pretty == "Menu") {
            status = "MENU";
        } else if (realm_pretty == "Shared") {
            if (status_c != status_s) {
                status = "ERROR";
                notes = `Inconsistent shared status, CL = `+status_c+", SV = "+status_s;
            } else if (status_c != null) {
                status = status_c;
            }
        } else if (realm_pretty == "Client") {
            if (status_c != null) {
                status = status_c;
            }
        }else if (realm_pretty == "Server") {
            if (status_s != null) {
                status = status_s;
            }
        }

        let space_index = status.indexOf(" ");
        if (space_index != -1) {
            notes = status.substr(space_index+1);
            status = status.substr(0,space_index);
            //console.log(`[${status}][${notes}]`)
        }
    
        counts[status]=(counts[status]||0)+1;
    
        let status_class = "st-"+status.toLowerCase();
        return `<tr><td class='${realms}'>${realm_pretty}</td><td>${name}</td><td class='${status_class}'>${status}</td><td>${notes}</td></tr>\n`;
    }

    function handleList(elems,prefix) {
        elems.each((i,elem)=>{
            let sub_title = $(elem).children("details").children("summary").text().trim();
            if (sub_title) {
                handleList($(elem).children("details").children("ul").children("li"),prefix+sub_title+".");
            } else {
                table_contents += generateRow(elem,prefix);
            }
        });
    }

    handleList(elems,prefix);

    for (let k in counts) {
        GLOBAL_COUNTS[k] = (GLOBAL_COUNTS[k]||0)+counts[k];
    }
    
    let section = `<div>
    <h2>${section_name_pretty}</h2>
    ${makeBar(counts,400,30)}
    <button onclick="showTable('section_${section_id}')">Toggle Details</button>
    <table id="section_${section_id}" ${force_open?'style="display: table;"':""}>
        ${table_contents}
    </table></div>`;
    section_id++;
    return section;
}

let lib_sections = makeSection("Global","",$("#sidebar details:has(summary:contains(Globals)) li"));
let libs = $("#sidebar details:has(summary:contains(Libraries))>ul>li");
libs.each((i,elem)=>{
    let name = $(elem).children("details").children("summary").text().trim();
    lib_sections += makeSection(name,name+".",$(elem).children("details").children("ul").children("li"));
});

let class_sections = "";
let classes = $("#sidebar details:has(summary:contains(Classes))>ul>li");
classes.each((i,elem)=>{
    let name = $(elem).children("details").children("summary").text().trim();
    class_sections += makeSection(name,"_R."+name+".",$(elem).children("details").children("ul").children("li"));
});

let header = "<p><i>Generated @ "+new Date()+"</i></p>";
header += makeBar(GLOBAL_COUNTS,1000,100);
header += "<ul>";
STATUS_TYPES.forEach((key)=>{
    let status_class = "st-"+key.toLowerCase();
    header += `<li><span class='${status_class}'>${GLOBAL_COUNTS[key]||0} ${key}</span></li>`;
});
header += "</ul>";

fs.writeFileSync("hmcs_status.html",TEMPLATE.replace("@HEADER@",header).replace("@LIBRARIES@",lib_sections).replace("@CLASSES@",class_sections));
