using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Net.NetworkInformation;

namespace TcpServer
{
    class Program
    {

        static private TcpListener tcpListener;//声明Linstener

        /// <summary>
        /// 主程序
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //实例化TcpListener
            //绑定IP地址和端口号，开始侦听
            tcpListener = new TcpListener(IPAddress.Any, 6000);
            tcpListener.Start();//start listening
            Console.WriteLine("开始侦听，等待接受连接...");


            try
            {
                while (true)
                { //blocks until a client has connected to the server
                    TcpClient theclient = tcpListener.AcceptTcpClient();//返回一个TcpCLient类实例
                    //实例化客户端处理类，用来接纳返回的TcpClient实例并且进一步处理数据的收发
                    ClientHandle m_handle = new ClientHandle();
                    m_handle.ClientSocket = theclient;

                    //create a thread to handle communication with connected client
                    Thread clientThread = new Thread(new ThreadStart(m_handle.RecvData));//在新线程中处理数据的收发
                    clientThread.Start();
                    #region Active_TCP_Connections
                    //Console.WriteLine("Active TCP Connections");
                    //IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
                    //TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
                    //foreach (TcpConnectionInformation c in connections)
                    //{
                    //    Console.WriteLine("{0} <==> {1}",
                    //        c.LocalEndPoint.ToString(),
                    //        c.RemoteEndPoint.ToString());
                    //}
                    #endregion
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("侦听异常：" + e.Message);
                tcpListener.Stop();
            }
            finally
            {
                tcpListener.Stop();
            }

        }


    }
}
