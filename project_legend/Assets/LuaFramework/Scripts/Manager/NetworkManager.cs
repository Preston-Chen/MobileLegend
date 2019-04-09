using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;

namespace LuaFramework {
    public class NetworkManager : Manager {
        private SocketClient socket;
        static readonly object m_lockObject = new object();
        static Queue<KeyValuePair<int, ByteBuffer>> mEvents = new Queue<KeyValuePair<int, ByteBuffer>>();

        SocketClient SocketClient {
            get {
                if (socket == null)
                    socket = new SocketClient();
                return socket;                    
            }
        }

        void Awake()
        {
            Init();
        }

        void Init()
        {
            //SocketClient.OnRegister();
        }

        public void OnInit()
        {
            CallMethod("Start");
        }

        public void Unload()
        {
            CallMethod("Unload");
        }

        /// <summary>
        /// ִ��Lua����
        /// </summary>
        public object[] CallMethod(string func, params object[] args)
        {
            return Util.CallMethod("Network", func, args);
        }

        ///------------------------------------------------------------------------------------
        public static void AddEvent(int _event, ByteBuffer data)
        {
            lock (m_lockObject)
            {
                mEvents.Enqueue(new KeyValuePair<int, ByteBuffer>(_event, data));
            }
        }

        /// <summary>
        /// ��������״̬
        /// </summary>
        void Update()
        {
            SocketClient.Update();
        }

        /// <summary>
        /// ������������Tcp�����˽�������
        /// </summary>
        public void SendConnect(string IP, Int32 PORT, SocketClient.ServerType type)
        {
            SocketClient.can_reconnect = false;
            SocketClient.CloseServer();
            SocketClient.InitSocket(IP, PORT, type);
        }

        /// <summary>
        /// ����SOCKET��Ϣ
        /// </summary>
        public void SendMessage(LuaByteBuffer data, Int32 MsgID)
        {
            SocketClient.SendMessage(data, MsgID);
        }

        public void HandleNetMsg(LuaByteBuffer data, int protocalID)
        {
            Util.CallMethod("Network", "HandleNetMsg", data, protocalID);
        }

        /// <summary>
        /// ��������
        /// </summary>
        new void OnDestroy()
        {
            SocketClient.OnRemove();
            Debug.Log("~NetworkManager was destroy");
        }
    }
}