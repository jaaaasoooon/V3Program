using BoqiangH5.BQProtocol;
using BoqiangH5.DDProtocol;
using BoqiangH5Entity;
using BoqiangH5Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BoqiangH5
{
    /// <summary>
    /// UserCtrlAFE.xaml 的交互逻辑
    /// </summary>
    public partial class UserCtrlAFE : UserControl
    {
        public static List<AFERegister> m_ListEepromRegister = new List<AFERegister>();
        public static List<AFERegister> m_ListOthersRegister = new List<AFERegister>();
        public static List<AdjustParam> m_ListAdjustParam = new List<AdjustParam>();
        public UserCtrlAFE()
        {
            InitializeComponent();
            m_ListEepromRegister.Clear();
            //string strConfigFile = System.AppDomain.CurrentDomain.BaseDirectory + @"BoqiangV3\ProtocolFiles\bq_h5_bms_info.xml"; //程序合并时使用
            string strConfigFile = XmlHelper.m_strBqProtocolFile;
            XmlHelper.LoadAFERegisterConfig(strConfigFile, m_ListEepromRegister,true);
            XmlHelper.LoadAFERegisterConfig(strConfigFile, m_ListOthersRegister, false);
            XmlHelper.LoadAdjustParam(strConfigFile, m_ListAdjustParam);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dgEepromInfo.ItemsSource = m_ListEepromRegister;
            dgOthersInfo.ItemsSource = m_ListOthersRegister;
            dgAdjustParam.ItemsSource = m_ListAdjustParam;
            dgEepromInfo.Items.Refresh();
            dgOthersInfo.Items.Refresh();
            dgAdjustParam.Items.Refresh();
            //if(SelectCANWnd.m_H5Protocol == H5Protocol.DI_DI)
            //{
            //    btnReadEepromInfo.IsEnabled = false;
            //    btnReadOthersInfo.IsEnabled = false;
            //}
        }

        bool isReadEeprom = false;
        private void btnReadEepromInfo_Click(object sender, RoutedEventArgs e)
        {
            if(MainWindow.m_statusBarInfo.IsOnline)
            {
                if(SelectCANWnd.m_H5Protocol == H5Protocol.BO_QIANG)
                    BqProtocol.BqInstance.m_bIsStopCommunication = true;
                else
                    DdProtocol.DdInstance.m_bIsStopCommunication = true;
                Thread.Sleep(100);
                isReadEeprom = true;
                BqProtocol.BqInstance.BQ_RequireReadEepromRegister();
            } 
            else
            {
                MessageBox.Show("系统未连接，请连接后再进行操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void HandleRecvEepromDataEvent(object sender, CustomRecvDataEventArgs e)
        {
            if(isReadEeprom)
            {
                if (e.RecvMsg[0] == 0xCC && e.RecvMsg.Count >= (e.RecvMsg[2] << 8 | e.RecvMsg[3]))
                {
                    if (e.RecvMsg[1] == 0xB0)
                    {
                        Thread.Sleep(1000);
                        BqProtocol.bReadBqBmsResp = true;
                        BqProtocol.BqInstance.ReadEepromData();
                    }
                    else if(e.RecvMsg[1] == 0xB1)
                    {
                        isReadEeprom = false;
                        int offset = 4;
                        foreach (var it in m_ListEepromRegister)
                        {
                            it.StrValue = e.RecvMsg[offset].ToString("X2");
                            offset++;
                        }

                        if (SelectCANWnd.m_H5Protocol == H5Protocol.BO_QIANG)
                            BqProtocol.BqInstance.m_bIsStopCommunication = false;
                        else
                            DdProtocol.DdInstance.m_bIsStopCommunication = false;
                        MessageBox.Show("读取Eeprom寄存器参数完成", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }

        }

        bool isReadOthers = false;
        private void btnReadOthersInfo_Click(object sender, RoutedEventArgs e)
        {
            if(MainWindow.m_statusBarInfo.IsOnline)
            {
                if (SelectCANWnd.m_H5Protocol == H5Protocol.BO_QIANG)
                    BqProtocol.BqInstance.m_bIsStopCommunication = true;
                else
                    DdProtocol.DdInstance.m_bIsStopCommunication = true;
                Thread.Sleep(100);
                isReadOthers = true;
                BqProtocol.BqInstance.BQ_RequireReadOthersRegister();
            }
            else
            {
                MessageBox.Show("系统未连接，请连接后再进行操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void HandleRecvOthersDataEvent(object sender, CustomRecvDataEventArgs e)
        {
            if(isReadOthers)
            {
                if (e.RecvMsg[0] == 0xCC && e.RecvMsg.Count >= (e.RecvMsg[2] << 8 | e.RecvMsg[3]))
                {
                    if(e.RecvMsg[1] == 0xB2)
                    {
                        Thread.Sleep(1000);
                        BqProtocol.bReadBqBmsResp = true;
                        BqProtocol.BqInstance.BQ_ReadOthersRegister();
                    }
                    else if(e.RecvMsg[1] == 0xB3)
                    {
                        isReadOthers = false;
                        int offset = 4;
                        foreach (var it in m_ListOthersRegister)
                        {
                            it.StrValue = e.RecvMsg[offset].ToString("X2");
                            offset++;
                        }
                        if (SelectCANWnd.m_H5Protocol == H5Protocol.BO_QIANG)
                            BqProtocol.BqInstance.m_bIsStopCommunication = false;
                        else
                            DdProtocol.DdInstance.m_bIsStopCommunication = false;
                        MessageBox.Show("读取普通寄存器参数完成", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }
        bool isReadAdjustParam = false;
        private void btnReadAdjustParam_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.m_statusBarInfo.IsOnline)
            {
                if (SelectCANWnd.m_H5Protocol == H5Protocol.BO_QIANG)
                    BqProtocol.BqInstance.m_bIsStopCommunication = true;
                else
                    DdProtocol.DdInstance.m_bIsStopCommunication = true;
                Thread.Sleep(100);
                isReadAdjustParam = true;
                BqProtocol.BqInstance.BQ_ReadAdjustParam();
            }
            else
            {
                MessageBox.Show("系统未连接，请连接后再进行操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void HandleRecvAdjustParamEvent(object sender,CustomRecvDataEventArgs e)
        {
            if (isReadAdjustParam)
            {
                isReadAdjustParam = false;
                if (SelectCANWnd.m_H5Protocol == H5Protocol.BO_QIANG)
                    BqProtocol.BqInstance.m_bIsStopCommunication = false;
                else
                    DdProtocol.DdInstance.m_bIsStopCommunication = false;
                if (e.RecvMsg[0] == 0xCC && e.RecvMsg[1] == 0xAE && e.RecvMsg.Count >= (e.RecvMsg[2] << 8 | e.RecvMsg[3]))
                {
                    List<byte> data = e.RecvMsg;
                    int offset = 4;
                    for (int i = 0; i < m_ListAdjustParam.Count; i++)
                    {

                        if(m_ListAdjustParam[i].ByteCount == "2")
                        {
                            byte[] bytes = new byte[2];
                            bytes[0] = data[offset + 1];
                            bytes[1] = data[offset];
                            if(m_ListAdjustParam[i].Name.Contains("电压"))
                            {
                                UInt16 nVal = BitConverter.ToUInt16(bytes, 0);
                                m_ListAdjustParam[i].StrValue = nVal.ToString();
                            }
                            else
                            {
                                Int16 nVal = BitConverter.ToInt16(bytes, 0);
                                m_ListAdjustParam[i].StrValue = nVal.ToString();
                            }
                            // nVal = (data[offset + 1] << 8 | data[offset]);

                            offset += 2;
                        }
                        else if (m_ListAdjustParam[i].ByteCount == "4")
                        {
                            int nVal = (data[offset + 3] << 24 | data[offset + 2] << 16 | data[offset + 1] << 8 | data[offset]);
                            m_ListAdjustParam[i].StrValue = nVal.ToString();
                            offset += 4;
                        }
                    }

                    MessageBox.Show("校准参数读取成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
