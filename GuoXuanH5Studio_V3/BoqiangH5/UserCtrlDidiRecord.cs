﻿using BoqiangH5.BQProtocol;
using BoqiangH5Entity;
using BoqiangH5Repository;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Text;
using System.Collections.ObjectModel;
using BoqiangH5.DDProtocol;
using System.Threading;

namespace BoqiangH5
{
    /// <summary>
    /// UserCtrlRecord.xaml 的交互逻辑
    /// </summary>
    public partial class UserCtrlDidiRecord : UserControl
    {
        public static ObservableCollection<H5DidiRecordInfo> m_ListRecordsInfo = new ObservableCollection<H5DidiRecordInfo>();
        Dictionary<string, string> recordTypeDic = new Dictionary<string, string>();
        Dictionary<string, Tuple<string, string>> recordEventTypeDic = new Dictionary<string, Tuple<string, string>>();
        List<Tuple<string, string, string>> packInfoList = new List<Tuple<string, string, string>>();
        List<Tuple<string, string, string>> batteryInfoList = new List<Tuple<string, string, string>>();
        Dictionary<string, string> recordInfoDic = new Dictionary<string, string>();
        public UserCtrlDidiRecord()
        {
            InitializeComponent();
            InitBqBmsInfoWnd();
        }

        private void InitBqBmsInfoWnd()
        {
            recordTypeDic.Clear();
            //string strConfigFile = System.AppDomain.CurrentDomain.BaseDirectory + @"BoqiangV3\ProtocolFiles\bq_h5_record_info.xml";//程序合并时使用
            string strConfigFile = System.AppDomain.CurrentDomain.BaseDirectory + @"ProtocolFiles\bq_h5_record_info.xml";
            XmlHelper.LoadDidiRecordConfig(strConfigFile, recordTypeDic);
            XmlHelper.LoadBackupRecordConfig(strConfigFile, recordInfoDic, recordEventTypeDic, packInfoList, batteryInfoList);
        }

