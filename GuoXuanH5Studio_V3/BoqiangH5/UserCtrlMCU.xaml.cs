using BoqiangH5.BQProtocol;
using BoqiangH5Entity;
using BoqiangH5Repository;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace BoqiangH5
{
    /// <summary>
    /// UserCtrlMCU.xaml 的交互逻辑
    /// </summary>
    public partial class UserCtrlMCU : UserControl
    {
        List<H5BmsInfo> ListSysInfo1 = new List<H5BmsInfo>();
        List<H5BmsInfo> ListSysInfo2 = new List<H5BmsInfo>();
        List<H5BmsInfo> ListChargeInfo = new List<H5BmsInfo>();


        public UserCtrlMCU()
        {
            InitializeComponent();

            InitMcuWnd();
        }

        private void InitMcuWnd()
        {
            string strConfigFile = XmlHelper.m_strBqProtocolFile;

            XmlHelper.LoadXmlConfig(strConfigFile, "mcu_info/system1/mcu_node_info", ListSysInfo1);
            XmlHelper.LoadXmlConfig(strConfigFile, "mcu_info/system2/mcu_node_info", ListSysInfo2);
            XmlHelper.LoadXmlConfig(strConfigFile, "mcu_info/charge_discharge/mcu_node_info", ListChargeInfo);

            dgMcuSys1.ItemsSource = ListSysInfo1;
            dgMcuSys2.ItemsSource = ListSysInfo2;
            dgMcuCharge.ItemsSource = ListChargeInfo;

            UpdateMcuWnd();
        }
        public bool isSetPassword = false;
        public void UpdateMcuWnd()
        {
            if(MainWindow.m_statusBarInfo.IsOnline)
            {
                if(isSetPassword)
                {
                    if(SelectCANWnd.m_IsClosePwd)
                    {
                        btnReadMcuData.IsEnabled = true;
                        btnSaveMcuData.IsEnabled = true;
                        btnLoadMcuPara.IsEnabled = true;
                    }
                    else
                    {
                        PasswordWnd wnd = new PasswordWnd("666");
                        wnd.ShowDialog();
                        if (wnd.isOK)
                        {
                            btnReadMcuData.IsEnabled = true;
                            btnSaveMcuData.IsEnabled = true;
                            btnLoadMcuPara.IsEnabled = true;
                        }
                        else
                        {
                            btnReadMcuData.IsEnabled = false;
                            btnWriteMcuData.IsEnabled = false;
                            btnLoadMcuPara.IsEnabled = false;
                            btnSaveMcuData.IsEnabled = false;
                        }
                    }
                }
                else
                {
                    btnReadMcuData.IsEnabled = true;
                    btnSaveMcuData.IsEnabled = true;
                    btnLoadMcuPara.IsEnabled = true;
                }
            }
            else
            {
                btnReadMcuData.IsEnabled = false;
                btnWriteMcuData.IsEnabled = false;
                btnLoadMcuPara.IsEnabled = false;
                btnSaveMcuData.IsEnabled = false;
            }
        }

        public void RequireReadMcuMsg()
        {
            isRequestReadMcu = true;
            btnReadMcuData_Click(null, null);
        }
        bool isReadMCU = false;
        bool isWriteMCU = false;
        bool isRequestReadMcu = false;
        private void btnReadMcuData_Click(object sender, RoutedEventArgs e)
        {
            BqProtocol.bReadBqBmsResp = true;
            isReadMCU = false;
            BqProtocol.BqInstance.m_bIsStopCommunication = true;
            System.Threading.Thread.Sleep(500);
            BqProtocol.BqInstance.ReadMcuData();
            isReadMCU = true;
            btnLoadMcuPara.IsEnabled = true;
            btnWriteMcuData.IsEnabled = true;
        }
        private void btnWriteMcuData_Click(object sender, RoutedEventArgs e)
        {
            List<byte> bytes = new List<byte>();
            if (!GetMcuDataBuf(ListSysInfo1,bytes))
            {
                return;
            }
            if (!GetMcuDataBuf(ListSysInfo2, bytes))
            {
                return;
            }
            if (!GetMcuDataBuf(ListChargeInfo, bytes))
            {
                return;
            }
            if(bytes.Count > 0)
            {
                isWriteMCU = true;
                BqProtocol.BqInstance.m_bIsStopCommunication = true;
                Thread.Sleep(100);
                BqProtocol.BqInstance.SendMultiFrame(bytes.ToArray(), bytes.Count, 0xA6);
            }
        }

        private void btnLoadMcuPara_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "程序文件(*.txt)|*.txt|所有文件(*.*)|*.*";
            ofd.FileName = System.Windows.Forms.Application.StartupPath + "\\ProtocolFiles\\V3_MCU_config.txt";

            bool? result = ofd.ShowDialog();
            if (result != true)
                return;

            FileStream fs = null;
            StreamReader sr = null;

            try
            { 
                Encoding encoding = System.Text.Encoding.UTF8; 

                fs = new FileStream(ofd.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
          
                sr = new StreamReader(fs, encoding);
   
                //记录每次读取的一行记录
                string strLine = "";

                int nIndex = 0;
                //逐行读取数据
                while ((strLine = sr.ReadLine()) != null)
                {
                    if(string.IsNullOrEmpty(strLine))
                    {
                        continue;
                    }

                    string[] arrVal = strLine.Split(':');

                    if (nIndex < ListSysInfo1.Count)
                    {
                        LoadMcuInfo(arrVal, ListSysInfo1);
                    }
                    else if (nIndex < (ListSysInfo1.Count + ListSysInfo2.Count))
                    {
                        LoadMcuInfo(arrVal, ListSysInfo2);
                    }
                    else 
                    {
                        LoadMcuInfo(arrVal, ListChargeInfo);
                        //continue;
                    }

                    nIndex++;
                }

            }
            catch(Exception ex)
            {

            }
        }

        private bool LoadMcuInfo(string[] arrVal, List<H5BmsInfo> ListMcuInfo)
        {
            bool bRet = false;
            for (int n = 0; n < ListMcuInfo.Count; n++)
            {
                if (arrVal[0] == ListMcuInfo[n].Description.Trim())
                {
                    ListMcuInfo[n].StrValue = arrVal[1].Trim();
                    bRet = true;
                    break;
                }
            }
            return bRet;
        }

        private void btnSaveMcuData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Title = "MCU 参数";
                sfd.Filter = "MCU 参数文件(*.txt)|*.txt|所有文件(*.*)|*.*";
                sfd.FileName = string.Format("MCU参数_{0:yyyyMMdd_HHmm}", DateTime.Now);

                bool? result = sfd.ShowDialog();

                if (result == true)
                {
                    FileStream fs = new FileStream(sfd.FileName, FileMode.OpenOrCreate);
                    StreamWriter sw = new StreamWriter(fs);

                    sw.WriteLine(SaveParaGetUIData(ListSysInfo1));
                    sw.WriteLine();

                    sw.WriteLine(SaveParaGetUIData(ListSysInfo2));
                    sw.WriteLine();

                    sw.WriteLine(SaveParaGetUIData(ListChargeInfo));
                    sw.WriteLine();

                    sw.Close();
                    fs.Close();
                    MessageBox.Show("MCU 参数保存成功! ", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch(Exception ex)
            {

            }

        }

        public void HandleMcuWndUpdateEvent(object sender, EventArgs e)
        {
            UpdateMcuWnd();
        }

        public void HandleRecvMcuDataEvent(object sender, CustomRecvDataEventArgs e)
        {   if(isReadMCU)
            {
                BqUpdateMcuInfo(e.RecvMsg);
                BqProtocol.BqInstance.m_bIsStopCommunication = false;
                isReadMCU = false;
            }
        }

        public void HandleWriteMcuDataEvent(object sender, CustomRecvDataEventArgs e)
        {
            if(isWriteMCU)
            {
                BqProtocol.bReadBqBmsResp = true;
                if (e.RecvMsg[0] == 0xDD || e.RecvMsg[1] == 0xA6 || e.RecvMsg.Count == (e.RecvMsg[2] << 8 | e.RecvMsg[3]))
                {
                    var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                    if (res == 0)
                    {
                        MessageBox.Show("写入 MCU 参数成功！", "写入MCU提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("写入 MCU 参数失败！", "写入MCU提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                BqProtocol.BqInstance.m_bIsStopCommunication = false;
                isWriteMCU = false;
            }
        }

        private bool GetMcuDataBuf(List<H5BmsInfo> list,List<byte> listbytes)
        {
            bool bRet = false;
       
            try
            {
                string pattern = @"^-?\d+$";
                foreach (var item in list)
                {
                    if (Regex.IsMatch(item.StrValue.Trim(), pattern))
                    {
                        if (item.ByteCount == 2)
                        {
                            if (item.Unit == "℃")
                            {
                                short paramVal = 0;
                                paramVal = (short)(short.Parse(item.StrValue.Trim()) * 10 + 2731);
                                if (paramVal >= item.MinValue && paramVal <= item.MaxValue)
                                {
                                    byte[] bytes = BitConverter.GetBytes(paramVal);
                                    listbytes.AddRange(bytes);
                                }
                                else
                                {
                                    MessageBox.Show(string.Format("{0} 的数据超出数据范围，请检查！", item.Description), "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                                    return false;
                                }
                            }
                            else
                            {
                                short val = 0;
                                if (short.TryParse(item.StrValue.Trim(), out val))
                                {
                                    if (val >= item.MinValue && val <= item.MaxValue)
                                    {
                                        byte[] bytes = BitConverter.GetBytes(val);
                                        listbytes.AddRange(bytes);
                                    }
                                    else
                                    {
                                        MessageBox.Show(string.Format("{0} 的数据超出数据范围，请检查！", item.Description), "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                                        return false;
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("参数中数据转换异常，请检查！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                                    return false;
                                }
                            }
                        }
                        else if (item.ByteCount == 4)
                        {
                            int val = 0;
                            if (int.TryParse(item.StrValue.Trim(), out val))
                            {
                                if (val >= item.MinValue && val <= item.MaxValue)
                                {
                                    byte[] bytes = BitConverter.GetBytes(val);
                                    listbytes.AddRange(bytes);
                                }
                                else
                                {
                                    MessageBox.Show(string.Format("{0} 的数据超出数据范围，请检查！", item.Description), "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                                    return false;
                                }
                            }
                            else
                            {
                                MessageBox.Show("参数中数据转换异常，请检查！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                                return false;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("参数中包含非数字项，请检查！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return false;
                    }
                }

                bRet = true;
            }
            catch(Exception ex)
            {

            }
            return bRet;

        }

        private string SaveParaGetUIData(List<H5BmsInfo> ListInfo)
        {
            string strMsg = null;

            for (int n = 0; n < ListInfo.Count; n++)
            {
                strMsg += ListInfo[n].Description + ": " + ListInfo[n].StrValue + "\r\n";
            }

            return strMsg;
        }

    }
}
