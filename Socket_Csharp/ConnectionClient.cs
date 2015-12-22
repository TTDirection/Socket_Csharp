using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

//namespace Server
//{

    

    /// <summary>
    /// 与客户端的 连接通信类(包含了一个 与客户端 通信的 套接字，和线程)
    /// </summary>
    public class ConnectionClient
    {
        public delegate void DGByteMsg(byte[] bytMsg, string clientName);
        public delegate void DGShowMsg(string strMsg);

        Socket sokMsg;
        DGByteMsg dgByteMsg;//负责 向主窗体文本框显示消息的方法委托
        DGShowMsg dgRemoveConnection;// 负责 从主窗体 中移除 当前连接
        Thread threadMsg;
        string myName = "";
        #region 构造函数
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sokMsg">通信套接字</param>
        /// <param name="dgShowMsg">向主窗体文本框显示消息的方法委托</param>
        public ConnectionClient(Socket sokMsg, DGByteMsg dgByteMsg, DGShowMsg dgRemoveConnection)
        {
            this.sokMsg = sokMsg;
            this.dgByteMsg = dgByteMsg;
            this.dgRemoveConnection = dgRemoveConnection;

            this.threadMsg = new Thread(RecMsg);
            this.threadMsg.IsBackground = true;
            this.threadMsg.Start();
        }
        #endregion

        bool isRec = true;
        #region 02负责监听客户端发送来的消息
        void RecMsg()
        {
            while (isRec)
            {
                try
                {
                    //Debug.Log("...");
                    byte[] arrMsg = new byte[1024];//new byte[1024 * 1024 * 2];//
                    //接收 对应 客户端发来的消息
                    int length = sokMsg.Receive(arrMsg);

                    if (length <= 0)
                    {
                        //isRec = false;
                        Debug.Log("length <= 0");
                        Close();
                        break;
                    }
                    //将接收到的消息数组里真实消息转成字符串
                    //string strMsg = System.Text.Encoding.UTF8.GetString(arrMsg, 0, length);
                    //string strMsg = System.Text.Encoding.Default.GetString(arrMsg);

                    //string strMsg = Encoding.ASCII.GetString(arrMsg).Trim(new char[] { '\0' });

                    if (myName == "") myName = sokMsg.RemoteEndPoint.ToString();
                    //Debug.Log(myName + ", length: " + length.ToString());
                    //通过委托 显示消息到 窗体的文本框
                    //ArraySegment<byte> byteSeg = new ArraySegment<byte>(arrMsg, 0, length);
                    //dgByteMsg(new ArraySegment<byte>(arrMsg, 0, length), myName);

                    //Byte[] newBytes = new Byte[length];
                    //Buffer.BlockCopy(arrMsg, 0, newBytes, 0, length);

                    dgByteMsg(arrMsg, myName);
                }
                catch (Exception ex)
                {
                    Debug.Log("Exception: " + ex.Message);
                    //从主窗体中 移除 下拉框中对应的客户端选择项，同时 移除 集合中对应的 ConnectionClient对象
                    //dgRemoveConnection(sokMsg.RemoteEndPoint.ToString());
                    Close();
                    break;
                }
            }
        }
        #endregion

        #region 03向客户端发送消息
        /// <summary>
        /// 向客户端发送消息
        /// </summary>
        /// <param name="strMsg"></param>
        public void SendString(string strMsg)
        {
            if (sokMsg == null || !sokMsg.Connected) return;
            //byte[] arrMsg = System.Text.Encoding.UTF8.GetBytes(strMsg);
            byte[] arrMsg = System.Text.Encoding.ASCII.GetBytes(strMsg);
            //byte[] arrMsgFinal = new byte[arrMsg.Length + 1];

            //arrMsgFinal[0] = 1;//设置 数据标识位等于1，代表 发送的是 文字
            //arrMsg.CopyTo(arrMsgFinal, 1);
            Debug.Log("Sending to: [" + myName + "]: " + strMsg);
            //sokMsg.Send(arrMsgFinal);
            sokMsg.Send(arrMsg);

        }

        //public void SendByte(byte[] bytMsg)
        //{
        //    byte[] arrMsgFinal = new byte[bytMsg.Length + 1];

        //    arrMsgFinal[0] = 0;//设置 数据标识位等于0，代表 发送的是 字节
        //    bytMsg.CopyTo(arrMsgFinal, 1);

        //    sokMsg.Send(arrMsgFinal);
        //}

        //public void SendSuccessTex(string strTexName)
        //{
        //    byte[] arrMsg = System.Text.Encoding.ASCII.GetBytes(strTexName);
        //    byte[] arrMsgFinal = new byte[arrMsg.Length + 1];

        //    arrMsgFinal[0] = 2;
        //    arrMsg.CopyTo(arrMsgFinal, 1);

        //    sokMsg.Send(arrMsgFinal);
        //}

        //public void SendSuccessShow()
        //{
        //    //byte[] arrMsg = System.Text.Encoding.ASCII.GetBytes("Shown");
        //    //byte[] arrMsgFinal = new byte[arrMsg.Length + 1];
        //    byte[] arrMsgFinal = new byte[1];

        //    arrMsgFinal[0] = 3;
        //    //arrMsg.CopyTo(arrMsgFinal, 1);

        //    sokMsg.Send(arrMsgFinal);
        //}
        #endregion

        #region 04向客户端发送文件数据 +void SendFile(string strPath)
        /// <summary>
        /// 04向客户端发送文件数据
        /// </summary>
        /// <param name="strPath">文件路径</param>
        //public void SendFile(string strPath)
        //{
        //    //通过文件流 读取文件内容
        //    using (FileStream fs = new FileStream(strPath, FileMode.OpenOrCreate))
        //    {
        //        byte[] arrFile = new byte[1024 * 1024 * 2];
        //        //读取文件内容到字节数组，并 获得 实际文件大小
        //        int length = fs.Read(arrFile, 0, arrFile.Length);
        //        //定义一个 新数组，长度为文件实际长度 +1
        //        byte[] arrFileFina = new byte[length + 1];
        //        arrFileFina[0] = 1;//设置 数据标识位等于1，代表 发送的是文件
        //        //将 文件数据数组 复制到 新数组中，下标从1开始
        //        //arrFile.CopyTo(arrFileFina, 1);
        //        Buffer.BlockCopy(arrFile, 0, arrFileFina, 1, length);
        //        //发送文件数据
        //        sokMsg.Send(arrFileFina);//, 0, length + 1, SocketFlags.None);
        //    }
        //}
        #endregion

        #region 06关闭与客户端连接
        /// <summary>
        /// 关闭与客户端连接
        /// </summary>
        public void Close()
        {
            isRec = false;

            dgRemoveConnection(myName);
            //try
            //{
                if (sokMsg != null && sokMsg.Connected)
                {
                    sokMsg.Shutdown(SocketShutdown.Both);
                    sokMsg.Close(800);
                    //sokMsg = null;
                }

                //if (threadMsg != null)
                //{
                //    threadMsg.Abort();
                //    threadMsg = null;
                //}

                
            //}
            //catch (Exception ex)
            //{
            //    Debug.Log(ex);
            //}   
                //SecondaryClose();
        }
        
        #endregion

    }
//}