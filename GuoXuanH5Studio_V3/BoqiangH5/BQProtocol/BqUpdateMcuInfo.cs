using BoqiangH5.BQProtocol;
using BoqiangH5Entity;
using BoqiangH5Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace BoqiangH5
{
    public partial class UserCtrlMCU : UserControl
    {

        int nMcuByteIndex = 1;
        public event EventHandler<EventArgs<string>> GetResistanceValEvent;
        string resistance = string.Empty;
        public void BqUpdateMcuInfo(List<byte> listRecv)
        {
            try
            {
                if (listRecv.Count < (listRecv[2] << 8 | listRecv[3]) || listRecv[0] != 0xCC || listRecv[1] != 0xA8)
                {
                    return;
                }

                BqProtocol.bReadBqBmsResp = true;

                nMcuByteIndex = 4;


                BqUpdateSysInfo1(listRecv);

                BqUpdateSysInfo2(listRecv);

                BqUpdateChargeInfo(listRecv);

                if(isRequestReadMcu)
                {
                    isRequestReadMcu = false;
                    GetResistanceValEvent?.Invoke(this, new EventArgs<string>(resistance));
                }
                else
                    MessageBox.Show("读取 MCU 参数成功！", "读取MCU提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception ex)
            {                
            }
        }

        private void BqUpdateSysInfo1(List<byte> listRecv)
        {
            for (int n = 0; n < ListSysInfo1.Count; n++)
            {
                string strVal = null;
                switch (ListSysInfo1[n].ByteCount)
                {
                    //case 1:
                    //    if(ListSysInfo1[n].Description == "自放电率")
                    //    {
                    //        strVal = (listRecv[nMcuByteIndex] & 0xFF).ToString();
                    //    }
                    //    else
                    //    {
                    //        strVal = listRecv[nMcuByteIndex].ToString();
                    //    }
                    //    break;

                    case 2:
                        strVal = BqUpdateMcuInfo_2Byte(ListSysInfo1[n], listRecv[nMcuByteIndex], listRecv[nMcuByteIndex + 1]);
                        if(ListSysInfo1[n].Description == "电流检测电阻")
                        {
                            resistance = strVal;
                        }
                        break;

                    case 4:
                        strVal = BqUpdateMcuInfo_4Byte(ListSysInfo1[n], listRecv, nMcuByteIndex);
                        break;

                    //case 16:
                    //    strVal = BqUpdateMcuInfo_16Byte(listRecv, nMcuByteIndex);
                    //    break;
                    default:
                        break;
                }

                ListSysInfo1[n].StrValue = strVal;

                nMcuByteIndex += ListSysInfo1[n].ByteCount;
            }
        }

        private void BqUpdateSysInfo2(List<byte> listRecv)
        {
            for (int n = 0; n < ListSysInfo2.Count; n++)
            {
                string strVal2 = null;
                switch (ListSysInfo2[n].ByteCount)
                {
                    //case 1:
                    //    strVal2 = listRecv[nMcuByteIndex].ToString();
                    //    break;

                    case 2:
                        strVal2 = BqUpdateMcuInfo_2Byte(ListSysInfo2[n], listRecv[nMcuByteIndex], listRecv[nMcuByteIndex + 1]);
                        break;

                    case 4:
                        strVal2 = BqUpdateMcuInfo_4Byte(ListSysInfo2[n], listRecv, nMcuByteIndex);
                        break;

                    //case 16:
                    //    strVal2 = BqUpdateMcuInfo_16Byte(listRecv, nMcuByteIndex);
                    //    break;
                    default:
                        break;
                }

                ListSysInfo2[n].StrValue = strVal2;

                nMcuByteIndex += ListSysInfo2[n].ByteCount;
            }
        }

        private void BqUpdateChargeInfo(List<byte> listRecv)
        {
            for (int n = 0; n < ListChargeInfo.Count; n++)
            {
                string strVal3 = null;
                switch (ListChargeInfo[n].ByteCount)
                {
                    //case 1:
                    //    strVal3 = listRecv[nMcuByteIndex].ToString();
                    //    break;

                    case 2:
                        strVal3 = BqUpdateMcuInfo_2Byte(ListChargeInfo[n], listRecv[nMcuByteIndex], listRecv[nMcuByteIndex + 1]);
                        break;

                    case 4:
                        strVal3 = BqUpdateMcuInfo_4Byte(ListChargeInfo[n], listRecv, nMcuByteIndex);
                        break;
                    default:
                        break;
                }

                ListChargeInfo[n].StrValue = strVal3;

                nMcuByteIndex += ListChargeInfo[n].ByteCount;
            }
        }

        private string BqUpdateMcuInfo_2Byte(H5BmsInfo nodeInfo,byte bt1, byte bt2)
        {
            byte[] byteVal = new byte[2] { bt2, bt1};
            if (nodeInfo.Description == "容量学习最低允许温度")
            {
                return ((((byteVal[0] << 8) | byteVal[1]) - 2731) / 10).ToString();
            }
            else
                return ((byteVal[0] << 8) | byteVal[1]).ToString();
        }

        private string BqUpdateMcuInfo_4Byte(H5BmsInfo nodeInfo, List<byte> listRecv, int nByteIndex)
        {
            //return ((listRecv[nByteIndex] << 24) | (listRecv[nByteIndex + 1] << 16) |
            //              (listRecv[nByteIndex + 2] << 8) | (listRecv[nByteIndex + 3])).ToString();
            return ((listRecv[nByteIndex + 3] << 24) | (listRecv[nByteIndex + 2] << 16) |
              (listRecv[nByteIndex + 1] << 8) | (listRecv[nByteIndex])).ToString();
        }
    }
}
