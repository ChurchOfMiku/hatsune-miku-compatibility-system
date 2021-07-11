// Common crap for parsing QC files and materials

function new_line() {
    let line = [];
    line.type = "line"
    return line;
}

function parse_studio_inner(text,i,terminator) {
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

                let res = parse_studio_inner(text,i,'}');
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

function parse_studio(text) {
    return parse_studio_inner(text,0,"<EOF>").block;
}

module.exports = parse_studio;
