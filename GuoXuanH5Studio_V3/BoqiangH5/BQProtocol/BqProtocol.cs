using BoqiangH5.ISO15765;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Timers;

namespace BoqiangH5.BQProtocol
{
    class BqProtocol
    {
        static BqProtocol m_bqInstance;

        public event EventHandler RaiseMenuBreakEvent;

        public System.Timers.Timer timer;

        public static  uint BqProtocolID = 0x1CEB0300;

        public static bool bReadBqBmsResp = true;

        public int nHandshakeFailure = 0;

        int nReadBmsTimes = 0;

        public bool m_bIsSaveBmsInfo = true;

        public bool m_bIsSaveCellInfo = true;

        public int m_bUpateBmsInterval = 500;

        public bool m_bIsStopCommunication = false;//用于下发消息时停止心跳、数据刷新和RTC的读取
        public bool m_bIsSendMultiFrame = false;
        public bool m_bIsStop = false;
        public bool m_bIsTest = false;
        byte m_bSourceAddress = 0x3A;


        private BqProtocol()
        {
        }
        public void SetProtocolID(uint id,byte _byte)
        {
            BqProtocolID = id;
            m_bSourceAddress = _byte;
        }
        public static BqProtocol BqInstance
        {
            get
            {
                if (m_bqInstance == null)
                {
                    m_bqInstance = new BqProtocol();
                }
                return m_bqInstance;
            }
        }

        #region  
        public void SetTimer()
        {
            timer = new System.Timers.Timer(1500);
            timer.Elapsed += OnTimerEvent;
            timer.AutoReset = true;
            //timer.Enabled = true;
        }

