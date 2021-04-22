using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using BoqiangH5.BQProtocol;
using BoqiangH5Entity;
using BoqiangH5Repository;

namespace BoqiangH5
{
    /// <summary>
    /// UserCtrlProtectParam.xaml 的交互逻辑
    /// </summary>
    public partial class UserCtrlProtectParam : UserControl
    {
        public static List<H5ProtectParamInfo> m_VoltageParamList = new List<H5ProtectParamInfo>();
        public static List<H5ProtectParamInfo> m_CurrentParamList = new List<H5ProtectParamInfo>();
        public static List<H5ProtectParamInfo> m_TemperatureParamList = new List<H5ProtectParamInfo>();
        public static List<H5ProtectParamInfo> m_WarningParamList = new List<H5ProtectParamInfo>();
        public static List<H5ProtectParamInfo> m_HumidityParamList = new List<H5ProtectParamInfo>();
        System.Timers.Timer timer;
        public UserCtrlProtectParam()
        {
            InitializeComponent();
            InitProtectParamWnd();
            timer = new System.Timers.Timer(1000);
            timer.Elapsed += OnTimer;
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Stop();
        }

        public void InitProtectParamWnd()
        {
            m_VoltageParamList.Clear();
            m_CurrentParamList.Clear();
            m_TemperatureParamList.Clear();
            m_WarningParamList.Clear();
            m_HumidityParamList.Clear();

            string strConfigFile = XmlHelper.m_strBqProtocolFile;
            XmlHelper.LoadProtectParamXmlConfig(strConfigFile, "protect_param/protect_voltage_param", m_VoltageParamList);
            XmlHelper.LoadProtectParamXmlConfig(strConfigFile, "protect_param/protect_current_param", m_CurrentParamList);

            XmlHelper.LoadProtectParamXmlConfig(strConfigFile, "protect_param/protect_temperature_param", m_TemperatureParamList);
            XmlHelper.LoadProtectParamXmlConfig(strConfigFile, "protect_param/protect_didi_warning_param", m_WarningParamList);
            XmlHelper.LoadProtectParamXmlConfig(strConfigFile, "protect_param/protect_humidity_param", m_HumidityParamList);
        }

        private void ucProtectParam_Loaded(object sender, RoutedEventArgs e)
        {
            dgVoltageParam.ItemsSource = m_VoltageParamList;
            dgCurrentParam.ItemsSource = m_CurrentParamList;
            dgTemperatureParam.ItemsSource = m_TemperatureParamList;
            dgWarningParam.ItemsSource = m_WarningParamList;
            dgHumidityParam.ItemsSource = m_HumidityParamList;
        }

        private void OnTimer(object sender,EventArgs e)
        {
            if(MainWindow.m_statusBarInfo.IsOnline)
            {
                BqProtocol.BqInstance.BQ_ReadVoltageProtectParam();
            }
        }

