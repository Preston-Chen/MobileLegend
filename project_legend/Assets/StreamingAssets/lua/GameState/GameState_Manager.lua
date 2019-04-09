require ('Common.FSM.rfsm')

local GameState_Manager = Class()

local fsm

-- 加载所有游戏状态对象
local gameStateType = {} 
gameStateType.login_state = require("GameState.login_state").new()
gameStateType.user_state = require("GameState.user_state").new()
gameStateType.lobby_state = require("GameState.lobby_state").new()
gameStateType.room_state = require("GameState.room_state").new()
gameStateType.hero_state = require("GameState.hero_state").new()
gameStateType.loading_state = require("GameState.loading_state").new()
gameStateType.play_state = require("GameState.play_state").new()
gameStateType.over_state = require("GameState.over_state").new()

-- 建立游戏状态之间的关系
local game_state_fsm = rfsm.state {

    login_state = rfsm.state{ 
        entry = gameStateType.login_state.enter,
        exit = gameStateType.login_state.leave,
    },



    rfsm.transition { src='initial', tgt='login_state' },
    --rfsm.transition { src='hello', tgt='world', events={ 'e_done' } },
    --rfsm.transition { src='world', tgt='hello', events={ 'e_restart' } },
}

-- 默认构造函数
function GameState_Manager:ctor()

end 

-- 进入登录状态
function GameState_Manager:enter_default_state()
    fsm = rfsm.init(game_state_fsm)
    rfsm.run(fsm) 
end

return GameState_Manager


