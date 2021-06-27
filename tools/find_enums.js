const cheerio = require('cheerio');
const fs = require('fs');

const $ = cheerio.load(fs.readFileSync('index.html'));

$("#sidebar a.enum").each((i,elem)=>{
    let e = $(elem);

    let realms = "";
    if (e.hasClass("rc")) {
        realms += "c";
    }
    if (e.hasClass("rs")) {
        realms += "s";
    }
    if (e.hasClass("rm")) {
        realms += "m";
    }

    console.log('{"'+realms+'","'+$(elem).text()+'"},');
});
