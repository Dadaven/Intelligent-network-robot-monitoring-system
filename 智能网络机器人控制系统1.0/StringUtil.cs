using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace 智能网络机器人控制系统1._0
{
    class StringUtil
    {
        public static bool isOk = false;
     // public  TimeSpan ts = DateTime.Now - startTime;
        public static DateTime startTime = DateTime.Now;//开始
     // public TimeSpan ts = DateTime.Now -  DateTime.Now  ;
      public  static    int receivedSerialNumber = 0;
      public static byte BodyLengthShort = 24;//消息体长度
      public static byte state = 0;  //1表示成功
      
      public static float longitude= 0;  //经度Longitude 
      public static float latitude = 0;  //纬度
      //public static short elevation= 0;  //海拔
      public static float voltage = 0; //电压
      public static float speed = 0; //速度
      public static byte warningMark = 0; //报警标志位
      public static byte temperature = 0; //温度
      public static byte humidity = 0; //湿度
      public static byte harmfulGas = 0; //光强
      public static byte checkNumber = 0; 

        public static int byte2Int32(byte[] arr, int start)
        {
            //还未校验arr是否合法
            return (int)(arr[start + 3] | arr[start + 2] << 8 | arr[start + 1] << 16 | arr[start] << 24);
        }
        public static short byte2Int16(byte[] arr, int start)
        {
            //还未校验arr是否合法
            return (short)(arr[start + 1] | arr[start] << 8);

        }

        public static byte[] int2Byte(int source)
        {
            byte[] arr = new byte[4];
            arr[3] = (byte)(source);
            arr[2] = (byte)(source >> 8);
            arr[1] = (byte)(source >> 16);
            arr[0] = (byte)(source >> 24);
            return arr;
        }

        public static int byte2Int(byte[] arr)
        {
            //还未校验arr是否合法
            return (int)(arr[3] | arr[2] << 8 | arr[1] << 16 | arr[0] << 24);
        }


        /// <summary>
        /// 4.0协议
        /// </summary>
        /// <returns></returns>
        public static string byte2String(byte[] arr)
        {
            //还未校验arr

            StringBuilder str = new StringBuilder();
            for (int i = 0; i <= 9; i++)
            {
                str.Append(arr[i] + " ");
            }
            int serial = StringUtil.byte2Int(new byte[] { arr[10], arr[11], arr[12], arr[13] });
            str.Append(serial + " ");
            str.Append(arr[14] + " ");
            str.Append(arr[15] + " ");
            return str.ToString();
        }
        public static string decodeMessage(byte[] buff)
        {
            // byte[] buff =  new byte[]{0xff, 0x7e,  0x18 , 0x1 , 0x2 , 0x1,  0x1,  0x0,  0x0 , 0x0,  0x1,  0x2 , 0x60 , 0xe9 , 0x14,  0x6 , 0xef,  0xde , 0x65 , 0x0,  0x5,  0x0 , 0x78,  0x0,  0x20 , 0x25 , 0x50,  0xaf };
            String message = "解析消息出差，消息不完整";
            if (buff != null  && buff.Length != 28)
            {
                 isOk = false;
                 return message;
            }

            //计算校验和
            int j = 0;
            byte check = 0;
            for (j = 3; j < 27; j++)
            {
                check += buff[j];
            }
            if (check != buff[27])
            {
                isOk = false;
                message = "解析消息出错，校验和错误";
                return message ;
            }

            latitude =  (float)byte2Int32(buff, 11)/100000;
            longitude = (float)byte2Int32(buff, 15)/100000;
            speed = (float)byte2Int16(buff, 19)/10;
            voltage = (float)byte2Int16(buff, 21)/10;
            warningMark = buff[23];//报警标志位
            temperature = buff[24]; //温度
            humidity = buff[25]; //湿度
            harmfulGas = buff[26]; //光强
            checkNumber = buff[27];
            isOk = true;
            startTime = DateTime.Now;
/*
            message = "经度： " + latitude + "\n纬度： " + longitude;
            message += "\n速速： " + speed + "\n电压： " + voltage;
            message += "\n温度： " + temperature + "\n湿度： " + humidity;
            message += "\n气体：" + harmfulGas ;
    
 * *///  Console.ReadKey();
            
            /*TimeSpan ts = DateTime.Now - startTime;
             if (ts.Milliseconds > 1000)//超时了
             { 
             }
             * */
            message = "解析消息正确";
            return message;
        }
   


    }
}
