ML_GameMaster = {}
local this = ML_GameMaster

function this.Awake(obj)
    this.gameObject = obj   
    this.transform = obj.transform
    this.mono = this.transform:GetComponent("LuaBehaviour")

    --Socket:IP
    AppConst.SocketAddress = "192.168.1.53";
    AppConst.SocketPort = 49998;

    GameObject.DontDestroyOnLoad(this.gameObject)                             -- 加载另一个场景时,ML_GameMaster不能destroy掉
    UnityEngine.Application.runInBackground = true                            -- 后台可运行
    UnityEngine.Screen.sleepTimeout = UnityEngine.SleepTimeout.NeverSleep     -- 玩家没有点击时，屏幕不能黑屏

    --UI_Manager:change_scense_to_login(sceneType.ST_None)
end

function this.Start()
    --GameState_Manager:enter_default_state()
    --local update = UpdateBeat:CreateListener(this.Update)
    --UpdateBeat:AddListener(update)
    print("<color=cyan>"..type(123).."</color>")    
   
end 

function this.Update()
    -- if UnityEngine.Input.GetKeyDown("a") then 
    --     Hello = require ("Controller.login_controller").new()
    -- end 

    -- if UnityEngine.Input.GetKeyDown("b") then 
    --     Hello.useformatnumberthousands()
    -- end 

    -- if UnityEngine.Input.GetKeyDown("c") then 
    --     Hello.destroyformatnumberthousands()
    -- end 

    -- if UnityEngine.Input.GetKeyDown("d") then 
    --     Hello = nil 
    --     package.loaded["Controller/login_controller"] = nil
    -- end 

end 













