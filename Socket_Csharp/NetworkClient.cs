using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Text;
using System.Collections.Generic;

public class NetworkClient : MonoBehaviour {

	public delegate void RecieveMessageDelegate(byte[] msg);
	public RecieveMessageDelegate RecieveMessage;
	public bool isConnected = false;
	string iport = "192.168.1.100:1000";
	int port = 1000;
	IPAddress ip = null;
	
	Thread thdRec;
	public Socket clientSocket;
	bool doRec = true;

	public static NetworkClient Instance;

	//bool sentAtLeastOneMsg = false;
	//public delegate void OnConnected;

	void Awake(){
		Instance = this;
	}

	void Start()
	{
		if (PlayerPrefs.HasKey("iport"))
		{
			iport = PlayerPrefs.GetString("iport");

			Connect_Click();
		}
	}



//	void FixedUpdate () {
//		if (!isConnected || !sentAtLeastOneMsg) {
//			SendString("ComeConnectMe!");
//			sentAtLeastOneMsg = true;
//		}
//	}

	string msg = "";

	void OnGUI()
	{

		if (!isConnected)
		{
			GUI.skin.textField.fontSize = 30;
			GUI.skin.button.fontSize = 30;

			iport = GUI.TextField(new Rect(10, 10, 400, 70), iport);
			
			if (GUI.Button(new Rect(10, 90, 400, 70), "Connect"))
			{
				Connect_Click();
			}

			if(msg != "") GUI.Box(new Rect(10, 170, 400, 50), msg);
		}
	}

	public void Connect_Click(){
		string[] splResult = iport.Split(':');
		if (splResult.Length != 2)
		{
			msg = "Wrong IP/Port!";
			return;
		}
		int tmpPort;
		if (!int.TryParse(splResult[1], out tmpPort) || tmpPort < 0 || tmpPort > 65535)
		{
			msg = "Wrong IP/Port!";
			return;
		}
		//Debug.Log(port);
		port = tmpPort;
		IPAddress tmpIp;
		try
		{
			tmpIp = IPAddress.Parse(splResult[0]);
		}
		catch
		{
			tmpIp = null;
			msg = "Wrong IP/Port!";
			return;
		}
		
		ip = tmpIp;
		
		msg = "Try connecting...";
		
		PlayerPrefs.SetString("iport", iport);
		PlayerPrefs.Save();
		
		StartConnecting();
	}


	public void StartConnecting()
	{
		if (!isConnected)
		{
			try
			{
				clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				IPEndPoint ipEndPoint = new IPEndPoint(ip, port);
				//Debug.Log(ip.ToString() + "|" + port.ToString());
				clientSocket.Connect(ipEndPoint);
				
				thdRec = new Thread(new ThreadStart(ClientReceiving));
				thdRec.Start();

				StartCoroutine(DetectConnection());
			}
			catch (Exception ex)
			{
				//Debug.Log(ex.Message);
				msg = ex.Message;
			}
		}
	}

	IEnumerator DetectConnection()
	{
		float notConnectedCount = 0;
		yield return new WaitForSeconds(0.5f);
		
		while (!isConnected && ((notConnectedCount += Time.deltaTime) < 5.5f))
		{
			yield return 2;
		}
		
		if (isConnected)
		{
			SendString("!");
			//Connect!
		}
		
	}
	
	private void ClientReceiving()
	{
		try
		{
			//ShowMessage("Ready...");
			while (doRec)
			{
				//Color: 27648 = 128 * 72 * 3 * 3  1166400 = 480 * 270 * 3 * 3 Depth:651264 = 512 * 424 * 3
				//			int rev = 0;
				byte[] bytes = new byte[1024]; //27648 + 2 //82945 //1024
				
				int rev = clientSocket.Receive(bytes, bytes.Length, 0);
				
				if (rev <= 0)
				{
					//ShowMessage("Disconnected.");
					doRec = false;
					Close();
					break;
				}
				//Debug.Log("Rev: " + rev.ToString());
				
				if (!isConnected)
				{
					//ShowMessage("Connected!", MessageFadeOutType.FadeOutNow);
					isConnected = true;
				}

				RecieveMessage(bytes);
				//ProcessData(bytes);
			}
		}catch(ThreadAbortException ex){
			Debug.Log(ex.Message);
		}
		
	}
	
//	private void ProcessData(byte[] data)
//	{
//
//		//string strData = System.Text.Encoding.ASCII.GetString(data).Trim(new char[] { '\0' });
//	}
	
	public void SendString(string str)
	{
		if (null != clientSocket && clientSocket.Connected)
		{
			byte[] strByte = Encoding.ASCII.GetBytes(str);
			//byte[] strMessage = new byte[strByte.Length + 1];
			//strByte.CopyTo(strMessage, 0);
			//strMessage[strByte.Length] = "\0";
			clientSocket.Send(strByte);
		}
		else
		{
			//Debug.Log("Disconnected.");
			//ShowMessage("Disconnected", MessageFadeOutType.FadeOutLater);
		}
	}
	
	void OnApplicationQuit()
	{
		Close();
	}

	public void Close(){
		doRec = false;
		if (clientSocket != null)
		{
			clientSocket.Shutdown(SocketShutdown.Both);
			clientSocket.Close();
		}
		if (thdRec != null) thdRec.Abort();
	}

	void OnDestroy(){
		Instance = null;
	}
}
