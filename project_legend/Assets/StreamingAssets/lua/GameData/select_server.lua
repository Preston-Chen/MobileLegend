local Select_Server = Class()

local server_info           -- 选择服务器列表

local cur_select_index      -- 当前选择服务器的下标
local cur_select_server     -- 当前选择服务器的信息

local gate_server_address 
local gate_server_port 
local gate_server_token 

local server_plafrom
local server_uin
local server_sion_id 
local gate_server_uin

local Server_Info = Class() 
function Server_Info:ctor(index, name, state, address, port)
    self.index = index
    self.name = name
    self.state = state
    self.address = address
    self.port = port
end 

-- 默认构造函数
function Select_Server:ctor( ... )
    server_info = {} 
end

-- gate_server_address: get/set
function Select_Server:gate_server_address(action, value)
    if action == "get" then 
        return gate_server_address
    elseif action == "set" then 
        gate_server_address = value
    else 
        error("<color=orange>".."gate_server_address_fail".."</color>")
    end 
end 

-- gate_server_port: get/set
function Select_Server:gate_server_port(action, value)
    if action == "get" then 
        return gate_server_port
    elseif action == "set" then 
        gate_server_port = value
    else 
        error("<color=orange>".."gate_server_port_fail".."</color>")
    end 
end 

-- gate_server_token: get/set
function Select_Server:gate_server_token(action, value)
    if action == "get" then 
        return gate_server_token
    elseif action == "set" then 
        gate_server_token = value
    else 
        error("<color=orange>".."gate_server_token_fail".."</color>")
    end 
end 

-- server_plafrom: get/set
function Select_Server:server_plafrom(action, value)
    if action == "get" then 
        return server_plafrom
    elseif action == "set" then 
        server_plafrom = value
    else 
        error("<color=orange>".."server_plafrom_fail".."</color>")
    end 
end 

-- server_uin: get/set
function Select_Server:server_uin(action, value)
    if action == "get" then 
        return server_uin
    elseif action == "set" then 
        server_uin = value
    else 
        error("<color=orange>".."server_uin_fail".."</color>")
    end 
end 

-- server_sion_id: get/set
function Select_Server:server_sion_id(action, value)
    if action == "get" then 
        return server_sion_id
    elseif action == "set" then 
        server_sion_id = value
    else 
        error("<color=orange>".."server_sion_id_fail".."</color>")
    end 
end 

-- gate_server_uin: get/set
function Select_Server:gate_server_uin(action, value)
    if action == "get" then 
        return gate_server_uin
    elseif action == "set" then 
        gate_server_uin = value
    else 
        error("<color=orange>".."gate_server_uin_fail".."</color>")
    end 
end 

-- cur_select_index: get/set
function Select_Server:cur_select_index(action, value)
    if action == "get" then 
        return cur_select_index
    elseif action == "set" then 
        cur_select_index = value
    else 
        error("<color=orange>".."cur_select_index_fail".."</color>")
    end 
end 

-- cur_select_server: get/set
function Select_Server:cur_select_server(action, value)
    if action == "get" then 
        return cur_select_server
    elseif action == "set" then 
        cur_select_server = value
    else 
        error("<color=orange>".."cur_select_server_fail".."</color>")
    end 
end 

-- 清空服务器列表
function Select_Server:clean_server_info()
    server_info = {}
end 

-- 返回服务器列表
function Select_Server:get_server_info()
    return server_info
end 

-- 创新服务器类型
function Select_Server:set_server_list(index, name, state, address, port)
    local info = Server_Info.new(index, name, state, address, port)
    server_info[index] = info
end 

-- 设置默认的服务器类型
function Select_Server:set_default_server()
    local index = 1
    self:set_select_server(index)
end 

--设置当前服务器信息
function Select_Server:set_server_info(plafrom, uin, sionid)
    server_plafrom = plafrom
    server_uin = uin
    server_sion_id = sionid
end 

--设置当前服务器信息
function Select_Server:set_gate_server_uin(uin)
    gate_server_uin = uin
end 

-- 设置当前的server类型
function Select_Server:set_select_server(index)
    cur_select_index = index
    cur_select_server = server_info[index]
end 

return Select_Server