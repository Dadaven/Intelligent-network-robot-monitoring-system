using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.DirectX.DirectInput;
using System.Threading;
using System.IO.Ports;
using PreviewDemo;
 
namespace 智能网络机器人控制系统1._0
{
    public partial class MainForm : Form
    {
        //摄像头
        private uint iLastErr = 0;
        private Int32 m_lUserID = -1;
        private bool m_bInitSDK = false;
        private bool m_bRecord = false;
        private Int32 m_lRealHandle = -1;
        private string str;


        //机器人
        private bool checkFlag = false;
        private System.IO.Ports.SerialPort moveSerialPort = null;
        private JoystickState state = new JoystickState();
        private Device applicationDevice = null;
        Thread JoysticktThread = null;
        private static bool JoysticWorkSwitch = false;


        SocketServer.SocketHost socketHost = new SocketServer.SocketHost { Port = 1234 };
         

         protected override void Dispose(bool disposing)
         {
             //关闭摄像头
             if (m_lRealHandle >= 0)
             {
                 CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle);
             }
             if (m_lUserID >= 0)
             {
                 CHCNetSDK.NET_DVR_Logout(m_lUserID);
             }
             if (m_bInitSDK == true)
             {
                 CHCNetSDK.NET_DVR_Cleanup();
             }
             
             
             //关闭遥控器;
             JoystickStop();
             //关闭机器人串口
             if (moveSerialPort !=null && moveSerialPort.IsOpen)
             {
                 moveSerialPort.Close();
             }
             //关闭日志文件
             FileUtil.close();

             if (disposing && (components != null))
             {
                 components.Dispose();
             }
             base.Dispose(disposing);
             
             //退出全部线程
             System.Environment.Exit(0);

         }

        public MainForm()
        {
            InitializeComponent();

            m_bInitSDK = CHCNetSDK.NET_DVR_Init();
            if (m_bInitSDK == false)
            {
                MessageBox.Show("NET_DVR_Init error!");
                return;
            }
            else
            {
                //保存SDK日志 To save the SDK log
                CHCNetSDK.NET_DVR_SetLogToFile(3, "C:\\SdkLog\\", true);
            }

            //启动服务器
            socketHost.StartNow();
        }

       

        /// <summary>
        /// 委托设置 串口接受的的消息提示
        /// </summary>
        /// <param name="message"></param>
        private delegate void setLblReceiveMessageDelegate( );
        private void setLblReceiveMessage( )
        {
            if (this.lblLatitude.InvokeRequired && this.lblLongitude.InvokeRequired&&this.lblSpeed.InvokeRequired 
                &&this.lblTemperature.InvokeRequired&&this.lblHumidity.InvokeRequired&&lblHarmfulGas.InvokeRequired&&lblVoltage.InvokeRequired  )//判断是否是自己的线程,是否必须调用invoke方法
            {
                setLblReceiveMessageDelegate d = new setLblReceiveMessageDelegate(setLblReceiveMessage);
                lblLatitude.Invoke(d, new object[] {   });
                lblLongitude.Invoke(d, new object[] { });
                lblSpeed.Invoke(d, new object[] { });
                lblTemperature.Invoke(d, new object[] { });
                lblHumidity.Invoke(d, new object[] { });
                lblHarmfulGas.Invoke(d, new object[] { });
                lblVoltage.Invoke(d, new object[] { });
            }
            else
            {
                lblLatitude.Text = StringUtil.latitude.ToString();
                lblLongitude.Text = StringUtil.longitude.ToString();
                lblSpeed.Text = StringUtil.speed.ToString();
                if (StringUtil.temperature > 50) {
                    lblTemperature.BackColor = Color.Red;
                }
                lblTemperature.Text =  StringUtil.temperature.ToString()   ;
                lblHumidity.Text  = StringUtil.humidity.ToString() ;
                if (StringUtil.harmfulGas > 15)
                {
                    lblHarmfulGas.BackColor = Color.Red;
                }
                lblHarmfulGas.Text  = StringUtil.harmfulGas.ToString() ;
                if (StringUtil.voltage <50)
                {
                    lblVoltage.BackColor = Color.Red;
                }
                lblVoltage.Text = StringUtil.voltage.ToString() ;
            }
        }
       
        /// <summary>
        /// 委托设置 手柄提示信息
        /// </summary>
        /// <param name="message"></param>
        private delegate void setLblJoystickStateDelegate(string message);
        private void setLblJoystickStateMessage(string message)
        {
            if (lblJoystickState.InvokeRequired)//判断是否是自己的线程,是否必须调用invoke方法
            {
                setLblJoystickStateDelegate d = new setLblJoystickStateDelegate(setLblJoystickStateMessage);
                lblJoystickState.Invoke(d, new object[] { message });
            }
            else
            {
                lblJoystickState.Text =   message;
            }
        }
        
