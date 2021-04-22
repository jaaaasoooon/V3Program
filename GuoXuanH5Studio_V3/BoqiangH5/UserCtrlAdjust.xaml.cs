using BoqiangH5.BQProtocol;
using BoqiangH5Entity;
using BoqiangH5Repository;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;

namespace BoqiangH5
{
    /// <summary>
    /// UserCtrlAdjust.xaml 的交互逻辑
    /// </summary>
    public partial class UserCtrlAdjust : UserControl
    {        
        List<H5BmsInfo> ListAdjustVoltage2 = new List<H5BmsInfo>();

        System.Windows.Threading.DispatcherTimer  timerRTC;

        int nStartAddr = 0xA200;
        int nCellVoltageAddr = 0xA210;

        public UserCtrlAdjust()
        {            
            InitializeComponent();
            InitAdjustWnd();

            UpdateAdjustWndStatus();
        }

        private void InitAdjustWnd()
        {
            string strConfigFile = XmlHelper.m_strBqProtocolFile;
            XmlHelper.LoadXmlConfig(strConfigFile, "bms_info/cell_votage_info", ListAdjustVoltage2);

            //cbIsRefresh.IsChecked = true;
            SetTimerHandshake();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dgAdjustVoltage2.ItemsSource = ListAdjustVoltage2;

            if (ListAdjustVoltage2.Count > 2)
            {
                DataGridRow row = DataGridExtension.GetRow(dgAdjustVoltage2, 0);
                row.Visibility = Visibility.Collapsed;
                DataGridRow _row = DataGridExtension.GetRow(dgAdjustVoltage2, 1);
                _row.Visibility = Visibility.Collapsed;
            }
        }
        private void SetTimerHandshake()
        {
            timerRTC = new System.Windows.Threading.DispatcherTimer();
            timerRTC.Tick += new EventHandler(OnTimedHandshakeEvent);
            timerRTC.Interval = new TimeSpan(0, 0, 1);
            timerRTC.Start();
        }

         private void OnTimedHandshakeEvent(Object sender, EventArgs e)
         {
            tbCurrentTime.Text = DateTime.Now.ToString();

            if (cbIsRefresh.IsChecked == true && MainWindow.m_statusBarInfo.IsOnline == true)
            {
                if (SelectCANWnd.m_H5Protocol == H5Protocol.BO_QIANG)
                    BqProtocol.BqInstance.BQ_ReadRTC();
            }
        }

        private void UpdateAdjustWndStatus()
        {
            if (MainWindow.m_statusBarInfo.IsOnline)
            {
                if(SelectCANWnd.m_H5Protocol == H5Protocol.BO_QIANG)
                {
                    btnAdjustCellVol.IsEnabled = true;
                    btnAdjustBatteryVol.IsEnabled = true;
                    btnAdjustLoadVol.IsEnabled = true;
                    btnAdRtCurrent.IsEnabled = true;
                    btnAdZeroCurrent.IsEnabled = true;
                    btnAdjustTemp.IsEnabled = true;
                    btnAdjustRTC.IsEnabled = true;
                    btnAdjustInnerResistance.IsEnabled = true;
                    btnAdjustOutResistance.IsEnabled = true;
                }    
            }
            else
            {
                btnAdjustCellVol.IsEnabled = false;
                btnAdjustBatteryVol.IsEnabled = false;
                btnAdjustLoadVol.IsEnabled = false;
                btnAdRtCurrent.IsEnabled = false;
                btnAdZeroCurrent.IsEnabled = false;
                btnAdjustTemp.IsEnabled = false;
                btnAdjustRTC.IsEnabled = false;
                btnAdjustInnerResistance.IsEnabled = false;
                btnAdjustOutResistance.IsEnabled = false;
            }

        }

        public void HandleAdjustWndUpdateEvent(object sender, EventArgs e)
        {
            UpdateAdjustWndStatus();
        }

        public void HandleRecvBmsInfoDataEvent(object sender, CustomRecvDataEventArgs e)
        {
            if (SelectCANWnd.m_H5Protocol == H5Protocol.BO_QIANG)
            {
                if (string.IsNullOrEmpty(tbRtc.Text.Trim()))
                {
                    BqProtocol.BqInstance.BQ_ReadRTC();
                }
                BqUpdateCellVoltage(e.RecvMsg);
            }
            else
            {
                ////if (string.IsNullOrEmpty(tbRtc.Text.Trim()))
                ////{
                ////    DDProtocol.DdProtocol.DdInstance.Didi_ReadRTC();
                ////}
                DdUpdateCellVoltage(e.RecvMsg);
            }
        }

