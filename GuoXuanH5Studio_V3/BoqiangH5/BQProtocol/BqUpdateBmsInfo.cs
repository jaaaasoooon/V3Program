using BoqiangH5.BQProtocol;
using BoqiangH5Entity;
using BoqiangH5Repository;
using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace BoqiangH5
{
    public partial class UserCtrlBqBmsInfo : UserControl
    {
        static SolidColorBrush brushGreen = new SolidColorBrush(Color.FromArgb(255, 150, 255, 150));
        static SolidColorBrush brushRed = new SolidColorBrush(Color.FromArgb(255, 255, 150, 150));
        //static SolidColorBrush brushGray = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
        static SolidColorBrush brushGray = new SolidColorBrush(Colors.LightGray);
        static SolidColorBrush brushYellow = new SolidColorBrush(Colors.Yellow);

        int nBqByteIndex = 1;
        static byte[] btHBalanceStatus = new byte[2];
        static byte[] btLBalanceStatus = new byte[2];
        static byte[] btBMSStatus = new byte[2];
        static byte[] btPackStatus = new byte[2];
        static byte[] btMOSStatus = new byte[2];
        static byte[] btVoltageStatus = new byte[2];
        static byte[] btCurrentStatus = new byte[2];
        static byte[] btTemperatureStatus = new byte[2];
        static byte[] btHumidityStatus = new byte[2];
        static byte[] btConfigStatus = new byte[2];
        static byte[] btCommunicationStatus = new byte[2];
        static byte[] btModeStatus = new byte[2];
        static byte[] btLogicStatus = new byte[2];
        bool isWarning = false;
        bool isProtect = false;
        public event EventHandler<EventArgs<List<bool>>> RefreshStatusEvent;
        public void BqUpdateBmsInfo(List<byte> rdBuf)
        {
            if (rdBuf[0] != 0xCC || rdBuf[1] != 0xA2)
            {
                return;
            }
            int len = rdBuf[2] << 8 | rdBuf[3];
            if (rdBuf.Count < len)
            {
                return;
            }
            //byte[] bytes = new byte[len - 2];
            //Buffer.BlockCopy(rdBuf.ToArray(), 0, bytes, 0, len - 2);
            //byte[] crc16 = CRC_Check.CRC16(bytes, 0, len - 2);
            //if(crc16[0] == rdBuf[len - 1] && crc16[1] == rdBuf[len - 2])
            {
                BqProtocol.bReadBqBmsResp = true;
                isWarning = false;
                isProtect = false;
                nBqByteIndex = 4;

                UpdateCellInfo(rdBuf);
                UpdateBmsInfo(rdBuf);

                UpdateStatusInfo(btPackStatus, m_ListPackStatus);
                UpdateStatusInfo(btMOSStatus, m_ListMosStatus);
                UpdateStatusInfo(btVoltageStatus, m_ListVoltageProtectStatus);
                UpdateStatusInfo(btCurrentStatus, m_ListCurrentProtectStatus);
                UpdateStatusInfo(btTemperatureStatus, m_ListTemperatureProtectStatus);
                UpdateStatusInfo(btHumidityStatus, m_ListHumidityProtectStatus);
                UpdateStatusInfo(btConfigStatus, m_ListConfigStatus);
                UpdateStatusInfo(btCommunicationStatus, m_ListCommunicationStatus);
                UpdateStatusInfo(btModeStatus, m_ListModeStatus);
                UpdateStatusInfo(btLogicStatus, m_ListLogicStatus);
                Array.Reverse(btHBalanceStatus);
                UpdateBalanceStatus(btHBalanceStatus, m_ListCellVoltage, true);
                Array.Reverse(btLBalanceStatus);
                UpdateBalanceStatus(btLBalanceStatus, m_ListCellVoltage, false);
                RefreshStatusEvent?.Invoke(this, new EventArgs<List<bool>>(new List<bool>() { isWarning, isProtect }));

                if(cbSimpleMode.IsChecked == true)
                {
                    m_ListErrorSysStatus.Clear();
                    foreach (var item in m_ListSysStatus)
                    {
                        if (item.IsSwitchOn == true)
                        {
                            m_ListErrorSysStatus.Add(item);
                        }
                    }

                    m_ListErrorProtectStatus.Clear();
                    foreach(var item in m_ListProtectStatus)
                    {
                        if(item.IsSwitchOn == true)
                        {
                            m_ListErrorProtectStatus.Add(item);
                        }
                    }
                }
            }
        }

        int cellMaxVoltage = 0;
        int cellMinVoltage = 0;
        int maxVoltageCellNum = 0;
        int minVoltageCellNum = 0;
        public void UpdateCellInfo(List<byte> rdBuf)
        {
            cellMinVoltage = -1;
            cellMaxVoltage = -1;
            maxVoltageCellNum = -1;
            minVoltageCellNum = -1;
            byte[] array = rdBuf.ToArray();
            for (int n = 0; n < m_ListCellVoltage.Count; n++)
            {
                int nCellVol = 0;
                //for (int m = m_ListCellVoltage[n].ByteCount - 1; m >= 0 ; m--)
                //{
                //    nCellVol = (nCellVol << 8 | rdBuf[nBqByteIndex + m]);
                //}
                byte[] bytes = new byte[m_ListCellVoltage[n].ByteCount];
                Buffer.BlockCopy(array, nBqByteIndex, bytes, 0, bytes.Length);
                //Array.Reverse(bytes);
                if (m_ListCellVoltage[n].Description == "电池包电压" || m_ListCellVoltage[n].Description == "实时电流")
                {
                    nCellVol = BitConverter.ToInt32(bytes, 0);
                }
                else
                {
                    nCellVol = BitConverter.ToInt16(bytes, 0);
                }

                m_ListCellVoltage[n].StrValue = nCellVol.ToString();
                if (m_ListCellVoltage[n].Description != "实时电流" && m_ListCellVoltage[n].Description != "电池包电压")
                {
                    if (nCellVol != 0)
                    {
                        if (nCellVol > cellMaxVoltage)
                        {
                            cellMaxVoltage = nCellVol;
                            maxVoltageCellNum = n - 1;
                        }
                        else
                        {
                            if (cellMinVoltage == -1 || nCellVol < cellMinVoltage)
                            {
                                cellMinVoltage = nCellVol;
                                minVoltageCellNum = n - 1;
                            }
                        }
                    }
                }
                nBqByteIndex += m_ListCellVoltage[n].ByteCount;
            }
        }
        int maxTemperature = 0;
        int minTemperature = 0;
        public void UpdateBmsInfo(List<byte> rdBuf)
        {
            maxTemperature = 0; minTemperature = 0;
            for (int i = 0; i < m_ListBmsInfo.Count; i++)
            {
                if (!m_ListBmsInfo[i].Description.Contains("状态"))
                {
                    if (m_ListBmsInfo[i].Description == "均衡通道_高")
                    {
                        for (int k = 0; k < m_ListBmsInfo[i].ByteCount; k++)
                        {
                            btHBalanceStatus[k] = rdBuf[nBqByteIndex + k];
                        }
                        m_ListBmsInfo[i].StrValue = string.Format("{0} {1}", btHBalanceStatus[1].ToString("X2"), btHBalanceStatus[0].ToString("X2"));
                    }
                    else if (m_ListBmsInfo[i].Description == "均衡通道_低")
                    {
                        for (int k = 0; k < m_ListBmsInfo[i].ByteCount; k++)
                        {
                            btLBalanceStatus[k] = rdBuf[nBqByteIndex + k];
                        }
                        m_ListBmsInfo[i].StrValue = string.Format("{0} {1}", btLBalanceStatus[1].ToString("X2"), btLBalanceStatus[0].ToString("X2"));
                    }
                    else
                    {
                        int nBmsVal = 0;
                        for (int j = m_ListBmsInfo[i].ByteCount - 1; j >= 0; j--)
                        {
                            nBmsVal = (nBmsVal << 8 | rdBuf[nBqByteIndex + j]);
                        }
                        if (m_ListBmsInfo[i].Description.Contains("温度"))
                        {
                            m_ListBmsInfo[i].StrValue = ((nBmsVal - 2731) / 10.0).ToString("F1");
                            if (nBmsVal > maxTemperature)
                                maxTemperature = nBmsVal;
                            else
                            {
                                if (minTemperature == 0 || nBmsVal < minTemperature)
                                    minTemperature = nBmsVal;
                            }
                        }
                        else if (m_ListBmsInfo[i].Description.Contains("FCC"))
                        {
                            m_ListBmsInfo[i].StrValue = (nBmsVal * decimal.Parse(m_ListBmsInfo[i].Scale)).ToString();
                        }
                        else if(m_ListBmsInfo[i].Description.Contains("-X") || m_ListBmsInfo[i].Description.Contains("-Y") || m_ListBmsInfo[i].Description.Contains("-Z"))
                        {
                            byte[] bytes = new byte[m_ListCellVoltage[i].ByteCount];
                            Buffer.BlockCopy(rdBuf.ToArray(), nBqByteIndex, bytes, 0, bytes.Length);
                            Int16 val = BitConverter.ToInt16(bytes,0);
                            m_ListBmsInfo[i].StrValue = (val / 1000.0).ToString();
                        }
                        else
                            m_ListBmsInfo[i].StrValue = (nBmsVal * decimal.Parse(m_ListBmsInfo[i].Scale)).ToString();
                    }
                }
                else
                {
                    byte[] bytes = new byte[2];
                    for (int k = 0; k < m_ListBmsInfo[i].ByteCount; k++)
                    {
                        bytes[k] = rdBuf[nBqByteIndex + k];
                    }
                    if (m_ListBmsInfo[i].Description == "BMS状态")
                    {
                        Buffer.BlockCopy(bytes, 0, btBMSStatus, 0, bytes.Length);
                        string strStatus = GetHexDataString(btBMSStatus);
                        if (strStatus == "00 00") m_ListBmsInfo[i].StrValue = "工作准备模式";
                        else if (strStatus == "00 01") m_ListBmsInfo[i].StrValue = "工作模式";
                        else if (strStatus == "00 10") m_ListBmsInfo[i].StrValue = "休眠准备模式";
                        else if (strStatus == "00 11") m_ListBmsInfo[i].StrValue = "休眠模式";
                        else if (strStatus == "00 12") m_ListBmsInfo[i].StrValue = "休眠退出模式";
                        else if (strStatus == "00 20") m_ListBmsInfo[i].StrValue = "过放准备模式";
                        else if (strStatus == "00 21") m_ListBmsInfo[i].StrValue = "过放模式";
                        else if (strStatus == "00 22") m_ListBmsInfo[i].StrValue = "过放退出模式";
                        else if (strStatus == "00 F0") m_ListBmsInfo[i].StrValue = "关机准备模式";
                        else if (strStatus == "00 F1") m_ListBmsInfo[i].StrValue = "关机模式";
                        else
                            m_ListBmsInfo[i].StrValue = strStatus;
                    }
                    else if (m_ListBmsInfo[i].Description == "电池包状态")
                    {
                        Buffer.BlockCopy(bytes, 0, btPackStatus, 0, bytes.Length);
                        m_ListBmsInfo[i].StrValue = GetHexDataString(btPackStatus);
                    }
                    else if (m_ListBmsInfo[i].Description == "MOS状态")
                    {
                        Buffer.BlockCopy(bytes, 0, btMOSStatus, 0, bytes.Length);
                        m_ListBmsInfo[i].StrValue = GetHexDataString(btMOSStatus);
                    }
                    else if (m_ListBmsInfo[i].Description == "电压保护状态")
                    {
                        Buffer.BlockCopy(bytes, 0, btVoltageStatus, 0, bytes.Length);
                        m_ListBmsInfo[i].StrValue = GetHexDataString(btVoltageStatus);
                    }
                    else if (m_ListBmsInfo[i].Description == "电流保护状态")
                    {
                        Buffer.BlockCopy(bytes, 0, btCurrentStatus, 0, bytes.Length);
                        m_ListBmsInfo[i].StrValue = GetHexDataString(btCurrentStatus);
                    }
                    else if (m_ListBmsInfo[i].Description == "温度保护状态")
                    {
                        Buffer.BlockCopy(bytes, 0, btTemperatureStatus, 0, bytes.Length);
                        m_ListBmsInfo[i].StrValue = GetHexDataString(btTemperatureStatus);
                    }
                    else if (m_ListBmsInfo[i].Description == "外挂通讯状态")
                    {
                        Buffer.BlockCopy(bytes, 0, btCommunicationStatus, 0, bytes.Length);
                        m_ListBmsInfo[i].StrValue = GetHexDataString(btCommunicationStatus);
                    }
                    else if (m_ListBmsInfo[i].Description == "湿度/进水状态")
                    {
                        Buffer.BlockCopy(bytes, 0, btHumidityStatus, 0, bytes.Length);
                        m_ListBmsInfo[i].StrValue = GetHexDataString(btHumidityStatus);
                    }
                    else if (m_ListBmsInfo[i].Description == "参数配置状态")
                    {
                        Buffer.BlockCopy(bytes, 0, btConfigStatus, 0, bytes.Length);
                        m_ListBmsInfo[i].StrValue = GetHexDataString(btConfigStatus);
                    }
                    else if (m_ListBmsInfo[i].Description == "模式状态")
                    {
                        Buffer.BlockCopy(bytes, 0, btModeStatus, 0, bytes.Length);
                        m_ListBmsInfo[i].StrValue = GetHexDataString(btModeStatus);
                    }
                    else if (m_ListBmsInfo[i].Description == "逻辑状态")
                    {
                        Buffer.BlockCopy(bytes, 0, btLogicStatus, 0, bytes.Length);
                        m_ListBmsInfo[i].StrValue = GetHexDataString(btLogicStatus);
                    }
                }

                nBqByteIndex += m_ListBmsInfo[i].ByteCount;
            }
        }
        private string GetHexDataString(byte[] bytes)
        {
            Array.Reverse(bytes);
            string Str = string.Empty;
            foreach (var it in bytes)
            {
                Str += it.ToString("X2");
                Str += " ";
            }
            return Str.Trim();
        }
        private void UpdateStatusInfo(byte[] byteArr, List<BitStatInfo> listBatInfo)
        {
            for (int k = 0; k < listBatInfo.Count; k++)
            {
                if (0 == ((1 << listBatInfo[k].BitIndex) & byteArr[listBatInfo[k].ByteIndex]))
                {
                    listBatInfo[k].IsSwitchOn = false;
                    listBatInfo[k].BackColor = brushGray;
                }
                else
                {
                    listBatInfo[k].IsSwitchOn = true;
                    if (listBatInfo[k].IsWarning)
                    {
                        isWarning = true;
                        listBatInfo[k].BackColor = brushYellow;
                    }
                    else
                    {
                        if (listBatInfo[k].IsProtect)
                        {
                            isProtect = true;
                            listBatInfo[k].BackColor = brushRed;
                        }
                        else
                        {
                            listBatInfo[k].BackColor = brushGreen;
                        }
                    }
                }

            }
        }

        private void UpdateBalanceStatus(byte[] byteArr, List<H5BmsInfo> listBmsInfo, bool ishigh)
        {
            int n = 2;
            if (ishigh)
            {
                n += 16;
                for (; n < listBmsInfo.Count; n++)
                {
                    int bitNoVal = 0;
                    if (n >= 18 && n < 26)
                    {
                        bitNoVal = (((int)Math.Pow(2, n - 18)) & byteArr[1]) == ((int)Math.Pow(2, n - 18)) ? 1 : 0;
                    }
                    else if (n >= 26)
                    {
                        bitNoVal = (((int)Math.Pow(2, n - 26)) & byteArr[0]) == ((int)Math.Pow(2, n - 26)) ? 1 : 0;
                    }
                    if (0 == bitNoVal)
                    {
                        listBmsInfo[n].BalanceStat = BoqiangH5Entity.BalanceStatusEnum.No;
                    }
                    else
                    {
                        listBmsInfo[n].BalanceStat = BoqiangH5Entity.BalanceStatusEnum.Yes;
                    }
                }
            }
            else
            {
                for (; n < listBmsInfo.Count - 16; n++)
                {
                    int bitNoVal = 0;
                    if (n < 10)
                    {
                        bitNoVal = (((int)Math.Pow(2, n - 2)) & byteArr[1]) == ((int)Math.Pow(2, n - 2)) ? 1 : 0;
                    }
                    else if (n >= 10 && n < 18)
                    {
                        bitNoVal = (((int)Math.Pow(2, n - 10)) & byteArr[0]) == ((int)Math.Pow(2, n - 10)) ? 1 : 0;
                    }
                    if (0 == bitNoVal)
                    {
                        listBmsInfo[n].BalanceStat = BoqiangH5Entity.BalanceStatusEnum.No;
                    }
                    else
                    {
                        listBmsInfo[n].BalanceStat = BoqiangH5Entity.BalanceStatusEnum.Yes;
                    }
                }
            }
            //for (; n < listBmsInfo.Count; n++)
            //{
            //    if (n < 10)
            //    {
            //        if (0 == ((1 << n - 2) & byteArr[1]))
            //        {
            //            listBmsInfo[n].BalanceStat = BoqiangH5Entity.BalanceStatusEnum.No;
            //        }
            //        else
            //        {
            //            listBmsInfo[n].BalanceStat = BoqiangH5Entity.BalanceStatusEnum.Yes;
            //        }
            //    }
            //    else if(n >= 10 && n <18)
            //    {
            //        if (0 == ((1 << (n - 10)) & byteArr[0]))
            //        {
            //            listBmsInfo[n].BalanceStat = BoqiangH5Entity.BalanceStatusEnum.No;
            //        }
            //        else
            //        {
            //            listBmsInfo[n].BalanceStat = BoqiangH5Entity.BalanceStatusEnum.Yes;
            //        }
            //    }
            //    else if (n >= 18 && n < 26)
            //    {
            //        if (0 == ((1 << (n - 18)) & byteArr[1]))
            //        {
            //            listBmsInfo[n].BalanceStat = BoqiangH5Entity.BalanceStatusEnum.No;
            //        }
            //        else
            //        {
            //            listBmsInfo[n].BalanceStat = BoqiangH5Entity.BalanceStatusEnum.Yes;
            //        }
            //    }
            //    else
            //    {
            //        if (0 == ((1 << (n - 26)) & byteArr[0]))
            //        {
            //            listBmsInfo[n].BalanceStat = BoqiangH5Entity.BalanceStatusEnum.No;
            //        }
            //        else
            //        {
            //            listBmsInfo[n].BalanceStat = BoqiangH5Entity.BalanceStatusEnum.Yes;
            //        }
            //    }
            //}
        }

    }
}
