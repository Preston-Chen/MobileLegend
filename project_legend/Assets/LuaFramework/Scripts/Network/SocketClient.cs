using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using JT.FWW.Tools;
using LuaFramework;
using LuaInterface;

public enum DisType
{
    Exception,
    Disconnect,
}

public class SocketClient
{
    public enum ServerType    //当前属于哪个服务器
    {
        GateServer = 1,       //基本战斗逻辑
        BalanceServer = 2,    //选择服务器
        LoginServer = 3       //登录逻辑
    }

    private TcpClient client = null;                                    //用来处理数据
    private TcpClient connecting = null;                                //用来处理连接
    private string ip;                                                  //TCP的Socket:ip
    private Int32 port;                                                 //TCP的Socket:port
    private Int32 connect_times = 0;
    private ServerType serverType = ServerType.BalanceServer;           //默认为选择服务器

    private float can_connect_time = 0f;
    private float receive_overtime = 0f;
    private float receive_overdelay_time = 2f;
    private Int32 connect_over_count = 0;
    private float connect_over_time = 0f;
    private Int32 receive_over_count = 0;
    public bool can_reconnect = false;

    private const int MAX_READ = 8192;
    private byte[] receive_buffer = new byte[MAX_READ];                 //定义缓存数组

    public Int32 receive_pos = 0;
    IAsyncResult receive_result;  
    IAsyncResult connect_result;

    //发送数据stream
    public System.IO.MemoryStream send_streams = new System.IO.MemoryStream();   
    //接收数据stream
    public List<int> receive_Msg_IDs = new List<int>();
    public List<System.IO.MemoryStream> receive_streams = new List<System.IO.MemoryStream>();
    public List<System.IO.MemoryStream> receive_streams_pool = new List<System.IO.MemoryStream>();

    // Use this for initialization
    public SocketClient()
    {
        //初始化Stream对象池
        for (int i = 0; i < 50; ++i)
        {
            receive_streams_pool.Add(new System.IO.MemoryStream());
        }
    }

    /// <summary>
    /// 析构函数
    /// </summary>
    public void OnRemove()
    {
        //接收stream
        foreach (System.IO.MemoryStream one in receive_streams)
        {
            one.Close();
        }
        foreach (System.IO.MemoryStream one in receive_streams_pool)
        {
            one.Close();
        }

        //发送stream
        if (send_streams != null)
        {
            send_streams.Close();
        }

        if (client != null)
        {
            client.Client.Shutdown(SocketShutdown.Both);
            client.GetStream().Close();
            client.Close();
            client = null;
        }
    }
    
