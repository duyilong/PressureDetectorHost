using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpServer
{
    /// <summary>
    /// 分割收到的tcp字符串，
    /// 构造相应的SQL字符串
    /// </summary>
    class StrSplit
    {
        #region 报警处理部分
        /// <summary>
        /// 判断是否有报警数据,
        /// 返回报警SQL命令“合集”，命令间以%分割
        /// </summary>
        /// <param name="tcpstrRecv">从TCP接收到的数据字符串</param>
        /// <returns></returns>
        public string MyAlert(string tcpstrRecv)
        {
            string strAlert = null;//初始化插入语句
            //int alarmCount=0;
            if ((tcpstrRecv.IndexOf("~01") > -1) && (tcpstrRecv.IndexOf("\r\n") > -1))
            {
                //先按照%作为分隔符，将命令分割开
                string[] tempStr = tcpstrRecv.Split('%');
                foreach (string splited in tempStr)
                {
                    if ((splited.IndexOf(":") > -1) && (splited.IndexOf("T") == -1))
                    {//有冒号，且没有‘T’，表示压强数据
                        string[] pt = splited.Split(':');
                        if ((float.Parse(pt[1]) >= 17.2) || (float.Parse(pt[1]) <= 13.6))
                        {//不在正常范围的时候需要报警
                            strAlert = strAlert + "insert into alert(UnitNumber,PressureNumber,PressurePro)values('"
                                       + tempStr[1] + "'," + pt[0] + "," + pt[1] + ")%";
                        }
                    }
                }
            }

            return strAlert;
        }
        #endregion

        /// <summary>
        /// 构造插入数据到单元气压表的SQL语句
        /// </summary>
        /// <param name="tcpstrRecv">从TCP接收到的数据字符串</param>
        /// <returns></returns>
        public string MySplit(string tcpstrRecv)
        {
            //分割字符串，并且返回构造的insertStr
            if ((tcpstrRecv.IndexOf("~01") > -1) && (tcpstrRecv.IndexOf("\r\n") > -1))//判断接收的信息是否完整 
            {
                //输入样例： ~01%000001%1:16.33%2:16.60%3:8.05%4:10.13%T:17.76%
                #region split strings
                string insertStr = "insert into ";
                string[] tempStr = tcpstrRecv.Split('%');//以%为分隔符将字符串分割，返回一个字符串数组
                if (tempStr[1].Length != 6)//如果单元编号不对
                {
                    return null;
                }
                insertStr = insertStr + "unitdata_" + tempStr[1] + "(Time,UnitNumber,";
                foreach (string splited in tempStr)//构造插入命令前半部分
                {
                    if (splited.IndexOf(":") > -1)//带冒号的可能是压强或者温度数据
                    {
                        string[] pt = splited.Split(':');
                        if (pt[0].IndexOf("T") > -1)
                            insertStr = insertStr + "Temperature)values(now(),";
                        else
                            insertStr = insertStr + "Pressure_" + pt[0] + ",";
                    }
                }
                insertStr = insertStr + "'" + tempStr[1] + "',";
                foreach (string splited in tempStr)//构造插入命令后半部分
                {
                    if (splited.IndexOf(":") > -1)
                    {
                        string[] pt = splited.Split(':');
                        if (pt[0].IndexOf("T") > -1)
                            insertStr = insertStr + pt[1] + ")";
                        else
                            insertStr = insertStr + pt[1] + ",";
                    }
                }
                //Console.WriteLine("将要插入的SQL命令是：" + insertStr);
                return insertStr;

                #endregion
            }
            else
            {
                //Console.WriteLine("Wrong recvStr!");
                return null;
            }
        }

        /// <summary>
        /// 构造同步时间命令，字符串形式
        /// </summary>
        /// <returns>时间同步命令</returns>
        static public string CurrentTimeStr()
        {
            string[] weekDays = { "7", "1", "2", "3", "4", "5", "6" };
            string week = weekDays[Convert.ToInt32(DateTime.Now.DayOfWeek)];
            string timeSync = "~10%TIME%"
                              + DateTime.Now.ToString("yyyy-MM-dd")//2008-01-01
                              + "-"
                              + week                               //星期:1/2/3/4/5/6/7
                              + "-"
                              + DateTime.Now.ToString("HH-mm-ss") // 14-03-25
                              + "\r\n";
            return timeSync;
        }
    }
}
