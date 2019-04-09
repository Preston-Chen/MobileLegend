local LoginUi = Class(BaseUi)

local account_input     -- 用户名输入
local password_input    -- 密码输入
local waiting_trans     -- 等待连接服务器中

local objectShow = Vector3.zero 
local objectHide = Vector3.New(0, 10000, 0) 

function LoginUi:ctor()
    self.sceneType = sceneType.ST_Login
    self.resName = gamedefine.constdefine.LoadLoginUI       
    self.resident = false   
end

-- 类对象初始化
function LoginUi:init()
    Event.AddListener(GameEvent.LoginEnter, function() self:show() end)
    Event.AddListener(GameEvent.LoginExit, function() self:hide() end)
end 

-- 类对象释放
function LoginUi:realse()
    Event.RemoveListener(GameEvent.LoginEnter, function() self:show() end)
    Event.RemoveListener(GameEvent.LoginExit, function() self:hide() end)
end 

-- 初始化各个窗口控件
function LoginUi:init_widget()
    account_input = self.uiRoot.transform:Find("AccountInput"):GetComponent("UIInput")       -- 获取用户名组件
    password_input = self.uiRoot.transform:Find("PasswordInput"):GetComponent("UIInput")     -- 获取密码组件
    --waiting_trans = self.uiRoot.transform:Find("Connecting").transform
    
    -- self.startgame_object = self.uiRoot.transform:Find("ConfirmLogin")
    -- self.server_object = self.uiRoot.transform:Find("Server")
    -- gamemethod.add_click(self.startgame_object, self.player_submit, self)
    -- gamemethod.add_click(self.server_object , self.login_submit, self)
end 


function LoginUi:player_submit()
    waiting_trans.localPosition = objectShow
    controller.login:start_game()
    gamemethod.remove_click(self.startgame_object)
end


function LoginUi:login_submit()
    if account_input.value == nil or password_input.value == nil then 
        print("<color=orange>".."account_input or password_input is null".."</color>")
        return
    end 

    waiting_trans.localPosition = objectShow
    controller.login:login_game(account_input.value, password_input.value) 
end 

return LoginUi
