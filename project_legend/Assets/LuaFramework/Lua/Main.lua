--主入口函数。从这里开始lua逻辑
function Main()					
	--print("logic start")	 		
end

--场景切换通知
function OnLevelWasLoaded(level)
	resMgr:LoadScene("mobilelegend/scenes", "ML_GameMaster")
	collectgarbage("collect")
	Time.timeSinceLevelLoad = 0
end

function OnApplicationQuit()
	
end