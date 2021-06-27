--[[

    [] rules:
        %_ work
        . matches literal .
    - = * outside of []
    ERRORS:
        mismatched () and []
    
]]



print(string.find("asdfa\0sdf", 'df'))
