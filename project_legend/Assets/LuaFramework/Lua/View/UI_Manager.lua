local UI_Manager = Class()

-- UI Panel是属于哪个应用场景的
sceneType = {} 
sceneType.ST_None = 1
sceneType.ST_Login = 2
sceneType.ST_Play = 3

-- 初始化所有UI类型
local windowType = {} 
windowType.Login_UI = require("View.gather.login_ui").new()
windowType.User_UI = nil
windowType.Lobby_UI = nil
windowType.Battle_UI = nil
windowType.Room_UI = nil
windowType.Hero_UI = nil
windowType.BattleInfo_UI = nil
windowType.Market_UI = nil
windowType.MarketHeroList_UI = nil
windowType.MarketHeroInfo_UI = nil
windowType.Social_UI = nil
windowType.GamePlay_UI = nil
windowType.Invite_UI = nil
windowType.ChatTask_UI = nil
windowType.Score_UI = nil
windowType.InviteAddRoom_UI = nil
windowType.RoomInvite_UI = nil
windowType.TeamMatch_UI = nil
windowType.TeamMatchInvitation_UI = nil
windowType.TeamMatchSearching_UI = nil
windowType.Mail_UI = nil
windowType.HomePage_UI = nil
windowType.PresonInfo_UI = nil
windowType.ServerMatchInvitation_UI = nil
windowType.SoleSoldier_UI = nil
windowType.Message_UI = nil
windowType.MarketRuneList_UI = nil
windowType.MiniMap_UI = nil
windowType.MarketRuneInfo_UI = nil
windowType.VIPPrerogative_UI = nil
windowType.RuneEquip_UI = nil
windowType.DaliyBonus_UI = nil
windowType.Equipment_UI = nil
windowType.SystemNotice_UI = nil
windowType.TimeDown_UI = nil
windowType.RuneCombine_UI = nil
windowType.HeroDatum_UI = nil
windowType.RuneRefresh_UI = nil
windowType.GamePlayGuide_UI = nil
windowType.PurchaseSuccess_UI = nil
windowType.GameSetting_UI = nil
windowType.AdvancedGuide_UI = nil
windowType.ExtraBonus_UI = nil
windowType.Enemy_UI = nil
windowType.HeroTimeLimit_UI = nil
windowType.Skill_UI = nil
windowType.SkillDescrible_UI = nil
windowType.RuneBuy_UI = nil
windowType.Death_UI = nil

function UI_Manager:ctor()
   
end

-- 返回指定的UI类型
function UI_Manager:get_window_type(type)
    return windowType[type]
end 

-- 所有UI在不同状态的帧同步
function UI_Manager:Update(deltaTime)
    for key, value in pairs(windowType) do
        if (value.visible) then 
            value:Update(deltaTime)
        end 
    end  
end 

-- 所有UI切换到登录状态
function UI_Manager:change_scense_to_login(forward)
    for key, value in pairs(windowType) do
        if (forward == sceneType.ST_None and value.sceneType == sceneType.ST_None) then 
            value:init()
            if (value.resident) then 
                value:preload_ui_resources()
            end 
        end 

        if (value.sceneType == sceneType.ST_Login) then 
            value:init()
            if (value.resident) then 
                value:preload_ui_resources()
            end 
        elseif (value.sceneType == sceneType.ST_Play and forward == sceneType.ST_Play) then 
            value:hide()
            value:realse()
            if (value.resident) then 
                value:delay_destroy_ui_resources()
            end 
        end 
    end 
end 

-- 所有UI切换到游戏状态
function UI_Manager:change_scense_to_play(forward)
    for key, value in pairs(windowType) do
        if (value.sceneType == sceneType.ST_Play) then 
            value:init()
            if (value.resident) then 
                value:preload_ui_resources()
            end 
        elseif value.sceneType == sceneType.ST_Login and forward == sceneType.ST_Login then 
            value:hide()
            value:realse() 
            if (value.resident) then 
                value:delay_destroy_ui_resources()
            end 
        end 
    end  
end 

-- 隐藏该类型的所有UI
function UI_Manager:hide_window_of_type(forward)
    for key, value in pairs(windowType) do
        if (value.sceneType == forward) then 
            value:hide()
        end 
    end 
end 

-- 显示该类型的所有UI
function UI_Manager:show_window_of_type(forward)
    for key, value in pairs(windowType) do
        if (value.sceneType == forward) then 
            value:show()
        end 
    end 
end 

return UI_Manager