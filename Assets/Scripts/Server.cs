using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine.Serialization;


public class Server : MonoBehaviour
{
    //변수 선언
    public InputField PortInput;

    List<ServerClient> clients;
    List<ServerClient> disconnectList;

    TcpListener server;
    private bool _serverStarted;
    public int gameRspStarted;
    public int gameMjpStarted;
    public int serverHand;
    public int clientHand;
    public int results;
    public int attackPlayer;
    public int gameCount;
    public int clientWinCount;
    public int totalGameCount;
    public int serverWinCount;
    public int drawCount;

    public enum State // 게임 상태 (확인용)
    {
        IDLE,
        RockScissorsPaper,
        MmokJjiPpa
    }
    
    int[,] gameResults = new int [3, 3]; // 게임의 승패 배열
    public State state = State.IDLE;
    
    
    //변수 초기화
    private void Awake()
    {
        //[server, enemy], scissors = 0 rock = 1 paper = 2 , draw = 0, win = 1, lose = 2
        //   S  R  P  --> server
        // S 0  1  2
        // R 2  0  1
        // P 1  2  0
        // ㄴ client
        // 
        gameResults[0, 0] = 0;
        gameResults[0, 1] = 1;
        gameResults[0, 2] = 2;

        gameResults[1, 0] = 2;
        gameResults[1, 1] = 0;
        gameResults[1, 2] = 1;

        gameResults[2, 0] = 1;
        gameResults[2, 1] = 2;
        gameResults[2, 2] = 0;

        gameRspStarted = 0; // 가위 바위 보 시작
        gameMjpStarted = 0; // 묵찌빠 시작

        serverHand = 3; // 서버가 낸 것
        results = 0; // 게임 승패
        attackPlayer = 0; // Player1(Server) = 1, Player2(Client) = 2
        
        gameCount = 0; // 묵찌빠 5회 제한 카운터
        clientWinCount = 0; // Client측 승리 횟수
        totalGameCount = 0; // 전체 게임 횟수
        serverWinCount = 0; // Server측 승리 횟수 (Client측 패배)
        drawCount = 0; // 비긴 횟수



    }


    public void ServerCreate() // 서버 생성
	{
        clients = new List<ServerClient>();
        disconnectList = new List<ServerClient>();
        
        try
        {
            int port = PortInput.text == "" ? 7777 : int.Parse(PortInput.text);
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            StartListening();
            _serverStarted = true;
            Chat.instance.ShowMessage($"서버가 {port}에서 시작되었습니다.");
        }
        catch (Exception e) 
        {
            Chat.instance.ShowMessage($"Socket error: {e.Message}");
        }
	}

	void Update()
	{
        
        
        if (!_serverStarted) return;
        
        foreach (ServerClient c in clients) 
        {
            // 클라이언트가 여전히 연결되있나?
            if (c != null && !IsConnected(c.tcp))
            {
                c.tcp.Close();
                disconnectList.Add(c);
                continue;
            }
            // 클라이언트로부터 체크 메시지를 받는다
            else 
            {
                NetworkStream s = c.tcp.GetStream();
                if (s.DataAvailable) 
                {
                    string data = new StreamReader(s, true).ReadLine();
                    if (data != null)
                        OnIncomingData(c, data);
                }
            }
        }

		for (int i = 0; i < disconnectList.Count - 1; i++)
		{
            Broadcast($"{disconnectList[i].clientName} 연결이 끊어졌습니다", clients);
            
            clients.Remove(disconnectList[i]);
            disconnectList.RemoveAt(i);
		}
        
	}

	

