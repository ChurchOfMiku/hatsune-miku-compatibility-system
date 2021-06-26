--[[

    [] rules:
        %_ work
        . matches literal .
    - = * outside of []
    ERRORS:
        mismatched () and []
    
]]



print(string.match("asdfasdf", '(((.)(.)(.)))'))
