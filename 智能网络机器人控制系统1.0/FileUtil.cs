using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace 智能网络机器人控制系统1._0
{
    public enum WriteModel
    {
        send = 1,
        receive = 2
    }

    class FileUtil
    {
        static StreamWriter swInfo = File.AppendText(Application.StartupPath + "\\Exception.txt");
        static StreamWriter swS = File.AppendText(Application.StartupPath + "\\send.txt");
        static StreamWriter swR = new StreamWriter(Application.StartupPath + "\\recive.txt", true);

        public static void write(string message)
        {
            swInfo.WriteLine(DateTime.Now.ToString("yyyyMMdd hh:mm:ss fff") + "  Exception: " + message);
            //清空缓冲区
            swInfo.Flush();
        }

        public static void write(string message, WriteModel model)
        {

            if (message == null || message.Length <= 0)
            {
                return;
            }
            if (model.Equals(WriteModel.send))
            {
                //开始写入
                swS.WriteLine("data:" + message + "    time:" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff"));
                //清空缓冲区
                swS.Flush();
            }
            else
            {
                //开始写入
                swR.WriteLine("data:" + message + "   time:" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff"));
                //清空缓冲区
                swR.Flush();
            }
        }

        public static void write(byte[] message, WriteModel model)
        {
        
            if (message == null || message.Length <= 0)
            {
                return;
            }
            StringBuilder str = new StringBuilder();
            for (int i = 0; i < message.Length; i++)
            {
                str.Append(" 0x" + Convert.ToString(message[i], 16));
            }

            if (model.Equals(WriteModel.send))
            {
                //4.0协议
                int serial = 0;
                if (message.Length > 13)
                {
                    serial = StringUtil.byte2Int(new byte[] { message[10], message[11], message[12], message[13] });
                }
                //开始写入
                swS.WriteLine("serial:" + serial + "  data:" + str.ToString() + "   time:" + DateTime.Now.ToString("yyyyMMddhhmmssfff"));
                //清空缓冲区
                swS.Flush();
            }
            else
            {
                //4.0协议
                int serial = 0;
                if (message.Length > 10)
                {
                    serial = StringUtil.byte2Int(new byte[] { message[7], message[8], message[9], message[10] });
                }

                //开始写入
                swR.WriteLine("serial:" + serial + "  data:" + str.ToString() + "   time:" + DateTime.Now.ToString("yyyyMMddhhmmssfff"));
                //清空缓冲区
                swR.Flush();
            }
            
        }

        public static void close()
        {
            try
            {
                swInfo.Close();
                swR.Close();
                swS.Close();
            }catch( Exception ){
            
            }
        }


    }
}
