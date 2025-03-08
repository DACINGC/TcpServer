using System.Net;
using System.Net.Sockets;

namespace TCPServerMax
{
    internal class ServerSocket
    {
        public Socket socket;
        public bool isClose = false;
        //存储所有连入的客户端
        public Dictionary<int, ClientSocket> clientDic = new Dictionary<int, ClientSocket>();
        private List<ClientSocket> delList = new List<ClientSocket>();
        /// <summary>
        /// 开启服务器的方法
        /// </summary>
        /// <param name="ip">Ip地址</param>
        /// <param name="port">端口号</param>
        /// <param name="num">接收的客户端数量</param>
        public void Start(string ip, int port, int num)
        {
            isClose = false;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //绑定IP和端口号
            socket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
            socket.Listen(num);
            Console.WriteLine("服务端已建立监听，等待客服端连入.....");

            //从线程池中开启一个线程，用于客服端的连入
            ThreadPool.QueueUserWorkItem(Accept);
            //开启一个线程，用于接收客服端的消息
            ThreadPool.QueueUserWorkItem(Receive);
        }

        /// <summary>
        /// 关闭服务器的方法
        /// </summary>
        public void Close()
        {
            isClose = true;
            foreach (ClientSocket client in clientDic.Values)
            {
                client.Close();
            }
            clientDic.Clear();
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            socket = null;
        }
        /// <summary>
        /// 等待客服端连入(线程池)
        /// </summary>
        private void Accept(Object info)
        {
            while (!isClose)
            {
                try
                {
                    //从服务端连入的客服端
                    Socket clientSocket = socket.Accept();
                    ClientSocket client = new ClientSocket(clientSocket);
                    Console.WriteLine("有客户端进入服务器");

                    //向客服端发送连入的消息
                    BroadMsg broadMsg = new BroadMsg();
                    broadMsg.msg = "消息一：欢迎连入服务器，客户端";
                    client.Send(broadMsg);

                    BroadMsg bro2 = new BroadMsg();
                    bro2.msg = "消息二：你已经成功连入服务器";
                    client.Send(bro2);

                    //client.Send("欢迎连入服务器");
                    //存储到字典中
                    lock (clientDic)
                    { 
                        clientDic.Add(client.ClientID, client);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"客服端连入失败:{e.Message}");
                }
            }
        }
        /// <summary>
        /// 接收客服端消息(线程池)
        /// </summary>
        private void Receive(object info)
        { 
            while(!isClose)
            {
                if (clientDic.Count > 0)
                {
                    lock (clientDic)
                    {
                        foreach (ClientSocket client in clientDic.Values)
                        {
                            client.Receive();
                        }

                        CloseDeleteSocket();
                    }
                }
            }
        } 
        /// <summary>
        /// 删除断开连接的客户端
        /// </summary>
        private void CloseDeleteSocket()
        {
            //遍历移除列表，将可移除的客户端移除
            for (int i = 0; i < delList.Count; i++)
                CloseClientSocket(delList[i]);

            delList.Clear();
        }

        /// <summary>
        /// 向所有客户端广播消息
        /// </summary>
        /// <param name="info"></param>
        public void Broadcast(BaseNetMsg info)
        {
            lock (clientDic)
            {
                foreach (ClientSocket client in clientDic.Values)
                {
                    client.Send(info);
                }
            }
        }
        /// <summary>
        /// 添加到删除列表中
        /// </summary>
        public void AddDelSocket(ClientSocket socket)
        {
            delList.Add(socket);
        }
        /// <summary>
        /// 关闭客户端的连接
        /// </summary>
        public void CloseClientSocket(ClientSocket socket)
        {
            lock (clientDic)
            {
                socket.Close();
                if (clientDic.ContainsKey(socket.ClientID))
                { 
                    clientDic.Remove(socket.ClientID);
                    Console.WriteLine($"客户端{socket.ClientID}主动断开连接");
                }
            }
        }
    }
}
