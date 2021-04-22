using BoqiangH5.BQProtocol;
using BoqiangH5.DDProtocol;
using BoqiangH5Entity;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using BoqiangH5Repository;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.IO;
using System.Threading;

namespace BoqiangH5
{

    public partial class UserCtrlDebug : UserControl
    {
        public static bool m_bIsNotUpdateBmsInfo = false;
        public static List<H5BmsInfo> m_ListBmsInfo = new List<H5BmsInfo>();
        public static List<H5BmsInfo> m_ListCellVoltage = new List<H5BmsInfo>();
        static byte[] btSysStatus = new byte[2];
        static byte[] btPackStatus = new byte[2];
        static byte[] btBalanceStatus = new byte[4];
        public UserCtrlDebug()
        {
            InitializeComponent();

            InitDebugWnd();
        }

        private void InitDebugWnd()
        {

            userCtrlDebug.IsEnabled = false;

            m_ListCellVoltage.Clear();
            m_ListBmsInfo.Clear();

            string strConfigFile = XmlHelper.m_strBqProtocolFile;

            XmlHelper.LoadXmlConfig(strConfigFile, "bms_info/cell_votage_info", m_ListCellVoltage);
            XmlHelper.LoadXmlConfig(strConfigFile, "bms_info/bms_info_node", m_ListBmsInfo);

        }

        public void SetVersionType(string type)
        {
            if (!type.Contains("研发版本号"))
            {
                gbParamSetting.Visibility = Visibility.Hidden;
                gbSleepTest.Visibility = Visibility.Hidden;
                gbEeprom.Visibility = Visibility.Hidden;
                btnCloseChargeMos.Visibility = Visibility.Hidden;
                btnOpenChargeMos.Visibility = Visibility.Hidden;
                btnExitChargeMos.Visibility = Visibility.Hidden;
                btnCloseDischargeMos.Visibility = Visibility.Hidden;
                btnOpenDischargeMos.Visibility = Visibility.Hidden;
                btnExitDischargeMos.Visibility = Visibility.Hidden;
            }
        }

        private void UpdateDebugWndStatus()
        {
            if (MainWindow.m_statusBarInfo.IsOnline)
            {
                userCtrlDebug.IsEnabled = true;
            }
            else
            {
                userCtrlDebug.IsEnabled = false;
                btnStop_Click(null, null);
                btnSleepStop_Click(null, null);
            }

        }

