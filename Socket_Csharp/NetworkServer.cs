using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Text;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Linq;

public class NetworkServer : MonoBehaviour {

    public delegate void GetByteMessage(byte[] msg, string clientName);
    public GetByteMessage RecieveMessage;
    public delegate void RemovedConnectionDelegate(string removedClient);
    public RemovedConnectionDelegate RemoveConnection;
    public IPAddress ip;
    [HideInInspector]
    public int port = 0;
    //Socket newSocket;
    EndPoint point;

    //public static bool hasConnected = false;

    void Start()
    {
        //Get IP and Port
        ip = GetIP();
        port = GetOpenPort();
        
        //Listen
        StartListening();
    }

    //byte[] texBytes = null;
    //bool doLoadImage = false;
    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            showMsg = !showMsg;
        }

    }

    public void GetByteMsg(byte[] msg, string clientName)
    {

        if (null != RecieveMessage) RecieveMessage(msg, clientName);

        //Debug.Log(clientName);

        ////return;
        //switch (msg[0])
        //{
        //    case 0:

        //        break;
        //    case 1:
        //        string strMsg = Encoding.ASCII.GetString(msg, 1, msg.Length - 1);
        //        //GetStrMsg(Encoding.ASCII.GetString(msg, 1, msg.Length));
        //        if (strMsg.StartsWith("t|")) // Get Tex Len To be Confirmed     t|texName|texLen
        //        {
        //            string[] texMsg = strMsg.Split('|');
        //            if (texMsg.Length != 3) return;
        //            int texLen = 0;
        //            if (!int.TryParse(texMsg[2], out texLen)) return;
        //        }
        //        break;
        //    //case 2:
        //        break;
        //}
    }

    //public void GetStrMsg(string str)
    //{
    //    strMsgToShow += str + "\r\n";
    //}

    private void StartListening()
    {
        //实例化 套接字 （ip4寻址协议，流式传输，TCP协议）
        sokWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        threadWatch = new Thread(StartWatch);
        threadWatch.IsBackground = true;
        threadWatch.Start();

        //GetStrMsg("Server started......");
    }

    Socket sokWatch = null;//负责监听 客户端段 连接请求的  套接字
    Thread threadWatch = null;//负责 调用套接字， 执行 监听请求的线程

    Dictionary<string, ConnectionClient> dictConn = new Dictionary<string, ConnectionClient>();
    
    bool isWatch = true;

    #region 1.被线程调用 监听连接端口
    /// <summary>
    /// 被线程调用 监听连接端口
    /// </summary>
    void StartWatch()
    {
        //创建网络节点对象 包含 ip和port
        IPEndPoint endpoint = new IPEndPoint(ip, port);
        //将 监听套接字  绑定到 对应的IP和端口
        sokWatch.Bind(endpoint);
        //设置 监听队列 长度为10(同时能够处理 10个连接请求)
        sokWatch.Listen(10);

        while (isWatch)
        {
            //threadWatch.SetApartmentState(ApartmentState.STA);
            //监听 客户端 连接请求，但是，Accept会阻断当前线程
            Socket sokMsg = sokWatch.Accept();//监听到请求，立即创建负责与该客户端套接字通信的套接字
            ConnectionClient connection = new ConnectionClient(sokMsg, GetByteMsg, RemoveClientConnection);
            //将负责与当前连接请求客户端 通信的套接字所在的连接通信类 对象 装入集合
            dictConn.Add(sokMsg.RemoteEndPoint.ToString(), connection);
            //将 通信套接字 加入 集合，并以通信套接字的远程IpPort作为键
            //dictSocket.Add(sokMsg.RemoteEndPoint.ToString(), sokMsg);
            //将 通信套接字的 客户端IP端口保存在下拉框里
            
            //GetStrMsg(sokMsg.RemoteEndPoint.ToString() + "接收连接成功......");
            //启动一个新线程，负责监听该客户端发来的数据
            //Thread threadConnection = new Thread(ReciveMsg);
            //threadConnection.IsBackground = true;
            //threadConnection.Start(sokMsg);

            connection.SendString("!");
            //if (!hasConnected) hasConnected = true;
        }
    } 
    #endregion

    //bool isRec = true;//与客户端通信的套接字 是否 监听消息

    #region 2 移除与指定客户端的连接 +void RemoveClientConnection(string key)
    /// <summary>
    /// 移除与指定客户端的连接
    /// </summary>
    /// <param name="key">指定客户端的IP和端口</param>
    public void RemoveClientConnection(string key)
    {
        if (dictConn.ContainsKey(key))
        {
            //Debug.Log("1:" + dictConn.ContainsKey(key));

            dictConn.Remove(key);
            RemoveConnection(key);

            //dictConn[key].CloseConnection();
            //try
            //{
            //    dictConn[key].CloseConnection();
            //}
            //catch (Exception ex)
            //{
            //    Debug.Log(ex);
            //    //dictConn[key].CloseConnection();
            //}
            
            //Debug.Log("2:" + dictConn.ContainsKey(key));
        }
    } 
    #endregion



    public void SendString(string data, string OneClientName = "")
    {

        if (OneClientName == "")
        {
            foreach (ConnectionClient conn in dictConn.Values)
            {
                conn.SendString(data);
            }
        }
        else
        {
            if (dictConn.Keys.Contains(OneClientName))
            {
                dictConn[OneClientName].SendString(data);
            }
        }

    }


    //选择要发送的文件
    //private void btnChooseFile_Click(object sender, EventArgs e)
    //{
    //    OpenFileDialog ofd = new OpenFileDialog();
    //    if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
    //    {
    //        txtFilePath.Text = ofd.FileName;
    //    }
    //}

    //发送文件
    //private void btnSendFile_Click(object sender, EventArgs e)
    //{
    //        //拿到下拉框中选中的客户端IPPORT
    //        string key = cboClient.Text;
    //        if (!string.IsNullOrEmpty(key))
    //        {
    //            dictConn[key].SendFile(txtFilePath.Text.Trim());
    //        }
    //}

    /*
    public void SendBytes(byte[] byteData)
    {
        if (newSocket == null || !newSocket.Connected)
        {
            hasConnected = false;
            //OnUpdateStatus("Disconnected.");
            return;
        }
        newSocket.Send(byteData);
    }
    public void SendString(string s)
    {
        if (newSocket == null || !newSocket.Connected)
        {
            hasConnected = false;
            //OnUpdateStatus("Disconnected.");
            return;
        }
        byte[] byteData = Encoding.Default.GetBytes(s);
        newSocket.Send(byteData);
    }
    */

    public static int GetOpenPort()
    {
        int PortStartIndex = 1000;
        int PortEndIndex = 2000;
        IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
        IPEndPoint[] tcpEndPoints = properties.GetActiveTcpListeners();

        List<int> usedPorts = tcpEndPoints.Select(p => p.Port).ToList<int>();
        int unusedPort = 0;

        for (int port = PortStartIndex; port < PortEndIndex; port++)
        {
            if (!usedPorts.Contains(port))
            {
                unusedPort = port;
                break;
            }
        }
        return unusedPort;
    }


    public static IPAddress GetIP()
    {
        IPHostEntry host;
        //string localIP = "?";
        host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip;
            }
        }

        return null;
    }

    //string msgToSend = "";
    //string selectedConnection = "";

    bool showMsg = false;

    void OnGUI()
    {
        if (showMsg)
        {
            GUILayout.TextField(ip.ToString() + ":" + port.ToString());
            //GUILayout.TextField(port.ToString());
        }
        
        //msgToSend = GUILayout.TextField(msgToSend);

        //if(GUILayout.Button("Send To One")){
        //    //byte[] arrMsg = System.Text.Encoding.UTF8.GetBytes(txtInput.Text.Trim());
        //    //从下拉框中 获得 要哪个客户端发送数据
        //    //string connectionSokKey = cboClient.Text;
        //    if (!string.IsNullOrEmpty(selectedConnection))
        //    {
        //        //从字典集合中根据键获得 负责与该客户端通信的套接字，并调用send方法发送数据过去
        //        SendString(msgToSend, selectedConnection);
        //        //sokMsg.Send(arrMsg);
        //    }
        //    else
        //    {
        //        Debug.Log("请选择要发送的客户端.");
        //    }
        //}

        //if (GUILayout.Button("Send To All"))
        //{
        //    SendString(msgToSend);
        //}

        //if (GUILayout.Button("End"))
        //{
        //    CloseConnections();
        //}


        //foreach(string connection in dictConn.Keys)
        //{
        //    if(GUILayout.Toggle(connection == selectedConnection, connection.ToString())){
        //        selectedConnection = connection.ToString();
        //    }
        //}

        //GUILayout.TextArea(strMsg);
        
    }

    public void CloseConnections()
    {
        isWatch = false;

        var buffer = new List<string>(dictConn.Keys);
        foreach (var key in buffer)
        {
            RemoveClientConnection(key);
        }

        if(sokWatch != null) sokWatch.Close();

        //foreach (string connection in dictConn.Keys)
        //{
        //    RemoveClientConnection(connection);
        //}

        if (threadWatch != null) threadWatch.Abort();
    }

    void OnApplicationQuit()
    {
        CloseConnections();
    }

}