        /// <summary>
        /// 委托设置 机器人提示信息
        /// </summary>
        /// <param name="message"></param>
        private delegate void setLblRobotStateDelegate(string message);
        private void setLblRobotStateMessage(string message)
        {
            if (lblRobotState.InvokeRequired)//判断是否是自己的线程,是否必须调用invoke方法
            {
                setLblRobotStateDelegate d = new setLblRobotStateDelegate(setLblRobotStateMessage);
                lblRobotState.Invoke(d, new object[] { message });
            }
            else
            {

                lblRobotState.Text = message;
            }
        }

        /// <summary>
        /// 委托设置 按钮是否可用
        /// </summary>
        /// <param name="message"></param>
        private delegate void setButtonMessageDelegate(bool portFlag, bool contFlag);
        private void setButtonMessage(bool portFlag, bool contFlag)
        {
            if (this.btPort.InvokeRequired && this.btCont.InvokeRequired)//判断是否是自己的线程,是否必须调用invoke方法
            {
                setButtonMessageDelegate d = new setButtonMessageDelegate(setButtonMessage);
                lblRobotState.Invoke(d, new object[] { portFlag, contFlag });
            }
            else
            {
                btCont.Enabled = contFlag;
                btPort.Enabled = portFlag;
            }
        }


        private void btPort_Click(object sender, EventArgs e)
        {
            int baudRate = int.Parse(cbRobotBaudRate.Text);
            try
            {
                moveSerialPort = SerialPortUtil.open(moveSerialPort, cbRobotPortName.Text, baudRate);
                if (moveSerialPort.IsOpen)
                {
                    //moveSerialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(serialPortTemperature_DataReceived);
                    setLblRobotStateMessage("机器人连接成功");
                    btPort.Enabled = false;
                    checkFlag = true;
                }
            }
            catch (Exception e1)
            {
                lblRobotState.Text = "串口无法打开，请确认您的参数";
                FileUtil.write(e1.ToString());
            }
        }

        private void btCont_Click(object sender, EventArgs e)
        {
            if (!checkFlag)
            {
                MessageBox.Show("请先连接机器人");
                return;
                //checkFlag = true;
            }
            applicationDevice = JoystickUtil.init(this, applicationDevice);

            if (applicationDevice != null)
            {

                JoystickStart();//启动手柄获取线程

            }
            else
            {
                setLblJoystickStateMessage("手柄连接失败,没有可用的手柄");
            }
        }

