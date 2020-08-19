using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
namespace 智能网络机器人控制系统1._0
{
    class SerialPortUtil
    {
        /// <summary>
        /// 打开串口 
        /// </summary>
        /// <param name="portName"></param>
        /// <returns></returns>
        public static System.IO.Ports.SerialPort open(System.IO.Ports.SerialPort moveSerialPort, string portName, int baudRate)
        {

            if (moveSerialPort == null || !moveSerialPort.IsOpen)
            {
               
                moveSerialPort = new System.IO.Ports.SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
                try
                {
                    moveSerialPort.Open();
                    moveSerialPort.ReadTimeout = 50;
                    moveSerialPort.WriteTimeout = 50;
                }
                catch (Exception e1)
                {
                    throw new Exception("无法连接机器人" + e1.Message);
                }
            }
            return moveSerialPort;
        }
    }
}