        private void DdUpdateCellVoltage(List<byte> listRecv)
        {
            if(listRecv.Count != 0xCE)
            {
                return;
            }

            int nDdByteIndex = (nCellVoltageAddr - nStartAddr) * 2;

            for (int n = 0; n < 16; n++)  // for (int n = 16; n < 32; n++)
            {
                ListAdjustVoltage2[n].StrValue = ((listRecv[nDdByteIndex] << 8) | listRecv[nDdByteIndex + 1]).ToString();
                nDdByteIndex += 2;
            }
            
        }


        private void BqUpdateCellVoltage(List<byte> listRecv)
        {
            if (listRecv[0] != 0xCC || listRecv[1] != 0xA2)
            {
                return;
            }
            if (listRecv.Count < (listRecv[2] << 8 | listRecv[3]))
            {
                return;
            }

            int nBqByteIndex = 4;

            for (int n = 0; n < ListAdjustVoltage2.Count; n++)
            {
                int nCellVol = 0;
                for (int m = ListAdjustVoltage2[n].ByteCount - 1; m >= 0; m--)
                {
                    nCellVol = (nCellVol << 8 | listRecv[nBqByteIndex + m]);
                }
                ListAdjustVoltage2[n].StrValue = nCellVol.ToString();

                nBqByteIndex += ListAdjustVoltage2[n].ByteCount;
            }

            //tbTotalVoltage.Text = ListAdjustVoltage2[1].StrValue;
            tbCurrent.Text = ListAdjustVoltage2[0].StrValue;

            int temp = (listRecv[nBqByteIndex + 1] << 8) | listRecv[nBqByteIndex];
            tbCellTemp1.Text = ((temp - 2731) / 10).ToString();
            nBqByteIndex += 2;
            temp = (listRecv[nBqByteIndex + 1] << 8) | listRecv[nBqByteIndex];
            tbCellTemp2.Text = ((temp - 2731) / 10).ToString();
            nBqByteIndex += 2;
            temp = (listRecv[nBqByteIndex + 1] << 8) | listRecv[nBqByteIndex];
            tbCellTemp3.Text = ((temp - 2731) / 10).ToString();
            nBqByteIndex += 2;
            temp = (listRecv[nBqByteIndex + 1] << 8) | listRecv[nBqByteIndex];
            tbCellTemp4.Text = ((temp - 2731) / 10).ToString();
            nBqByteIndex += 2;
            temp = (listRecv[nBqByteIndex + 1] << 8) | listRecv[nBqByteIndex];
            tbCellTemp5.Text = ((temp - 2731) / 10).ToString();
            nBqByteIndex += 2;
            temp = (listRecv[nBqByteIndex + 1] << 8) | listRecv[nBqByteIndex];
            tbCellTemp6.Text = ((temp - 2731) / 10).ToString();
            nBqByteIndex += 2;
            temp = (listRecv[nBqByteIndex + 1] << 8) | listRecv[nBqByteIndex];
            tbAmbientTemp.Text = ((temp - 2731) / 10).ToString();
            nBqByteIndex += 2;
            temp = (listRecv[nBqByteIndex + 1] << 8) | listRecv[nBqByteIndex];
            tbMosTemp1.Text = ((temp - 2731) / 10).ToString();
            nBqByteIndex += 2;
            temp = (listRecv[nBqByteIndex + 1] << 8) | listRecv[nBqByteIndex];
            tbMosTemp2.Text = ((temp - 2731) / 10).ToString();
            nBqByteIndex += 2;
            int resistance = (listRecv[nBqByteIndex + 1] << 8) | listRecv[nBqByteIndex];
            tbInnerResistance.Text = resistance.ToString();
            nBqByteIndex += 2;
            resistance = (listRecv[nBqByteIndex + 1] << 8) | listRecv[nBqByteIndex];
            tbOutResistance.Text = resistance.ToString();
            nBqByteIndex += 4;
            int voltage = (listRecv[nBqByteIndex + 3] << 24) | (listRecv[nBqByteIndex + 2] << 16) | (listRecv[nBqByteIndex + 1] << 8) | listRecv[nBqByteIndex];
            tbBatteryVoltage.Text = voltage.ToString();
            nBqByteIndex += 4;
            voltage = (listRecv[nBqByteIndex + 3] << 24) | (listRecv[nBqByteIndex + 2] << 16) | (listRecv[nBqByteIndex + 1] << 8) | listRecv[nBqByteIndex];
            tbLoadVoltage.Text = voltage.ToString();
        }

        bool isAdjustCurrent = false;
        bool isAdjustZeroCurrent = false;
        private void btnAdjustCurrent_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null)
                return;

