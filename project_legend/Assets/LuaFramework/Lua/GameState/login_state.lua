local LoginState = Class()

-- 默认构造函数
function LoginState:ctor( ... )
    
end

-- 返回当前属于哪个状态
function LoginState:get_state_type()
    return GameState.Login
end 

-- 进入当前状态的一系列行为
function LoginState:enter()
    -- 显示登入UI,连接SDK
    controller.login:enter()

    -- Event.AddListener(GameEvent.InputUserData, self.handle_event)
    -- Event.AddListener(GameEvent.IntoLobby, self.handle_event)
    -- Event.AddListener(GameEvent.SdkLogOff, self.sdk_log_off)
end 

-- 离开当前状态的一系列行为
function LoginState:leave()
    Event.RemoveListener(GameEvent.InputUserData, self.handle_event)
    Event.RemoveListener(GameEvent.IntoLobby, self.handle_event)
    Event.RemoveListener(GameEvent.SdkLogOff, self.sdk_log_off)
end 

function LoginState:handle_event(event_type) 
    if (event_type == GameEvent.InputUserData) then 
        print("111")
    elseif (event_type == GameEvent.IntoLobby) then 
        print("222")   
    end 
end 

function LoginState:sdk_log_off()

end 

return LoginState