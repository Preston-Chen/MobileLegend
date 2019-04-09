Network = {};
local this = Network;

function Network.Start() 
    print("Network.Start!!");
    
end

-- 收到的消息分发给逻辑层
function Network.HandleNetMsg(data, protocal_id)
    print("<color=orange>".."HandleNetMsg".."</color>")
    
end 

--卸载网络监听--
function Network.Unload()
    
end