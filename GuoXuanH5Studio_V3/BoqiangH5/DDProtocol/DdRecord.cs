using BoqiangH5Entity;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Controls;
using BoqiangH5Repository;
using BoqiangH5.DDProtocol;
using System.Linq;
using System.Text;

namespace BoqiangH5
{
    public partial class UserCtrlDidiRecord :UserControl
    {
        public H5DidiRecordInfo UpdateDidiRecord(List<byte> rdBuf)
        {
            try
            {
                int offset = 0;
                if (rdBuf[0] != 0x03)
                {
                    if (rdBuf[0] != 0xCC || rdBuf[1] != 0xA4)
                    {
                        return null;
                    }
                    if (rdBuf.Count < (rdBuf[2] << 8 | rdBuf[3]))
                    {
                        return null;
                    }
                    offset += 6;
                }
                else
                {
                    if (rdBuf.Count < 0x48)
                    {
                        return null;
                    }
                    offset += 2;
                }

                H5DidiRecordInfo recordInfo = new H5DidiRecordInfo();
                if(rdBuf[0] == 0xCC || rdBuf[1] == 0xA4)
                {
                    if(rdBuf[4] == 0x00 && rdBuf[5] == 0x05)
                    {
                        recordInfo.EventType = "错误历史故障";
                        recordInfo.RecordType = "读取数据记录的校验不通过";
                    }
                    else if(rdBuf[4] == 0x00 && rdBuf[5] == 0x06)
                    {
                        recordInfo.EventType = "错误历史故障";
                        recordInfo.RecordType = "与外挂FLASH通讯失败";
                    }
                    else
                    {
                        string value = (rdBuf[offset] & 0xFF).ToString("X2");
                        if (recordEventTypeDic.Keys.Contains(value))
                        {
                            recordInfo.RecordType = recordEventTypeDic[value].Item1;
                            if(recordEventTypeDic[value].Item2 == "operation")
                            {
                                recordInfo.EventType = "操作事件";
                            }
                            else if(recordEventTypeDic[value].Item2 == "status")
                            {
                                recordInfo.EventType = "状态事件";
                            }
                            else
                            {
                                recordInfo.EventType = "未知事件";
                            }
                        }
                        else
                        {
                            recordInfo.RecordType = value;
                            recordInfo.EventType = "未知事件";
                        }
                    }

                    offset += 2;
                    StringBuilder sb = new StringBuilder();
                    sb.Append("curr__addr：");
                    sb.Append(rdBuf[offset + 1].ToString("X2"));
                    sb.Append(" ");
                    sb.Append(rdBuf[offset].ToString("X2"));
                    sb.Append(" ");
                    sb.Append(rdBuf[offset + 3].ToString("X2"));
                    sb.Append(" ");
                    sb.Append(rdBuf[offset + 2].ToString("X2"));
                    sb.Append("   ");
                    sb.Append("read__start：");
                    sb.Append(rdBuf[offset + 5].ToString("X2"));
                    sb.Append(" ");
                    sb.Append(rdBuf[offset + 4].ToString("X2"));
                    sb.Append(" ");
                    sb.Append(rdBuf[offset + 7].ToString("X2"));
                    sb.Append(" ");
                    sb.Append(rdBuf[offset + 6].ToString("X2"));
                    sb.Append("   ");
                    sb.Append("recodenums：");
                    sb.Append(rdBuf[offset + 9].ToString("X2"));
                    sb.Append(" ");
                    sb.Append(rdBuf[offset + 8].ToString("X2"));
                    sb.Append("   ");
                    sb.Append("cycle__flag：");
                    sb.Append(rdBuf[offset + 11].ToString("X2"));
                    sb.Append(" ");
                    sb.Append(rdBuf[offset + 10].ToString("X2"));
                    offset += 12;//6个uint数组的保留
                    LabStatus.Content = sb.ToString();

                    recordInfo.FCC = (int)(rdBuf[offset] | rdBuf[offset + 1] << 8 | rdBuf[offset + 2] << 16 | rdBuf[offset + 3] << 24); offset += 4;
                    recordInfo.LoopNumber = (int)(rdBuf[offset] | rdBuf[offset + 1] << 8); offset += 2;
                    offset += 12;//6个uint数组的保留
                    recordInfo.PackStatus = string.Format("{0} {1}", rdBuf[offset + 1].ToString("X2"), rdBuf[offset].ToString("X2")); offset += 2;
                    recordInfo.MosStatus = string.Format("{0} {1}", rdBuf[offset + 1].ToString("X2"), rdBuf[offset].ToString("X2")); offset += 2;
                    recordInfo.VoltageStatus = string.Format("{0} {1}", rdBuf[offset + 1].ToString("X2"), rdBuf[offset].ToString("X2")); offset += 2;
                    recordInfo.CurrentStatus = string.Format("{0} {1}", rdBuf[offset + 1].ToString("X2"), rdBuf[offset].ToString("X2")); offset += 2;
                    recordInfo.TemperatureStatus = string.Format("{0} {1}", rdBuf[offset + 1].ToString("X2"), rdBuf[offset].ToString("X2")); offset += 2;
                    recordInfo.HumidityStatus = string.Format("{0} {1}", rdBuf[offset + 1].ToString("X2"), rdBuf[offset].ToString("X2")); offset += 2;
                    recordInfo.ConfigStatus = string.Format("{0} {1}", rdBuf[offset + 1].ToString("X2"), rdBuf[offset].ToString("X2")); offset += 2;
                    recordInfo.CommunicationStatus = string.Format("{0} {1}", rdBuf[offset + 1].ToString("X2"), rdBuf[offset].ToString("X2")); offset += 2;
                    recordInfo.ModeStatus = string.Format("{0} {1}", rdBuf[offset + 1].ToString("X2"), rdBuf[offset].ToString("X2")); offset += 2;
                    recordInfo.LogicStatus = string.Format("{0} {1}", rdBuf[offset + 1].ToString("X2"), rdBuf[offset].ToString("X2")); offset += 2;
                }
                string year = rdBuf[offset].ToString().PadLeft(2,'0'); offset += 1;
                string month = rdBuf[offset].ToString().PadLeft(2, '0'); offset += 1;
                string day = rdBuf[offset].ToString().PadLeft(2, '0'); offset += 1;
                string hour = rdBuf[offset].ToString().PadLeft(2, '0'); offset += 1;
                string minute = rdBuf[offset].ToString().PadLeft(2, '0'); offset += 1;
                string second = rdBuf[offset].ToString().PadLeft(2, '0'); offset += 1;
                recordInfo.RecordTime = string.Format("{0}年{1}月{2}日 {3}时{4}分{5}秒", "20" + year, month, day, hour, minute, second);
                recordInfo.TotalVoltage = (double)(rdBuf[offset + 3] | rdBuf[offset + 2] << 8 | rdBuf[offset + 1] << 16 | rdBuf[offset] << 24); offset += 4;
                recordInfo.Current = (rdBuf[offset + 3] | rdBuf[offset + 2] << 8 | rdBuf[offset + 1] << 16 | rdBuf[offset] << 24).ToString(); offset += 4;
                recordInfo.Cell1Temp = ((rdBuf[offset + 1] | rdBuf[offset] << 8) - 2731) / 10.0; offset += 2;
                recordInfo.Cell2Temp = ((rdBuf[offset + 1] | rdBuf[offset] << 8) - 2731) / 10.0; offset += 2;
                recordInfo.Cell3Temp = ((rdBuf[offset + 1] | rdBuf[offset] << 8) - 2731) / 10.0; offset += 2;
                recordInfo.Cell4Temp = ((rdBuf[offset + 1] | rdBuf[offset] << 8) - 2731) / 10.0; offset += 2;
                recordInfo.Cell5Temp = ((rdBuf[offset + 1] | rdBuf[offset] << 8) - 2731) / 10.0; offset += 2;
                recordInfo.Humidity = (rdBuf[offset + 1] | rdBuf[offset] << 8); offset += 2;
                byte[] bytes = new byte[2]; Buffer.BlockCopy(rdBuf.ToArray(), offset, bytes, 0, bytes.Length); Array.Reverse(bytes);
                recordInfo.Cell1Voltage = BitConverter.ToInt16(bytes,0); offset += 2; Buffer.BlockCopy(rdBuf.ToArray(), offset, bytes, 0, bytes.Length); Array.Reverse(bytes);
                recordInfo.Cell2Voltage = BitConverter.ToInt16(bytes, 0); offset += 2; Buffer.BlockCopy(rdBuf.ToArray(), offset, bytes, 0, bytes.Length); Array.Reverse(bytes);
                recordInfo.Cell3Voltage = BitConverter.ToInt16(bytes, 0); offset += 2; Buffer.BlockCopy(rdBuf.ToArray(), offset, bytes, 0, bytes.Length); Array.Reverse(bytes);
                recordInfo.Cell4Voltage = BitConverter.ToInt16(bytes, 0); offset += 2; Buffer.BlockCopy(rdBuf.ToArray(), offset, bytes, 0, bytes.Length); Array.Reverse(bytes);
                recordInfo.Cell5Voltage = BitConverter.ToInt16(bytes, 0); offset += 2; Buffer.BlockCopy(rdBuf.ToArray(), offset, bytes, 0, bytes.Length); Array.Reverse(bytes);
                recordInfo.Cell6Voltage = BitConverter.ToInt16(bytes, 0); offset += 2; Buffer.BlockCopy(rdBuf.ToArray(), offset, bytes, 0, bytes.Length); Array.Reverse(bytes);
                recordInfo.Cell7Voltage = BitConverter.ToInt16(bytes, 0); offset += 2; Buffer.BlockCopy(rdBuf.ToArray(), offset, bytes, 0, bytes.Length); Array.Reverse(bytes);
                recordInfo.Cell8Voltage = BitConverter.ToInt16(bytes, 0); offset += 2; Buffer.BlockCopy(rdBuf.ToArray(), offset, bytes, 0, bytes.Length); Array.Reverse(bytes);
                recordInfo.Cell9Voltage = BitConverter.ToInt16(bytes, 0); offset += 2; Buffer.BlockCopy(rdBuf.ToArray(), offset, bytes, 0, bytes.Length); Array.Reverse(bytes);
                recordInfo.Cell10Voltage = BitConverter.ToInt16(bytes, 0); offset += 2; Buffer.BlockCopy(rdBuf.ToArray(), offset, bytes, 0, bytes.Length); Array.Reverse(bytes);
                recordInfo.Cell11Voltage = BitConverter.ToInt16(bytes, 0); offset += 2; Buffer.BlockCopy(rdBuf.ToArray(), offset, bytes, 0, bytes.Length); Array.Reverse(bytes);
                recordInfo.Cell12Voltage = BitConverter.ToInt16(bytes, 0); offset += 2; Buffer.BlockCopy(rdBuf.ToArray(), offset, bytes, 0, bytes.Length); Array.Reverse(bytes);
                recordInfo.Cell13Voltage = BitConverter.ToInt16(bytes, 0); offset += 2; Buffer.BlockCopy(rdBuf.ToArray(), offset, bytes, 0, bytes.Length); Array.Reverse(bytes);
                recordInfo.Cell14Voltage = BitConverter.ToInt16(bytes, 0); offset += 2; Buffer.BlockCopy(rdBuf.ToArray(), offset, bytes, 0, bytes.Length); Array.Reverse(bytes);
                recordInfo.Cell15Voltage = BitConverter.ToInt16(bytes, 0); offset += 2; Buffer.BlockCopy(rdBuf.ToArray(), offset, bytes, 0, bytes.Length); Array.Reverse(bytes);
                recordInfo.Cell16Voltage = BitConverter.ToInt16(bytes, 0); offset += 2;
                offset += 4;
                offset += 1;
                recordInfo.DischargeEnableStatus = (rdBuf[offset] & 0x08) == 0x08 ? "闭合" : "断开";
                recordInfo.DetStatus = (rdBuf[offset] & 0x04) == 0x04 ? "闭合" : "断开";
                recordInfo.DischargeMOSStatus = (rdBuf[offset] & 0x02) == 0x02 ? "闭合" : "断开";
                recordInfo.ChargeMOSStatus = (rdBuf[offset] & 0x01) == 0x01 ? "闭合" : "断开";
                offset += 1;
                recordInfo.SOC = (uint)rdBuf[offset]; offset += 2;
                //string val = (rdBuf[offset] << 24 | rdBuf[offset + 1] << 16 | rdBuf[offset + 2] << 8 | rdBuf[offset + 3]).ToString("X8"); offset += 4;
                //if (recordTypeDic.Keys.Contains(val))
                //    recordInfo.BatteryStatus = recordTypeDic[val];
                //else
                //    recordInfo.BatteryStatus = "0x" +  val;
                byte[] _bytes = new byte[4] { rdBuf[offset], rdBuf[offset + 1], rdBuf[offset + 2], rdBuf[offset + 3] };
                offset += 4;
                recordInfo.BatteryStatus = GetDidiRecordType(_bytes);
                recordInfo.Balance = "0x" + (rdBuf[offset] << 8 | rdBuf[offset + 1]).ToString("X4"); offset += 2;
                return recordInfo;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public int GetRecordCount(List<byte> rdBuf)
        {
            try
            {
                if (rdBuf[0] != 0x03 || rdBuf.Count != 0x08)
                {
                    return -1;
                }
                //int count = (rdBuf[2]<< 8 | rdBuf[3]);
                int count = rdBuf[2];
                return count;
            }
            catch(Exception ex)
            {
                return -1;
            }
        }

        string GetDidiRecordType(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            int offset = 0;
            if ((bytes[offset] & 0x20) == 0x20) sb.Append("系统复位；");
            if ((bytes[offset] & 0x10) == 0x10) sb.Append("系统关机；");
            if ((bytes[offset] & 0x08) == 0x08) sb.Append("系统开机；");
            if ((bytes[offset] & 0x04) == 0x04) sb.Append("非法充电；");
            if ((bytes[offset] & 0x02) == 0x02) sb.Append("总压过低保护；");
            if ((bytes[offset] & 0x01) == 0x01) sb.Append("总压过高保护；");
            offset++;
            if ((bytes[offset] & 0x80) == 0x80) sb.Append("MOS过温；");
            if ((bytes[offset] & 0x40) == 0x40) sb.Append("温差过大故障；");
            if ((bytes[offset] & 0x20) == 0x20) sb.Append("充电压差过大故障；");
            if ((bytes[offset] & 0x10) == 0x10) sb.Append("电芯掉线；");
            if ((bytes[offset] & 0x08) == 0x08) sb.Append("放电保护；");
            if ((bytes[offset] & 0x04) == 0x04) sb.Append("充电保护；");
            if ((bytes[offset] & 0x02) == 0x02) sb.Append("压差过大故障；");
            if ((bytes[offset] & 0x01) == 0x01) sb.Append("压差过大告警；");
            offset++;
            if ((bytes[offset] & 0x80) == 0x80) sb.Append("电芯故障；");
            if ((bytes[offset] & 0x40) == 0x40) sb.Append("放电MOS故障；");
            if ((bytes[offset] & 0x20) == 0x20) sb.Append("充电MOS故障；");
            if ((bytes[offset] & 0x10) == 0x10) sb.Append("电芯严重过压保护；");
            if ((bytes[offset] & 0x08) == 0x08) sb.Append("电芯过压二级保护；");
            if ((bytes[offset] & 0x04) == 0x04) sb.Append("电芯过压一级保护；");
            if ((bytes[offset] & 0x02) == 0x02) sb.Append("充电低温；");
            if ((bytes[offset] & 0x01) == 0x01) sb.Append("充电过温；");
            offset++;
            if ((bytes[offset] & 0x80) == 0x80) sb.Append("充电过流；");
            if ((bytes[offset] & 0x40) == 0x40) sb.Append("二级欠压保护；");
            if ((bytes[offset] & 0x20) == 0x20) sb.Append("欠压保护；");
            if ((bytes[offset] & 0x10) == 0x10) sb.Append("短路保护；");
            if ((bytes[offset] & 0x08) == 0x08) sb.Append("放电低温；");
            if ((bytes[offset] & 0x04) == 0x04) sb.Append("放电过温；");
            if ((bytes[offset] & 0x02) == 0x02) sb.Append("放电二级过流；");
            if ((bytes[offset] & 0x01) == 0x01) sb.Append("放电一级过流；");

            if (sb.Length == 0)
                sb.Append("0x" + (bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3]).ToString("X8"));
            return sb.ToString().Trim('；');
        }
    }
}
