using System;
using System.Text;
using System.Net.Sockets;
using MySql.Data.MySqlClient;
using System.Xml;

namespace TcpServer
{
    /// <summary>
    /// 接收和处理client通信
    /// </summary>
    class ClientHandle
    {
        // 成员变量
        private TcpClient clientSocket = null;
        private string recvStr; //接收到的字符串
        //byte[] data = new byte[1024];
        //byte[] dataSend = new byte[1024];

        public TcpClient ClientSocket//属性
        {
            get
            {
                return clientSocket;
            }
            set
            {
                clientSocket = value;
            }
        }

        /// <summary>
        /// 数据处理线程的入口
        /// 在这个函数中完成数据的接收和处理
        /// 包括写入数据库
        /// </summary>
        public void RecvData()
        {
            //获取网络流对象
            NetworkStream clientStream = this.ClientSocket.GetStream();

            Console.WriteLine("<----已接受来自" + this.ClientSocket.Client.RemoteEndPoint.ToString() + "的连接请求---->"
                              + DateTime.Now.ToString());

            #region try
            try
            {
                while (true)
                {
                    byte[] dataRecv = new byte[1024];
                    byte[] dataSend = new byte[1024];
                    //从网络流读取数据存到data数组里，返回读取到的长度
                    int recvLength = clientStream.Read(dataRecv, 0, 1000);
                    recvStr = Encoding.Default.GetString(dataRecv).Trim('\0');//去掉字符串中所有的无效项“\0”
                    if (recvLength > 0)
                    {
                        //下面两行最后上线运行取消掉注释，调试阶段可以将这两行注释掉让所有信息显示出来
                        //if (recvStr.IndexOf("INVALID") > -1)
                        //    continue;

                        Console.WriteLine("\n【接收来自：" + this.ClientSocket.Client.RemoteEndPoint.ToString() + "】"
                                          + DateTime.Now.ToString());
                        string showStr = recvStr.Replace('%', ' ').Replace("~01", " ").Replace("\r", " ").Replace("\n", " ");
                        if (showStr.IndexOf("TIMESYNC") > -1)
                            showStr = "同步时间OK！";
                        Console.WriteLine(showStr);

                        //如果收到的是下位机回复的非气压数据命令，就不进行下面的一些列写入工作，continue，继续下一次循环
                        //~01% TIMESYNCHR _OK\r\n
                        //~01% INVALIDCOMMMD\r\n
                        if (recvStr.IndexOf("TIMESYNC") > -1 || recvStr.IndexOf("INVALID") > -1)
                            continue;

                        //如果收到的是下位机发送过来的气压数据，进行拆分和写入数据库操作
                        //~01%000001%1:16.33%2:16.60%3:8.05%4:10.13%T:17.76%
                        if ((recvStr.IndexOf("~01") > -1) && (recvStr.IndexOf(":") > -1)
                            && (recvStr.IndexOf("%") > -1) && (recvStr.IndexOf("T") > -1))
                        {
                            //Todo:split & construct & insert into DB
                            //一：正常压强数据的写入
                            //Step1:构造正常的压强的插入语句
                            StrSplit cmdSplit = new StrSplit();
                            string insertPressure = cmdSplit.MySplit(recvStr);
                            if (insertPressure != null)
                            {
                                //step2:调用自定义数据库插入函数，在函数里面实例化MySQLConnection和MySqlCommand，指明命令内容和连接名                                
                                //step3:执行插入（气压表）操作
                                DatabaseHandle.MyInsert(insertPressure);
                            }
                        }
                        //同步时间
                        //step1：获取当前时间，并按约定格式形成命令
                        string timeSync = StrSplit.CurrentTimeStr();
                        //step2：将命令转码为byte[]格式，发送出去
                        dataSend = Encoding.ASCII.GetBytes(timeSync);
                        clientStream.Write(dataSend, 0, dataSend.Length);
                    }
                }
            }
            #endregion
            catch (Exception e)
            {
                Console.WriteLine("提示：" + this.ClientSocket.Client.RemoteEndPoint.ToString() + "：" + e.Message);
                this.ClientSocket.Close();
            }
            finally
            {
                clientStream.Close();
                clientStream.Dispose();
                this.clientSocket.Close();
            }
        }
    }
}
