ML_GameMaster = {}
local this = ML_GameMaster


function this.Awake(obj)
    this.gameObject = obj   
    this.transform = obj.transform
    this.mono = this.transform:GetComponent("LuaBehaviour")

    -- Socket:IP
    AppConst.SocketAddress = "192.168.1.53";
    AppConst.SocketPort = 49998;

    GameObject.DontDestroyOnLoad(this.gameObject)                             -- 加载另一个场景时,ML_GameMaster不能destroy掉
    UnityEngine.Application.runInBackground = true                            -- 后台可运行
    UnityEngine.Screen.sleepTimeout = UnityEngine.SleepTimeout.NeverSleep     -- 玩家没有点击时，屏幕不能黑屏

    UI_Manager:change_scense_to_login(sceneType.ST_None)
end

function this.Start()
    -- print("<color=orange>".."ML_GameMaster".."</color>")
    GameState_Manager:enter_default_state()
end 