        //手柄开启
        private void JoystickStart()
        {

            JoysticWorkSwitch = true;
            JoysticktThread = new Thread(JoystickWork);
            JoysticktThread.Start();
            setLblJoystickStateMessage("手柄连接成功");
            btCont.Enabled = false;
        }
        // //手柄关闭
        private void JoystickStop()
        {
            JoysticWorkSwitch = false;
        }
        //获取手柄状态的线程体
        public void JoystickWork()
        {
            // Make sure there is a valid device.
            if (null == applicationDevice)
            {
                setLblJoystickStateMessage("没有手柄，请插入后重启程序");

                //btCont.Enabled = true;
                setButtonMessage(false, true);

                return;
            }
            try
            {
                // Poll the device for info.
                applicationDevice.Poll();
            }
            catch (InputException inputex)
            {
                if ((inputex is NotAcquiredException) || (inputex is InputLostException))
                {
                    try
                    {
                        applicationDevice.Acquire();
                    }
                    catch (InputException e)
                    {

                        setLblJoystickStateMessage("无法获取手柄状态："+e.ToString());
                        //btCont.Enabled = true;
                        setButtonMessage(false, true);
                        FileUtil.write(e.ToString());
                        return;
                    }
                }

            } //catch(InputException inputex)
            int ReconnectCount = 1;
            //需要先保障连接到机器人串口 checkFlag
            int ReadConut =  1 ;
            //JoysticWorkSwitch  控制线程的开关
            while (checkFlag && JoysticWorkSwitch)
            {
            //tipMessage("" + isActivite);
            // Get the state of the device.
                ReconnectCount = 0;
            tagGetState:
                try
                {
                    state = applicationDevice.CurrentJoystickState;
                }
                catch (InputException e)
                {// Catch any exceptions. None will be handled here, 
                    // any device re-aquisition will be handled above.  
                    ReconnectCount++;
                    setLblJoystickStateMessage("手柄状态获取失败,休眠0.1秒,尝试 " + ReconnectCount + " 次重连接");
                    if (ReconnectCount > 5)
                    {
                        setLblJoystickStateMessage("手柄状态获取失败,0.5秒没有响应，请重连");
                        //btCont.Enabled = true;
                        setButtonMessage(false, true);
                        JoysticWorkSwitch = false;
                        FileUtil.write("手柄状态获取失败,休眠0.1秒,尝试 " + ReconnectCount + " 次重连接");
                        FileUtil.write(e.ToString());
                        return;
                    }
                    Thread.Sleep(100);
                    goto tagGetState;
                    //buttonMessage(false, true);
                    //return;
                }

                /////发送消息
                int x = Int16.Parse(state.X.ToString());
                x = 300 - x;
                int z = Int16.Parse(state.Z.ToString());

                //对关闭机器人的临界值设置阈值
                if (x >= 148 && x <= 152 && z >= 148 && z <= 152)
                {
                    z = 150;
                    x = 150;
                }
                setLblJoystickStateMessage(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") + ", x = " + x + " z= " + z);

                //拼装消息
                byte[] data = SendMsg.getData((byte)z, (byte)x);
                //发送消息
                try
                {
                    moveSerialPort.Write(data, 0, data.Length);
                    FileUtil.write(data, WriteModel.send);
                    Thread.Sleep(110);//等待机器人工作完毕，返回数据
                }
                catch (Exception e1)
                {
                   // lblRobotState.Text = "串口 发送 信息失败";
                    setLblRobotStateMessage("串口 发送 信息失败:"+e1.ToString());
                    FileUtil.write(e1.ToString());
                    moveSerialPort = null;
                    setButtonMessage(true, true);
                    break ;
                }

                //接受消息
                if (moveSerialPort != null && moveSerialPort.IsOpen)
                {
                    int BytesToReadCount = moveSerialPort.BytesToRead;
                    if (BytesToReadCount == 28)
                    {
                        ReadConut = 0;
                        byte[] readBuffer = new byte[BytesToReadCount];
                        try
                        {
                            /*byte[] readBuffer = new byte[28];
                            int readedCount = moveSerialPort.Read(readBuffer, 0, 28);
                              for (int i = 0; i < 28; i++)
                              {
                                  readBuffer[i] = (byte)moveSerialPort.ReadByte();
                              }
                           */
                            moveSerialPort.Read(readBuffer, 0, BytesToReadCount);//从串口读取消息
                            //将收到的消息存入文件
                           /* StringBuilder str = new StringBuilder();
                            for (int i = 0; i < readBuffer.Length; i++)
                            {
                                str.Append(" 0x" + Convert.ToString(readBuffer[i], 16));
                            }
                            */
                            FileUtil.write(readBuffer, WriteModel.receive);
                            //解析消息
                            String message = "解析消息出错";
                            message = StringUtil.decodeMessage(readBuffer);//解析消息
                            setLblJoystickStateMessage(message);
                            //在界面显示数据，用委托方法
                            setLblReceiveMessage();

                            //如果解析成功，想app发送接收到的消息
                            if (StringUtil.isOk)
                            {
                                StringBuilder stringBuilder = new StringBuilder();
                                stringBuilder.Append(StringUtil.latitude.ToString());
                                stringBuilder.Append("#");
                                stringBuilder.Append(StringUtil.longitude.ToString());
                                stringBuilder.Append("#");
                                stringBuilder.Append(StringUtil.temperature.ToString());
                                stringBuilder.Append("#");
                                stringBuilder.Append(StringUtil.humidity.ToString());
                                stringBuilder.Append("#");
                                stringBuilder.Append(StringUtil.harmfulGas.ToString());
                                stringBuilder.Append("#");
                                stringBuilder.Append(StringUtil.speed.ToString());
                                stringBuilder.Append("#");
                                stringBuilder.Append(StringUtil.voltage.ToString());
                                //发送
                                socketHost.sendmessage(stringBuilder.ToString());
                            }
  
                        }
                        catch (TimeoutException e)
                        {
                            setLblRobotStateMessage("串口 接收 信息失败" + e.ToString());
                            FileUtil.write(e.ToString());
                            moveSerialPort = null;
                            setButtonMessage(true, true);
                            break;
                        }
                    }
                    else
                    {
                        if (ReadConut > 6)
                        {
                            setLblRobotStateMessage("串口 接收 信息失败,收到的字节数：" + BytesToReadCount);
                            FileUtil.write("串口 接收 信息失败,收到的字节数：" + BytesToReadCount);
                            moveSerialPort = null;
                            setButtonMessage(true, true);
                            break;
                        }
                        else
                        {
                            ReadConut++;
                            byte[] readBuffer1 = new byte[BytesToReadCount];
                            try
                            {
                                /*byte[] readBuffer = new byte[28];
                                int readedCount = moveSerialPort.Read(readBuffer, 0, 28);
                                  for (int i = 0; i < 28; i++)
                                  {
                                      readBuffer[i] = (byte)moveSerialPort.ReadByte();
                                  }
                               */
                                moveSerialPort.Read(readBuffer1, 0, BytesToReadCount);

                            }
                            catch (Exception e)
                            {
                                //将收到的消息存入文件
                                StringBuilder str = new StringBuilder();
                                for (int i = 0; i < readBuffer1.Length; i++)
                                {
                                    str.Append(" 0x" + Convert.ToString(readBuffer1[i], 16));
                                }
                                FileUtil.write(readBuffer1, WriteModel.receive);
                            }
                        }
                    }

                }//接受消息

            }//end while
        } //end JoystickWork(  )
   
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

          
            // Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cbRobotPortName.SelectedIndex = 0;
            this.cbRobotBaudRate.SelectedIndex = 5;


        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //退出全部线程
            System.Environment.Exit(0);
        }

