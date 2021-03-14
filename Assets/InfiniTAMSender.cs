using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class InfiniTAMSender : MonoBehaviour
{
	public string testMessage = "Hello test 123";
	
	public string ipAddress = "localhost";
	public int port = 5447;
	public static InfiniTAMSender instance;
	
	#region private members 	
	private TcpClient socketConnection;
	private Thread clientReceiveThread;
	#endregion

	void Start()
	{
		if (instance != null)
		{
			Destroy(instance);
			Debug.LogError("Two instances of InfiniTAM Sender");
		}
		instance = this;
			
		ConnectToTcpServer();
	}
	
	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			SendMessage("test 123");
		}
	}
	/// <summary> 	
	/// Setup socket connection. 	
	/// </summary> 	
	public void ConnectToTcpServer()
	{
		try
		{
			socketConnection = new TcpClient(ipAddress, port);
			Debug.Log("Successfully connected to server.");
		}
		catch (Exception e)
		{
			Debug.Log("On client connect exception " + e);
		}
	}
	
	/// <summary> 	
	/// Send message to server using socket connection. 	
	/// </summary> 	
	public new void SendMessage(string message)
	{
		if (socketConnection == null)
		{
			return;
		}
		try
		{
			// Get a stream object for writing. 			
			NetworkStream stream = socketConnection.GetStream();
			if (stream.CanWrite)
			{
				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(message);
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
				Debug.Log("Client sent his message - should be received by server");
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}
}