    /// <summary>
    /// 设置ip 和 port为TCP做准备
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="type"></param>
    public void InitSocket(string IP, Int32 PORT, ServerType type)
    {
        Debugger.Log("set network ip:" + IP + " port:" + PORT + " type:" + type);
        ip = IP;
        port = PORT;
        serverType = type;

        connect_times = 0;
        can_reconnect = true;
        receive_pos = 0;

#if UNITY_EDITOR
        receive_overdelay_time = 20000f;
#endif
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    public void ConnectServer()
    {
        if (!can_reconnect)  //还没有初始化Socket: 一旦初始化host和port就进入这函数
        {
            return;
        }   

        if (client != null) //判断tcpClient已经连接上,已经连接上还连个毛
        {       
            Debug.LogError("fuck, The socket is connecting, cannot connect again!");
        }

        if (connecting != null)
        {
            Debug.LogError("fuck, The socket is connecting, cannot connect again!");
        }
        Debug.LogError("Client Start To Build Connect On Server");

        try
        {
            connecting = new TcpClient();
            connect_result = connecting.BeginConnect(ip, port, null, null); //初始化IP地址,异步连接服务器的方式(返回IasyncResult类型)
            //这两个参数主要是判断连接超时用的
            connect_over_count = 0;              //表示连接过程中的帧数
            connect_over_time = Time.time + 2;   //表示连上服务器的时刻,加2表示要两秒内就得连接上，不然就是坑爹网络
        }
        catch (Exception exc)
        {
            Debug.LogError(exc.ToString());
            client = connecting;
            connecting = null;
            connect_result = null;
            OnConnectError(client, null);
        }
    }

    /// <summary>
    /// 断开服务器连接
    /// </summary>
    public void CloseServer()
    {
        if (client != null)
        {
            //EventCenter.Broadcast(EGameEvent.eGameEvent_ConnectServerFail);
            try
            {
                client.Client.Shutdown(SocketShutdown.Both);
                client.GetStream().Close();
                client.Close();
                client = null;
            }
            catch (Exception exc)
            {
                Debug.LogError(exc.ToString());
            }

            receive_result = null;
            client = null;
            receive_pos = 0;
            receive_over_count = 0;
            connect_over_count = 0;
            receive_streams.Clear();
            receive_Msg_IDs.Clear();
        }
    }

    /// <summary>
    /// 更新网络状态
    /// </summary>
    public void Update()
    {
        if (client != null)  //不断的接收消息
        {
            HandleMessage();

            if (receive_result != null) //是否成功收到消息返回异步类型对象
            {
                if (receive_over_count > 200 && Time.time > receive_overtime) //首先判断接收帧数是否超过200帧,还有是否接收超时,如果是表示渣渣网络,关闭网络
                {
                    Debug.LogError("recv data over 200, so close network.");
                    CloseServer();
                    return;
                }

                ++receive_over_count;

                if (receive_result.IsCompleted) //表示接收消息完毕,就读取接收消息的大小Size
                {
                    try
                    {
                        Int32 n32BytesRead = client.GetStream().EndRead(receive_result);  //结束接收,返回到数据的大小
                        receive_pos += n32BytesRead;                                      //读指针加上第一条消息大小,实现指针偏移

                        if (n32BytesRead == 0)
                        {
                            Debug.LogError("can't recv data now, so close network 2.");
                            CloseServer();
                            return;
                        }
                    }
                    catch (Exception exc)
                    {
                        Debug.LogError(exc.ToString());
                        CloseServer();
                        return;
                    }

                    ParseReceivedData(); //解析接收到的消息

                    if (client != null)
                    {
                        try
                        {
                            receive_result = client.GetStream().BeginRead(receive_buffer, receive_pos, receive_buffer.Length - receive_pos, null, null);
                            receive_overtime = Time.time + receive_overdelay_time; ;
                            receive_over_count = 0;
                        }
                        catch (Exception exc)
                        {
                            Debug.LogError(exc.ToString());
                            CloseServer();
                            return;
                        }
                    }
                }
            }

            if (client != null && client.Connected == false)
            {
                Debug.LogError("client is close by system, so close it now.");
                CloseServer();
                return;
            }
        }
        else if (connecting != null) //当ConnectServer与服务器建立连接成功,就马上执行接收方法 
        {
            //判断开始异步连接后帧数是否过去200，时间是否超过连接的时刻,如果是就表示渣渣网络，返回连接错误消息
            if (connect_over_count > 200 && Time.time > connect_over_time)  
            {
                Debug.LogError("can't connect, so close network.");
                client = connecting;
                connecting = null;
                connect_result = null;
                OnConnectError(client, null); //关闭tcpClient,重新设置为空
                return;
            }

            ++connect_over_count;

            if (connect_result.IsCompleted)
            {
                client = connecting; 
                connecting = null;
                connect_result = null;

                if (client.Connected) //已经连上了
                {
                    try
                    {
                        client.NoDelay = true;
                        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 2000);
                        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 2000);
                        client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);  //异步接收消息,返回IAsynResult类型
                        connect_result = client.GetStream().BeginRead(receive_buffer, 0, receive_buffer.Length, null, null);
                        // Byte[], Int32, Int32, AsyncCallback, Object 这里调用网络数据流的异步接收消息方法
                        // Byte[]: 缓存数组,也就是把接收到的消息存到这个byte[]数组里面
                        // Int32:  从缓存数组的开始索引位置开始存
                        // Int32:  从networkStream流中读取的字节大小
                        // AsyncCallback: 回调,读完之后执行的方法
                        // Object: 包含用户定义的任何附加数据的对象

                        //判断接收是否超时
                        receive_overtime = Time.time + receive_overdelay_time;
                        receive_over_count = 0;

                        //OnConnected(m_Client, null); //如果连接上的话,就发送登陆请求消息
                    }
                    catch (Exception exc)
                    {
                        Debug.LogError(exc.ToString());
                        CloseServer();
                        return;
                    }
                }
                else
                {
                    OnConnectError(client, null);
                }
            }
        }
        else
        {
            ConnectServer();
        }
    }

    /// <summary>
    /// 把解析出来的消息,进行处理
    /// 处理接收不同类型的消息,然后做出不同的处理 
    /// </summary>
    public void HandleMessage()
    {
        while (receive_Msg_IDs.Count > 0 && receive_streams.Count > 0)
        {
            int type = receive_Msg_IDs[0]; //先解析出消息id
            System.IO.MemoryStream iostream = receive_streams[0]; //然后解析出Stream,然后移除
            receive_Msg_IDs.RemoveAt(0);
            receive_streams.RemoveAt(0);

            //然后把解析出来的stream 和 id传到这里去处理 (根据不同的消息id,然后switch不同做不同的处理)
            //CGLCtrl_GameLogic.Instance.HandleNetMsg(iostream, type);
            Util.CallMethod("Network", "HandleNetMsg", iostream, type);

            if (receive_streams_pool.Count < 100)  //然后这里stream对象池问题,这里搞了100个
            {
                receive_streams_pool.Add(iostream);
            }
            else
            {
                iostream = null;
            }
        }
    }

    /// <summary>
    /// 把接收到的消息解析出来
    /// </summary>
    public void ParseReceivedData()
    {
        int m_CurPos = 0;
        while (receive_pos - m_CurPos >= 8) //表示消息超过消息头(是否接收消息超过消息头的大小,如果比消息头都小,表示消息还没有接收完毕,继续接收)
        {
            int len = BitConverter.ToInt32(receive_buffer, m_CurPos);  //总长度
            int type = BitConverter.ToInt32(receive_buffer, m_CurPos + 4); //消息id
            if (len > receive_buffer.Length) //解析出消息的长度len和id
            {
                Debug.LogError("can't pause message" + "type=" + type + "len=" + len);
                break;
            }

            //这个判断和上面有点类似,但是有点不一样,上面的只能判断一条消息的时候,这个可以假如一次收到的是两条消息
            //那么m_CurPos指针会偏移,那么是不是可以判断第二条消息是否出错
            if (len > receive_pos - m_CurPos) //如果接收的消息本来是len这么长度,过这个判断,发现收到的竟然少了,说明出现错误
            {
                break;//wait net recv more buffer to parse.
            }
            //获取stream (这里搞了一个stream对象池,为什么要用对象池,因为消息不断发送过来,你的stream是要不断地new,很耗内存，所以这里搞了个stream对象池存stream,需要的时候直接取)
            System.IO.MemoryStream tempStream = null;
            if (receive_streams_pool.Count > 0)
            {
                tempStream = receive_streams_pool[0];
                tempStream.SetLength(0);
                tempStream.Position = 0;
                receive_streams_pool.RemoveAt(0);
            }
            else
            {
                tempStream = new System.IO.MemoryStream();
            }
            //往stream填充网络数据
            tempStream.Write(receive_buffer, m_CurPos + 8, len - 8);
            tempStream.Position = 0;
            m_CurPos += len; //偏移指针,到下条消息开始处
            receive_Msg_IDs.Add(type);
            receive_streams.Add(tempStream);
        }
        if (m_CurPos > 0) //当你解析出所有消息后,判断指针是否移动到缓存数组的最后(正常的话.是为0)
        {
            receive_pos = receive_pos - m_CurPos;
            if (receive_pos < 0)
            {
                Debug.LogError("m_RecvPos < 0");
            }
            if (receive_pos > 0)
            {
                Buffer.BlockCopy(receive_buffer, m_CurPos, receive_buffer, 0, receive_pos);
            }
        }
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="pMsg"></param>
    /// <param name="MsgID"></param>
    public void SendMessage(LuaByteBuffer data, Int32 MsgID)
    {
        if (client != null)
        {
            //清除stream (可见,我们要发送消息,一定要有发送消息发送流)
            send_streams.SetLength(0);
            send_streams.Position = 0;

            //添加到send_streams里面 
            send_streams.Write(data.buffer, 0, data.buffer.Length);
            CMsg sendMessage = new CMsg((int)send_streams.Length);    //添加消息头部
            sendMessage.SetProtocalID(MsgID);
            sendMessage.Add(send_streams.ToArray(), 0, (int)send_streams.Length);
            //发送消息的最重要的代码,将网络数据流发送到指定服务器流中
            client.GetStream().Write(sendMessage.GetMsgBuffer(), 0, (int)sendMessage.GetMsgSize()); 
        }
    }
    
    /// <summary>
    /// 打印连接错误
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void OnConnectError(object sender, ErrorEventArgs e)
    {
        Debug.LogError("OnConnectError begin");
        try
        {
            client.Client.Shutdown(SocketShutdown.Both);
            client.GetStream().Close();
            client.Close();
            client = null;
        }
        catch (Exception exc)
        {
            Debug.LogError(exc.ToString());
        }
        receive_result = null;
        client = null;
        receive_pos = 0;
        receive_over_count = 0;
        connect_over_count = 0;
        //EventCenter.Broadcast(EGameEvent.eGameEvent_ConnectServerFail); //如果连接不上清除所有战斗信息
        Debug.LogError("OnConnectError end");
    }

}
