BaseUi = Class()

local objectShow = Vector3.zero 
local objectHide = Vector3.New(0, 10000, 0) 

-- 默认构造函数
function BaseUi:ctor()
    self.uiRoot = nil       -- 当前所加载的UI类型作为跟目录
    self.sceneType = nil    -- 场景类型
    self.resName = nil      -- 资源名
    self.resident = nil     -- 是否常驻
    self.visible = false    -- 是否可见
end

-- ui初始化:注册事件监听
function BaseUi:init()

end 

-- ui释放:释放事件监听
function BaseUi:realse()

end 

-- 初始化各个UI组件
function BaseUi:init_widget()

end 

-- 释放各个UI组件
function BaseUi:realse_widget()

end 

-- 注册游戏事件
function BaseUi:on_add_listener()

end 

-- 注销游戏事件
function BaseUi:on_remove_listener()

end 

-- 显示初始化
function BaseUi:on_enable()

end 

-- 隐藏处理
function BaseUi:on_disable()

end 

-- UI显示
function BaseUi:show()
    if (self.uiRoot == nil) then 
        if (self:load_ui_resources()) then 
            self:init_widget()
        end 
    end 

    if (self.visible == false) then      
        self.uiRoot.transform.localPosition = objectShow
        self:on_enable()
        self:on_add_listener()
        self.visible = true
    end
end 

-- UI隐藏
function BaseUi:hide()     
    if (self.visible == true) then 
        self:on_remove_listener()
        self:on_disable() 

        if (self.resident) then 
            self.uiRoot.transform.localPosition = objectHide      
        else 
            self:realse_widget()
            self:destroy_ui_resources()
        end 
        self.visible = false
    end 
end 

-- 从Assetbundle加载预制体
function BaseUi:load_ui_resources()
    if (self.uiRoot) then 
        print("<color=cyan>".."ui resource is already exist".."</color>")
        return false
    end 

    if (self.resName == nil) then 
        print("<color=cyan>".."resource name is empty".."</color>")        
        return false 
    end 

    if (gamemethod.get_ui_camera().transform == nil) then 
        print("<color=cyan>".."fail to create ui camera".."</color>")        
        return false         
    end 

    local obj = resMgr:LoadRes(gamemethod.get_ui_camera().transform, self.resName)
    if (obj == nil) then 
        print("<color=cyan>".."fail to load obj from resources".."</color>")        
        return false       
    end 
    
    self.uiRoot = obj

    self.uiRoot.transform.localPosition = objectHide

    return true
end 

-- 删除预制体 释放内存
function BaseUi:destroy_ui_resources()
    if (self.uiRoot ~= nil) then 
        resMgr:DestroyRes(self.uiRoot)
        self.uiRoot = nil
    end 
end 

-- 预加载Assetbundle预制体
function BaseUi:preload_ui_resources()
    if (self.uiRoot == nil) then 
        if (self:load_ui_resources()) then 
            self:init_widget()
        end 
    end 
end 

-- 延迟删除预制体 释放内存
function BaseUi:delay_destroy_ui_resources()
    if (self.uiRoot ~= nil) then 
        self:realse_widget()
        self:destroy_ui_resources()
    end 
end 

-- 返回当前类型的UI界面
function BaseUi:get_ui_root()
    return self.uiRoot
end 