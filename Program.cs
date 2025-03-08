using System.Net.Sockets;
using System.Text;

namespace TCPServerMax
{
    class Program
    {
        public static ServerSocket socket;
        public static UserStorage userStorage;
        static void Main(string[] args)
        {
            socket = new ServerSocket();
            userStorage = new UserStorage();
            socket.Start("127.0.0.1", 8080, 1024);

            while (true)
            {
                string input = Console.ReadLine();
                if (input == "Quit")
                {
                    socket.Close();
                }
                //定义一个广播
                else if (input.Substring(0, 2) == "B:")
                {
                    if (input.Substring(2) == "1001")
                    {
                        Console.WriteLine("向客户端发送玩家数据");
                        PlayerMsg playerMsg = new PlayerMsg();
                        playerMsg.playerData = new PlayerData();
                        playerMsg.playerID = 5555;
                        playerMsg.playerData.name = "玩家10";
                        playerMsg.playerData.atk = 10;
                        playerMsg.playerData.lev = 99;
                        socket.Broadcast(playerMsg);
                    }
                    else if (input.Substring(2) == "1000")
                    {
                        Console.WriteLine("向客户端广播信息");
                        BroadMsg broadMsg = new BroadMsg();
                        broadMsg.msg = "你好，这是服务器的一条广播信息";
                        socket.Broadcast(broadMsg);
                    }
                    else if (input.Substring(2) == "1010")
                    { 

                    }
                    //BroadMsg msg = new BroadMsg();
                    //msg.msg = input.Substring(2);
                    //socket.Broadcast(msg);
                    Console.WriteLine("服务器试图广播:" +  input);
                }
            }
        }
    }
}