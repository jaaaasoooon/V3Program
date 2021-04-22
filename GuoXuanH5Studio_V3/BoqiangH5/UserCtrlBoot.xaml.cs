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
    /// UserCtrlBoot.xaml 的交互逻辑
    /// </summary>
    public partial class UserCtrlBoot : UserControl
    {
        //List<H5BmsInfo> ListDeviceInfo = new List<H5BmsInfo>();
        public UserCtrlBoot()
        {
            InitializeComponent();

        }
        public event EventHandler RequireReadDeviceInfoEvent;
        private void btnReadDevice_Clicked(object sender, RoutedEventArgs e)
        {
            tbDeviceType.Text = string.Empty;
            tbFirmwareNo.Text = string.Empty;
            tbHardwareNo.Text = string.Empty;
            tbManufactureInfo.Text = string.Empty;
            tbDeviceSn.Text = string.Empty;
            tbNumber.Text = string.Empty;
            tbFirmwareNum.Text = string.Empty;
            tbHardwareNum.Text = string.Empty;
            tbStatus.Text = string.Empty;
            isMsgVisible = true;
            RequireReadDeviceInfoEvent?.Invoke(this, EventArgs.Empty);
        }
        bool isMsgVisible = true;

        bool isReadBoot = false;
        private void btnReadBoot_Click(object sender, RoutedEventArgs e)
        {
            isReadBoot = false;
            BqProtocol.BqInstance.m_bIsStopCommunication = true;
            Thread.Sleep(100);
            BqProtocol.BqInstance.BQ_ReadBootInfo();
            isReadBoot = true;
        }
        public void HandleReadBqBootEvent(object sender, CustomRecvDataEventArgs e)
        {
            try
            {
                if (isReadBoot)
                {
                    if (e.RecvMsg[0] != 0xCC || e.RecvMsg[1] != 0xB9 || e.RecvMsg.Count < (e.RecvMsg[2] << 8 | e.RecvMsg[3]))
                    {
                        return;
                    }

                    int offset = 4;
                    byte[] array = e.RecvMsg.ToArray();
                    string projectName = System.Text.Encoding.ASCII.GetString(array, offset, 8);
                    offset += 8;
                    string hardwareVersion = System.Text.Encoding.ASCII.GetString(array, offset, 8);
                    offset += 8;
                    string bootVersion = System.Text.Encoding.ASCII.GetString(array, offset, 8);
                    offset += 8;
                    string appNum = System.Text.Encoding.ASCII.GetString(array, offset, 8);
                    offset += 8;
                    string company = System.Text.Encoding.ASCII.GetString(array, offset, 16);
                    offset += 16;
                    string programStatusStr = string.Empty;
                    byte[] _array = new byte[2];
                    Buffer.BlockCopy(array, offset, _array, 0, _array.Length);
                    int programStatus = ((_array[1] << 8) | (((_array[0]))));
                    if (programStatus == 1)
                        programStatusStr = "APP初始化完成";
                    else if (programStatus == 0)
                        programStatusStr = "BOOT";
                    else if (programStatus == 2)
                        programStatusStr = "APP初始化";

                    List<string> list = new List<string>();
                    list.Add(projectName.Substring(0, projectName.IndexOf('\0')));
                    list.Add(hardwareVersion.Substring(0, hardwareVersion.IndexOf('\0')));
                    list.Add(bootVersion.Substring(0, bootVersion.IndexOf('\0')));
                    list.Add(appNum.Substring(0, appNum.IndexOf('\0')));
                    list.Add(company.Substring(0, company.IndexOf('\0')));
                    list.Add(programStatusStr);

                    UpdateBqBootInfo(list);
                    MessageBox.Show("读取Boot信息成功！", "读取Boot提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    isReadBoot = false;
                }
                BqProtocol.BqInstance.m_bIsStopCommunication = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Boot信息解析异常", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void UpdateBqBootInfo(List<string> list)
        {
            if (list.Count == 6)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    tbProjectName.Text = list[0].Trim();
                    tbHardwareVersion.Text = list[1].Trim();
                    tbBootVersion.Text = list[2].Trim();
                    tbAppNum.Text = list[3].Trim();
                    tbCompany.Text = list[4].Trim();
                    tbProgramStatus.Text = list[5].Trim();
                }));
            }
        }
        public void SetDeviceInfo(List<string> list)
        {
            UpdateDeviceInfo(list);
        }
        private void UpdateDeviceInfo(List<string> list)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                //for(int i = 0;i < list.Count;i++)
                //{
                //    ListDeviceInfo[i].StrValue = list[i];
                //}
                tbDeviceType.Text = list[0];
                tbFirmwareNo.Text = list[1];
                tbHardwareNo.Text = list[2];
                tbManufactureInfo.Text = list[3];
                tbDeviceSn.Text = list[4];
                tbNumber.Text = list[5];
                tbFirmwareNum.Text = list[6];
                tbHardwareNum.Text = list[7];
                tbStatus.Text = list[8];
                if(isMsgVisible)
                {
                    System.Windows.Forms.MessageBox.Show("读取设备信息成功！", "读取设备信息提示", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
                    isMsgVisible = false;
                }

            }));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //XmlHelper.LoadXmlConfig("ProtocolFiles\\didi_h5_bms_info.xml", "device_info/register_node_info", ListDeviceInfo);
            //dgDeviceInfo.ItemsSource = ListDeviceInfo;
            //btnReadBoot.IsEnabled = false;
            //btnReadDevice.IsEnabled = false;
            //btnReadUID.IsEnabled = false;
            //if (SelectCANWnd.m_H5Protocol == H5Protocol.BO_QIANG)
            //    gbSleepTest.IsEnabled = false;
            //else
            //    gbSleepTest.IsEnabled = true;
        }

        public void HandleBootTestWndUpdateEvent(object sender, EventArgs e)
        {
            //if(MainWindow.m_statusBarInfo.IsOnline)
            //{
            //    btnReadBoot.IsEnabled = true;
            //    btnReadDevice.IsEnabled = true;
            //    btnReadUID.IsEnabled = true;
            //}
            //else
            //{
            //    btnReadBoot.IsEnabled = false;
            //    btnReadDevice.IsEnabled = false;
            //    btnReadUID.IsEnabled = false;
            //}
        }
        //long sleepNum = 0;
        //long readNum = 0;
        //long recvNum = 0;
        //int timerCount = 0;
        //System.Timers.Timer timer;
        //bool isTest = false;
        //bool isAwake = false;
        //private void btnTest_Click(object sender, RoutedEventArgs e)
        //{
        //    if(MainWindow.m_statusBarInfo.IsOnline)
        //    {
        //        sleepNum = 0;
        //        labSleepCount.Content = sleepNum.ToString();
        //        readNum = 0;
        //        labReadCount.Content = readNum.ToString();
        //        recvNum = 0;
        //        labRecvCount.Content = readNum.ToString();
        //        timerCount = 0;
        //        isSend = false;
        //        failNum = 0;
        //        isReadDd = false;
        //        btnTest.Content = "测试中";
        //        btnStop.IsEnabled = true;
        //        btnTest.IsEnabled = false;
        //        if (SelectCANWnd.m_H5Protocol == H5Protocol.BO_QIANG)
        //            BqProtocol.BqInstance.m_bIsTest = true;
        //        else
        //            DdProtocol.DdInstance.m_bIsFlag = true;
        //        isTest = true;
        //        timer = new System.Timers.Timer(1000);
        //        timer.Elapsed += OnTimer;
        //        timer.AutoReset = true;
        //        timer.Enabled = true;
        //    }
        //    else
        //    {
        //        MessageBox.Show("系统未连接，请连接后再进行操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //}
        //bool isSend = false;
        //int failNum = 0;
        //bool isReadDd = false;
        //private void OnTimer(object sender, EventArgs e)
        //{
        //    timerCount++;
        //    if (timerCount == 1)//进行唤醒
        //    {
        //        isAwake = true;
        //        isSend = true;
        //        isReadDd = true;
        //        DdProtocol.DdInstance.ReadDdBmsInfo();
        //        readNum++;
        //    }
        //    else if (timerCount == 2)//读指令
        //    {
        //        isAwake = false;
        //        if (isSend) failNum++;
        //        isReadDd = true;
        //        DdProtocol.DdInstance.ReadDdBmsInfo();
        //        readNum++;
        //    }
        //    else if (timerCount == 3)//浅休眠指令
        //    {
        //        if (isSend) failNum++;
        //        BqProtocol.bReadBqBmsResp = true;
        //        BqProtocol.BqInstance.BQ_Sleep();
        //        sleepNum++;
        //    }
        //    else if (timerCount == 6)
        //    {
        //        timerCount = 0;
        //        if (isSend) failNum++;
        //        if(failNum == 3)
        //        {
        //            if (timer != null)
        //            {
        //                timer.Stop();
        //                timer.Close();
        //            }
        //            Dispatcher.Invoke(new Action(() =>
        //            {
        //                MessageBox.Show("通讯连接失败，请检查！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        //            }));
        //        }
        //        failNum = 0;
        //    }
        //    Dispatcher.Invoke(new Action(() =>
        //    {
        //        labSleepCount.Content = sleepNum.ToString();
        //        labReadCount.Content = readNum.ToString();
        //    }));
        //}
        //private void btnStop_Click(object sender, RoutedEventArgs e)
        //{
        //    btnTest.IsEnabled = true;
        //    btnStop.IsEnabled = false;
        //    btnTest.Content = "开始测试";
        //    isTest = false;
        //    if (timer != null)
        //    {
        //        timer.Stop();
        //        timer.Close();
        //    }
        //    if (SelectCANWnd.m_H5Protocol == H5Protocol.BO_QIANG)
        //        BqProtocol.BqInstance.m_bIsTest = false;
        //    else
        //        DdProtocol.DdInstance.m_bIsFlag = false;
        //}

        //public void HandleRecvBmsInfoDataEvent(object sender, CustomRecvDataEventArgs e)
        //{
        //    if(isTest && isReadDd)
        //    {
        //        //if (e.RecvMsg.Count < 0xCA || e.RecvMsg[1] != 0xC8)
        //        //{
        //        //    return;
        //        //}
        //        isReadDd = false;
        //        isSend = false;
        //        if (isAwake)
        //        {
        //            isAwake = false;
        //        }
        //        else
        //        {
        //            recvNum++;
        //            Dispatcher.Invoke(new Action(() =>
        //            {
        //                labRecvCount.Content = recvNum.ToString();
        //            }));
        //        }
        //    }
        //}

        //public void HandleSleepEvent(object sender, CustomRecvDataEventArgs e)
        //{
        //    if(isTest)
        //    {
        //        if (e.RecvMsg[0] == 0xD5)
        //        {
        //            isSend = false;
        //        }
        //    }
        //}
    }
}
