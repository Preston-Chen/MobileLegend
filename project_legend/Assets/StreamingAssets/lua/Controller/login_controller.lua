local login_controller = Class()

local server_type = {}
server_type.gate_server = 1
server_type.balance_server = 2 
server_type.login_server = 3 

-- 默认构造函数
function login_controller:ctor()
    
end

-- 进入当前状态的一系列行为
function login_controller:enter()
    Event.Brocast(GameEvent.LoginEnter)
end 

-- 离开当前状态的一系列行为
function login_controller:leave()
    Event.Brocast(GameEvent.LoginExit)    
end     

-- 选择服务器
function login_controller:select_server(id)
    
end 

-- 开始游戏
function login_controller:start_game()
    local index = gamedata.select_server:cur_select_index("get")
    local info = gamedata.select_server:get_server_info()[index] 
    networkMgr:SendConnect(info.address, info.port, server_type.gate_server) 
end 

-- 登陆请求
function login_controller:login_game(account, password)
    gamedata.select_server:set_server_info(0, account, password)
    networkMgr:SendConnect(AppConst.SocketAddress, AppConst.SocketPort, server_type.login_server) 

end 

return login_controller