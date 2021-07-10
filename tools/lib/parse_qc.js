function new_line() {
    let line = [];
    line.type = "line"
    return line;
}

function parse_qc_inner(text,i,terminator) {
    let block = [];
    block.type = "block";
    let line = new_line();

    for (;;) {
        let char = text[i++];
        if (char == null) char = "<EOF>";
        switch (char) {
            case '\n':
                // handle lines
                if (line.length>0) {
                    block.push(line);
                    line = new_line();
                }
                break;
            case '\r':
            case '\t':
            case ' ':
                // skip whitespace
                break;
            case '/': {
                // skip comments
                let c2 = text[i++];
                if (c2 == '/') {
                    while (text[i++] != '\n');
                } else {
                    throw new Error("c2? "+char);
                }
                break;
            }
            case '%': {
                // skip these weird-ass expressions
                while (text[i++] != '\n');
                break;
            }
            case '"': {
                // quoted strings
                let start_i = i;
                while (true) {
                    let c = text[i++];

                    /*if (c == '\\') {
                        throw new Error("escape "+text.substring(i,i+100));
                    } else*/
                    // Evidently there are no escapes?
                    if (c == '"') {
                        break;
                    }
                }
                let str = text.substring(start_i,i-1);
                line.push(str);
                break;
            }
            case '{':
                if (line.length>0) {
                    block.push(line);
                    line = new_line();
                }

                let res = parse_qc_inner(text,i,'}');
                block.push(res.block);
                i = res.i;
                break;
            default:
                if (char == terminator) {
                    if (line.length>0) {
                        block.push(line);
                    }
                    return {block,i};
                } else if (char.match(/^[$_a-z]$/i)) {
                    let start_i = i-1;
                    while (text[i].match(/^[_a-z0-9]$/i)) i++;
                    let str = text.substring(start_i,i);
                    line.push(str);
                } else if (char.match(/^[\-\.0-9]$/i)) {
                    let start_i = i-1;
                    while (text[i].match(/^[\.0-9]$/i)) i++;
                    let str = text.substring(start_i,i);
                    line.push(parseFloat(str));
                } else {
                    throw new Error("Unexpected character in QC: "+char);
                }
        }
    }
}

function find_line(block, key) {
    for (let i=0;i<block.length;i++) {
        let line = block[i];
        if (line.type == "line" && line[0] == key) {
            return line;
        }
    }
    return null;
}

function parse_qc(text) {
    let res = parse_qc_inner(text,0,"<EOF>");
    let block = res.block;
    let model = {groups:{},models:{}};
    let i=0;
    function get_next_block() {
        let sub_block = block[i+1];
        if (sub_block.type == "block") {
            return sub_block;
        }
        return null;
    }
    for (;i<block.length;i++) {
        let line = block[i];
        if (line.type == "line") {
            let cmd = line[0];
            if (cmd == "$model") {
                if (typeof line[1] != "string" || typeof line[2] != "string") {
                    throw new Error("bad model = "+line[1]+", "+line[2]);
                }
                model.models[line[1]] = line[2];
            } else if (cmd == "$bodygroup") {
                let sub_block = get_next_block(cmd);
                group_opts = sub_block.map(x=>{
                    if (x.type == "block") {
                        throw new Error("block in bodygroup");
                    }
                    if (x[0] == "studio") {
                        return x[1];
                    }
                    if (x[0] == "blank") {
                        return null;
                    }
                    throw new Error("bad bodygroup: "+x[0]);
                });
                model.groups[line[1]] = group_opts;
            } else if (cmd == "$texturegroup") {
                if (model.skins != null) {
                    throw new Error("multiple $texturegroup's");
                }
                let sub_block = get_next_block(cmd);
                for (let i=0;i<sub_block.length;i++) {
                    if (sub_block[i].length != 1 || sub_block[i][0].type != "line") {
                        throw new Error("bad skin block "+sub_block[i]);
                    }
                    delete sub_block[i][0].type;
                    sub_block[i] = sub_block[i][0];
                }
                delete sub_block.type;
                model.skins = sub_block;
            } else {
                console.log("ignoring",cmd);
            }
        }
    }
    return model;
}

module.exports = parse_qc;
