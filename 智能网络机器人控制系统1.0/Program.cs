using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace 智能网络机器人控制系统1._0
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
           // Application.Run(new MainForm());
               Application.Run(new Login());
        }
    }
}