        private void ucDidiRecordInfo_Loaded(object sender, RoutedEventArgs e)
        {
            m_ListRecordsInfo.Clear();
            dataGridRecord.Items.Clear();
            dataGridRecord.ItemsSource = m_ListRecordsInfo;
            btnStopRead.IsEnabled = false;
            timer = new System.Windows.Forms.Timer();
            timer.Tick += OnTimer;
            timer.Interval = 1000;
        }
        int timeOutNum = 0;
        private void OnTimer(object sender, EventArgs e)
        {
            if(timeOutNum == 3)
            {
                timeOutNum = 0;
                isRead = false;
                isReadAll = false;
                isStopRead = true;
                btnReadAllData.IsEnabled = true;
                btnReadData.IsEnabled = true;
                btnStopRead.IsEnabled = false;
                btnClear.IsEnabled = true;
                btnSave.IsEnabled = true;
                DdProtocol.DdInstance.m_bIsStopCommunication = false;
                timer.Stop();
                MessageBox.Show("故障读取失败，请重试！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                timeOutNum++;
                timer.Stop();
                Thread.Sleep(100);
                timer.Start();
                if(isReadAll)
                    DdProtocol.DdInstance.ReadDidiCurrentRecordData();
                else
                    DdProtocol.DdInstance.ReadDidiRecord();
            }
        }
        System.Windows.Forms.Timer timer;
        bool isReadAll = false;
        private void btnReadAllData_Click(object sender,RoutedEventArgs e)
        {
            if (MainWindow.m_statusBarInfo.IsOnline)
            {
                if (isRead == false)
                {
                    isReadAll = true;
                    m_ListRecordsInfo.Clear();
                    dataGridRecord.Columns[2].Visibility = Visibility.Visible;
                    dataGridRecord.Columns[3].Visibility = Visibility.Visible;
                    for (int i = 12; i < 22; i++)
                    {
                        dataGridRecord.Columns[i].Visibility = Visibility.Visible;
                    }
                    dataGridRecord.Items.Refresh();
                    isRead = false;
                    errorNum = 0;
                    errorCount = 0;
                    faultCount = 0;
                    timeOutNum = 0;
                    DdProtocol.bReadDdBmsResp = true;
                    DdProtocol.DdInstance.m_bIsStopCommunication = true;
                    Thread.Sleep(200);
                    timer.Start();
                    DdProtocol.DdInstance.ReadDidiFirstRecordData();
                    btnStopRead.IsEnabled = true;
                    btnReadData.IsEnabled = false;
                    btnReadAllData.IsEnabled = false;
                    btnClear.IsEnabled = false;
                    btnSave.IsEnabled = false;
                    isRead = true;
                    isStopRead = false;
                }
                else
                {
                    MessageBox.Show("正在读取备份数据，请先停止读取，再进行操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("系统未连接，请连接后再进行操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        bool isRead = false;
        private void btnReadData_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (MainWindow.m_statusBarInfo.IsOnline)
            {
                if (isRead == false)
                {
                    isReadAll = false;
                    m_ListRecordsInfo.Clear();
                    dataGridRecord.Columns[2].Visibility = Visibility.Collapsed;
                    dataGridRecord.Columns[3].Visibility = Visibility.Collapsed;
                    for (int i = 12; i < 22; i++)
                    {
                        dataGridRecord.Columns[i].Visibility = Visibility.Collapsed;
                    }
                    dataGridRecord.Items.Refresh();
                    isRead = false;
                    errorNum = 0;
                    errorCount = 0;
                    faultCount = 0;
                    timeOutNum = 0;
                    DdProtocol.bReadDdBmsResp = true;
                    DdProtocol.DdInstance.m_bIsStopCommunication = true;
                    Thread.Sleep(200);
                    timer.Start();
                    DdProtocol.DdInstance.ReadDidiRecordCount();
                    btnStopRead.IsEnabled = true;
                    btnReadData.IsEnabled = false;
                    btnReadAllData.IsEnabled = false;
                    btnClear.IsEnabled = false;
                    btnSave.IsEnabled = false;
                    isRead = true;
                    isStopRead = false;
                }
                else
                {
                    MessageBox.Show("正在读取备份数据，请先停止读取，再进行操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("系统未连接，请连接后再进行操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        public event EventHandler RequireReadUIDInfoEvent;
        public event EventHandler RequireReadDeviceInfoEvent;
        bool isSave = false;
        private void btnSave_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                if (MainWindow.m_statusBarInfo.IsOnline)
                {
                    if (m_ListRecordsInfo.Count == 0)
                    {
                        MessageBox.Show("请先读取故障记录，再进行导出操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    UID = string.Empty;
                    isSave = true;
                    RequireReadUIDInfoEvent?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    MessageBox.Show("系统未连接，请连接后再进行操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                isSave = false;
            }
        }

        string UID = string.Empty;
        public void SetUID(string uid)
        {
            if(isSave)
            {
                UID = uid;
                DdProtocol.DdInstance.m_bIsStopCommunication = true;
                RequireReadDeviceInfoEvent?.Invoke(this, EventArgs.Empty);
            }
        }
        public void SetBqDeviceInfo(List<string> list)
        {
            try
            {
                if(isSave)
                {
                    Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                    dlg.Filter = "csv files(*.csv)|*.csv";
                    dlg.FileName = System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";
                    //dlg.InitialDirectory = "D:\\";
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
                        if (dataGridRecord.Columns[2].Visibility == Visibility.Visible)
                            CSVFileHelper.SaveDdRecordDataCSV(m_ListRecordsInfo, desFilePath, true, UID, list);
                        else
                            CSVFileHelper.SaveDdRecordDataCSV(m_ListRecordsInfo, desFilePath, false, UID, list);
                        MessageBox.Show("备份数据保存成功！");
                        isSave = false;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
                isSave = false;
            }
        }

        bool isErase = false;
        private void btnClear_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (MainWindow.m_statusBarInfo.IsOnline)
            {
                if (MessageBoxResult.Yes == MessageBox.Show("确定要擦除备份数据？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Information))
                {
                    isErase = false;
                    DdProtocol.bReadDdBmsResp = true;
                    DdProtocol.DdInstance.EraseDidiRecord();
                    isErase = true;
                 }
            }
            else
            {
                MessageBox.Show("系统未连接，请连接后再进行操作！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        bool isStopRead = false;
        int errorNum = 0;
        int errorCount = 0;
        int faultCount = 0;

        public void HandleReadRecordInfoDataEvent(object sender, CustomRecvDataEventArgs e)
        {
            try
            {
                if (isStopRead)
                {
                    isRead = false;
                    DdProtocol.DdInstance.m_bIsStopCommunication = false;
                    return;
                }
                if (isRead)
                {
                    if(isReadAll)
                    {
                        timer.Stop();
                        if (e.RecvMsg[5] == 0x00)
                        {
                            isRead = false;
                            btnStopRead.IsEnabled = false;
                            btnReadData.IsEnabled = true;
                            btnReadAllData.IsEnabled = true;
                            MessageBox.Show("没有备份数据！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else if (e.RecvMsg[5] == 0x01 || e.RecvMsg[5] == 0x02)
                        {
                            H5DidiRecordInfo info = UpdateDidiRecord(e.RecvMsg);
                            if (info == null)
                            {
                                timer.Start();
                                DdProtocol.DdInstance.ReadDidiCurrentRecordData();
                                return;
                            }
                            info.Index = m_ListRecordsInfo.Count + 1;
                            m_ListRecordsInfo.Add(info);
                            DdProtocol.bReadDdBmsResp = true;
                            timer.Start();
                            DdProtocol.DdInstance.ReadDidiNextRecordData();
                        }
                        else if (e.RecvMsg[5] == 0x03)
                        {
                            H5DidiRecordInfo info = UpdateDidiRecord(e.RecvMsg);
                            if (info == null)
                            {
                                timer.Start();
                                DdProtocol.DdInstance.ReadDidiCurrentRecordData();
                                return;
                            }
                            info.Index = m_ListRecordsInfo.Count + 1;
                            m_ListRecordsInfo.Add(info);
                            isRead = false;
                            MessageBox.Show("备份数据读取完成！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                            DdProtocol.DdInstance.m_bIsStopCommunication = false;
                            btnStopRead.IsEnabled = false;
                            btnReadData.IsEnabled = true;
                            btnReadAllData.IsEnabled = true;
                            btnClear.IsEnabled = true;
                            btnSave.IsEnabled = true;
                        }
                        else if(e.RecvMsg[5] == 0x05 || e.RecvMsg[5] == 0x06)
                        {
                            H5DidiRecordInfo info = UpdateDidiRecord(e.RecvMsg);
                            if (info == null)
                            {
                                timer.Start();
                                DdProtocol.DdInstance.ReadDidiCurrentRecordData();
                                return;
                            }
                            info.Index = m_ListRecordsInfo.Count + 1;
                            m_ListRecordsInfo.Add(info);
                            DdProtocol.bReadDdBmsResp = true;
                            if(faultCount == 10)
                            {
                                MessageBox.Show("多次读取到错误数据，请查找原因！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                                DdProtocol.DdInstance.m_bIsStopCommunication = false;
                                isRead = false;
                                btnReadAllData.IsEnabled = true;
                                btnReadData.IsEnabled = true;
                                btnStopRead.IsEnabled = false;
                                btnClear.IsEnabled = true;
                                btnSave.IsEnabled = true;
                                return;
                            }
                            faultCount++;
                            timer.Start();
                            DdProtocol.DdInstance.ReadDidiNextRecordData();
                        }
                        else
                        {
                            if (errorCount == 3)
                            {
                                MessageBox.Show("多次读取到异常数据，请查找原因！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                                DdProtocol.DdInstance.m_bIsStopCommunication = false;
                                isRead = false;
                                btnReadAllData.IsEnabled = true;
                                btnReadData.IsEnabled = true;
                                btnStopRead.IsEnabled = false;
                                return;
                            }
                            errorNum++;
                            if(errorNum == 3)
                            {
                                errorNum = 0;
                                errorCount++;
                                timer.Start();
                                DdProtocol.DdInstance.ReadDidiNextRecordData();
                            }
                            else
                            {
                                timer.Start();
                                DdProtocol.DdInstance.ReadDidiCurrentRecordData();
                            }
                        }
                    }
                    else
                    {
                        timer.Stop();
                        H5DidiRecordInfo info = UpdateDidiRecord(e.RecvMsg);
                        if (info != null)
                        {
                            info.Index = m_ListRecordsInfo.Count + 1;
                            m_ListRecordsInfo.Add(info);
                        }
                        DdProtocol.bReadDdBmsResp = true;
                        Thread.Sleep(100);
                        timer.Start();
                        DdProtocol.DdInstance.ReadDidiRecordCount();
                    }
                }
            }
            catch (Exception ex)
            {
                btnReadData.IsEnabled = true;
                btnReadAllData.IsEnabled = true;
                btnStopRead.IsEnabled = false;
                btnClear.IsEnabled = true;
                btnSave.IsEnabled = true;
                MessageBox.Show(ex.Message);
            }
        }
        public void HandleGetRecordCountEvent(object sender, CustomRecvDataEventArgs e)
        {
            if (e.RecvMsg.Count == 8)
            {
                timer.Stop();
                int count = GetRecordCount(e.RecvMsg);
                if (count == 0)
                {
                    MessageBox.Show("备份数据读取完成！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    DdProtocol.DdInstance.m_bIsStopCommunication = false;
                    isRead = false;
                    btnReadData.IsEnabled = true;
                    btnReadAllData.IsEnabled = true;
                    btnStopRead.IsEnabled = false;
                    btnClear.IsEnabled = true;
                    btnSave.IsEnabled = true;
                }
                else if (count < 0)
                {
                    MessageBox.Show("备份数据读取异常！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    DdProtocol.DdInstance.m_bIsStopCommunication = false;
                    isRead = false;
                    btnReadData.IsEnabled = true;
                    btnReadAllData.IsEnabled = true;
                    btnStopRead.IsEnabled = false;
                    btnClear.IsEnabled = true;
                    btnSave.IsEnabled = true;
                }
                else
                {
                    DdProtocol.bReadDdBmsResp = true;
                    timer.Start();
                    DdProtocol.DdInstance.ReadDidiRecord();
                }
            }
        }
        public void HandleEraseInfoDataEvent(object sender, CustomRecvDataEventArgs e)
        {
            if(isErase)
            {
                DdProtocol.bReadDdBmsResp = true;
                if (e.RecvMsg[0] == 0xDD && e.RecvMsg[1] == 0xBD && e.RecvMsg.Count == (e.RecvMsg[2] << 8 | e.RecvMsg[3]))
                {
                    var res = e.RecvMsg[5] << 8 | e.RecvMsg[4];
                    if (res == 0)
                    {
                        MessageBox.Show("备份数据擦除成功！", "擦除提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        m_ListRecordsInfo.Clear();
                    }
                    else
                    {
                        MessageBox.Show("备份数据擦除失败！", "擦除提示", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                isErase = false;
            }
        }

        private void btnStopRead_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            isStopRead = true;
            isRead = false;
            btnReadData.IsEnabled = true;
            btnReadAllData.IsEnabled = true;
            btnStopRead.IsEnabled = false;
            btnClear.IsEnabled = true;
            btnSave.IsEnabled = true;
            DdProtocol.DdInstance.m_bIsStopCommunication = false;
        }
    }
}
