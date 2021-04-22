using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using BoqiangH5Entity;

namespace BoqiangH5
{
    public partial class UserCtrlProtectParam : UserControl
    {
        public void UpdateProtectParam(List<byte> rdBuf,List<H5ProtectParamInfo> list)
        {
            if (rdBuf[0] == 0xCC  && rdBuf.Count < (rdBuf[2] << 8 | rdBuf[3]))
            {
                return;
            }

            int offset = 4;
            byte[] array = rdBuf.ToArray();
            bool isLittleEndian = BitConverter.IsLittleEndian;
            
            for (int n = 0; n < list.Count; n++)
            {
                byte[] bytes = new byte[list[n].ByteCount];
                Buffer.BlockCopy(array, offset, bytes, 0, bytes.Length);
                if(!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(bytes);
                }
                if (list[n].ByteCount == 2)
                {
                    if(list[n].isUnsigned)
                    {
                        ushort paramVal = BitConverter.ToUInt16(bytes, 0);
                        if(list[n].Unit == "℃")//使用单位，区分温度值
                        {
                            if(list[n].Description == "电芯温度差保护" || list[n].Description == "电芯温度差保护释放"
                                || list[n].Description == "电芯温度不平衡警告" || list[n].Description == "电芯温度不平衡警告释放")
                            {
                                list[n].StrValue = (paramVal / 10).ToString();
                            }
                            else
                                list[n].StrValue = ((paramVal - 2731) / 10).ToString();
                        }
                        else
                            list[n].StrValue = paramVal.ToString();
                    }
                    else
                    {
                        short paramVal = BitConverter.ToInt16(bytes, 0);
                        if (list[n].Unit == "℃")
                        {
                            if (list[n].Description == "电芯温度差保护" || list[n].Description == "电芯温度差保护释放"
                                                                || list[n].Description == "电芯温度不平衡警告" || list[n].Description == "电芯温度不平衡警告释放")
                            {
                                list[n].StrValue = (paramVal / 10).ToString();
                            }
                            else
                                list[n].StrValue = ((paramVal - 2731) / 10).ToString();
                        }
                        else
                            list[n].StrValue = paramVal.ToString();
                    }

                }
                else if(list[n].ByteCount == 4)
                {
                    if(list[n].isUnsigned)
                    {
                        uint paramVal = BitConverter.ToUInt32(bytes, 0);
                        list[n].StrValue = paramVal.ToString();
                    }
                    else
                    {
                        int paramVal = BitConverter.ToInt32(bytes, 0);
                        list[n].StrValue = paramVal.ToString();
                    }
                }
                offset += list[n].ByteCount;
            }
        }
    }
}
