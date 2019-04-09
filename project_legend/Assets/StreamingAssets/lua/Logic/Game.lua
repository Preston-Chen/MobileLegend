require "3rd/pblua/login_pb"
require "3rd/pbc/protobuf"

local lpeg = require "lpeg"

local json = require "cjson"
local util = require "3rd/cjson/util"

local sproto = require "3rd/sproto/sproto"
local core = require "sproto.core"
local print_r = require "3rd/sproto/print_r"

Event = require 'events'

require "Logic/Game_init"

--管理器--
Game = {};
local this = Game;

local game; 
local transform;
local gameObject;
local WWW = UnityEngine.WWW;

--初始化完成，发送链接服务器信息--
function Game.OnInitOK()
    print('LuaFramework InitOK--->>>');
    OnLevelWasLoaded("ML_GameMaster")
end