        private void OnTimerEvent(Object source, ElapsedEventArgs e)
        {
            if (isReturn == false)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    nHandshakeFailure = 0;
                    OnRaiseMenuBreakEvent(null, null);
                    BoqiangH5Repository.CSVFileHelper.WriteLogs("log", "error", "断开完成",true);
                }), null);
            }

        }

        public void StopTimer()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Close();
            }
        }
        bool isReturn = false;
        #endregion

        #region 超时定时器 
        public System.Timers.Timer timeOutTimer;
        public void SetTimeOutTimer()
        {
            timeOutTimer = new System.Timers.Timer(100);
            timeOutTimer.Elapsed += OnTimeOutTimerEvent;
            timeOutTimer.AutoReset = true;
        }
        int timeOutNum = 0;
        private void OnTimeOutTimerEvent(Object source, ElapsedEventArgs e)
        {
            if(m_bIsStopCommunication)
            {
                timeOutNum++;
                BoqiangH5Repository.CSVFileHelper.WriteLogs("log", "error", string.Format("超时计数 {0}",timeOutNum), true);
                if (timeOutNum == 5)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        System.Windows.MessageBox.Show("操作超时！","提示",System.Windows.MessageBoxButton.OK,System.Windows.MessageBoxImage.Information);
                    }));
                    timeOutNum = 0;
                    m_bIsStopCommunication = false;
                }
            }
            else
            {
                timeOutNum = 0;
            }
        }

        public void StopTimeOutTimer()
        {
            timeOutNum = 0;
            m_bIsStopCommunication = false;
            if (timeOutTimer != null)
            {
                timeOutTimer.Stop();
                timeOutTimer.Close();
            }
        }
        #endregion
        private void OnRaiseMenuBreakEvent(Object source, EventArgs e)
        {
            EventHandler handler = RaiseMenuBreakEvent;

            if (handler != null)
            {
                handler(this, e);
            }
        }


        public void ThreadReadMasterTeleData(object o)
        {
            while (!MainWindow.bIsBreak)
            {
                //if(MainWindow.m_statusBarInfo.IsOnline)
                {
                    if (m_bIsStop)
                    {
                        continue;
                    }
                    if (m_bIsStopCommunication)
                    {
                        continue;
                    }
                    if (m_bIsTest)
                    {
                        continue;
                    }
                    if (m_bIsSendMultiFrame)
                    {
                        continue;
                    }

                    if (nHandshakeFailure >= 3)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            //StopTimerHandshake();
                            OnRaiseMenuBreakEvent(null, null);
                            nHandshakeFailure = 0;
                            BoqiangH5Repository.CSVFileHelper.WriteLogs("log", "error", "断线",true);
                        }), null);
                    }
                    else
                    {
                        nHandshakeFailure++;
                        byte[] rdBmsBuf = new byte[] { m_bSourceAddress, 0x03,0xCC,0xA2,0x00,0x06,0x90, 0x85 };
                        //isReturn = false;
                        //timer.Start();
                        SendSingleFrameData(rdBmsBuf);
                        //timer.Stop();
                        //isReturn = true;
                        Thread.Sleep(m_bUpateBmsInterval);//lipeng  2020.03.27修改BMS信息刷新时间
                    }
                }
            }
        }

        public void ReadMcuData()
        {
            while (MainWindow.m_statusBarInfo.IsOnline)
            {
                byte[] rdMcuBuf = new byte[] { m_bSourceAddress, 0x03, 0xCC, 0xA8, 0x00, 0x06, 0x00, 0x00 };

                if (bReadBqBmsResp)
                {
                    SendSingleFrameData(rdMcuBuf);
                    break;
                }
                else
                {
                    ReadDataNoResponse();
                }     
            }
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       }
        
        public void ReadEepromData()
        {
            while (MainWindow.m_statusBarInfo.IsOnline)
            {    
                byte[] rdEepromBuf = new byte[] { m_bSourceAddress, 0x03, 0xCC, 0xB1, 0x00, 0x06, 0x00, 0x00 };

                if (bReadBqBmsResp)
                {
                    SendSingleFrameData(rdEepromBuf);
                    break;
                }
                else
                {
                    ReadDataNoResponse();
                }
                
            }
        }

        //lipeng 2020.03.26增加读取备份数据的命令
        public void ReadRecordData(int readtype)
        {
            while (MainWindow.m_statusBarInfo.IsOnline)
            {
                byte[] rdRecordmBuf;
                if(readtype == 0)
                    rdRecordmBuf = new byte[] { m_bSourceAddress, 0x03, 0xA6, 0x00, 0x00, 0x00 };
                else if(readtype == 1)
                    rdRecordmBuf = new byte[] { m_bSourceAddress, 0x03, 0xA6, 0x01, 0x00, 0x00 };
                else
                    rdRecordmBuf = new byte[] { m_bSourceAddress, 0x03, 0xA6, 0x02, 0x00, 0x00 };

                if (bReadBqBmsResp)
                {
                    SendSingleFrameData(rdRecordmBuf);
                    //BoqiangH5Repository.CSVFileHelper.WriteLogs("log", "send", "fasongshuju");
                    break;
                }
                else
                {
                    ReadDataNoResponse();
                }
            }
        }

        //lipeng 2020.03.31增加擦除数据的命令
        public void EraseRecord()
        {
            while (MainWindow.m_statusBarInfo.IsOnline)
            {
                byte[] rdRecordmBuf = new byte[] { m_bSourceAddress, 0x03, 0xD6,  0x00, 0x00 };

                if (bReadBqBmsResp)
                {
                    SendSingleFrameData(rdRecordmBuf);
                    break;
                }
                else
                {
                    ReadDataNoResponse();
                }
            }
        }

        private void SendSingleFrameData(byte[] rdBuf)
        {
            byte[] crc16 = CRC_Check.CRC16(rdBuf, 0, rdBuf.Length - 2);

            rdBuf[rdBuf.Length - 2] = crc16[1];
            rdBuf[rdBuf.Length - 1] = crc16[0];

            byte[] cmdBuf = new byte[rdBuf.Length - 2];
            Buffer.BlockCopy(rdBuf, 2, cmdBuf, 0, rdBuf.Length - 2);

            DataLinkLayer.SendCanFrame(BqProtocolID, cmdBuf);

            bReadBqBmsResp = false;
        }

        private void ReadDataNoResponse()
        {
            Thread.Sleep(200);
            nReadBmsTimes++;
            if (nReadBmsTimes > 3)
            {
                bReadBqBmsResp = true;
            }
        }


        public void BQ_ReadDeviceInfo()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xCC, 0xA0, 0x00,0x06,0x00,0x00 };

            SendSingleFrameData(buf);
        }

        public void BQ_WriteBMSInfo(byte[] bytes)
        {
            SendMultiFrame(bytes, bytes.Length, 0xB0);
        }
        public void BQ_WritePackInfo(byte[] bytes)
        {
            SendMultiFrame(bytes, bytes.Length, 0xB1);
        }

        public void BQ_JumpToBoot()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xD0, 0x00, 0x00 };

            SendSingleFrameData(buf);
        }

        public void BQ_Reset()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xDD,0xBC,0x00,0x08,0x00,0x00, 0x00, 0x00 };

            SendSingleFrameData(buf);
        }

        public void BQ_AlterSOC(byte[] byteSOC)
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xDD, 0xB9, 0x00, 0x08,0x00,0x00, 0x00, 0x00 };
            buf[6] = byteSOC[0];
            buf[7] = byteSOC[1];

            SendSingleFrameData(buf);
        }

        public void BQ_FactoryReset()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xDD,0xBE,0x00,0x08,0x00,0x00, 0x00, 0x00 };

            SendSingleFrameData(buf);
        }

        public void BQ_Shutdown()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xDD, 0xBA, 0x00, 0x08,0x00,0x00, 0x00, 0x00 };

            SendSingleFrameData(buf);
        }

        public void BQ_Sleep()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xDD, 0xBB, 0x00, 0x08,0x00,0x00, 0x00, 0x00 };

            SendSingleFrameData(buf);
        }

        public void BQ_OverDischarge()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xDD, 0xBF, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00 };

            SendSingleFrameData(buf);
        }
        public void BQ_ReadBootInfo()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xCC, 0xB9, 0x00, 0x06, 0x00, 0x00 };

            SendSingleFrameData(buf);
        }

        public void BQ_ReadUID()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xCC, 0xA1, 0x00 ,0x06,0x00,0x00};

            SendSingleFrameData(buf);
        }

        public void BQ_WriteManufacturingInformation (string dataStr)// lipeng  2020.4.2 制造信息写入
        {
            int d = int.Parse(dataStr);
            byte data = (byte)((d / 10) << 4 | (d % 10));
            byte[] buf = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            buf[1] = data;
            //SendSingleFrameData(buf);
            SendMultiFrame(buf, buf.Length, 0xDE);
        }

        public void BQ_ReadRTC()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03,0xCC,0xA3, 0x00, 0x06, 0x00, 0x00 };

            SendSingleFrameData(buf);
        }

        public void SendMultiFrame(byte[] dataBuf, int len, byte nCmd)
        {
            byte[] cmdBuf = new byte[len + 8];
            cmdBuf[0] = m_bSourceAddress;
            cmdBuf[1] = 0x03;
            cmdBuf[2] = 0xDD;
            cmdBuf[3] = nCmd;
            byte[] lenBuf = BitConverter.GetBytes((short)cmdBuf.Length - 2);
            cmdBuf[4] = lenBuf[1];
            cmdBuf[5] = lenBuf[0];
            Buffer.BlockCopy(dataBuf, 0, cmdBuf, 6, len);

            byte[] crc16 = CRC_Check.CRC16(cmdBuf, 0, cmdBuf.Length - 2);

            cmdBuf[cmdBuf.Length - 2] = crc16[1];
            cmdBuf[cmdBuf.Length - 1] = crc16[0];

            int nFrameLen = 0;
            if ((cmdBuf.Length - 2) % 8 == 0)
            {
                nFrameLen = (cmdBuf.Length - 2) / 8;
            }
            else
            {
                nFrameLen = (cmdBuf.Length - 2) / 8 + 1;
            }
            m_bIsSendMultiFrame = true;
            int nSendIndex = 2;
            for (int n = nFrameLen; n > 0; n--)
            {
                byte[] byteCmdBuf = new byte[8];
                if (nSendIndex + 8 > cmdBuf.Length)
                {
                    Buffer.BlockCopy(cmdBuf, nSendIndex, byteCmdBuf, 0, (cmdBuf.Length - nSendIndex));
                }
                else
                {
                    Buffer.BlockCopy(cmdBuf, nSendIndex, byteCmdBuf, 0, 8);
                }

                uint BqProtID = (uint)(0x1CEB0300 | (n - 1));
                DataLinkLayer.SendCanFrame(BqProtID, byteCmdBuf);
              
                nSendIndex += 8;
                Thread.Sleep(5);
            }
            m_bIsSendMultiFrame = false;
        }


        public void AdjustRtCurrent(int nRTCurrent)
        {
            byte[] arrRtCur = BitConverter.GetBytes(nRTCurrent);

            //byte[] adCmdBuf = new byte[] { m_bSourceAddress, 0x03,0xDD,0xA9, 0x00,0x08, 0x00, 0x00, 0x00, 0x00 };

            //adCmdBuf[6] = arrRtCur[1];
            //adCmdBuf[7] = arrRtCur[0];

            //SendSingleFrameData(adCmdBuf);
            SendMultiFrame(arrRtCur, arrRtCur.Length, 0xA9);
        }

        //lipeng 2020.03.31增加校准RTC的命令
        public void AdjustRTC(DateTime dt)
        {
            TimeSpan ts = dt - (new DateTime(1970, 1, 1, 8, 0, 0));
            byte[] data = BitConverter.GetBytes(((uint)(ts.Ticks / Math.Pow(10, 7))));
            byte[] rdBuf = new byte[4];
            Buffer.BlockCopy(data, 0, rdBuf, 0, rdBuf.Length);
            //Array.Reverse(rdBuf);
            SendMultiFrame(rdBuf, rdBuf.Length,0xA7);
        }
        public void AdjustZeroCurrent(int nCurrent)
        {
            byte[] arrRtCur = BitConverter.GetBytes(nCurrent);

            //byte[] adCmdBuf = new byte[] { m_bSourceAddress, 0x03, 0xDD, 0xA8, 0x00, 0x08, 0x00, 0x00,0x00,0x00, 0x00, 0x00 };

            //adCmdBuf[10] = arrRtCur[1];
            //adCmdBuf[11] = arrRtCur[0];

            //SendSingleFrameData(adCmdBuf);
            SendMultiFrame(arrRtCur, arrRtCur.Length, 0xA8);
        }

        public void BQ_EnterTestMode()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03,0xDD,0xB8,0x00, 0x08,0x01, 0x00, 0x00, 0x00 };

            SendSingleFrameData(buf);
        }

        public void BQ_ExitTestMode()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xDD, 0xB8, 0x00, 0x08,0x00, 0x00, 0x00, 0x00 };

            SendSingleFrameData(buf);
        }

        public void BQ_ReadVoltageProtectParam()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xCC, 0xA9, 0x00, 0x06,0x00,0x00 };

            SendSingleFrameData(buf);
        }

        public void BQ_WriteVoltageProtectParam(List<byte> bytes)
        {
            SendMultiFrame(bytes.ToArray(), bytes.Count, 0xA0);
        }

        public void BQ_ReadCurrentProtectParam()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xCC, 0xAA, 0x00, 0x06, 0x00, 0x00 };

            SendSingleFrameData(buf);
        }

        public void BQ_WriteCurrentProtectParam(List<byte> bytes)
        {
            SendMultiFrame(bytes.ToArray(), bytes.Count, 0xA1);
        }

        public void BQ_ReadTemperatureProtectParam()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xCC, 0xAB, 0x00, 0x06,0x00, 0x00 };

            SendSingleFrameData(buf);
        }

        public void BQ_WriteTemperatureProtectParam(List<byte> bytes)
        {
            SendMultiFrame(bytes.ToArray(), bytes.Count, 0xA2);
        }

        public void BQ_ReadWarningProtectParam()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xCC, 0xAC, 0x00, 0x06, 0x00, 0x00 };

            SendSingleFrameData(buf);
        }
        public void BQ_WriteWarningProtectParam(List<byte> bytes)
        {
            SendMultiFrame(bytes.ToArray(), bytes.Count, 0xA3);
        }
        public void BQ_ReadHumidityProtectParam()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xCC, 0xAD, 0x00, 0x06, 0x00, 0x00 };

            SendSingleFrameData(buf);
        }

        public void BQ_WriteHumidityProtectParam(List<byte> bytes)
        {
            SendMultiFrame(bytes.ToArray(), bytes.Count, 0xA4);
        }

        public void BQ_RequireReadEepromRegister()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xCC, 0xB0, 0x00, 0x06, 0x00, 0x00 };

            SendSingleFrameData(buf);
        }

        public void BQ_RequireReadOthersRegister()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xCC, 0xB2, 0x00, 0x06, 0x00, 0x00 };

            SendSingleFrameData(buf);
        }

        public void BQ_ReadOthersRegister()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xCC, 0xB3, 0x00, 0x06, 0x00, 0x00 };

            SendSingleFrameData(buf);
        }

        public void BQ_ReadAdjustParam()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xCC, 0xAE, 0x00, 0x06, 0x00, 0x00 };

            SendSingleFrameData(buf);
        }

        public void BQ_AdjustBatteryVoltageParam(uint voltage)
        {
            byte[] buf = BitConverter.GetBytes(voltage);

            SendMultiFrame(buf, buf.Length, 0xAA);
        }

        public void BQ_AdjustLoadVoltageParam(uint voltage)
        {
            byte[] buf = BitConverter.GetBytes(voltage);

            SendMultiFrame(buf, buf.Length, 0xAB);
        }

        public void BQ_AdjustInnerResistanceParam(ushort resistance)
        {
            byte[] array = BitConverter.GetBytes(resistance);
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xDD, 0xAC, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00 };
            buf[6] = array[0];
            buf[7] = array[1];
            SendSingleFrameData(buf);
        }

        public void BQ_AdjustOutResistanceParam(ushort resistance)
        {
            byte[] array = BitConverter.GetBytes(resistance);
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xDD, 0xAD, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00 };
            buf[6] = array[0];
            buf[7] = array[1];
            SendSingleFrameData(buf);
        }

        public void BQ_ExitChargeMos()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xDD, 0xC0, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00 };
            SendSingleFrameData(buf);
        }
        public void BQ_CloseChargeMos()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xDD, 0xC0, 0x00, 0x08, 0x01, 0x00, 0x00, 0x00 };
            SendSingleFrameData(buf);
        }
        public void BQ_OpenChargeMos()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xDD, 0xC0, 0x00, 0x08, 0x02, 0x00, 0x00, 0x00 };
            SendSingleFrameData(buf);
        }
        public void BQ_ExitDischargeMos()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xDD, 0xC1, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00 };
            SendSingleFrameData(buf);
        }
        public void BQ_CloseDischargeMos()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xDD, 0xC1, 0x00, 0x08, 0x01, 0x00, 0x00, 0x00 };
            SendSingleFrameData(buf);
        }
        public void BQ_OpenDischargeMos()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xDD, 0xC1, 0x00, 0x08, 0x02, 0x00, 0x00, 0x00 };
            SendSingleFrameData(buf);
        }

        public void BQ_ReadFlash()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xCC, 0xBA, 0x00, 0x06, 0x00, 0x00 };

            SendSingleFrameData(buf);
        }

        public void BQ_EnterProductionMode(byte val)
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xDD, 0xC3, 0x00, 0x08, val, 0x00, 0x00, 0x00 };
            SendSingleFrameData(buf);
        }

        public void BQ_ExitProductionMode()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0xDD, 0xC3, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00 };

            SendSingleFrameData(buf);
        }

        #region
        public UpdateInformation BQ_RequestUpdateApp()
        {
            byte[] buf = new byte[] { m_bSourceAddress, 0x03, 0x45, 0x01, 0x00, 0x00,0x00,0x00 };
            SendSingleFrameData(buf);

            UpdateInformation info = new UpdateInformation();
            info.DirectionStr = "发送";
            byte[] _bytes = new byte[6];
            Buffer.BlockCopy(buf,2,_bytes,0,_bytes.Length);
            info.Content = BitConverter.ToString(_bytes);
            info.Length = _bytes.Length.ToString();
            info.Comments = "进入固件升级模式";
            info.TimeStr = DateTime.Now.ToString("MM/dd HH:mm:ss") + string.Format(":{0}", DateTime.Now.Millisecond);
            info.ID = string.Format("0x{0}", BqProtocolID.ToString("X"));
            return info;
        }

        public void BQ_SendUpdateAppInfo(byte[] bytes, List<UpdateInformation> queue, bool isAddQueue, object lockobj)
        {
            SendUpdateAppMultiFrame(bytes, bytes.Length, 0x13,queue,isAddQueue,lockobj);
        }

        public void BQ_SendUpdateAppData(byte[] bytes, List<UpdateInformation> queue,bool isAddQueue,object lockobj)
        {
            SendUpdateAppMultiFrame(bytes, bytes.Length, 0x15,queue,isAddQueue,lockobj);
        }

        public void SendUpdateAppMultiFrame(byte[] dataBuf, int len, byte nCmd, List<UpdateInformation> queue,bool isAddQueue,object lockobj)
        {
            byte[] cmdBuf = new byte[len + 8];
            cmdBuf[0] = m_bSourceAddress;
            cmdBuf[1] = 0x03;
            cmdBuf[2] = 0x45;
            cmdBuf[3] = nCmd;
            byte[] lenBuf = BitConverter.GetBytes((short)len);
            cmdBuf[4] = lenBuf[1];
            cmdBuf[5] = lenBuf[0];
            Buffer.BlockCopy(dataBuf, 0, cmdBuf, 6, len);

            byte[] crc16 = CRC_Check.CRC16(cmdBuf, 0, cmdBuf.Length - 2);

            cmdBuf[cmdBuf.Length - 2] = crc16[1];
            cmdBuf[cmdBuf.Length - 1] = crc16[0];

            int nFrameLen = 0;
            if ((cmdBuf.Length - 2) % 8 == 0)
            {
                nFrameLen = (cmdBuf.Length - 2) / 8;
            }
            else
            {
                nFrameLen = (cmdBuf.Length - 2) / 8 + 1;
            }
            m_bIsSendMultiFrame = true;
            int nSendIndex = 2;
            for (int n = nFrameLen; n > 0; n--)
            {
                byte[] byteCmdBuf = new byte[8];
                if (nSendIndex + 8 > cmdBuf.Length)
                {
                    Buffer.BlockCopy(cmdBuf, nSendIndex, byteCmdBuf, 0, (cmdBuf.Length - nSendIndex));
                }
                else
                {
                    Buffer.BlockCopy(cmdBuf, nSendIndex, byteCmdBuf, 0, 8);
                }

                uint BqProtID = (uint)(0x1CEB0300 | (n - 1));
                DataLinkLayer.SendCanFrame(BqProtID, byteCmdBuf);

                nSendIndex += 8;
                //Thread.Sleep(5);
                if(isAddQueue)
                {
                    UpdateInformation info = new UpdateInformation();
                    info.DirectionStr = "发送";
                    info.Length = byteCmdBuf.Length.ToString();
                    info.TimeStr = DateTime.Now.ToString("MM/dd HH:mm:ss") + string.Format(":{0}", DateTime.Now.Millisecond);
                    info.ID = string.Format("0x{0}", BqProtID.ToString("X"));
                    info.Content = BitConverter.ToString(byteCmdBuf);
                    if (n == nFrameLen)
                    {
                        if (nCmd == 0x13)
                            info.Comments = string.Format("根据最新固件信息获取固件状态—多帧第{0}帧，共{1}帧", nFrameLen - n + 1, nFrameLen);
                        else if (nCmd == 0x15)
                        {
                            uint index = (uint)(dataBuf[4] << 8 | dataBuf[5]);
                            info.Comments = string.Format("第{0}块从机升级包—多帧第{1}帧，共{2}帧", index, nFrameLen - n + 1, nFrameLen);
                        }
                    }
                    else
                    {
                        info.Comments = string.Format("第{0}帧，共{1}帧", nFrameLen - n + 1, nFrameLen);
                    }
                    //queue.Enqueue(info);
                    lock(lockobj)
                    {
                        queue.Add(info);
                    }
                }
            }
            m_bIsSendMultiFrame = false;
        }


        public void SendCommunicationMessage(byte[] bytes,List<UpdateInformation> infoList,object lockobj)
        {
            int nFrameLen = 0;
            if ((bytes.Length) % 8 == 0)
            {
                nFrameLen = (bytes.Length) / 8;
            }
            else
            {
                nFrameLen = (bytes.Length) / 8 + 1;
            }
            m_bIsSendMultiFrame = true;
            int nSendIndex = 0;
            for (int n = nFrameLen; n > 0; n--)
            {
                byte[] byteCmdBuf = new byte[8];
                if (nSendIndex + 8 > bytes.Length)
                {
                    Buffer.BlockCopy(bytes, nSendIndex, byteCmdBuf, 0, (bytes.Length - nSendIndex));
                }
                else
                {
                    Buffer.BlockCopy(bytes, nSendIndex, byteCmdBuf, 0, 8);
                }

                uint BqProtID = (uint)(0x1CEB0300 | (n - 1));
                DataLinkLayer.SendCanFrame(BqProtID, byteCmdBuf);

                nSendIndex += 8;

                UpdateInformation info = new UpdateInformation();
                info.DirectionStr = "发送";
                info.Length = byteCmdBuf.Length.ToString();
                info.TimeStr = DateTime.Now.ToString("MM/dd HH:mm:ss") + string.Format(":{0}", DateTime.Now.Millisecond);
                info.ID = string.Format("0x{0}",BqProtID.ToString("X"));
                info.Content = BitConverter.ToString(byteCmdBuf);
                if(nFrameLen > 1)
                {
                    info.Comments = string.Format("多帧第{0}帧，共{1}帧", nFrameLen - n + 1, nFrameLen);
                }
                else
                {
                    info.Comments = string.Format("第{0}帧，共{1}帧", nFrameLen - n + 1, nFrameLen);
                }
                lock(lockobj)
                {
                    infoList.Add(info);
                }
            }
            m_bIsSendMultiFrame = false;
        }
        #endregion
    }
}