            string strPatten = @"^[\-|0-9][0-9]*$";

            if (btn.Name == "btnAdRtCurrent")
            {
                if (!Regex.IsMatch(this.tbRtCurrent.Text, strPatten))
                {
                    MessageBox.Show("请输入正确的电流值！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                isAdjustCurrent = false;
                BqProtocol.BqInstance.AdjustRtCurrent(int.Parse(tbRtCurrent.Text));
                isAdjustCurrent = true;
            }

            else if (btn.Name == "btnAdZeroCurrent")
            {
                if (!Regex.IsMatch(this.tbZeroCurrent.Text, strPatten))
                {
                    MessageBox.Show("请输入正确的电流值！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                isAdjustZeroCurrent = false;
                BqProtocol.BqInstance.AdjustZeroCurrent(int.Parse(tbZeroCurrent.Text));
                isAdjustZeroCurrent = true;
            }

        }

        public void HandleAdjustRTCurrenEvent(object sender, CustomRecvDataEventArgs e)
        {
            if(isAdjustCurrent)
            {
                BqProtocol.bReadBqBmsResp = true;
                if (e.RecvMsg[0] == 0xDD && e.RecvMsg[1] == 0xA9 && e.RecvMsg.Count == (e.RecvMsg[2] << 8 | e.RecvMsg[3]))
                {
                    var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                    if (res == 0)
                    {
                        MessageBox.Show("校准实时电流成功！", "校准提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("校准实时电流失败！", "校准提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                isAdjustCurrent = false;
            }
        }

        public void HandleAdjustZeroCurrenEvent(object sender, CustomRecvDataEventArgs e)
        {
            if(isAdjustZeroCurrent)
            {
                BqProtocol.bReadBqBmsResp = true;
                if (e.RecvMsg[0] == 0xDD && e.RecvMsg[1] == 0xA8 && e.RecvMsg.Count == (e.RecvMsg[2] << 8 | e.RecvMsg[3]))
                {
                    var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                    if (res == 0)
                    {
                        MessageBox.Show("校准零点电流成功！", "校准提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("校准零点电流失败！", "校准提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                isAdjustZeroCurrent = false;
            }
        }

        bool isAdjustRTC = false;
        private void btnAdjustRTC_Click(object sender, RoutedEventArgs e)
        {
            if(MainWindow.m_statusBarInfo.IsOnline)
            {
                try
                {
                    DateTime dt = DateTime.Parse(tbCurrentTime.Text.Trim());
                    if(null != dt)
                    {
                        isAdjustRTC = false;
                        BqProtocol.bReadBqBmsResp = true;
                        BqProtocol.BqInstance.AdjustRTC(dt);
                        isAdjustRTC = true;
                    }
                    else
                    {
                        MessageBox.Show("请检查RTC时间格式是否正确！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("请检查RTC时间格式是否正确！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("系统未连接，请连接后再进行操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void HandleAdjustRTCEvent(object sender, CustomRecvDataEventArgs e)
        {
            if(isAdjustRTC)
            {
                BqProtocol.bReadBqBmsResp = true;
                if (e.RecvMsg[0] == 0xDD && e.RecvMsg[1] == 0xA7 && e.RecvMsg.Count == (e.RecvMsg[2] << 8 | e.RecvMsg[3]))
                {
                    var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                    if (res == 0)
                    {
                        MessageBox.Show("校准RTC成功！", "校准提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        BqProtocol.BqInstance.BQ_ReadRTC();
                    }
                    else
                    {
                        MessageBox.Show("校准RTC失败！", "校准提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                isAdjustRTC = false;
            }

        }

        public static DateTime systemStartTime = new DateTime(1970, 1, 1, 8, 0, 0);
        public void HandleReadBqRTCEvent(object sender, CustomRecvDataEventArgs e)
        {
            if (SelectCANWnd.m_H5Protocol == H5Protocol.BO_QIANG)
            {
                BqProtocol.bReadBqBmsResp = true;
                if (e.RecvMsg[1] == 0xA3 && e.RecvMsg[0] == 0xCC && (e.RecvMsg.Count >= (e.RecvMsg[2] << 8 | e.RecvMsg[3])))
                {
                    int nRegister = ((e.RecvMsg[7] << 24) | (e.RecvMsg[6] << 16) | (e.RecvMsg[5] << 8) | e.RecvMsg[4]);
                    TimeSpan ts = new TimeSpan((long)(nRegister * Math.Pow(10, 7)));
                    tbRtc.Text = (systemStartTime + ts).ToString("yyyy/MM/dd HH:mm:ss");
                }
                else
                {
                    string str = "RTC数据读取失败！";
                    tbRtc.Text = str;
                }
            }
        }

        private void btnAdjustLoadVol_Click(object sender, RoutedEventArgs e)
        {
            uint voltage = 0;
            if(uint.TryParse(tbLoadVoltage.Text.Trim(),out voltage))
            {
                BqProtocol.BqInstance.m_bIsStopCommunication = true;
                Thread.Sleep(200);
                BqProtocol.BqInstance.BQ_AdjustLoadVoltageParam(voltage);
            }
            else
            {
                MessageBox.Show("输入的负载端电压格式不正确！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        public void HandleRecvAdjustLoadVoltageEvent(object sender, CustomRecvDataEventArgs e)
        {
            BqProtocol.BqInstance.m_bIsStopCommunication = false;
            if (e.RecvMsg[0] == 0xDD && e.RecvMsg[1] == 0xAB && e.RecvMsg.Count == (e.RecvMsg[2] << 8 | e.RecvMsg[3]))
            {
                var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                if (res == 0)
                {
                    MessageBox.Show("校准负载端电压成功！", "校准提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("校准负载端电压失败！", "校准提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        private void btnAdjustBatteryVol_Click(object sender, RoutedEventArgs e)
        {
            uint voltage = 0;
            if (uint.TryParse(tbBatteryVoltage.Text.Trim(), out voltage))
            {
                BqProtocol.BqInstance.m_bIsStopCommunication = true;
                Thread.Sleep(200);
                BqProtocol.BqInstance.BQ_AdjustBatteryVoltageParam(voltage);
            }
            else
            {
                MessageBox.Show("输入的电池端电压格式不正确！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        public void HandleRecvAdjustBatteryVoltageEvent(object sender, CustomRecvDataEventArgs e)
        {
            BqProtocol.BqInstance.m_bIsStopCommunication = false;
            if (e.RecvMsg[0] == 0xDD && e.RecvMsg[1] == 0xAA && e.RecvMsg.Count == (e.RecvMsg[2] << 8 | e.RecvMsg[3]))
            {
                var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                if (res == 0)
                {
                    MessageBox.Show("校准电池端电压成功！", "校准提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("校准电池端电压失败！", "校准提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        private void btnAdjustOutResistance_Click(object sender, RoutedEventArgs e)
        {
            ushort resistance = 0;
            if (ushort.TryParse(tbAdjustOutResistance.Text.Trim(), out resistance))
            {
                BqProtocol.BqInstance.m_bIsStopCommunication = true;
                Thread.Sleep(200);
                BqProtocol.BqInstance.BQ_AdjustOutResistanceParam(resistance);
            }
            else
            {
                MessageBox.Show("输入的外包进水阻抗格式不正确！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        public void HandleRecvAdjustOutResistanceEvent(object sender, CustomRecvDataEventArgs e)
        {
            BqProtocol.BqInstance.m_bIsStopCommunication = false;
            if (e.RecvMsg[0] == 0xDD && e.RecvMsg[1] == 0xAD && e.RecvMsg.Count == (e.RecvMsg[2] << 8 | e.RecvMsg[3]))
            {
                var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                if (res == 0)
                {
                    MessageBox.Show("校准外包进水阻抗成功！", "校准提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("校准外包进水阻抗失败！", "校准提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        private void btnAdjustInnerResistance_Click(object sender, RoutedEventArgs e)
        {
            ushort resistance = 0;
            if (ushort.TryParse(tbAdjustInnerResistance.Text.Trim(), out resistance))
            {
                BqProtocol.BqInstance.m_bIsStopCommunication = true;
                Thread.Sleep(200);
                BqProtocol.BqInstance.BQ_AdjustInnerResistanceParam(resistance);
            }
            else
            {
                MessageBox.Show("输入的内包进水阻抗格式不正确！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        public void HandleRecvAdjustInnerResistanceEvent(object sender, CustomRecvDataEventArgs e)
        {
            BqProtocol.BqInstance.m_bIsStopCommunication = false;
            if (e.RecvMsg[0] == 0xDD && e.RecvMsg[1] == 0xAC && e.RecvMsg.Count == (e.RecvMsg[2] << 8 | e.RecvMsg[3]))
            {
                var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                if (res == 0)
                {
                    MessageBox.Show("校准内包进水阻抗成功！", "校准提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("校准内包进水阻抗失败！", "校准提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }


        //private void cbIsRefresh_Click(object sender, RoutedEventArgs e)
        //{
        //    if (cbIsRefresh.IsChecked == true)
        //        timerRTC.Start();
        //    else
        //        timerRTC.Stop();
        //}

    }
}