        private void btnLogin_Click(object sender, System.EventArgs e)
        {
            if (textBoxIP.Text == "" || textBoxPort.Text == "" ||
                textBoxUserName.Text == "" || textBoxPassword.Text == "")
            {
                MessageBox.Show("请输入正确的IP地址，端口号，用户名和密码。");
                return;
            }
            if (m_lUserID < 0)
            {
                string DVRIPAddress = textBoxIP.Text; //设备IP地址或者域名
                Int16 DVRPortNumber = Int16.Parse(textBoxPort.Text);//设备服务端口号
                string DVRUserName = textBoxUserName.Text;//设备登录用户名
                string DVRPassword = textBoxPassword.Text;//设备登录密码

                CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();

                //登录设备 Login the device
                m_lUserID = CHCNetSDK.NET_DVR_Login_V30(DVRIPAddress, DVRPortNumber, DVRUserName, DVRPassword, ref DeviceInfo);
                if (m_lUserID < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_Login_V30 failed, error code= " + iLastErr; //登录失败，输出错误号
                    MessageBox.Show(str);
                     
                    return;
                }
                else
                {
                    //登录成功
                    MessageBox.Show("登录成功!");
                    btnLogin.Text = "注销";
                }

            }
            else
            {
                //注销登录 Logout the device
                if (m_lRealHandle >= 0)
                {
                    MessageBox.Show("Please stop live view firstly");
                    return;
                }

                if (!CHCNetSDK.NET_DVR_Logout(m_lUserID))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_Logout failed, error code= " + iLastErr;
                    MessageBox.Show(str);
                    return;
                }
                m_lUserID = -1;
                btnLogin.Text = "登录";
            }
            return;
        }

        private void btnPreview_Click(object sender, System.EventArgs e)
        {
            if (m_lUserID < 0)
            {
                MessageBox.Show("Please login the device firstly");
                return;
            }

            if (m_lRealHandle < 0)
            {
                CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
                lpPreviewInfo.hPlayWnd = RealPlayWnd.Handle;//预览窗口
                lpPreviewInfo.lChannel = Int16.Parse(textBoxChannel.Text);//预te览的设备通道
                lpPreviewInfo.dwStreamType = 0;//码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
                lpPreviewInfo.dwLinkMode = 0;//连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP 
                lpPreviewInfo.bBlocked = true; //0- 非阻塞取流，1- 阻塞取流
                lpPreviewInfo.dwDisplayBufNum = 15; //播放库播放缓冲区最大缓冲帧数

                CHCNetSDK.REALDATACALLBACK RealData = new CHCNetSDK.REALDATACALLBACK(RealDataCallBack);//预览实时流回调函数
                IntPtr pUser = new IntPtr();//用户数据

                //打开预览 Start live view 
                m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, null/*RealData*/, pUser);
                if (m_lRealHandle < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_RealPlay_V40 failed, error code= " + iLastErr; //预览失败，输出错误号
                    MessageBox.Show(str);
                    return;
                }
                else
                {
                    //预览成功
                    btnPreview.Text = "停止预览";
                }
            }
            else
            {
                //停止预览 Stop live view 
                if (!CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_StopRealPlay failed, error code= " + iLastErr;
                    MessageBox.Show(str);
                    return;
                }
                m_lRealHandle = -1;
                btnPreview.Text = "开始预览";

            }
            return;
        }

        public void RealDataCallBack(Int32 lRealHandle, UInt32 dwDataType, ref byte pBuffer, UInt32 dwBufSize, IntPtr pUser)
        {
        }

        private void btnBMP_Click(object sender, EventArgs e)
        {
            string sBmpPicFileName;
            //图片保存路径和文件名 the path and file name to save
            sBmpPicFileName = "BMP_test.bmp";

            //BMP抓图 Capture a BMP picture
            if (!CHCNetSDK.NET_DVR_CapturePicture(m_lRealHandle, sBmpPicFileName))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_CapturePicture failed, error code= " + iLastErr;
                MessageBox.Show(str);
                return;
            }
            else
            {
                str = "Successful to capture the BMP file and the saved file is " + sBmpPicFileName;
                MessageBox.Show(str);
            }
            return;
        }

        private void btnJPEG_Click(object sender, EventArgs e)
        {
            string sJpegPicFileName;
            //图片保存路径和文件名 the path and file name to save
            sJpegPicFileName = DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss-fff") +".jpg";
            //sJpegPicFileName = "t.jpg";
            int lChannel = Int16.Parse(textBoxChannel.Text); //通道号 Channel number

            CHCNetSDK.NET_DVR_JPEGPARA lpJpegPara = new CHCNetSDK.NET_DVR_JPEGPARA();
            lpJpegPara.wPicQuality = 0; //图像质量 Image quality
            lpJpegPara.wPicSize = 0xff; //抓图分辨率 Picture size: 2- 4CIF，0xff- Auto(使用当前码流分辨率)，抓图分辨率需要设备支持，更多取值请参考SDK文档

            //JPEG抓图 Capture a JPEG picture
            if (!CHCNetSDK.NET_DVR_CaptureJPEGPicture(m_lUserID, lChannel, ref lpJpegPara, sJpegPicFileName))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_CaptureJPEGPicture failed, error code= " + iLastErr;
                MessageBox.Show(str);
                return;
            }
            else
            {
                str = "Successful to capture the JPEG file and the saved file is " + sJpegPicFileName;
                MessageBox.Show(str);
            }
            return;
        }

        private void btnRecord_Click(object sender, EventArgs e)
        {
            //录像保存路径和文件名 the path and file name to save
            string sVideoFileName;
            sVideoFileName = DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss-fff") + ".mp4";

            if (m_bRecord == false)
            {
                //强制I帧 Make a I frame
                int lChannel = Int16.Parse(textBoxChannel.Text); //通道号 Channel number
                CHCNetSDK.NET_DVR_MakeKeyFrame(m_lUserID, lChannel);

                //开始录像 Start recording
                if (!CHCNetSDK.NET_DVR_SaveRealData(m_lRealHandle, sVideoFileName))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_SaveRealData failed, error code= " + iLastErr;
                    MessageBox.Show(str);
                    return;
                }
                else
                {
                    btnRecord.Text = "停止录像";
                    m_bRecord = true;
                }
            }
            else
            {
                //停止录像 Stop recording
                if (!CHCNetSDK.NET_DVR_StopSaveRealData(m_lRealHandle))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_StopSaveRealData failed, error code= " + iLastErr;
                    MessageBox.Show(str);
                    return;
                }
                else
                {
                    str = "Successful to stop recording and the saved file is " + sVideoFileName;
                    MessageBox.Show(str);
                    btnRecord.Text = "开始录像";
                    m_bRecord = false;
                }
            }

            return;
        }

        private void btn_Exit_Click(object sender, EventArgs e)
        {
            //停止预览 Stop live view 
            if (m_lRealHandle >= 0)
            {
                CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle);
                m_lRealHandle = -1;
            }

            //注销登录 Logout the device
            if (m_lUserID >= 0)
            {
                CHCNetSDK.NET_DVR_Logout(m_lUserID);
                m_lUserID = -1;
            }

            CHCNetSDK.NET_DVR_Cleanup();

            Application.Exit();
        }

        private void btnPTZ_Click(object sender, EventArgs e)
        {
            PTZControl dlg = new PTZControl();
            dlg.m_lUserID = m_lUserID;
            dlg.m_lChannel = 1;
            dlg.m_lRealHandle = m_lRealHandle;
            dlg.ShowDialog();
        }

        private void RealPlayWnd_DoubleClick(object sender, EventArgs e)
        {

            if (RealPlayWnd.Dock == System.Windows.Forms.DockStyle.None)
            {
                RealPlayWnd.Dock = System.Windows.Forms.DockStyle.Fill;
            }
            else {

                RealPlayWnd.Dock = System.Windows.Forms.DockStyle.None;
            }
           
        }
    }
}
