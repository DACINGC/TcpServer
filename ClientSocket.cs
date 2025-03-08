using System.Net.Sockets;

namespace TCPServerMax
{
    internal class ClientSocket
    {
        private static int CLIENT_ID = 1;
        public int ClientID;
        public Socket socket;

        private byte[] cacheBytes = new byte[1024 * 1024];//处理分包时的字节数组
        private int cacheNum = 0;//分包字节数组长度

        private long frontTime = -1;//上一次收到心跳消息的时间
        private static int TIME_OUT_TIME = 10;

        public ClientSocket(Socket _socket)
        {
            socket = _socket;
            ClientID = CLIENT_ID;
            ++CLIENT_ID;

            //ThreadPool.QueueUserWorkItem(CheckTimeOut);
        }
        /// <summary>
        /// 是否处于连接状态
        /// </summary>
        public bool IsConnected => socket.Connected;
        /// <summary>
        /// 关闭Socket方法
        /// </summary>
        public void Close()
        {
            if (socket != null)
            {
                //停止收发消息
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket = null;
            }
        }
        /// <summary>
        /// 检测心跳消息是否超时
        /// </summary>
        private void CheckTimeOut(/*object info*/)
        {
            //while (IsConnected)
            //{
                if (frontTime != -1 && DateTime.Now.Ticks / TimeSpan.TicksPerSecond - frontTime >= TIME_OUT_TIME)
                {
                    Program.socket.AddDelSocket(this);
                    //break;
                }
                //每5秒进行一次检测
                //Thread.Sleep(5000);
            //}

        }
        /// <summary>
        /// 向客户端发送消息的函数（二进制信息）
        /// </summary>
        /// <param name="msg">发送的消息</param>
        public void Send(BaseNetMsg msg)
        {
            if (IsConnected)
            {
                try
                {
                    //Console.WriteLine("发送的数据为:" + msg);
                    socket.Send(msg.Writing());
                }
                catch (Exception e)
                {
                    Console.WriteLine($"客户端发送消息失败：{e.Message}");
                }
            }
            else
            {
                Program.socket.AddDelSocket(this);
            }
        }

