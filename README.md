# TCP_MmokJjiPpa  
  
TCP 프로토콜을 이용한 묵찌빠 게임  

![TCP_MmokJjiPpa](https://user-images.githubusercontent.com/42915126/152273595-ca52c8ac-e49b-4a8b-94ac-d93babb6136e.png)

# 게임 방식
1. 서버 오픈 후 플레이어 접속 (서버, 클라이언트 동시 사용 가능)
2. "게임 시작" 버튼을 눌러 게임 시작
3. 첫 게임은 가위바위보를 통해 공격자와 수비자를 결정, 무승부시 다시 가위바위보 시행
4. 공격자와 수비자가 정해졌다면, 묵찌빠 게임 시작  
4-1. 공격자와 수비자가 같은 카드를 낸다면, 공격자 승리  
4-2. 공격자와 수비자가 다른 카드를 낸다면, 가위바위보 룰을 따라서 공격자 결정  
4-3. 최대 5회까지 묵찌빠 게임 시행 후, 승리자가 없다면 무승부  
5. 묵찌빠 게임 종료  
  
# 기능
- 한 프로그램에서 서버 열기, 접속 가능
- TCP 프로토콜을 이용해 다른 서버에 접속 가능
- 서버에서 승패 기록 저장
- 플레이어가 승패 기록 및 승률 확인 가능
- 도움말 출력 기능
- 게임 도중 접속 종료 기능
- 프로그램 종료 기능
  
# 참고한 자료
https://youtu.be/y3FU6d_BpjI  
  
# 사용한 에셋
AllSky Free: https://assetstore.unity.com/packages/2d/textures-materials/sky/allsky-free-10-sky-skybox-set-146014  
Arm Chair: https://assetstore.unity.com/packages/3d/props/furniture/arm-chair-80384  
PBR Table: https://assetstore.unity.com/packages/3d/props/wooden-pbr-table-112005  
Free Playing Cards Pack: https://assetstore.unity.com/packages/3d/props/tools/free-playing-cards-pack-154780
