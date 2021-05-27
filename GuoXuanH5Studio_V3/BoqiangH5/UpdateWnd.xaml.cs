using BoqiangH5Repository;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace BoqiangH5
{
    /// <summary>
    /// UpdateWnd.xaml 的交互逻辑
    /// </summary>
    public partial class UserCtrlUpdate : UserControl
    {
        private List<UpdateInformation> InfoList = new List<UpdateInformation>();

        string configPath = AppDomain.CurrentDomain.BaseDirectory + "ProtocolFiles\\bq_config_info.xml";
        static object lockObj = new object();
        private void LockAddToList(UpdateInformation info)
        {
            lock(lockObj)
            {
                InfoList.Add(info);
            }
        }
        private void LockAddToList(List<UpdateInformation> list)
        {
            lock (lockObj)
            {
                InfoList.AddRange(list);
            }
        }
        private void LockRemoveList(int index,int count)
        {
            lock (lockObj)
            {
                InfoList.RemoveRange(0,count);
            }
        }
        private void LockClearList()
        {
            lock (lockObj)
            {
                InfoList.Clear();
            }
        }
        public UserCtrlUpdate()
        {
            InitializeComponent();
        }
        int retryNum = 1;
        int skipInterval = 0;
        UpdateViewModel model;
        uint canID = 0x1CEB0300;
        public void SetCanID(uint id)
        {
            canID = id;
        }
        private void ucUpdateWnd_Loaded(object sender, RoutedEventArgs e)
        {
            //dgDataInfo.ItemsSource = InfoList;
            model = new UpdateViewModel();
            this.DataContext = model;
            tbID.Text = "0x" + canID.ToString("X");
            timer.Tick += new EventHandler(OnTimer);
            timeOutTimer.Elapsed +=OnTimeOutTimer;

            List<string> list = XmlHelper.LoadUpdateInfo(configPath);
            if(list != null)
            {
                tbCompany.Text = list[0];
                tbHardware.Text = list[1];
                tbErrorNum.Text = list[2];
                tbTimeOut.Text = list[3];
                tbCustomerCode.Text = list[4];
                tbSoftware.Text = list[5];
                tbSkipInterval.Text = list[6];
                tbResultInterval.Text = list[7];
            }
            rtbInfo.Document.Blocks.Clear();
        }
        private void btnClearInfo_Click(object sender, RoutedEventArgs e)
        {
            rtbInfo.Document.Blocks.Clear();
        }
        private void ShowStatusInformation(string msg,bool flag)
        {
            Dispatcher.BeginInvoke( new Action(() =>
            {
                Paragraph paragraph = new Paragraph(new Run(msg));
                if (flag)
                {
                    paragraph.FontSize = 12;
                    paragraph.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    paragraph.FontSize = 14;
                    paragraph.Foreground = new SolidColorBrush(Colors.Red);
                }
                rtbInfo.Document.Blocks.Add(paragraph);
                rtbInfo.ScrollToEnd();
            }));
        }
        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "源文件 | *.bin;*.hex;*.ehex";

            bool? result = ofd.ShowDialog();
            if (result != true)
                return;

            tbFilePath.Text = ofd.FileName;
            ShowStatusInformation("加载源文件成功！", true);
        }

        ushort firmwareNo = 0;
        ushort firmwareCount = 0;
        ushort remainder = 0;
        bool isCancelDownload = false;
        bool isDownload = false;
        private void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(btnDownload.Content.ToString() == "下载")
                {
                    string filePath = tbFilePath.Text.Trim();
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        if (!File.Exists(filePath))
                        {
                            MessageBox.Show(string.Format("文件 {0} 不存在，请检查！", filePath), "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                            return ;
                        }
                    }
                    else
                    {
                        MessageBox.Show("加载文件路径不能为空！", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                        return ;
                    }
                    if (!CheckParam())
                    {
                        return;
                    }
                    firmwareCount = 0; firmwareNo = 0; remainder = 0;
                    isCancelDownload = false;isDownload = true;
                    LockClearList();
                    model.InformationList = InfoList;
                    BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = true;
                    Thread.Sleep(200);

                    Task.Factory.StartNew(new Action(() =>
                    {
                        int count = 0;
                        isRequestUpdateAppTimeOut = true;
                        while (count < retryNum)
                        {
                            if (isRequestUpdateAppTimeOut)
                            {
                                timeOutTimer.Start();
                                UpdateInformation info = BQProtocol.BqProtocol.BqInstance.BQ_RequestUpdateApp();
                                Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    if (chkShowData.IsChecked == true)
                                    {
                                        LockAddToList(info);
                                    }
                                }));
                                count++;
                                ShowStatusInformation(string.Format("请求进入固件升级模式命令......第{0}次尝试，共{1}次！",count,retryNum),true);
                            }
                            else 
                            {
                                break;
                            }

                            Thread.Sleep(200);
                        }
                    }));

                    btnDownload.Content = "取消";

                    //Task.Factory.StartNew(new Action(() =>
                    //{
                    //    while (isDownload)
                    //    {
                    //        if (InfoQueue.Count != 0)
                    //        {
                    //            UpdateInformation info = InfoQueue.Dequeue();
                    //            AddToObservableCollection(info);

                    //        }
                    //    }
                    //}));
                }
                else
                {
                    btnDownload.Content = "下载";
                    isCancelDownload = true;
                    isDownload = false;
                    pbDownload.Value = 0;
                    labDownload.Content = "0.0%";
                    BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
                    ShowStatusInformation("取消下载！", true);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CheckParam()
        {
            string company = tbCompany.Text.Trim();
            if (string.IsNullOrEmpty(company))
            {
                MessageBox.Show("公司信息不能为空！", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            string hardwareStr = tbHardware.Text.Trim();
            if (string.IsNullOrEmpty(hardwareStr))
            {
                MessageBox.Show("硬件版本号不能为空", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            short hardware;
            if (short.TryParse(hardwareStr, out hardware))
            {
                if (hardware > 255 || hardware < 0)
                {
                    MessageBox.Show("硬件版本号为0~255之间的整数", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
            }
            else
            {
                MessageBox.Show("硬件版本号为0~255之间的整数", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            string customStr = tbCustomerCode.Text.Trim();
            if (string.IsNullOrEmpty(customStr))
            {
                MessageBox.Show("客户代码不能为空", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            short custom;
            if (short.TryParse(customStr, out custom))
            {
                if (custom > 255 || custom < 0)
                {
                    MessageBox.Show("客户代码为0~255之间的整数", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
            }
            else
            {
                MessageBox.Show("客户代码为0~255之间的整数", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            string softwareStr = tbSoftware.Text.Trim();
            if (!string.IsNullOrEmpty(softwareStr))
            {
                string[] software = softwareStr.Split('.');
                if (software.Length == 3)
                {
                    foreach (var it in software)
                    {
                        short no;
                        if (short.TryParse(it, out no))
                        {
                        }
                        else
                        {
                            MessageBox.Show("软件版本号格式为： *.*.* ,其中*为0~9之间的整数！", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                            return false;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("软件版本号格式为： *.*.* ,其中*为0~9之间的整数！", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
            }
            else
            {
                MessageBox.Show("软件版本号不能为空！", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            string timeOutStr = tbTimeOut.Text.Trim();
            if (!string.IsNullOrEmpty(timeOutStr))
            {
                int timeOut = 0;
                if (int.TryParse(timeOutStr, out timeOut))
                {
                    timeOutTimer.Interval = timeOut;
                }
                else
                {
                    MessageBox.Show("命令超时时间应输入整数！", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
            }
            else
            {
                MessageBox.Show("命令超时时间不能为空！", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            string skipIntervalStr = tbSkipInterval.Text.Trim();
            if (!string.IsNullOrEmpty(skipIntervalStr))
            {
                if (int.TryParse(skipIntervalStr, out skipInterval))
                {
                }
                else
                {
                    MessageBox.Show("等待跳转间隔应输入整数！", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
            }
            else
            {
                MessageBox.Show("等待跳转间隔不能为空！", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            string retryNumStr = tbErrorNum.Text.Trim();
            if (!string.IsNullOrEmpty(retryNumStr))
            {
                if (int.TryParse(retryNumStr, out retryNum))
                {
                }
                else
                {
                    MessageBox.Show("错误重传次数应输入整数！", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
            }
            else
            {
                MessageBox.Show("错误重传次数不能为空！", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            List<string> list = new List<string>();
            list.Add(tbCompany.Text.Trim());
            list.Add(tbHardware.Text.Trim());
            list.Add(tbErrorNum.Text.Trim());
            list.Add(tbTimeOut.Text.Trim());
            list.Add(tbCustomerCode.Text.Trim());
            list.Add(tbSoftware.Text.Trim());
            list.Add(tbSkipInterval.Text.Trim());
            list.Add(tbResultInterval.Text.Trim());
            XmlHelper.SaveUpdateInfo(list, configPath);
            return true;
        }
        bool isRequestUpdateAppTimeOut = false;
        bool isSendUpdateAppInfoTimeOut = false;
        bool SendUpdateAppInfo(List<byte> list,int fileLength)
        {
            string company = tbCompany.Text.Trim();
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(company);
            for(int i = 0;i < 10;i++)
            {
                if (i > bytes.Length - 1)
                {
                    list.Add(0x00);
                }
                else
                {
                    list.Add(bytes[i]);
                }
            }
            string hardwareStr = tbHardware.Text.Trim();
            if (string.IsNullOrEmpty(hardwareStr))
            {
                MessageBox.Show("硬件版本号不能为空", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            short hardware;
            if(short.TryParse(hardwareStr,out hardware))
            {
                if(hardware > 255 || hardware < 0)
                {
                    MessageBox.Show("硬件版本号为0~255之间的整数", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
                else
                {
                    list.Add((byte)(0xff & hardware));
                }
            }
            else
            {
                MessageBox.Show("硬件版本号为0~255之间的整数", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            string customStr = tbCustomerCode.Text.Trim();
            if (string.IsNullOrEmpty(customStr))
            {
                MessageBox.Show("客户代码不能为空", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            short custom;
            if (short.TryParse(customStr, out custom))
            {
                if (custom > 255 || custom < 0)
                {
                    MessageBox.Show("客户代码为0~255之间的整数", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
                else
                {
                    list.Add((byte)(0xff & custom));
                }
            }
            else
            {
                MessageBox.Show("客户代码为0~255之间的整数", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            list.Add(0x04);//固件类型，0x04表示MC固件信息

            string softwareStr = tbSoftware.Text.Trim();
            if(!string.IsNullOrEmpty(softwareStr))
            {
                string[] software = softwareStr.Split('.');
                if(software.Length == 3)
                {
                    foreach(var it in software)
                    {
                        short no;
                        if(short.TryParse(it,out no))
                        {
                            list.Add((byte)(0xff & custom));
                        }
                        else
                        {
                            MessageBox.Show("软件版本号格式为： *.*.* ,其中*为0~9之间的整数！", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                            return false;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("软件版本号格式为： *.*.* ,其中*为0~9之间的整数！", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
            }
            else
            {
                MessageBox.Show("软件版本号不能为空！", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            byte[] fileLengthArray = BitConverter.GetBytes(fileLength);
            Array.Reverse(fileLengthArray);//大端模式
            list.AddRange(fileLengthArray.ToList());
            list.Add(0x00);//增加两个CRC校验位
            list.Add(0x00);
            return true;
        }

        void UpdateFirmwareInfo(List<byte> list,string firmwareStr)
        {
            if (!string.IsNullOrEmpty(firmwareStr))
            {
                string[] firmware = firmwareStr.Split('.');
                foreach (var it in firmware)
                {
                    short no;
                    if (short.TryParse(it, out no))
                    {
                        list.Add((byte)(0xff & no));
                    }
                }
            }
            else
            {
                MessageBox.Show("软件版本号不能为空！", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            list.Add(0x04);//固件类型，0x04表示MC固件信息
            byte[] bytes = BitConverter.GetBytes(firmwareNo);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            foreach(var it in bytes)
            {
                list.Add(it);
            }
            byte[] _bytes = BitConverter.GetBytes(firmwareCount);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(_bytes);
            foreach (var it in _bytes)
            {
                list.Add(it);
            }
            //list.Add(0x00);//增加两个CRC校验位
            //list.Add(0x00);
        }
        void SendUpdateAppData(int DataLength)
        {
            string filePath = tbFilePath.Text.Trim();
            if (File.Exists(filePath))
            {
                string firmware = tbSoftware.Text.Trim();
                Task.Factory.StartNew(new Action(() =>
                {
                    FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    BinaryReader br = new BinaryReader(fs);
                    int count = 0;
                    remainder = (ushort)(fs.Length % DataLength);
                    if (remainder == 0)
                    {
                        firmwareCount = (ushort)(fs.Length / DataLength);
                    }
                    else
                    {
                        firmwareCount = (ushort)((fs.Length / DataLength) + 1);
                    }
                    ShowStatusInformation(string.Format("开始下载第{0}块固件数据，共{1}块...", firmwareNo + 1, firmwareCount), true);
                   
                    while (true)
                    {
                        if (isCancelDownload)
                            break;

                        if(isUpdateNext)
                        {
                            List<byte> firmwareInfoList = new List<byte>();
                            UpdateFirmwareInfo(firmwareInfoList,firmware);
                            byte[] firmwreInfoArray = firmwareInfoList.ToArray();
                            if (count + DataLength > fs.Length)
                                DataLength = remainder;
                            byte[] bytes = new byte[DataLength];
                            int len = br.Read(bytes, 0, DataLength);
                            if (len == 0)
                            {
                                byte[] _bytes = new byte[DataLength];
                                int _count = br.Read(_bytes, 0, _bytes.Length);

                                if (_count == 0 && fs.Length == count)
                                    break;
                            }

                            byte[] crc16 = CRC_Check.CRC16(bytes, 0, bytes.Length); ;
                            byte[] buffer = new byte[bytes.Length + firmwreInfoArray.Length + 2];
                            Buffer.BlockCopy(firmwreInfoArray, 0, buffer, 0, firmwreInfoArray.Length);
                            Buffer.BlockCopy(crc16, 0, buffer, firmwreInfoArray.Length, crc16.Length);
                            Buffer.BlockCopy(bytes, 0, buffer, firmwreInfoArray.Length + crc16.Length, bytes.Length);
                            count += len;

                            isUpdateNext = false;
                            timeOutTimer.Start();
                            if (isShowData)
                                BQProtocol.BqProtocol.BqInstance.BQ_SendUpdateAppData(buffer, InfoList, true,lockObj);
                            else
                                BQProtocol.BqProtocol.BqInstance.BQ_SendUpdateAppData(buffer, InfoList, false, lockObj);

                            //BQProtocol.BqProtocol.BqInstance.BQ_SendUpdateAppData(buffer);
                            //Task.Factory.StartNew(new Action(() =>
                            //{
                            //    Dispatcher.BeginInvoke(new Action(() =>
                            //    {
                            //        if (chkShowData.IsChecked == true)
                            //        {
                            //            foreach (var it in infoList)
                            //            {
                            //                InfoList.Add(it);
                            //            }
                            //        }
                            //    }));

                            //}));
                        }

                        float val = (float)firmwareNo / (float)(firmwareCount) * 100;
                        Dispatcher.Invoke(new Action(() =>
                        {
                            pbDownload.Value = val;
                            labDownload.Content = string.Format("{0}%", val.ToString("F2"));
                        }));
                    }

                    br.Close();
                    fs.Close();
                }));
            }
            else
            {
                MessageBox.Show(string.Format("文件 {0} 不存在，请检查！", filePath), "信息", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void HandleRecvRequestUpdateAppEvent(object sender, CustomRecvDataEventArgs e)
        {
            isRequestUpdateAppTimeOut = false;
            timeOutTimer.Stop();
            AddCommunicationMessage(e.RecvMsg.ToArray(), e.IDStr);
            if (e.RecvMsg[0] == 0x45 && e.RecvMsg[1] == 0x02)
            {
                if(isDownload)
                {
                    if (e.RecvMsg[4] == 0x00)
                    {
                        ShowStatusInformation("进入固件升级模式成功！", true);
                        Thread.Sleep(100);
                        List<byte> bytes = new List<byte>();
                        FileStream fs = new FileStream(tbFilePath.Text.Trim(), FileMode.Open, FileAccess.Read);
                        if (SendUpdateAppInfo(bytes, (int)(fs.Length)))
                        {
                            byte[] array = bytes.ToArray();
                            byte[] crc = CRC_Check.CRC16(array, 0, array.Length);
                            array[array.Length - 1] = crc[1];
                            array[array.Length - 2] = crc[0];
                            //timeOutTimer.Start();
                            //BQProtocol.BqProtocol.BqInstance.BQ_SendUpdateAppInfo(array);

                            Task.Factory.StartNew(new Action(() =>
                            {
                                int count = 0;
                                isSendUpdateAppInfoTimeOut = true;
                                while (count < retryNum)
                                {
                                    if (isSendUpdateAppInfoTimeOut)
                                    {
                                        timeOutTimer.Start();
                                        if (isShowData)
                                        {
                                            BQProtocol.BqProtocol.BqInstance.BQ_SendUpdateAppInfo(array, InfoList, true, lockObj);
                                        }
                                        else
                                            BQProtocol.BqProtocol.BqInstance.BQ_SendUpdateAppInfo(array, InfoList, false, lockObj);
                                        count++;
                                        ShowStatusInformation(string.Format("发送最新固件信息并尝试开始升级从机......第{0}次尝试，共{1}次！", count, retryNum), true);
                                    }
                                    else
                                    {
                                        break;
                                    }

                                    Thread.Sleep(200);
                                }
                            }));
                        }
                    }
                    else
                    {
                        ShowStatusInformation("进入固件升级模式失败", false);
                        if (e.RecvMsg[5] == 0x01)
                        {
                            ShowStatusInformation("电量过低", false);
                        }
                        else if (e.RecvMsg[5] == 0x02)
                        {
                            ShowStatusInformation("不支持固件升级", false);
                        }
                        else if (e.RecvMsg[5] == 0x03)
                        {
                            ShowStatusInformation("从机处于异常保护状态，不允许升级", false);
                        }
                        else
                        {
                            ShowStatusInformation(string.Format("请求升级错误码：{0}", e.RecvMsg[5].ToString("X2")), false);
                        }
                        BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
                    }
                }
                //else
                //{
                //    if (isSendMsg) isSendMsg = false;
                //    SendCommunicationMsgEvent?.Invoke(this, new EventArgs<bool>(false));
                //    BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
                //}
            }
            else
            {
                ShowStatusInformation("接收到异常数据！", false);
                BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
            }
        }
        bool isReadDeviceInfoTimeout = false;
        public void HandleRecvSendUpdateAppInfoEvent(object sender, CustomRecvDataEventArgs e)
        {
            isSendUpdateAppInfoTimeOut = false;
            timeOutTimer.Stop();
            AddCommunicationMessage(e.RecvMsg.ToArray(), e.IDStr);
            if (e.RecvMsg[0] == 0x45 && e.RecvMsg[1] == 0x14)
            {
                if(isDownload)
                {
                    if (e.RecvMsg[4] == 0x01)
                    {
                        ShowStatusInformation("固件块擦写进行中", true);
                        Thread.Sleep(100);
                        int DataLength = 0;
                        if (e.RecvMsg[5] == 0x01) DataLength = 64;
                        else if (e.RecvMsg[5] == 0x02) DataLength = 128;
                        else if (e.RecvMsg[5] == 0x04) DataLength = 192;
                        else
                        {
                            ShowStatusInformation("固件分块请求固件包大小异常，请检查！", false);
                            BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
                            return;
                        }
                        isUpdateNext = true;
                        ShowStatusInformation("尝试开始升级主机成功！", true);
                        SendUpdateAppData(DataLength);
                    }
                    else
                    {
                        if (e.RecvMsg[4] == 0x00)
                        {
                            ShowStatusInformation("不需要升级", true);
                            BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
                        }
                        else if (e.RecvMsg[4] == 0x02)
                        {
                            ShowStatusInformation("升级完成", true);

                            Task.Factory.StartNew(new Action(() =>
                            {
                                int count = 0;
                                isReadDeviceInfoTimeout = true;
                                Thread.Sleep(skipInterval);
                                while (count < retryNum)
                                {
                                    if (isReadDeviceInfoTimeout)
                                    {
                                        timeOutTimer.Start();
                                        DDProtocol.DdProtocol.DdInstance.ReadDeviceInfo();
                                        count++;
                                        ShowStatusInformation(string.Format("读取BMS设备信息......第{0}次尝试，共{1}次！", count, retryNum), true);
                                    }
                                    else
                                    {
                                        break;
                                    }

                                    Thread.Sleep(200);
                                }
                            }));
                        }
                        else if (e.RecvMsg[4] == 0x03)
                        {
                            ShowStatusInformation("升级出错，应用程序无法正常执行", false);
                            BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
                        }
                        else if (e.RecvMsg[4] == 0x04)
                        {
                            ShowStatusInformation("固件分块接收校验失败", false);
                            BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
                        }
                        else if (e.RecvMsg[4] == 0x05)
                        {
                            ShowStatusInformation("固件分块接收擦写失败", false);
                            BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
                        }
                        else if (e.RecvMsg[4] == 0x06)
                        {
                            ShowStatusInformation("固件分块请求中", false);
                            BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
                        }
                        else if (e.RecvMsg[4] == 0x07)
                        {
                            ShowStatusInformation("升级失败", false);
                            BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
                        }
                        else if (e.RecvMsg[4] == 0x08)
                        {
                            ShowStatusInformation("从机应用程序无法正常执行", false);
                            BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
                        }
                        else
                        {
                            ShowStatusInformation(string.Format("错误码：{0}", e.RecvMsg[4].ToString("X2")), false);
                            BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
                        }
                    }
                }
                //else
                //{
                //    if (isSendMsg) isSendMsg = false;
                //    SendCommunicationMsgEvent?.Invoke(this, new EventArgs<bool>(false));
                //    BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
                //}
            }
            else
            {
                ShowStatusInformation("接收到异常数据！", false);
                BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
            }
        }
        bool isUpdateNext = false;
        public void HandleRecvSendUpdateAppDataEvent(object sender, CustomRecvDataEventArgs e)
        {
            AddCommunicationMessage(e.RecvMsg.ToArray(), e.IDStr);
            timeOutTimer.Stop();
            if (e.RecvMsg[0] == 0x45 && e.RecvMsg[1] == 0x16)
            {
                if(isDownload)
                {
                    firmwareNo++;
                    if (e.RecvMsg[4] == 0x00)
                    {
                        isUpdateNext = true;
                        if (firmwareNo == firmwareCount)
                        {
                            //ShowStatusInformation("升级完成", true);
                            //BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
                            List<byte> bytes = new List<byte>();
                            FileStream fs = new FileStream(tbFilePath.Text.Trim(), FileMode.Open, FileAccess.Read);
                            if (SendUpdateAppInfo(bytes, (int)(fs.Length)))
                            {
                                byte[] array = bytes.ToArray();
                                byte[] crc = CRC_Check.CRC16(array, 0, array.Length);
                                array[array.Length - 1] = crc[1];
                                array[array.Length - 2] = crc[0];
                                ShowStatusInformation("根据固件信息获取升级状态......", true);
                                timeOutTimer.Start();
                                if (isShowData)
                                {
                                    BQProtocol.BqProtocol.BqInstance.BQ_SendUpdateAppInfo(array, InfoList, true, lockObj);
                                }
                                else
                                    BQProtocol.BqProtocol.BqInstance.BQ_SendUpdateAppInfo(array, InfoList, false, lockObj);
                            }
                        }
                        else
                        {
                            ShowStatusInformation(string.Format("开始下载第{0}块固件数据，共{1}块...", firmwareNo + 1, firmwareCount), true);
                        }
                    }
                    else
                    {
                        if (e.RecvMsg[4] == 0x01)
                        {
                            ShowStatusInformation("校验失败", false);
                        }
                        else if (e.RecvMsg[4] == 0x02)
                        {
                            ShowStatusInformation("烧写失败", false);
                        }
                        else if (e.RecvMsg[4] == 0x03)
                        {
                            ShowStatusInformation("固件分块请求中", false);
                        }
                        else if (e.RecvMsg[4] == 0x04)
                        {
                            ShowStatusInformation("完整固件包接收完成，长度异常", false);
                        }
                        else if (e.RecvMsg[4] == 0x05)
                        {
                            ShowStatusInformation("固件完整性异常", false);
                        }
                        else
                        {
                            ShowStatusInformation(string.Format("错误码：{0}", e.RecvMsg[5].ToString("X2")), false);
                        }
                        BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
                    }
                }
                //else
                //{
                //    if (isSendMsg) isSendMsg = false;
                //    SendCommunicationMsgEvent?.Invoke(this, new EventArgs<bool>(false));
                //    BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
                //}
            }
            else
            {
                isUpdateNext = false;
                ShowStatusInformation("接收到异常数据！", false);
                BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
            }
        }

        public void HandleRecvDdDeviceInfoEvent(object sender,CustomRecvDataEventArgs e)
        {
            isReadDeviceInfoTimeout = false;
            timeOutTimer.Stop();
            AddCommunicationMessage(e.RecvMsg.ToArray(), e.IDStr);
            if (e.RecvMsg[0] == 0x03 && e.RecvMsg[1] == 0x56)
            {
                if(isDownload)
                {

                    isDownload = false;
                    BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
                    ShowStatusInformation("读取BMS设备信息成功！", true);
                    ShowStatusInformation("升级完成，并记录成功！", true);
                    btnDownload.Content = "下载";
                }
                //else
                //{
                //    if (isSendMsg) isSendMsg = false;
                //    SendCommunicationMsgEvent?.Invoke(this, new EventArgs<bool>(false));
                //    BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
                //}
            }
        }
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            LockClearList();
            model.InformationList = InfoList;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MainWindow.m_statusBarInfo.IsOnline)
                {
                    Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                    dlg.Filter = "csv files(*.csv)|*.csv";
                    dlg.FileName = System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
                    dlg.AddExtension = false;
                    dlg.RestoreDirectory = true;
                    System.Nullable<bool> result = dlg.ShowDialog();
                    if (result == true)
                    {
                        string desFilePath = dlg.FileName.ToString();
                        if (File.Exists(desFilePath))
                        {
                            File.Delete(desFilePath);
                        }
                        SaveUpdateInformation(desFilePath, model.InformationList);
                    }
                }
                else
                {
                    MessageBox.Show("设备未连接，请连接后再进行操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void SaveUpdateInformation(string path, List<UpdateInformation> datas)
        {
            bool isCreate = false;
            FileStream fs = null;
            StreamWriter sw = null;

            try
            {
                FileInfo fi = new FileInfo(path);
                if (!fi.Directory.Exists)
                {
                    fi.Directory.Create();
                }

                if (!File.Exists(path))
                {
                    fs = File.Create(path);//创建该文件
                    isCreate = true;
                }
                else
                {
                    fs = new FileStream(path, System.IO.FileMode.Append, System.IO.FileAccess.Write);
                }

                sw = new StreamWriter(fs, System.Text.Encoding.Default);
                if (isCreate)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("方向,");
                    sb.Append("时间,");
                    sb.Append("ID,");
                    sb.Append("长度,");
                    sb.Append("内容,");
                    sb.Append("备注");
                    sw.WriteLine(sb.ToString());
                }

                if (datas != null)
                {
                    foreach(var item in datas)
                    {
                        string strLine = string.Format("{0},{1},{2},{3},{4},{5}", item.DirectionStr, item.TimeStr,item.ID,item.Length,item.Content,item.Comments);
                        sw.WriteLine(strLine);
                    }
                }
            }
            catch (Exception ex)
            { }
            finally
            {
                if (null != sw)
                    sw.Close();
                if (null != fs)
                    fs.Close();
            }
        }

        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        System.Timers.Timer timeOutTimer = new System.Timers.Timer();
        byte[] dataBytes;
        bool isSendMsg = false;
        public event EventHandler<EventArgs<Tuple<bool,byte>>> SendCommunicationMsgEvent;
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(MainWindow.m_statusBarInfo.IsOnline)
                {
                    if(btnSend.Content.ToString() == "发送")
                    {
                        if (!CheckParam())
                        {
                            return;
                        }
                        string regexStr = @"^[A-Fa-f0-9]+$";
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
                            }
                            else
                            {
                                MessageBox.Show("发送的数据包含非十六进制数字，请检查！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }
                        }
                        else
                        {
                            MessageBox.Show("请输入发送数据！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        uint loopInterval = 0;
                        if (chkLoop.IsChecked == true)
                        {
                            string intervalStr = tbLoopSpan.Text.Trim();
                            if (!string.IsNullOrEmpty(intervalStr))
                            {
                                if (uint.TryParse(intervalStr, out loopInterval))
                                {
                                    timer.Interval = (int)loopInterval;
                                    timer.Start();
                                    BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = true;
                                    Thread.Sleep(200);
                                    SendCommunicationMsgEvent?.Invoke(this, new EventArgs<Tuple<bool,byte>>(new Tuple<bool, byte>(true,dataBytes[0])));
                                }
                                else
                                {
                                    MessageBox.Show("请输入正确的循环时间！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                                    return;
                                }
                            }
                            else
                            {
                                MessageBox.Show("请输入循环间隔！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }
                            btnSend.Content = "停止发送";
                        }
                        else
                        {
                            BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = true;
                            Thread.Sleep(300);
                            List<UpdateInformation> infoList = new List<UpdateInformation>();
                            timeOutTimer.Start();
                            isSendMsg = true;
                            SendCommunicationMsgEvent?.Invoke(this, new EventArgs<Tuple<bool, byte>>(new Tuple<bool, byte>(true, dataBytes[0])));
                            BQProtocol.BqProtocol.BqInstance.SendCommunicationMessage(dataBytes,infoList, lockObj);
                            LockAddToList(infoList);
                        }
                    }
                    else
                    {
                        timer.Stop();
                        timeOutTimer.Stop();
                        SendCommunicationMsgEvent?.Invoke(this, new EventArgs<Tuple<bool, byte>>(new Tuple<bool, byte>(true,0x00)));
                        BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
                        btnSend.Content = "发送";
                        isSendMsg = false;
                    }
                }
                else
                {
                    MessageBox.Show("设备未连接，请连接后再进行操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "异常", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnTimer(object sender,EventArgs e)
        {
            timeOutTimer.Stop();
            isSendMsg = true;
            List<UpdateInformation> infoList = new List<UpdateInformation>();
            BQProtocol.BqProtocol.BqInstance.SendCommunicationMessage(dataBytes, infoList, lockObj);
            timeOutTimer.Start();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (chkShowData.IsChecked == true)
                {
                    LockAddToList(infoList);
                }
            }));
        }

        ////bool isTimeOut = false;
        private void OnTimeOutTimer(object sender,EventArgs e)
        {
            if (isRequestUpdateAppTimeOut) isRequestUpdateAppTimeOut = false;
            if (isSendUpdateAppInfoTimeOut) isSendUpdateAppInfoTimeOut = false;
            if (isReadDeviceInfoTimeout) isReadDeviceInfoTimeout = false;
            timeOutTimer.Stop();
            ShowStatusInformation("命令响应超时！", false);
            BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
        }
        bool isShowData = false;
        private void chkShowData_Click(object sender, RoutedEventArgs e)
        {
            if(chkShowData.IsChecked == true)
            {
                isShowData = true;
            }
            else
            {
                isShowData = false;
            }
        }

        public void HandleRecvCommunicationMessageEvent(object sender,CustomRecvDataEventArgs e)
        {
            if(isSendMsg)
            {
                timeOutTimer.Stop();
                isSendMsg = false;
                if(chkLoop.IsChecked != true)
                {
                    SendCommunicationMsgEvent?.Invoke(this, new EventArgs<Tuple<bool, byte>>(new Tuple<bool, byte>(false, 0x00)));
                    AddCommunicationMessage(e.RecvMsg.ToArray(), e.IDStr);
                    Thread.Sleep(300);
                    BQProtocol.BqProtocol.BqInstance.m_bIsStopCommunication = false;
                }
                else
                    AddCommunicationMessage(e.RecvMsg.ToArray(),e.IDStr);
            }
        }

        public void AddCommunicationMessage(byte[] bytes,string idStr)
        {
            try
            {
                if (isShowData)
                {
                    int _remainder = bytes.Length % 8;
                    int num = bytes.Length / 8;
                    int offset = 0;
                    string[] ids = idStr.Split(',');
                    for (int i = 0; i < num; i++)
                    {
                        byte[] _bytes = new byte[8];
                        UpdateInformation info = new UpdateInformation();
                        info.DirectionStr = "接收";
                        Buffer.BlockCopy(bytes, offset, _bytes, 0, _bytes.Length);
                        offset += 8;
                        info.Content = BitConverter.ToString(_bytes);
                        info.Length = _bytes.Length.ToString();
                        info.Comments = string.Empty;
                        info.TimeStr = DateTime.Now.ToString("MM/dd HH:mm:ss") + string.Format(":{0}", DateTime.Now.Millisecond);
                        if (i < ids.Length)
                            info.ID = "0x" + ids[i];
                        LockAddToList(info);
                    }
                    if (_remainder != 0)
                    {
                        byte[] _bytes = new byte[_remainder];
                        UpdateInformation info = new UpdateInformation();
                        info.DirectionStr = "接收";
                        Buffer.BlockCopy(bytes, offset, _bytes, 0, _bytes.Length);
                        offset += _remainder;
                        info.Content = BitConverter.ToString(_bytes);
                        info.Length = _bytes.Length.ToString();
                        info.Comments = string.Empty;
                        info.TimeStr = DateTime.Now.ToString("MM/dd HH:mm:ss") + string.Format(":{0}", DateTime.Now.Millisecond);
                        info.ID = "0x" + ids[ids.Length - 1];
                        LockAddToList(info);
                    }
                    Task.Factory.StartNew(new Action(() =>
                    {
                        lock(lockObj)
                        {
                            if (InfoList.Count > 2000)
                                InfoList.RemoveRange(0, InfoList.Count() - 2000);
                            model.InformationList = new List<UpdateInformation>(InfoList);
                        }
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (model.InformationList.Count > 0)
                                dgDataInfo.SelectedItem = model.InformationList.Last();
                        }));
                    }));
                }
            }
            catch(Exception ex)
            {

            }
        }
        delegate void SetSelectedItemDelegate(UpdateInformation item);
        private void SetSelectedItemInBackground(UpdateInformation item)
        {
            dgDataInfo.SelectedItem = item;
            dgDataInfo.ScrollIntoView(item);
        }

        private void dgDataInfo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(dgDataInfo.Items.Count > 0)
            {
                dgDataInfo.ScrollIntoView((UpdateInformation)dgDataInfo.SelectedItem);
                //SetSelectedItemDelegate selectDelegate = new SetSelectedItemDelegate(SetSelectedItemInBackground);
                //dgDataInfo.Dispatcher.BeginInvoke(selectDelegate, System.Windows.Threading.DispatcherPriority.Background, (UpdateInformation)dgDataInfo.SelectedItem);
            }
        }
    }

    public class UpdateInformation : INotifyPropertyChanged
    {
        private string directionStr = string.Empty;
        private string timeStr = string.Empty;
        private string id = string.Empty;
        private string length = string.Empty;
        private string content = string.Empty;
        private string comments = string.Empty;

        public string DirectionStr
        {
            set { directionStr = value; OnPropertyChanged(nameof(DirectionStr)); }
            get { return directionStr; }
        }
        public string TimeStr
        {
            set { timeStr = value; OnPropertyChanged(nameof(TimeStr)); }
            get { return timeStr; }
        }
        public string ID
        {
            set { id = value; OnPropertyChanged(nameof(ID)); }
            get { return id; }
        }
        public string Length
        {
            set { length = value; OnPropertyChanged(nameof(Length)); }
            get { return length; }
        }
        public string Content
        {
            set { content = value; OnPropertyChanged(nameof(Content)); }
            get { return content; }
        }
        public string Comments
        {
            set { comments = value;OnPropertyChanged(nameof(Comments)); }
            get { return comments; }
        }

        private void UpdateProperty<T>(ref T properValue, T newValue, string propertyName)
        {
            //if (object.Equals(properValue, newValue))
            //{
            //    return;
            //}
            properValue = newValue;

            OnPropertyChanged(propertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    class UpdateViewModel : INotifyPropertyChanged
    {
        private List<UpdateInformation> informationList;
        public List<UpdateInformation> InformationList
        {
            set { UpdateProperty(ref informationList, value, nameof(InformationList)); }
            get
            {
                return informationList;
            }
        }

        private void UpdateProperty<T>(ref T properValue, T newValue, string propertyName)
        {
            //if (object.Equals(properValue, newValue))
            //{
            //    return;
            //}
            properValue = newValue;

            OnPropertyChanged(propertyName);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