        /// <summary>
        /// 接收客户端消息的函数
        /// </summary>
        public void Receive()
        {
            if (IsConnected)
            {
                try
                {
                    //如果有收到的消息
                    if (socket.Available > 0)
                    {
                        byte[] receiveBytes = new byte[1024 * 1024];//用于收消息的容器
                        int receiveNum = socket.Receive(receiveBytes);//接收到的消息的大小 
                        HandleReceiveMsg(receiveBytes, receiveNum);
                        #region 注释的没有进行分包粘包检测的代码
                        //byte[] result = new byte[1024];
                        //int reciveNum = socket.Receive(result);
                        //int msgID = BitConverter.ToInt32(result, 0);
                        //BaseMsg msg = null;
                        //switch (msgID)
                        //{
                        //    case 1000:
                        //        msg = new BroadMsg();
                        //        msg.Reading(result, 4);
                        //        break;
                        //    case 1001:
                        //        msg = new PlayerMsg();
                        //        msg.Reading(result, 4);
                        //        break;
                        //}

                        //if (msg == null)//为空说明消息类型不存在
                        //{
                        //    Console.WriteLine("消息类型不存在");
                        //    return;
                        //}
                        //ThreadPool.QueueUserWorkItem(MsgHandle, msg);
                        #endregion
                    }
                    CheckTimeOut();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"客户端接收消息出错：{e.Message}");
                }
            }
            else
            {
                //没有连接了，需要断开
                Program.socket.AddDelSocket(this);
            }
        }
        private void HandleReceiveMsg(byte[] receiveBytes, int receiveNum)
        {
            int msgID = -1;
            int msgLength = 0;
            int nowIndex = 0;

            // 查看之前有没有分包的缓存(有就拼接在后面)
            Array.Copy(receiveBytes, 0, cacheBytes, cacheNum, receiveNum);
            //receiveBytes.CopyTo(cacheBytes, cacheNum);
            cacheNum += receiveNum;

            while (true)
            {
                msgLength = -1;
                // 解析一条消息
                if (cacheNum - nowIndex >= 8)
                {
                    msgID = BitConverter.ToInt32(cacheBytes, nowIndex);
                    nowIndex += 4;
                    msgLength = BitConverter.ToInt32(cacheBytes, nowIndex);
                    nowIndex += 4;
                }

                if (cacheNum - nowIndex >= msgLength && msgLength != -1)
                {
                    // 解析消息体
                    BaseNetMsg baseMsg = null;
                    switch (msgID)
                    {
                        case 999:
                            baseMsg = new HeartMsg();
                            //消息体
                            break;
                        case 1000:
                            baseMsg = new BroadMsg();
                            baseMsg.Reading(cacheBytes, nowIndex);
                            break;
                        case 1001:
                            baseMsg = new PlayerMsg();
                            baseMsg.Reading(cacheBytes, nowIndex);
                            break;
                        case 1003:
                            baseMsg = new QuitMsg();
                            //无消息体
                            break;
                        case 1010:
                            baseMsg = new SignInMsg();
                            baseMsg.Reading(cacheBytes, nowIndex);
                            break;
                        case 1011:
                            baseMsg = new SignUpMsg();
                            baseMsg.Reading(cacheBytes, nowIndex);
                            break;
                    }
                    if (baseMsg == null)
                    {
                        Console.WriteLine("消息ID不存在");
                        return;
                    }
                    // 处理消息
                    ThreadPool.QueueUserWorkItem(MsgHandle, baseMsg);
                    nowIndex += msgLength;
                    if (nowIndex == cacheNum)
                    {
                        cacheNum = 0;
                        break;
                    }
                }
                else
                {
                    // 不满足，证明有分包
                    // 如果进行了 ID 和长度的解析 但是 没有成功解析消息体 那么我们需要减去 nowIndex 移动的位置
                    if (msgLength != -1)
                        nowIndex -= 8;
                    // 将剩余没有解析的字节数组内容 移到前面来 用于缓存下次继续解析
                    Array.Copy(cacheBytes, nowIndex, cacheBytes, 0, cacheNum - nowIndex);
                    cacheNum = cacheNum - nowIndex;
                    break;
                }
            }
        }
        private void MsgHandle(object info)
        {
            BaseNetMsg msg = (BaseNetMsg)info;
            //Console.WriteLine($"收到客服端{socket.RemoteEndPoint}的消息：{msg}");
            if (msg is PlayerMsg)
            {
                Console.WriteLine("收到客户端的玩家信息");
                PlayerMsg playerMsg = msg as PlayerMsg;
                Console.WriteLine(playerMsg.playerID);
                Console.WriteLine(playerMsg.playerData.name);
                Console.WriteLine(playerMsg.playerData.atk);
                Console.WriteLine(playerMsg.playerData.lev);
            }
            else if (msg is BroadMsg)
            {
                BroadMsg broadMsg = msg as BroadMsg;
                Console.WriteLine($"收到客户端的信息：{broadMsg.msg}");
            }
            else if (msg is QuitMsg)
            {
                Program.socket.AddDelSocket(this);
            }
            else if (msg is HeartMsg)
            {
                //记录心跳消息的时间
                Console.WriteLine($"收到客户端{ClientID}的心跳消息");
                frontTime = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
            }
            else if (msg is SignInMsg)
            {
                Console.WriteLine($"收到客户端的登录请求：{msg}");
                SignInMsg signInMsg = msg as SignInMsg;
                Console.WriteLine("账号：" + signInMsg.signData.username);
                Console.WriteLine("密码：" + signInMsg.signData.password);

                if (Program.userStorage.ValidateUser(signInMsg.signData.username, signInMsg.signData.password))
                {
                    BroadMsg broadMsg = new BroadMsg();
                    broadMsg.msg = $"客户端{ClientID}登录成功";
                    Send(broadMsg);
                    Console.WriteLine($"客户端{ClientID}登录成功");

                    //发送回去一条账号为1的登录数据，表示登录成功
                    SignInMsg signInMsgSend = new SignInMsg();
                    signInMsgSend.signData = new SignData();
                    signInMsgSend.signData.username = "1";
                    Send(signInMsgSend);

                }
                else
                {
                    BroadMsg broadMsg = new BroadMsg();
                    broadMsg.msg = $"客户端{ClientID}登录失败";
                    Send(broadMsg);
                    Console.WriteLine($"客户端{ClientID}登录失败");

                    //发送回去一条账号为0的登录数据，表示登录失败
                    SignInMsg signInMsgSend = new SignInMsg();
                    signInMsgSend.signData = new SignData();
                    signInMsgSend.signData.username = "0";
                    Send(signInMsgSend);
                }
            }
            else if (msg is SignUpMsg)
            {
                Console.WriteLine($"收到客户端的注册请求：{msg}");
                SignUpMsg signUpMsg = msg as SignUpMsg;

                Console.WriteLine("账号：" + signUpMsg.signData.username);
                Console.WriteLine("密码：" + signUpMsg.signData.password);

                if (Program.userStorage.AddUser(signUpMsg.signData.username, signUpMsg.signData.password))
                {
                    BroadMsg broadMsg = new BroadMsg();
                    broadMsg.msg = $"客户端{ClientID}注册成功";
                    Send(broadMsg);

                    //发送一条账号为1的注册数据，表示注册成功
                    SignUpMsg signUpMsgSend = new SignUpMsg();
                    signUpMsgSend.signData = new SignData();
                    signUpMsgSend.signData.username = "1";
                    Send(signUpMsgSend);
                }
                else
                {
                    BroadMsg broadMsg = new BroadMsg();
                    broadMsg.msg = $"客户端{ClientID}注册失败";
                    Send(broadMsg);

                    //发送一条账号为0的注册数据，表示注册失败
                    SignUpMsg signUpMsgSend = new SignUpMsg();
                    signUpMsgSend.signData = new SignData();
                    signUpMsgSend.signData.username = "0";
                    Send(signUpMsgSend);
                }
            }
        }
    }
}
