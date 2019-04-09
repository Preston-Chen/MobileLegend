module("gamemethod",package.seeall)

-- 获取游戏的UI Camera
function get_ui_camera()
    if (UICamera.currentCamera == nil) then 
        local gameui = newObject(resMgr:LoadBundle("mobilelegend/gameui", "GameUI"))
        gameui.name = "GameUI"
        gameui.transform.localPosition = Vector3.zero
        UICamera.currentCamera = gameui.transform:GetChild(0).gameObject:GetComponent("Camera")
    end 
    return UICamera.currentCamera
end

-- 添加单击事件
function add_click(object, func, ...)
    ML_GameMaster.mono:AddClick(object, func, ...)
end 

-- 取消单击事件
function remove_click(object)
    ML_GameMaster.mono:RemoveClick(object)
end 

