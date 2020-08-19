using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace 智能网络机器人控制系统1._0
{
    class SendMsg
    {
        private static int serialNumber = 1;
        public static byte[] getData(byte speed, byte direction)
        {
            /*
           0xFF	
           0	0x7E	
           1	0x07	信息包长度（下一字节到校验和前一字节长度）
           2	0x01	命令字，下发行指令
           3	0x02	主端地址02
           4	0x01	从短地址
           5	0xXX	速度命令（100-150-200）；100-前进；150-停止；200-后退
           6	0xXX	转向命令（100-150-200）；100-左转；150-执行；200-右转
           7	0xXX	
           8	0xXX
           9~12 int    
           13	0xXX	校验和（1-8字节之和）
            * */

            byte[] data = new byte[16];
            data[0] = 0xff;
            data[1] = 0x7e;
            data[2] = 0x0b;//长度
            data[3] = 0x01;//命令字
            data[4] = 0x02;
            data[5] = 0x01;
            data[6] = speed;
            data[7] = direction;
            data[8] = 0x00;
            data[9] = 0x00;
            byte[] arr = StringUtil.int2Byte((serialNumber++));
            data[10] = arr[0];
            data[11] = arr[1];
            data[12] = arr[2];
            data[13] = arr[3];
            byte t = data[2];
            for (int i = 3; i < 15; i++)
            {
                t += data[i];
            }
            data[14] = t;
            data[15] = 0x0d;
            return data;
        }
    }
}
