using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.IO;
using System;

public class Client : MonoBehaviour
{
	public InputField IPInput, PortInput, NickInput;
	string clientName;

	public static int gamestart;
    bool socketReady;
    TcpClient socket;
    NetworkStream stream;
	StreamWriter writer;
    StreamReader reader;
    
    public static int receiveServerHand;
    public static int result;
    public int clientHand;
    public GameObject gameObject;

    private void Awake()
    {
	    gamestart = 0;
	    receiveServerHand = 3;
	    result = 3;
	    clientHand = 3;
    }


    public void ConnectToServer() // 서버에 연결
	{
		// 이미 연결되었다면 함수 무시
		if (socketReady) return;

		// 기본 호스트/ 포트번호
		string ip = IPInput.text == "" ? "127.0.0.1" : IPInput.text;
		int port = PortInput.text == "" ? 7777 : int.Parse(PortInput.text);

		// 소켓 생성
		try
		{
			socket = new TcpClient(ip, port);
			stream = socket.GetStream();
			writer = new StreamWriter(stream);
			reader = new StreamReader(stream);
			socketReady = true;
			gameObject = GameObject.Find("ServerCreateBtn");
			gameObject.SetActive(false);
		}
		catch (Exception e) 
		{
			Chat.instance.ShowMessage($"소켓에러 : {e.Message}");
		}
		
	}

	void Update()
	{
		if (socketReady && stream.DataAvailable) 
		{
			string data = reader.ReadLine();
			if (data != null)
				OnIncomingData(data);
		}
	}

	void OnIncomingData(string data) // 받은 데이터 분석
	{
		if (data == "%NAME") 
		{
			clientName = NickInput.text == "" ? "Guest" + UnityEngine.Random.Range(1000, 10000) : NickInput.text;
			Send($"&NAME|{clientName}");
			return;
		}
		
		
		if (data.Contains("&1"))
		{
			receiveServerHand = 1;
			CheckWinLose(data);
			Debug.Log("receive: rock");
		}
		
		else if (data.Contains("&0"))
		{
			receiveServerHand = 0;
			CheckWinLose(data);
			Debug.Log("receive: scissors");
		}
		
		else if (data.Contains("&2"))
		{
			receiveServerHand = 2;
			CheckWinLose(data);
			Debug.Log("receive: paper");
		}

		else if(data.Contains("Game End"))
		{
			gamestart = 0;
		}
		
		else if (data.Contains("Game RSP Start"))
		{
			Chat.instance.ShowMessage("서버와 게임 시작 통신 확인");
			gamestart = 1;
		}
		
		else if (data.Contains("Disconnect"))
		{
			gamestart = 0;
			Chat.instance.ShowMessage("접속 종료");
			gameObject.SetActive(true);
			CloseSocket();	
		}

		


		Chat.instance.ShowMessage(data);
	}

	void CheckWinLose(string data) // 승패 분석
	{
		if(data.Contains("You Win"))
		{
			result = 1;
		}
		
		else if(data.Contains("Draw"))
		{
			result = 0;
		}
		
		else if(data.Contains("You Lose"))
		{
			result = 2;
		}
		else
		{
			result = 3;
		}
	}
	
	// 해당하는 버튼 눌리면 Server로 문자열 보냄
	public void Rock() 
	{
		if (gamestart == 1)
		{
			Debug.Log("&Rock");
			clientHand = 1;
			Send("&Rock"); // Server로 "Rock" 문자열 보냄
			
		}
	}
	
	public void Paper()
	{
		if (gamestart == 1)
		{
			Debug.Log("&Paper");
			clientHand = 2;
			Send("&Paper");
			
		}
	}
	
	public void Scissors()
	{
		if (gamestart == 1)
		{
			Debug.Log("&Scissors");
			clientHand = 0;
			Send("&Scissors");
			
		}
	}

	public void Gamestart()
	{
		
		result = 3;
		receiveServerHand = 3;
		Debug.Log("Start");
		Debug.Log(socketReady);
		Send("Start");
	}
	
	public void Gamestat()
	{
		//ConnectToServer();
		Debug.Log("Stat");
		Debug.Log(socketReady);
		Send("Stat");
	}

	public void Help()
	{
		Debug.Log("Help");
		Chat.instance.ShowMessage("닉네임: 묵찌빠 게임에서 사용할 닉네임을 정합니다.");
		Chat.instance.ShowMessage("SERVER IP: 묵찌빠 게임 서버의 IP를 입력합니다.");
		Chat.instance.ShowMessage("PORT: 묵찌빠 게임 서버의 PORT를 입력합니다.");
		Chat.instance.ShowMessage("서버 열기: 묵찌빠 게임 서버를 엽니다.");
		Chat.instance.ShowMessage("클라이언트 접속: 묵찌빠 게임 서버에 접속합니다.");
		Chat.instance.ShowMessage("게임 시작: 묵찌빠 게임을 시작합니다.");
		Chat.instance.ShowMessage("승률: 전체 게임 기록과 승률을 불러옵니다.");
		Chat.instance.ShowMessage("도움말: 각 기능에 대한 설명을 나타냅니다.");
		Chat.instance.ShowMessage("접속 종료: 접속한 서버와의 연결을 끊습니다.");
		Chat.instance.ShowMessage("프로그램 종료: 프로그램을 종료합니다.");
	}

	public void Exit()
	{
		Send("Disconnect");
		
		
	}

	public void Program_Exit()
	{
		Chat.instance.ShowMessage("프로그램 종료");
		CloseSocket();
		Application.Quit();
	}
	//
	

	void Send(string data) // 서버로 데이터 전송
	{
		if (!socketReady) return;
		Debug.Log("Data Sended");
		writer.WriteLine(data);
		writer.Flush();
	}

	public void OnSendButton(InputField SendInput) 
	{
#if (UNITY_EDITOR || UNITY_STANDALONE)
		if (!Input.GetButtonDown("Submit")) return;
		SendInput.ActivateInputField();
#endif
		if (SendInput.text.Trim() == "") return;

		string message = SendInput.text;
		SendInput.text = "";
		Send(message);
	}


	void OnApplicationQuit()
	{
		CloseSocket();
	}

	void CloseSocket() // 접속 종료
	{
		if (!socketReady) return;

		writer.Close();
		reader.Close();
		socket.Close();
		socketReady = false;
	}
}