        public void StartOrStopTimer(bool flag)
        {
            if (MainWindow.m_statusBarInfo.IsOnline)
            {
                //if (SelectCANWnd.m_H5Protocol == H5Protocol.BO_QIANG)
                {
                    if (flag)
                    {
                        timer.Stop();
                        BqProtocol.BqInstance.m_bIsStopCommunication = false;
                    }
                    else
                    {
                        BqProtocol.BqInstance.m_bIsStopCommunication = true;
                        timer.Start();
                        BqProtocol.BqInstance.BQ_ReadVoltageProtectParam();
                    }
                }
            }
        }
        public void HandleRecvReadVoltageProtectParamEvent(object sender, CustomRecvDataEventArgs e)
        {
            UpdateProtectParam(e.RecvMsg, m_VoltageParamList);
            BqProtocol.BqInstance.BQ_ReadCurrentProtectParam();
        }
        public void HandleRecvReadCurrentProtectParamEvent(object sender, CustomRecvDataEventArgs e)
        {
            UpdateProtectParam(e.RecvMsg, m_CurrentParamList);
            BqProtocol.BqInstance.BQ_ReadTemperatureProtectParam();
        }
        public void HandleRecvReadTemperatureProtectParamEvent(object sender, CustomRecvDataEventArgs e)
        {
            UpdateProtectParam(e.RecvMsg, m_TemperatureParamList);
            BqProtocol.BqInstance.BQ_ReadWarningProtectParam();
        }
        public void HandleRecvReadWarningParamEvent(object sender, CustomRecvDataEventArgs e)
        {
            UpdateProtectParam(e.RecvMsg, m_WarningParamList);
            BqProtocol.BqInstance.BQ_ReadHumidityProtectParam();
        }
        public void HandleRecvReadHumidityProtectParamEvent(object sender, CustomRecvDataEventArgs e)
        {
            UpdateProtectParam(e.RecvMsg, m_HumidityParamList);
            StartOrStopTimer(true);
            if(isRead)
            {
                isRead = false;
                MessageBox.Show("保护参数读取完成！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        bool isRead = false;
        private void BtnRead_Click(object sender, RoutedEventArgs e)
        {
            isRead = true;
            StartOrStopTimer(false);
        }
        private bool GetDataBytes(List<H5ProtectParamInfo> list,List<byte> listbytes)
        {
            string pattern = @"^-?\d+$";
            foreach (var item in list)
            {
                if(Regex.IsMatch(item.StrValue.Trim(),pattern))
                {
                    if(item.ByteCount == 2)
                    {
                        if(item.Unit == "℃")
                        {
                            short paramVal = 0;
                            if (item.Description == "电芯温度差保护" || item.Description == "电芯温度差保护释放"
                                || item.Description == "电芯温度不平衡警告" || item.Description == "电芯温度不平衡警告释放")
                            {
                                paramVal = (short)(short.Parse(item.StrValue.Trim()) * 10);
                            }
                            else
                            {
                                paramVal = (short)(short.Parse(item.StrValue.Trim()) * 10 + 2731);
                            }
                            if (paramVal >= item.MinValue && paramVal <= item.MaxValue)
                            {
                                byte[] bytes = BitConverter.GetBytes(paramVal);
                                //Array.Reverse(bytes);
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
                                    //Array.Reverse(bytes);
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
                    else if(item.ByteCount == 4)
                    {
                        int val = 0;
                        if (int.TryParse(item.StrValue.Trim(), out val))
                        {
                            if (val >= item.MinValue && val <= item.MaxValue)
                            {
                                byte[] bytes = BitConverter.GetBytes(val);
                                //Array.Reverse(bytes);
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
                    MessageBox.Show("参数中包含非数字项，请检查！","提示",MessageBoxButton.OK,MessageBoxImage.Information);
                    return false;
                }
            }

            return true;
        }
        private void BtnWriteVoltage_Click(object sender, RoutedEventArgs e)
        {
            if(MainWindow.m_statusBarInfo.IsOnline)
            {
                List<byte> bytes = new List<byte>();
                if(GetDataBytes(m_VoltageParamList,bytes))
                {
                    if(bytes.Count > 0)
                    {
                        BqProtocol.BqInstance.m_bIsStopCommunication = true;
                        Thread.Sleep(200);
                        BqProtocol.BqInstance.BQ_WriteVoltageProtectParam(bytes);
                    }
                }
            }
            else
            {
                MessageBox.Show("系统未连接，请连接后再进行操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        public void HandleRecvWriteVoltageProtectParamEvent(object sender, CustomRecvDataEventArgs e)
        {
            BqProtocol.BqInstance.m_bIsStopCommunication = false;
            if (e.RecvMsg[0] == 0xDD && e.RecvMsg[1] == 0xA0 && e.RecvMsg.Count >= (e.RecvMsg[2] << 8 | e.RecvMsg[3]))
            {
                var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                if(res == 0)
                {
                    MessageBox.Show("电压保护参数写入成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("电压保护参数写入失败！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        private void BtnWriteCurrent_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.m_statusBarInfo.IsOnline)
            {
                List<byte> bytes = new List<byte>();
                if (GetDataBytes(m_CurrentParamList, bytes))
                {
                    if (bytes.Count > 0)
                    {
                        BqProtocol.BqInstance.m_bIsStopCommunication = true;
                        Thread.Sleep(200);
                        BqProtocol.BqInstance.BQ_WriteCurrentProtectParam(bytes);
                    }
                }
            }
            else
            {
                MessageBox.Show("系统未连接，请连接后再进行操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        public void HandleRecvWriteCurrentProtectParamEvent(object sender, CustomRecvDataEventArgs e)
        {
            BqProtocol.BqInstance.m_bIsStopCommunication = false;
            if (e.RecvMsg[0] == 0xDD && e.RecvMsg[1] == 0xA1 && e.RecvMsg.Count >= (e.RecvMsg[2] << 8 | e.RecvMsg[3]))
            {
                var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                if (res == 0)
                {
                    MessageBox.Show("电流保护参数写入成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("电流保护参数写入失败！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        private void BtnWriteTemperature_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.m_statusBarInfo.IsOnline)
            {
                List<byte> bytes = new List<byte>();
                if (GetDataBytes(m_TemperatureParamList, bytes))
                {
                    if (bytes.Count > 0)
                    {
                        BqProtocol.BqInstance.m_bIsStopCommunication = true;
                        Thread.Sleep(200);
                        BqProtocol.BqInstance.BQ_WriteTemperatureProtectParam(bytes);
                    }
                }
            }
            else
            {
                MessageBox.Show("系统未连接，请连接后再进行操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        public void HandleRecvWriteTemperatureProtectParamEvent(object sender, CustomRecvDataEventArgs e)
        {
            BqProtocol.BqInstance.m_bIsStopCommunication = false;
            if (e.RecvMsg[0] == 0xDD && e.RecvMsg[1] == 0xA2 && e.RecvMsg.Count >= (e.RecvMsg[2] << 8 | e.RecvMsg[3]))
            {
                var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                if (res == 0)
                {
                    MessageBox.Show("温度保护参数写入成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("温度保护参数写入失败！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        private void BtnWriteHumidity_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.m_statusBarInfo.IsOnline)
            {
                List<byte> bytes = new List<byte>();
                if (GetDataBytes(m_HumidityParamList, bytes))
                {
                    if (bytes.Count > 0)
                    {
                        BqProtocol.BqInstance.m_bIsStopCommunication = true;
                        Thread.Sleep(200);
                        BqProtocol.BqInstance.BQ_WriteHumidityProtectParam(bytes);
                    }
                }
            }
            else
            {
                MessageBox.Show("系统未连接，请连接后再进行操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        public void HandleRecvWriteHumidityProtectParamEvent(object sender, CustomRecvDataEventArgs e)
        {
            BqProtocol.BqInstance.m_bIsStopCommunication = false;
            if (e.RecvMsg[0] == 0xDD && e.RecvMsg[1] == 0xA4 && e.RecvMsg.Count >= (e.RecvMsg[2] << 8 | e.RecvMsg[3]))
            {
                var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                if (res == 0)
                {
                    MessageBox.Show("湿度/进水阻抗保护参数写入成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("湿度/进水阻抗保护参数写入失败！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        private void BtnWriteWarning_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.m_statusBarInfo.IsOnline)
            {
                List<byte> bytes = new List<byte>();
                if (GetDataBytes(m_WarningParamList, bytes))
                {
                    if (bytes.Count > 0)
                    {
                        BqProtocol.BqInstance.m_bIsStopCommunication = true;
                        Thread.Sleep(200);
                        BqProtocol.BqInstance.BQ_WriteWarningProtectParam(bytes);
                    }
                }
            }
            else
            {
                MessageBox.Show("系统未连接，请连接后再进行操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        public void HandleRecvWriteWarningProtectParamEvent(object sender, CustomRecvDataEventArgs e)
        {
            BqProtocol.BqInstance.m_bIsStopCommunication = false;
            if (e.RecvMsg[0] == 0xDD && e.RecvMsg[1] == 0xA3 && e.RecvMsg.Count >= (e.RecvMsg[2] << 8 | e.RecvMsg[3]))
            {
                var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                if (res == 0)
                {
                    MessageBox.Show("告警配置参数写入成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("告警配置参数写入失败！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
