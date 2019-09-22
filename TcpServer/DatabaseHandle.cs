using System;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Xml;
namespace TcpServer
{
    /// <summary>
    /// 处理数据库的连接和增删改查等操作的类
    /// </summary>
    class DatabaseHandle
    {
        //public MySqlConnection mySqlCon;
        //public MySqlCommand mySqlCmd;

        /// <summary>
        /// 执行将数据插入到表格的操作
        /// </summary>
        /// <param name="con">连接名</param>
        /// <param name="cmd">SQL命令</param>
        static public void MyInsert(string SQListStr)
        {

            //读取xml文件，获得实例化操作数据库的对象的一些信息
            XmlDocument doc = new XmlDocument();
            doc.Load("MySQL.xml");
            //读取服务器地址
            XmlNodeList Serverlis = doc.GetElementsByTagName("server");
            string strServerName = Serverlis[0].InnerText;
            //读取id
            XmlNodeList Idlis = doc.GetElementsByTagName("id");
            string strId = Idlis[0].InnerText;
            //读取密码
            XmlNodeList Paslis = doc.GetElementsByTagName("password");
            string strPas = Paslis[0].InnerText;
            //读取数据库名称
            XmlNodeList Databaselis = doc.GetElementsByTagName("database");
            string strDatabase = Databaselis[0].InnerText;
            //构造连接数据库字符串
            string constr = "server=" + strServerName
                           + "; user id=" + strId
                           + ";password=" + strPas
                           + ";database=" + strDatabase + " ";
            //string constr = "server=127.0.0.1;User Id=root;password=123456;Database=pressure_detect";

            MySqlConnection con = new MySqlConnection(constr);
            MySqlCommand cmd = new MySqlCommand(SQListStr, con);
            try
            {
                if (con.State == System.Data.ConnectionState.Closed)
                    con.Open();
                if (cmd.ExecuteNonQuery() > 0)
                {
                    Console.WriteLine("数据插入成功!");
                }
            }
            catch (MySqlException myex)
            {
                Console.WriteLine("数据库写入异常：" + myex.Message);
                cmd.Dispose();
                con.Close();
            }
            finally
            {
                cmd.Dispose();
                con.Close();
            }
        }
    }
}