        public void HandleDebugWndUpdateEvent(object sender, EventArgs e)
        {
            UpdateDebugWndStatus();
        }
        public event EventHandler DeepSleepEvent;
        public event EventHandler ShallowSleepEvent;
        public void HandleDebugEvent(object sender, CustomRecvDataEventArgs e)
        {
            BqProtocol.bReadBqBmsResp = true;
            BqProtocol.BqInstance.m_bIsStopCommunication = false;
            if (e.RecvMsg[0] == 0xDD && e.RecvMsg.Count == (e.RecvMsg[2] << 8 | e.RecvMsg[3]))
            {
                switch (e.RecvMsg[1])
                {
                    case 0xD0:
                        if (isJumpBoot)
                        {
                            var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                            if (res == 0)
                                MessageBox.Show("跳转boot成功！", "跳转boot提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            else
                                MessageBox.Show("跳转boot失败！", "跳转boot提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            isJumpBoot = false;
                        }
                        break;
                    case 0xBC:
                        if (isSoftReset)
                        {
                            var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                            if (res == 0)
                                MessageBox.Show("系统复位成功！", "系统复位提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            else
                                MessageBox.Show("系统复位成功！", "系统复位提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            isSoftReset = false;
                        }
                        break;
                    case 0xB9:
                        if (isAlterSOC)
                        {
                            var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                            if (res == 0)
                                MessageBox.Show("SOC设置成功！", "设置SOC提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            else
                                MessageBox.Show("SOC设置失败！", "设置SOC提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            isAlterSOC = false;
                        }
                        break;
                    case 0xBE:
                        if (isFactoryReset)
                        {
                            var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                            if (res == 0)
                                MessageBox.Show("恢复出厂设置成功！", "恢复出厂设置提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            else
                                MessageBox.Show("恢复出厂设置失败！", "恢复出厂设置提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            isFactoryReset = false;
                        }
                        break;
                    case 0xB8:
                        if (isTestMode)
                        {
                            var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                            if (res == 0)
                                MessageBox.Show("进入测试模式设置成功！", "进入测试模式提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            else
                                MessageBox.Show("进入测试模式设置失败！", "进入测试模式提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            isTestMode = false;
                        }
                        if (isExitTestMode)
                        {
                            var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                            if (res == 0)
                                MessageBox.Show("退出测试模式设置成功！", "退出测试模式提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            else
                                MessageBox.Show("退出测试模式设置失败！", "退出测试模式提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            isExitTestMode = false;
                        }
                        break;
                    //case 0xBA:
                    //    if (isDeepSleep)
                    //    {
                    //        isDeepSleep = false;
                    //        DeepSleepEvent?.Invoke(this, EventArgs.Empty);//设置深休眠成功，断开连接
                    //        var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                    //        if (res == 0)
                    //            MessageBox.Show("进入深休眠设置成功！", "进入深休眠提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    //        else
                    //            MessageBox.Show("进入深休眠设置失败！", "进入深休眠提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    //    }
                    //    break;
                    //case 0xBB:
                    //    if (isShallowSleep)
                    //    {
                    //        isShallowSleep = false;
                    //        ShallowSleepEvent?.Invoke(this, EventArgs.Empty);//设置浅休眠成功，断开连接
                    //        var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                    //        if (res == 0)
                    //            MessageBox.Show("进入浅休眠设置成功！", "进入浅休眠提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    //        else
                    //            MessageBox.Show("进入浅休眠设置失败！", "进入浅休眠提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    //    }
                    //    break;
                    case 0xC0:
                        if (isExitChargeMos)
                        {
                            isExitChargeMos = false;
                            var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                            if (res == 0)
                                MessageBox.Show("充电MOS开关退出设置成功！", "充电MOS开关提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            else
                                MessageBox.Show("充电MOS开关退出设置失败！", "充电MOS开关提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else if (isCloseChargeMos)
                        {
                            isCloseChargeMos = false;
                            var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                            if (res == 0)
                                MessageBox.Show("充电MOS开关闭合设置成功！", "充电MOS开关提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            else
                                MessageBox.Show("充电MOS开关闭合设置失败！", "充电MOS开关提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else if (isOpenChargeMos)
                        {
                            isOpenChargeMos = false;
                            var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                            if (res == 0)
                                MessageBox.Show("充电MOS开关断开设置成功！", "充电MOS开关提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            else
                                MessageBox.Show("充电MOS开关断开设置失败！", "充电MOS开关提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        break;
                    case 0xC1:
                        if (isExitDischargeMos)
                        {
                            isExitDischargeMos = false;
                            var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                            if (res == 0)
                                MessageBox.Show("放电MOS开关退出设置成功！", "放电MOS开关提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            else
                                MessageBox.Show("放电MOS开关退出设置失败！", "放电MOS开关提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else if (isCloseDischargeMos)
                        {
                            isCloseDischargeMos = false;
                            var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                            if (res == 0)
                                MessageBox.Show("放电MOS开关闭合设置成功！", "放电MOS开关提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            else
                                MessageBox.Show("放电MOS开关闭合设置失败！", "放电MOS开关提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else if (isOpenDischargeMos)
                        {
                            isOpenDischargeMos = false;
                            var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                            if (res == 0)
                                MessageBox.Show("放电MOS开关断开设置成功！", "放电MOS开关提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            else
                                MessageBox.Show("放电MOS开关断开设置失败！", "放电MOS开关提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        break;
                    case 0xC3:
                        if(isEnterProductionMode)
                        {
                            isEnterProductionMode = false;
                            var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                            if (res == 0)
                                MessageBox.Show("进入生产模式设置成功！", "进入生产模式提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            else
                                MessageBox.Show("进入生产模式设置失败！", "进入生产模式提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else if(isExitProductionMode)
                        {
                            isExitProductionMode = false;
                            var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                            if (res == 0)
                                MessageBox.Show("退出生产模式设置成功！", "退出生产模式提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            else
                                MessageBox.Show("退出生产模式设置失败！", "退出生产模式提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        break;
                    default:
                        break;

                }
            }
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            InitDebugWnd();
        }

        bool isBqPowerOn = false;
        bool isBqPowerOff = false;

        /// <summary>
        /// 上电
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPowerOn_Click(object sender, RoutedEventArgs e)
        {
            isBqPowerOn = false;
            BqProtocol.BqInstance.m_bIsStopCommunication = true;
            Thread.Sleep(100);
            DdProtocol.DdInstance.DD_PowerOn();
            isBqPowerOn = true;
        }


        /// <summary>
        /// 下电
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPowerOff_Click(object sender, RoutedEventArgs e)
        {
            isBqPowerOff = false;
            BqProtocol.BqInstance.m_bIsStopCommunication = true;
            Thread.Sleep(100);
            DdProtocol.DdInstance.DD_PowerOff();
            isBqPowerOff = true;
        }

        public void HandleRaisePowerOnOffEvent(object sender, EventArgs e)
        {
            DdProtocol.bReadDdBmsResp = true;
            if (isDdPowerOn || isBqPowerOn)
            {
                MessageBox.Show("上电成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                isDdPowerOn = false;
                isBqPowerOn = false;
            }

            if (isDdPowerOff || isBqPowerOff)
            {
                MessageBox.Show("下电成功！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                isDdPowerOff = false;
                isBqPowerOff = false;
            }


        }

        bool isJumpBoot = false;
        private void btnJumpBoot_Click(object sender, RoutedEventArgs e)
        {
            isJumpBoot = false;
            BqProtocol.BqInstance.BQ_JumpToBoot();
            isJumpBoot = true;
        }

        bool isSoftReset = false;
        private void btnSoftReset_Click(object sender, RoutedEventArgs e)
        {
            isSoftReset = false;
            BqProtocol.BqInstance.m_bIsStopCommunication = true;
            Thread.Sleep(200);
            BqProtocol.BqInstance.BQ_Reset();
            isSoftReset = true;
        }

        bool isAlterSOC = false;
        // 3 
        private void btnAlterSOC_Click(object sender, RoutedEventArgs e)
        {

            string str = @"^[0-9]{1,3}$";
            if (!Regex.IsMatch(tbSOC.Text, str))    // if (string.IsNullOrEmpty(txtBoxBarcode.Text))
            {
                MessageBox.Show("请输入正确的 SOC 值！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ushort socVal = ushort.Parse(tbSOC.Text);

            if (socVal < 0 || socVal > 100)
            {
                MessageBox.Show("请输入正确的 SOC 值！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            isAlterSOC = false;
            BqProtocol.BqInstance.m_bIsStopCommunication = true;
            Thread.Sleep(200);
            BqProtocol.BqInstance.BQ_AlterSOC(BitConverter.GetBytes(socVal));
            isAlterSOC = true;
        }

        public void HandleRecvBmsInfoDataEvent(object sender, CustomRecvDataEventArgs e)
        {
            if (e.RecvMsg[0] != 0xCC && e.RecvMsg[1] != 0xA2)
            {
                return;
            }
            if (e.RecvMsg.Count < (e.RecvMsg[2] << 8 | e.RecvMsg[3]))
            {
                return;
            }
            BqProtocol.bReadBqBmsResp = true;
            int nBqByteIndex = 4;
            for (int n = 0; n < m_ListCellVoltage.Count; n++)
            {
                int nCellVol = 0;
                for (int m = m_ListCellVoltage[n].ByteCount - 1; m >= 0; m--)
                {
                    nCellVol = (nCellVol >> 8 | e.RecvMsg[nBqByteIndex + m]);
                }

                m_ListCellVoltage[n].StrValue = nCellVol.ToString();

                nBqByteIndex += m_ListCellVoltage[n].ByteCount;
            }

            for (int i = 0; i < m_ListBmsInfo.Count; i++)
            {
                int nBmsVal = 0;

                if (!m_ListBmsInfo[i].Description.Contains("状态"))
                {
                    for (int j = m_ListBmsInfo[i].ByteCount - 1; j >= 0; j--)
                    {
                        nBmsVal = (nBmsVal << 8 | e.RecvMsg[nBqByteIndex + j]);
                    }
                    if (m_ListBmsInfo[i].Description.Contains("温度"))
                    {
                        m_ListBmsInfo[i].StrValue = ((nBmsVal - 2731) / 10.0).ToString("F1");
                    }
                    else
                        m_ListBmsInfo[i].StrValue = (nBmsVal * decimal.Parse(m_ListBmsInfo[i].Scale)).ToString();
                }

                nBqByteIndex += m_ListBmsInfo[i].ByteCount;
            }

            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var item = m_ListBmsInfo.SingleOrDefault(p => p.Description == "SOC");
                if (null != item)
                {
                    tbCurrentSOC.Text = item.StrValue.ToString();
                }

                var utc = m_ListBmsInfo.SingleOrDefault(p => p.Description == "UTC");
                if (utc != null)
                {
                    uint dt = 0;
                    bool ret = UInt32.TryParse(utc.StrValue, out dt);
                    if (dt < 4294967295)
                    {
                        if (ret)
                        {
                            TimeSpan ts = new TimeSpan((long)(dt * Math.Pow(10, 7)));
                            tbDdUTCRTC.Text = (new DateTime(1970, 1, 1, 8, 0, 0) + ts).ToString("yyyy/MM/dd HH:mm:ss");
                            tbDdCurrentUTC.Text = dt.ToString();
                        }
                    }
                }
            }), null);
        }

        bool isFactoryReset = false;
        private void btnFactoryReset_Click(object sender, RoutedEventArgs e)
        {
            isFactoryReset = true;
            BqProtocol.BqInstance.m_bIsStopCommunication = true;
            Thread.Sleep(200);
            BqProtocol.BqInstance.BQ_FactoryReset();
        }

        public void SetSOCValue(string soc)
        {
            tbCurrentSOC.Text = soc;
        }
        //bool isDeepSleep = false;
        //private void btnShutdown_Click(object sender, RoutedEventArgs e)
        //{
        //    isDeepSleep = true;
        //    BqProtocol.BqInstance.m_bIsStopCommunication = true;
        //    Thread.Sleep(100);
        //    BqProtocol.BqInstance.BQ_Shutdown();
        //}

        //bool isShallowSleep = false;
        //private void btnSleep_Click(object sender, RoutedEventArgs e)
        //{
        //    isShallowSleep = true;
        //    BqProtocol.BqInstance.m_bIsStopCommunication = true;
        //    Thread.Sleep(100);
        //    BqProtocol.BqInstance.BQ_Sleep();
        //}

        //bool isReadBoot = false;
        //private void btnReadBoot_Click(object sender, RoutedEventArgs e)
        //{
        //    isReadBoot = false;
        //    BqProtocol.BqInstance.m_bIsStopCommunication = true;
        //    Thread.Sleep(100);
        //    BqProtocol.BqInstance.BQ_ReadBootInfo();
        //    isReadBoot = true;
        //}

        //public void HandleReadBqBootEvent(object sender, CustomRecvDataEventArgs e)
        //{
        //    try
        //    {
        //        if(isReadBoot)
        //        {
        //            if (e.RecvMsg[0] != 0xDA || e.RecvMsg.Count < 0x2B)
        //            {
        //                return;
        //            }

        //            int offset = 1;
        //            byte[] array = e.RecvMsg.ToArray();
        //            string projectName = System.Text.Encoding.ASCII.GetString(array, offset, 16);
        //            offset += 16;
        //            string hardwareVersion = System.Text.Encoding.ASCII.GetString(array, offset, 8);
        //            offset += 8;
        //            string bootVersion = System.Text.Encoding.ASCII.GetString(array, offset, 8);
        //            offset += 8;
        //            string appNum = System.Text.Encoding.ASCII.GetString(array, offset, 8);
        //            offset += 8;
        //            string programPhase = string.Empty;
        //            if(array[offset] == 1) { programPhase = "App"; }
        //            else if(array[offset] == 0) { programPhase = "Boot"; }
        //            offset++;
        //            string appExists = string.Empty;
        //            if (array[offset] == 0x01) { appExists = "存在"; }
        //            else if (array[offset] == 0xFF) { appExists = "不存在"; }

        //            List<string> list = new List<string>();
        //            list.Add(projectName.Substring(0, projectName.IndexOf('\0')));
        //            list.Add(hardwareVersion.Substring(0, hardwareVersion.IndexOf('\0')));
        //            list.Add(bootVersion.Substring(0, bootVersion.IndexOf('\0')));
        //            list.Add(appNum.Substring(0, appNum.IndexOf('\0')));
        //            list.Add(programPhase);
        //            list.Add(appExists);

        //            UpdateBqBootInfo(list);
        //            GetBootInfoEvent?.Invoke(this, new EventArgs<List<string>>(list));
        //            MessageBox.Show("读取Boot信息成功！", "读取Boot提示", MessageBoxButton.OK, MessageBoxImage.Information);
        //            isReadBoot = false;
        //        }
        //        BqProtocol.BqInstance.m_bIsStopCommunication = false;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Boot信息解析异常", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //}

        //private void UpdateBqBootInfo(List<string> list)
        //{
        //    if (list.Count == 6)
        //    {
        //        System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        //        {
        //            tbProjectName.Text = list[0].Trim();
        //            tbHardwareVersion.Text = list[1].Trim();
        //            tbBootVersion.Text = list[2].Trim();
        //            tbAppNum.Text = list[3].Trim();
        //            tbProgramPhase.Text = list[4].Trim();
        //            tbAppExist.Text = list[5].Trim();
        //        }));
        //    }
        //}

        bool isAdjustRTC = false;
        //private void btnAdjustRTC_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        DateTime dt = DateTime.Parse(tbAlterRTC.Text.Trim());
        //        if (null != dt)
        //        {
        //            if(dt.Year < 2000 || dt.Year > 2999)
        //            {
        //                MessageBox.Show("RTC的年份范围为：2000~2999");
        //            }
        //            else
        //            {
        //                isAdjustRTC = false;
        //                BqProtocol.bReadBqBmsResp = true;
        //                BqProtocol.BqInstance.m_bIsStopCommunication = true;
        //                Thread.Sleep(100);
        //                BqProtocol.BqInstance.AdjustRTC(dt);
        //                isAdjustRTC = true;
        //            }
        //        }
        //        else
        //        {
        //            MessageBox.Show("请检查RTC时间格式是否正确！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("请检查RTC时间格式是否正确！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //}

        public void HandleAdjustRTCEvent(object sender, CustomRecvDataEventArgs e)
        {
            if (isAdjustRTC)
            {
                BqProtocol.bReadBqBmsResp = true;
                //if (e.RecvMsg[0] == 0xB5)
                //{
                //    MessageBox.Show("校准RTC成功！", "校准提示", MessageBoxButton.OK, MessageBoxImage.Information);
                //    BqProtocol.BqInstance.BQ_ReadRTC();
                //}
                if (e.RecvMsg[0] == 0x10)
                {
                    MessageBox.Show("校准UTC成功！", "校准提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    DDProtocol.DdProtocol.DdInstance.Didi_ReadRTC();
                }
                else
                {
                    MessageBox.Show("校准UTC失败！", "校准提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                BqProtocol.BqInstance.m_bIsStopCommunication = false;
                isAdjustRTC = false;
            }

        }

        public void HandleReadBqRTCEvent(object sender, CustomRecvDataEventArgs e)
        {
            if (SelectCANWnd.m_H5Protocol == H5Protocol.BO_QIANG)
            {
                BqProtocol.bReadBqBmsResp = true;
                if (e.RecvMsg[1] == 0xA3 && e.RecvMsg[0] == 0xCC)
                {
                    int nRegister = ((e.RecvMsg[7] << 24) | (e.RecvMsg[6] << 16) | (e.RecvMsg[5] << 8) | e.RecvMsg[4]);
                    TimeSpan ts = new TimeSpan((long)(nRegister * Math.Pow(10, 7)));
                    tbDdUTCRTC.Text = (new DateTime(1970, 1, 1, 8, 0, 0) + ts).ToString("yyyy/MM/dd HH:mm:ss");
                    tbDdCurrentUTC.Text = nRegister.ToString();
                }
            }
            else
            {
                DDProtocol.DdProtocol.bReadDdBmsResp = true;
                if (e.RecvMsg[0] == 0x03 && e.RecvMsg[1] == 0x04)
                {
                    int nRegister = ((e.RecvMsg[2] << 24) | (e.RecvMsg[3] << 16) | (e.RecvMsg[4] << 8) | e.RecvMsg[5]);
                    tbDdCurrentUTC.Text = nRegister.ToString();
                    TimeSpan ts = new TimeSpan((long)(nRegister * Math.Pow(10, 7)));
                    tbDdUTCRTC.Text = (new DateTime(1970, 1, 1, 8, 0, 0) + ts).ToString("yyyy/MM/dd HH:mm:ss");
                };
            }
        }
        public void SetUTCValue(string val)
        {;
            tbDdCurrentUTC.Text = val;
            long nVal = long.Parse(val);
            TimeSpan ts = new TimeSpan(((long)(nVal * Math.Pow(10, 7))));
            tbDdUTCRTC.Text = (new DateTime(1970, 1, 1, 8, 0, 0) + ts).ToString("yyyy/MM/dd HH:mm:ss");
        }
        bool isDdPowerOn = false;
        bool isDdPowerOff = false;
        private void btnDDPowerOff_Click(object sender, RoutedEventArgs e)
        {
            isDdPowerOff = false;
            DdProtocol.DdInstance.DD_PowerOff();
            isDdPowerOff = true;
        }

        private void btnDDPowerOn_Click(object sender, RoutedEventArgs e)
        {
            isDdPowerOn = false;
            DdProtocol.DdInstance.DD_PowerOn();
            isDdPowerOn = true;
        }

        private void btnDdAdjustUTC_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.m_statusBarInfo.IsOnline)
            {
                try
                {
                    uint dt = 0;
                    bool ret = UInt32.TryParse(tbDdAlterUTC.Text.Trim(), out dt);
                    if (dt >= 4294967295)
                    {
                        MessageBox.Show("UTC值的范围0~4294967295，请检查UTC输入！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        if (ret)
                        {
                            isAdjustRTC = false;
                            BqProtocol.BqInstance.m_bIsStopCommunication = true;
                            Thread.Sleep(100);
                            DDProtocol.DdProtocol.DdInstance.AdjustDidiRTC(dt);
                            isAdjustRTC = true;
                        }
                        else
                        {
                            MessageBox.Show("请检查修改UTC值是否正确！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("请检查修改UTC值是否正确！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("系统未连接，请连接后再进行操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnCalRTC_Click(object sender, RoutedEventArgs e)
        {
            uint dt = 0;
            bool ret = UInt32.TryParse(tbDdAlterUTC.Text.Trim(), out dt);
            if (dt >= 4294967295)
            {
                MessageBox.Show("UTC值的范围0~4294967294，请检查UTC输入！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                if (ret)
                {
                    TimeSpan ts = new TimeSpan((long)(dt * Math.Pow(10, 7)));
                    tbDdAlterRTC.Text = (new DateTime(1970, 1, 1, 8, 0, 0) + ts).ToString("yyyy/MM/dd HH:mm:ss");
                }
                else
                {
                    MessageBox.Show("请检查修改UTC值是否正确！", "计算时间提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void btnCalUTC_Click(object sender, RoutedEventArgs e)
        {
            DateTime dt;
            bool ret = DateTime.TryParse(tbDdAlterRTC.Text.Trim(), out dt);
            if (ret)
            {
                TimeSpan ts = dt - new DateTime(1970, 1, 1, 8, 0, 0);
                long ticks = (long)(ts.Ticks / Math.Pow(10, 7));
                if (ticks >= 4294967295)
                {
                    MessageBox.Show("输入时间大于最大UTC值，请输入正确的时间！", "计算UTC提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    tbDdAlterUTC.Text = ticks.ToString();
                }
            }
            else
            {
                MessageBox.Show("请检查RTC时间格式是否正确！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        bool isWrite = false;
        private void btnWrite_Click(object sender, RoutedEventArgs e)
        {
            byte[] registerAddrBytes;
            byte registerNum;
            byte[] dataBytes;
            string regexStr = @"^[A-Fa-f0-9]+$";
            string registerAddrStr = tbRegisterAddr.Text.Trim();
            if (!string.IsNullOrEmpty(registerAddrStr))
            {
                registerAddrStr = registerAddrStr.Replace(" ", "");
                if (Regex.IsMatch(registerAddrStr, regexStr))
                {
                    if ((registerAddrStr.Length % 2) != 0)
                        registerAddrStr += "0";
                    registerAddrBytes = new byte[registerAddrStr.Length / 2];
                    for (int i = 0; i < registerAddrBytes.Length; i++)
                        registerAddrBytes[i] = Convert.ToByte(registerAddrStr.Substring(i * 2, 2), 16);
                }
                else
                {
                    MessageBox.Show("输入的寄存器地址包含非十六进制数字，请检查！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
            else
            {
                MessageBox.Show("请输入寄存器地址！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            string registerNumStr = tbRegisterNum.Text.Trim();
            if (!string.IsNullOrEmpty(registerNumStr))
            {
                uint num = 0;
                if (uint.TryParse(registerNumStr, out num))
                {
                    if (num > 255 || num <= 0)
                    {
                        MessageBox.Show("请输入正确的寄存器个数！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    registerNum = Convert.ToByte(num);
                }
                else
                {
                    MessageBox.Show("请输入正确的寄存器个数！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
            else
            {
                MessageBox.Show("请输入寄存器个数！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string dataStr = tbData.Text.Trim();
            if (!string.IsNullOrEmpty(dataStr))
            {
                dataStr = dataStr.Replace(" ", "");
                if (Regex.IsMatch(dataStr, regexStr))
                {
                    if ((dataStr.Length % 2) != 0)
                        dataStr += "0";
                    dataBytes = new byte[dataStr.Length / 2];
                    for (int i = 0; i < dataBytes.Length; i++)
                        dataBytes[i] = Convert.ToByte(dataStr.Substring(i * 2, 2), 16);

                    if (dataBytes.Length != 2 * Convert.ToInt16(registerNum))
                    {
                        MessageBox.Show("输入的寄存器个数和输入的数据不匹配，请检查！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("输入的数据包含非十六进制数字，请检查！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

            }
            else
            {
                MessageBox.Show("请输入要发送的数据！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            isWrite = false;
            DdProtocol.DdInstance.DD_WriteRegister(registerAddrBytes, registerNum, dataBytes);
            isWrite = true;
        }

        public void HandleWriteRegisterEvent(object sender, CustomRecvDataEventArgs e)
        {
            if (isWrite)
            {
                if (e.RecvMsg[0] == 0x10)
                {
                    MessageBox.Show("写寄存器成功！", "写寄存器提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                isWrite = false;
            }
        }

        bool isRead = false;
        private void btnRead_Click(object sender, RoutedEventArgs e)
        {
            byte[] registerAddrBytes;
            byte registerNum;
            string regexStr = @"^[A-Fa-f0-9]+$";
            string registerAddrStr = tbRegisterAddr.Text.Trim();
            if (!string.IsNullOrEmpty(registerAddrStr))
            {
                registerAddrStr = registerAddrStr.Replace(" ", "");
                if (Regex.IsMatch(registerAddrStr, regexStr))
                {
                    if ((registerAddrStr.Length % 2) != 0)
                        registerAddrStr += "0";
                    registerAddrBytes = new byte[registerAddrStr.Length / 2];
                    for (int i = 0; i < registerAddrBytes.Length; i++)
                        registerAddrBytes[i] = Convert.ToByte(registerAddrStr.Substring(i * 2, 2), 16);
                }
                else
                {
                    MessageBox.Show("输入的寄存器地址包含非十六进制数字，请检查！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
            else
            {
                MessageBox.Show("请输入寄存器地址！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            string registerNumStr = tbRegisterNum.Text.Trim();
            if (!string.IsNullOrEmpty(registerNumStr))
            {
                uint num = 0;
                if (uint.TryParse(registerNumStr, out num))
                {
                    if (num > 255 || num <= 0)
                    {
                        MessageBox.Show("请输入正确的寄存器个数！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    registerNum = Convert.ToByte(num);
                }
                else
                {
                    MessageBox.Show("请输入正确的寄存器个数！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
            else
            {
                MessageBox.Show("请输入寄存器个数！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            isRead = false;
            DdProtocol.DdInstance.m_bIsStopCommunication = true;
            DdProtocol.DdInstance.DD_ReadRegister(registerAddrBytes, registerNum);
            isRead = true;
        }
        public void HandleReadRegisterEvent(object sender, CustomRecvDataEventArgs e)
        {
            try
            {
                if (isRead)
                {
                    if (e.RecvMsg[0] == 0x03)
                    {
                        string registerNumStr = tbRegisterNum.Text.Trim();
                        int registerNum = int.Parse(registerNumStr);
                        byte[] bytes = new byte[registerNum * 2 + 4];
                        if (e.RecvMsg.Count() >= bytes.Length)
                            Buffer.BlockCopy(e.RecvMsg.ToArray(), 0, bytes, 0, bytes.Length);
                        else
                            Buffer.BlockCopy(e.RecvMsg.ToArray(), 0, bytes, 0, e.RecvMsg.Count());
                        tbData.Text = BitConverter.ToString(bytes);
                        MessageBox.Show("读寄存器成功！", "读寄存器提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        DdProtocol.DdInstance.m_bIsStopCommunication = false;
                    }
                    isRead = false;

                }
            }
            catch (Exception ex)
            {
                isRead = false;
            }
        }
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            tbData.Text = string.Empty;
        }

        bool isTestMode = false;
        private void btnEnterTestMode_Click(object sender, RoutedEventArgs e)
        {
            isTestMode = false;
            BqProtocol.BqInstance.m_bIsStopCommunication = true;
            Thread.Sleep(200);
            BqProtocol.BqInstance.BQ_EnterTestMode();
            isTestMode = true;
        }
        bool isExitTestMode = false;
        private void btnExitTestMode_Click(object sender, RoutedEventArgs e)
        {
            isExitTestMode = false;
            BqProtocol.BqInstance.m_bIsStopCommunication = true;
            Thread.Sleep(200);
            BqProtocol.BqInstance.BQ_ExitTestMode();
            isExitTestMode = true;
        }

        int[] EepromData = new int[40] { 0, 64, 0, 16, 128, 0, 16, 0, 1, 3, 1, 7, 7, 5, 8, 5, 5, 11, 4, 2, 12, 0, 1, 3, 3700, 3450, 2200, 2800, 3500, 2000, 1500, 3900, 55, 50, 0, 5, 60, 55, -20, -15 };
        long loopNum = 0;
        long OKNum = 0;
        long NGNum = 0;
        int timerCount = 0;
        System.Timers.Timer timer;
        bool isTest = false;
        string filePath = string.Empty;
        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            loopNum = 0;
            labLoopCount.Content = loopNum.ToString();
            OKNum = 0;
            labOKCount.Content = OKNum.ToString();
            NGNum = 0;
            labNGCount.Content = NGNum.ToString();
            timerCount = 0;
            btnTest.Content = "测试中";
            btnStop.IsEnabled = true;
            btnTest.IsEnabled = false;
            BqProtocol.BqInstance.m_bIsTest = true;
            isTest = true;
            InitTempTable();
            filePath = System.AppDomain.CurrentDomain.BaseDirectory + @"Eeprom\" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
            //filePath = System.AppDomain.CurrentDomain.BaseDirectory + @"BoqiangV3\Eeprom\" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";//程序合并时使用
            timer = new System.Timers.Timer(1000);
            timer.Elapsed += OnTimer;
            timer.AutoReset = true;
            timer.Enabled = true;

        }
        private void OnTimer(object sender, EventArgs e)
        {
            timerCount++;
            if (timerCount == 13)//13秒发送浅休眠指令
            {
                BqProtocol.bReadBqBmsResp = true;
                BqProtocol.BqInstance.BQ_Sleep();
            }
            else if (timerCount == 28)//28秒发送读boot指令进行唤醒
            {
                BqProtocol.BqInstance.BQ_ReadBootInfo();
            }
            else if (timerCount == 30)//30秒发送读Eeprom指令
            {
                BqProtocol.bReadBqBmsResp = true;
                BqProtocol.BqInstance.ReadEepromData();
                timerCount = 0;
                loopNum++;
                Dispatcher.Invoke(new Action(() =>
                {
                    labLoopCount.Content = loopNum.ToString();
                }));
            }
        }
        int nRomStartIndex = 1;
        public void HandleRecvEepromDataEvent(object sender, CustomRecvDataEventArgs e)
        {
            if (isTest)
            {
                if (e.RecvMsg[0] == 0xA3 && e.RecvMsg.Count >= 0x1A)
                {
                    nRomStartIndex = 1;
                    List<byte> listRecv = e.RecvMsg;
                    List<int> listParseVal = new List<int>();
                    int cbEnpch = SetBitStatus(listRecv[nRomStartIndex], 7); listParseVal.Add(cbEnpch);
                    int cbEnmos = SetBitStatus(listRecv[nRomStartIndex], 6); listParseVal.Add(cbEnmos);
                    int cbOcpm = SetBitStatus(listRecv[nRomStartIndex], 5); listParseVal.Add(cbOcpm);
                    int cbBal = SetBitStatus(listRecv[nRomStartIndex], 4); listParseVal.Add(cbBal);
                    int cbE0VB = SetBitStatus(listRecv[nRomStartIndex + 1], 7); listParseVal.Add(cbE0VB);
                    int cbUV_OP = SetBitStatus(listRecv[nRomStartIndex + 1], 5); listParseVal.Add(cbUV_OP);
                    int cbDIS_PF = SetBitStatus(listRecv[nRomStartIndex + 1], 4); listParseVal.Add(cbDIS_PF);
                    int cbOCRA = SetBitStatus(listRecv[nRomStartIndex + 1], 1); listParseVal.Add(cbOCRA);
                    int cbEUVR = SetBitStatus(listRecv[nRomStartIndex + 1], 0); listParseVal.Add(cbEUVR);

                    int cbCTL = SetComboBoxVal(listRecv[nRomStartIndex + 1], 0x0C, 2); listParseVal.Add(cbCTL);
                    int cbLDRT = SetComboBoxVal(listRecv[nRomStartIndex + 2], 0x0C, 2); listParseVal.Add(cbLDRT);
                    int cbOVT_2_74 = SetComboBoxVal(listRecv[nRomStartIndex + 2], 0xF0, 4); listParseVal.Add(cbOVT_2_74);
                    int cbUVT_4_74 = SetComboBoxVal(listRecv[nRomStartIndex + 4], 0xF0, 4); listParseVal.Add(cbUVT_4_74);
                    int cbOCD1V_C_74 = SetComboBoxVal(listRecv[nRomStartIndex + 12], 0xF0, 4); listParseVal.Add(cbOCD1V_C_74);
                    int cbOCD1T_C_30 = SetComboBoxVal(listRecv[nRomStartIndex + 12], 0x0F, 0); listParseVal.Add(cbOCD1T_C_30);
                    int cbOCD2V_D_74 = SetComboBoxVal(listRecv[nRomStartIndex + 13], 0xF0, 4); listParseVal.Add(cbOCD2V_D_74);
                    int cbOCD2T_D_30 = SetComboBoxVal(listRecv[nRomStartIndex + 13], 0x0F, 0); listParseVal.Add(cbOCD2T_D_30);
                    int cbSCV74 = SetComboBoxVal(listRecv[nRomStartIndex + 14], 0xF0, 4); listParseVal.Add(cbSCV74);
                    int cbSCT30 = SetComboBoxVal(listRecv[nRomStartIndex + 14], 0x0F, 0); listParseVal.Add(cbSCT30);
                    int cbOCCV74 = SetComboBoxVal(listRecv[nRomStartIndex + 15], 0xF0, 4); listParseVal.Add(cbOCCV74);
                    int cbOCCT30 = SetComboBoxVal(listRecv[nRomStartIndex + 15], 0x0F, 0); listParseVal.Add(cbOCCT30);
                    int cbCHS76 = SetComboBoxVal(listRecv[nRomStartIndex + 16], 0xC0, 6); listParseVal.Add(cbCHS76);
                    int cbMOST54 = SetComboBoxVal(listRecv[nRomStartIndex + 16], 0x30, 4); listParseVal.Add(cbMOST54);
                    int cbOCRT32 = SetComboBoxVal(listRecv[nRomStartIndex + 16], 0x0C, 2); listParseVal.Add(cbOCRT32);

                    int tbOV_23 = ((((listRecv[nRomStartIndex + 2] & 0x03) << 8) | listRecv[nRomStartIndex + 3]) * 5);
                    listParseVal.Add(tbOV_23);
                    int tbOVR_45 = ((((listRecv[nRomStartIndex + 4] & 0x03) << 8) | listRecv[nRomStartIndex + 5]) * 5);
                    listParseVal.Add(tbOVR_45);
                    int tbUV_6 = (listRecv[nRomStartIndex + 6] * 20);
                    listParseVal.Add(tbUV_6);
                    int tbUVR_7 = (listRecv[nRomStartIndex + 7] * 20);
                    listParseVal.Add(tbUVR_7);
                    int tbBALV_8 = (listRecv[nRomStartIndex + 8] * 20);
                    listParseVal.Add(tbBALV_8);
                    int tbPREV_9 = (listRecv[nRomStartIndex + 9] * 20);
                    listParseVal.Add(tbPREV_9);
                    int tbL0V_A = (listRecv[nRomStartIndex + 10] * 20);
                    listParseVal.Add(tbL0V_A);
                    int tbPFV_B = (listRecv[nRomStartIndex + 11] * 20);
                    listParseVal.Add(tbPFV_B);
                    double nRref = GetRrefVal(listRecv[29]);

                    int nRt1 = GetHighTempRt1(listRecv[nRomStartIndex + 17], nRref);
                    int tbOTC_11 = GetTempFromTable(nRt1);
                    listParseVal.Add(tbOTC_11);
                    nRt1 = GetHighTempRt1(listRecv[nRomStartIndex + 18], nRref);
                    int tbOTCR_12 = GetTempFromTable(nRt1);
                    listParseVal.Add(tbOTCR_12);
                    nRt1 = GetLowTempRt1(listRecv[nRomStartIndex + 19], nRref);
                    int tbUTC_13 = GetTempFromTable(nRt1);
                    listParseVal.Add(tbUTC_13);
                    nRt1 = GetLowTempRt1(listRecv[nRomStartIndex + 20], nRref);
                    int tbUTCR_14 = GetTempFromTable(nRt1);
                    listParseVal.Add(tbUTCR_14);
                    nRt1 = GetHighTempRt1(listRecv[nRomStartIndex + 21], nRref);
                    int tbOTD_15 = GetTempFromTable(nRt1);
                    listParseVal.Add(tbOTD_15);
                    nRt1 = GetHighTempRt1(listRecv[nRomStartIndex + 22], nRref);
                    int tbOTDR_16 = GetTempFromTable(nRt1);
                    listParseVal.Add(tbOTDR_16);
                    nRt1 = GetLowTempRt1(listRecv[nRomStartIndex + 23], nRref);
                    int tbUTD_17 = GetTempFromTable(nRt1);
                    listParseVal.Add(tbUTD_17);
                    nRt1 = GetLowTempRt1(listRecv[nRomStartIndex + 24], nRref);
                    int tbUTDR_18 = GetTempFromTable(nRt1);
                    listParseVal.Add(tbUTDR_18);

                    if (Enumerable.SequenceEqual(EepromData, listParseVal.ToArray()))
                    {
                        OKNum++;
                        labOKCount.Content = OKNum.ToString();
                    }
                    else
                    {
                        NGNum++;
                        labNGCount.Content = NGNum.ToString();

                        CSVFileHelper.SaveEepromData(filePath, true, EepromData.ToList());
                        CSVFileHelper.SaveEepromData(filePath, false, listParseVal);
                    }
                }
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            btnTest.IsEnabled = true;
            btnStop.IsEnabled = false;
            btnTest.Content = "开始测试";
            isTest = false;
            if (timer != null)
            {
                timer.Stop();
                timer.Close();
            }
            BqProtocol.BqInstance.m_bIsTest = false;
        }
        Dictionary<int, int> DicTempTable = new Dictionary<int, int>();
        private void InitTempTable()
        {
            FileStream fs = null;
            StreamReader sr = null;
            string strFilePath = System.Windows.Forms.Application.StartupPath + "\\ProtocolFiles\\Temp_103AT.txt";

            try
            {
                System.Text.Encoding encoding = System.Text.Encoding.ASCII;

                fs = new FileStream(strFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);

                sr = new StreamReader(fs, encoding);

                //记录每次读取的一行记录
                string strLine = "";

                int nIndex = 0;
                //逐行读取数据
                while ((strLine = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(strLine))
                    {
                        continue;
                    }

                    string[] arrVal = strLine.Split(',');
                    DicTempTable.Add(int.Parse(arrVal[0]), int.Parse(arrVal[1]));
                }
            }
            catch (Exception ex)
            {

            }
        }
        private int SetBitStatus(byte byteVal, int bitIndex)
        {
            return (byteVal & (1 << bitIndex));
        }
        private int SetComboBoxVal(byte byteVal, byte twoBitVal, byte rightShift)
        {
            return ((byteVal & twoBitVal) >> rightShift);
        }
        private double GetRrefVal(byte TRValue)
        {
            double nRref = 6.8 + 0.05 * TRValue;
            return nRref;
        }
        private int GetHighTempRt1(byte byteVal, double dRref)
        {
            double nTemp = 0;

            nTemp = byteVal * dRref / (512 - byteVal);

            return (int)(nTemp * 1000);
        }
        private int GetLowTempRt1(byte byteVal, double dRref)
        {
            double nTemp = 0;

            nTemp = (byteVal + 256) * dRref / (256 - byteVal);

            return (int)(nTemp * 1000);

        }
        private int GetTempFromTable(int nRt1)
        {
            int nTemp = -100;

            int nAbs = Math.Abs((int)(DicTempTable[0] - nRt1));

            int low = -40;
            int high = 85;

            if (nRt1 < DicTempTable[high])
            {
                nTemp = high;
            }
            else if (nRt1 > DicTempTable[low])
            {
                nTemp = low;
            }
            else
            {
                while (low <= high)
                {
                    int middle = (low + high) >> 1;
                    int midNext = middle + 1;

                    if (nRt1 == middle)
                    {
                        nTemp = middle;
                        break;
                    }
                    else if (nRt1 < DicTempTable[middle] && nRt1 > DicTempTable[midNext])
                    {
                        int nAbs1 = Math.Abs((int)(DicTempTable[middle] - nRt1));
                        int nAbs2 = Math.Abs((int)(nRt1 - DicTempTable[middle + 1]));
                        if (nAbs1 > nAbs2)
                        {
                            nTemp = middle + 1;
                        }
                        else
                        {
                            nTemp = middle;
                        }
                        break;
                    }
                    else if (nRt1 < DicTempTable[middle])
                    {
                        low = middle + 1;
                    }
                    else if (nRt1 > DicTempTable[middle])
                    {
                        high = middle - 1;
                    }
                    else if (nRt1 == DicTempTable[middle])
                    {
                        nTemp = middle;
                        break;
                    }
                }
            }

            return nTemp;
        }

        //bool isReadUID = false;
        //private void btnReadUID_Click(object sender, RoutedEventArgs e)
        //{
        //    isReadUID = false;
        //    BqProtocol.BqInstance.m_bIsStopCommunication = true;
        //    Thread.Sleep(100);
        //    BqProtocol.BqInstance.BQ_ReadUID();
        //    isReadUID = true;
        //}

        //public void HandleReadUIDEvent(object sender, CustomRecvDataEventArgs e)
        //{
        //    try
        //    {
        //        if (isReadUID)
        //        {
        //            if (e.RecvMsg[0] != 0xCC || e.RecvMsg[1] != 0xA1 || e.RecvMsg.Count < (e.RecvMsg[2] << 8 | e.RecvMsg[3]))
        //            {
        //                return;
        //            }

        //            byte[] array = new byte[16];
        //            Buffer.BlockCopy(e.RecvMsg.ToArray(), 4, array, 0, array.Length);
        //            tbUID.Text = CSVFileHelper.ToHexStrFromByte(array,true);
        //            GetUIDEvent?.Invoke(this, new EventArgs<string>(tbUID.Text));
        //            if(isMsgVisible)
        //            {
        //                MessageBox.Show("读取UID成功！", "读取UID提示", MessageBoxButton.OK, MessageBoxImage.Information);
        //                isMsgVisible = false;
        //            }
        //            isReadUID = false;
        //        }
        //        BqProtocol.BqInstance.m_bIsStopCommunication = false;
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

        //public event EventHandler<EventArgs<string>> GetUIDEvent;
        //bool isMsgVisible = true;
        //public void RequireReadUID()
        //{
        //    isMsgVisible = false;
        //    btnReadUID_Click(null,null);
        //}
        //public event EventHandler<EventArgs<List<string>>> GetBootInfoEvent;
        //public void RequireReadBootInfo()
        //{
        //    btnReadBoot_Click(null, null);
        //}

        long sleepNum = 0;
        long readNum = 0;
        long recvNum = 0;
        int sleepTimerCount = 0;
        uint sleepInterval = 0;
        System.Timers.Timer sleepTimer;
        bool isSleepTest = false;
        bool isAwake = false;
        string testMode = "休眠模式";
        private void btnSleepTest_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.m_statusBarInfo.IsOnline)
            {
                sleepInterval = 0;
                string intervalStr = tbInterval.Text.Trim();
                if (!string.IsNullOrEmpty(intervalStr))
                {
                    if (uint.TryParse(intervalStr, out sleepInterval))
                    {
                        tbInterval.IsReadOnly = true;
                    }
                    else
                    {
                        MessageBox.Show("请输入正确的休眠时间！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("请输入休眠时间！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                sleepNum = 0;
                labSleepCount.Content = sleepNum.ToString();
                readNum = 0;
                labReadCount.Content = readNum.ToString();
                recvNum = 0;
                labRecvCount.Content = readNum.ToString();
                sleepTimerCount = 0;
                isSend = false;
                failNum = 0;
                isReadDd = false;
                btnSleepTest.Content = "测试中";
                btnSleepStop.IsEnabled = true;
                btnSleepTest.IsEnabled = false;
                cmbTestMode.IsEnabled = false;
                testMode = cmbTestMode.SelectedIndex == 0 ? "休眠模式" : "过放模式";
                BqProtocol.BqInstance.m_bIsTest = true;
                isSleepTest = true;
                sleepTimer = new System.Timers.Timer(1000);
                sleepTimer.Elapsed += OnSleepTimer;
                sleepTimer.AutoReset = true;
                sleepTimer.Enabled = true;
            }
            else
            {
                MessageBox.Show("系统未连接，请连接后再进行操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        bool isSend = false;
        int failNum = 0;
        bool isReadDd = false;
        private void OnSleepTimer(object sender, EventArgs e)
        {
            sleepTimerCount++;
            if (sleepTimerCount == 1)//进行唤醒
            {
                isAwake = true;
                isSend = true;
                isReadDd = true;
                DdProtocol.DdInstance.ReadDdBmsInfo();
                readNum++;
            }
            else if (sleepTimerCount == 2)//读指令
            {
                isAwake = false;
                if (isSend) failNum++;
                isReadDd = true;
                DdProtocol.DdInstance.ReadDdBmsInfo();
                readNum++;
            }
            else if (sleepTimerCount == 3)//浅休眠指令
            {
                if (isSend) failNum++;
                BqProtocol.bReadBqBmsResp = true;
                if (testMode == "休眠模式")
                    BqProtocol.BqInstance.BQ_Sleep();
                else
                    BqProtocol.BqInstance.BQ_OverDischarge();
                sleepNum++;
            }
            else if (sleepTimerCount == sleepInterval + 3)
            {
                sleepTimerCount = 0;
                if (isSend) failNum++;
                if (failNum == 3)
                {
                    if (sleepTimer != null)
                    {
                        sleepTimer.Stop();
                        sleepTimer.Close();
                    }
                    Dispatcher.Invoke(new Action(() =>
                    {
                        MessageBox.Show("通讯连接失败，请检查！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }));
                }
                failNum = 0;
            }

            Dispatcher.Invoke(new Action(() =>
            {
                labSleepCount.Content = sleepNum.ToString();
                labReadCount.Content = readNum.ToString();
            }));
        }
        private void btnSleepStop_Click(object sender, RoutedEventArgs e)
        {
            btnSleepTest.IsEnabled = true;
            btnSleepStop.IsEnabled = false;
            btnSleepTest.Content = "开始测试";
            isSleepTest = false;
            tbInterval.IsReadOnly = false;
            cmbTestMode.IsEnabled = true;
            if (sleepTimer != null)
            {
                sleepTimer.Stop();
                sleepTimer.Close();
            }
            BqProtocol.BqInstance.m_bIsTest = false;
        }

        public void HandleRecvDdBmsInfoDataEvent(object sender, CustomRecvDataEventArgs e)
        {
            if (isSleepTest && isReadDd)
            {
                //if (e.RecvMsg.Count < 0xCA || e.RecvMsg[1] != 0xC8)
                //{
                //    return;
                //}
                isReadDd = false;
                isSend = false;
                if (isAwake)
                {
                    isAwake = false;
                }
                else
                {
                    recvNum++;
                    Dispatcher.Invoke(new Action(() =>
                    {
                        labRecvCount.Content = recvNum.ToString();
                    }));
                }
            }
        }

        public void HandleSleepEvent(object sender, CustomRecvDataEventArgs e)
        {
            if (isSleepTest)
            {
                if (e.RecvMsg[1] == 0xBB || e.RecvMsg[1] == 0xBF)
                {
                    isSend = false;
                }
            }
        }
        bool isExitChargeMos = false;
        private void btnExitChargeMos_Click(object sender, RoutedEventArgs e)
        {
            isExitChargeMos = true;
            BqProtocol.BqInstance.m_bIsStopCommunication = true;
            Thread.Sleep(200);
            BqProtocol.BqInstance.BQ_ExitChargeMos();
        }
        bool isCloseChargeMos = false;
        private void btnCloseChargeMos_Click(object sender, RoutedEventArgs e)
        {
            isCloseChargeMos = true;
            BqProtocol.BqInstance.m_bIsStopCommunication = true;
            Thread.Sleep(200);
            BqProtocol.BqInstance.BQ_CloseChargeMos();
        }
        bool isOpenChargeMos = false;
        private void btnOpenChargeMos_Click(object sender, RoutedEventArgs e)
        {
            isOpenChargeMos = true;
            BqProtocol.BqInstance.m_bIsStopCommunication = true;
            Thread.Sleep(200);
            BqProtocol.BqInstance.BQ_OpenChargeMos();
        }
        bool isExitDischargeMos = false;
        private void btnExitDischargeMos_Click(object sender, RoutedEventArgs e)
        {
            isExitDischargeMos = true;
            BqProtocol.BqInstance.m_bIsStopCommunication = true;
            Thread.Sleep(200);
            BqProtocol.BqInstance.BQ_ExitDischargeMos();
        }
        bool isCloseDischargeMos = false;
        private void btnCloseDischargeMos_Click(object sender, RoutedEventArgs e)
        {
            isCloseDischargeMos = true;
            BqProtocol.BqInstance.m_bIsStopCommunication = true;
            Thread.Sleep(200);
            BqProtocol.BqInstance.BQ_CloseDischargeMos();
        }
        bool isOpenDischargeMos = false;
        private void btnOpenDischargeMos_Click(object sender, RoutedEventArgs e)
        {
            isOpenDischargeMos = true;
            BqProtocol.BqInstance.m_bIsStopCommunication = true;
            Thread.Sleep(200);
            BqProtocol.BqInstance.BQ_OpenDischargeMos();
        }

        private void btnSettingBatteryStatus_Click(object sender, RoutedEventArgs e)
        {
            byte status = Convert.ToByte(cmbBatteryStatus.SelectedIndex);
            
            BqProtocol.BqInstance.m_bIsStopCommunication = true;
            Thread.Sleep(200);
            DDProtocol.DdProtocol.DdInstance.DD_SettingBatteryStatus(status);
        }

        public void HandleRecvSettingBatteryStatusEvent(object sender, CustomRecvDataEventArgs e)
        {
            BqProtocol.BqInstance.m_bIsStopCommunication = false ;
            MessageBox.Show("设置电池使用状态成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        bool isEnterProductionMode = false;
        private void btnEnterProductionMode_Click(object sender, RoutedEventArgs e)
        {
            uint days = 0;
            if(uint.TryParse(tbCountDown.Text.Trim(),out days))
            {
                BqProtocol.BqInstance.m_bIsStopCommunication = true;
                Thread.Sleep(200);
                isEnterProductionMode = true;
                BqProtocol.BqInstance.BQ_EnterProductionMode((byte)days);
            }
            else
            {
                MessageBox.Show("请输入正确的生产模式倒计时天数！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        bool isExitProductionMode = false;
        private void btnExitProductionMode_Click(object sender, RoutedEventArgs e)
        {
            BqProtocol.BqInstance.m_bIsStopCommunication = true;
            Thread.Sleep(200);
            isExitProductionMode = true;
            BqProtocol.BqInstance.BQ_ExitProductionMode();
        }
    }
}
