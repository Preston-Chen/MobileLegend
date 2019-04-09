
require "Common/define"       -- 初始化协议,Manager指针
require "Common/functions"    -- 初始化方法
require "Logic/LuaClass"      -- 声明了类名
require "Logic/Class"         -- 添加面向对象基础类 

require "Config.GameDefine"   -- 添加游戏定义的参数/值/类型
require "Config/EventDefine"  -- 游戏事件注册
require ('Common.gamemethod')  --游戏的通用方法

require ("Controller/controller_Init")    
require ("GameData/gamedata_Init")

-- UI类
require ("View/gather/base_ui")   
UI_Manager = require ("View.UI_Manager").new()   

--游戏状态
GameState_Manager = require ("GameState.GameState_Manager").new()  


require "ML_GameMaster"       -- lua入口