	bool IsConnected(TcpClient c)
	{
        try
        {
            if (c != null && c.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);

                return true;
            }
            else
                return false;
        }
        catch 
        {
            return false;
        }
	}

	void StartListening()
	{
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
	}

    void AcceptTcpClient(IAsyncResult ar) 
    {
        TcpListener listener = (TcpListener)ar.AsyncState;
        clients.Add(new ServerClient(listener.EndAcceptTcpClient(ar)));
        StartListening();

        // 메시지를 연결된 모두에게 보냄
        Broadcast("%NAME", new List<ServerClient>() { clients[clients.Count - 1] });
    }


    void OnIncomingData(ServerClient c, string data) // 받은 데이터 분석
    {
        if (data.Contains("&NAME")) 
        {
            c.clientName = data.Split('|')[1];
            Broadcast($"{c.clientName}이 연결되었습니다", clients);
            return;
        }

        // Client로 받은 데이터 분석
        // Start -> 가위바위보 시작, Rock(1), Scissors(0), Paper(2) -> Client측 (enemy Hand) 데이터
        if (data.Contains("Start"))
        {
            gameRspStarted = 1;
            
            GameStart(); // 게임 시작
            
        }
        
        else if (data.Contains("&Rock"))
        {
            clientHand = 1;
        }
        
        else if (data.Contains("&Scissors"))
        {
            clientHand = 0;
        }
        
        else if (data.Contains("&Paper"))
        {
            clientHand = 2;
        }
        else if (data.Contains("Stat"))
        {
            Chat.instance.ShowMessage("Server Gamestat");
            Debug.Log("Server Gamestat");
            StartCoroutine(SendGameStat());
            StopCoroutine(SendGameStat());
        }
        
        else if (data.Contains("Disconnect"))
        {
            Broadcast("Disconnect", clients);
            gameRspStarted = 0;
            gameMjpStarted = 0;
            serverHand = 3;
            StopAllCoroutines();
        }
        else
        {
            Debug.Log("Nothing");
            Chat.instance.ShowMessage("Nothing");
        }
        
        
        Broadcast($"{c.clientName} : {data}", clients);
    }

    // 게임 시작하는 함수
    public void GameStart()
    {
        // Client측 데이터를 데이터 받는 상태(3)으로 설정
        clientHand = 3;


        if (gameRspStarted == 1)
        {
            Debug.Log("Game Rock Scissors Paper started");
            Chat.instance.ShowMessage("Game Rock Scissors Paper started");
            Broadcast("Game RSP Start", clients);
            state = State.RockScissorsPaper;
            StartCoroutine(GameRsp()); // 가위바위보 코루틴 시작
        }

    }

    public void GameEnd() // 게임 종료시키는 함수
    {
        if (gameRspStarted == 0 && gameMjpStarted == 1) // 가위바위보 코루틴 종료, 묵찌빠 시작
        {
            Debug.Log("Game Rock Scissors Paper end");
            Chat.instance.ShowMessage("Game Rock Scissors Paper end");
            StopCoroutine(GameRsp());
            state = State.MmokJjiPpa;
            Debug.Log("Game Mmok Jji Ppa start");
            Chat.instance.ShowMessage("Game Mmok Jji Ppa start");
            StartCoroutine(GameMjp());
            
        }
        
        else if (gameMjpStarted == 0)
        {
            Debug.Log("Game Mmok Jji Ppa end");
            Chat.instance.ShowMessage("Game Mmok Jji Ppa end");
            gameCount = 0;
            StopCoroutine(GameMjp());
            StartCoroutine(SendGameEnd());
            StopCoroutine(SendGameEnd());
        }
    }

    IEnumerator SendGameEnd()
    {
        yield return new WaitForSeconds(0.5f);
        Broadcast("Game End", clients);
    }
    
    IEnumerator SendGameStat()
    {
        yield return new WaitForSeconds(0.5f);
        Broadcast($"Total Game : {totalGameCount} "+$"Win : {clientWinCount} "+$"Lose : {serverWinCount} "
                  +$"Draw : {drawCount} "+$"Win Rate : {(float)clientWinCount/(float)totalGameCount}", clients);
    }
    
    

    IEnumerator GameRsp() //rock scissors paper game logic
    {
        while (state == State.RockScissorsPaper)
        {
            if (clientHand == 3)
            {
                serverHand = UnityEngine.Random.Range(0,3);
                Debug.Log("RSP Random number " + serverHand);
                Chat.instance.ShowMessage("RSP Random number " + serverHand);
            }

            else
            {
                results = gameResults[serverHand, clientHand];
                
                if (results == 0)
                {
                    Debug.Log("Draw");
                    Chat.instance.ShowMessage("Draw");
                    gameMjpStarted = 0;
                    gameRspStarted = 1;
                    clientHand = 3;
                    
                    Broadcast($"&{serverHand} "+"Again", clients);
                }
                else if (results == 1)
                {
                    Debug.Log("RSP Enemy Win");
                    Chat.instance.ShowMessage("RSP Enemy Win");
                    clientHand = 3;
                    gameMjpStarted = 1;
                    gameRspStarted = 0;
                    attackPlayer = 2;
                    
                    Broadcast($"&{serverHand} "+"You are attack!", clients);
                    GameEnd();
                }
                    
                else if (results == 2)
                {
                    Debug.Log("RSP Server Win");
                    Chat.instance.ShowMessage("RSP Server Win");
                    clientHand = 3;
                    gameMjpStarted = 1;
                    gameRspStarted = 0;
                    attackPlayer = 1;
                    
                    Broadcast($"&{serverHand} "+"Enemy attack", clients);
                    GameEnd();
                }
                
                
            }
            
            yield return new WaitForSeconds(0.1f);
            
        }
    }
    
    IEnumerator GameMjp() //Mmok Jji Ppa game logic
    {
        while (state == State.MmokJjiPpa)
        {
            
            
            if (clientHand == 3)
            {
                serverHand = UnityEngine.Random.Range(0,3);
                Debug.Log("MJP Random Number " + serverHand);
                Chat.instance.ShowMessage("MJP Random Number " + serverHand);
            }

            else
            {
                results = gameResults[serverHand, clientHand];
                
                if (results == 0)
                {
                    Debug.Log("Player " + attackPlayer + " Win");
                    Chat.instance.ShowMessage("Player " + attackPlayer + " Win");
                    if (attackPlayer == 1)
                    {
                        serverWinCount++;
                        Broadcast($"&{serverHand} "+"You Lose...", clients);
                    }
                    else if (attackPlayer == 2)
                    {
                        clientWinCount++;
                        Broadcast($"&{serverHand} "+"You Win", clients);
                    }
                    totalGameCount++;
                    gameMjpStarted = 0;
                    state = State.IDLE;
                    GameEnd();
                    
                }
                
                else if (gameCount == 5)
                {
                    Debug.Log("Draw");
                    Chat.instance.ShowMessage("Draw");
                    gameCount = 0;
                    attackPlayer = 0;
                    gameMjpStarted = 0;
                    drawCount++;
                    state = State.IDLE;
                    Broadcast($"&{serverHand} "+"Draw", clients);
                    GameEnd();
                    
                }
                
                else if (results == 1)
                {
                    Debug.Log("MJP Client Win");
                    Chat.instance.ShowMessage("MJP Client Win");
                    attackPlayer = 2;
                    clientHand = 3;
                    gameCount++;
                    Broadcast($"&{serverHand} "+"You are attack", clients);
                }
                    
                else if (results == 2)
                {
                    Debug.Log("MJP Server Win");
                    Chat.instance.ShowMessage("MJP Server Win");
                    attackPlayer = 1;
                    clientHand = 3;
                    gameCount++;
                    Broadcast($"&{serverHand} "+"You are defense", clients);
                }
                
                
            }
            
            yield return new WaitForSeconds(0.1f);
            
        }
    }

    

    void Broadcast(string data, List<ServerClient> cl) // 접속된 모든 Client에 메시지 전송 
    {
        
        foreach (var c in cl) 
        {
            try 
            {
                StreamWriter writer = new StreamWriter(c.tcp.GetStream());
                for(int i = 0; i < clients.Count; i++)
                    Debug.Log(clients[i]);
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e) 
            {
                Chat.instance.ShowMessage($"쓰기 에러 : {e.Message}를 클라이언트에게 {c.clientName}");
            }
        }
    }
    /*
    void Unicast(string data, List<ServerClient> cl) // 단일 유저에게 메시지 전송
    {
        
        foreach (var c in cl) 
        {
            
            try 
            {
                StreamWriter writer = new StreamWriter(c.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
                
            }
            catch (Exception e) 
            {
                Chat.instance.ShowMessage($"쓰기 에러 : {e.Message}를 클라이언트에게 {c.clientName}");
            }
        }
    }
    */
}


public class ServerClient
{
    public TcpClient tcp;
    public string clientName;

    public ServerClient(TcpClient clientSocket) 
    {
        clientName = "Guest";
        tcp = clientSocket;
    }
} 
