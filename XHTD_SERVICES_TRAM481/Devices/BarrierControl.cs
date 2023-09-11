﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XHTD_SERVICES.Device.PLCM221;
using XHTD_SERVICES.Device;
using XHTD_SERVICES.Data.Repositories;
using System.Threading;
using log4net;

namespace XHTD_SERVICES_TRAM481.Devices
{
    public class BarrierControl
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(BarrierControl));

        protected readonly PLCBarrier _barrier;

        private const string IP_ADDRESS = "10.0.20.2";

        private const int SCALE_IN_I1 = 0;
        private const int SCALE_IN_Q1 = 0;
        private const int SCALE_IN_Q2 = 1;

        private const int SCALE_OUT_I1 = 1;
        private const int SCALE_OUT_Q1 = 2;
        private const int SCALE_OUT_Q2 = 3;

        public BarrierControl(
            PLCBarrier barrier
            )
        {
            _barrier = barrier;
        }

        // Barrier chiều vào
        public bool OpenBarrierScaleIn()
        {
            var isConnectSuccessed = false;
            int count = 0;

            try
            {
                while (!isConnectSuccessed && count < 6)
                {
                    count++;

                    _logger.Info($@"OpenBarrierScaleIn: count={count}");

                    M221Result isConnected = _barrier.ConnectPLC(IP_ADDRESS);

                    if (isConnected == M221Result.SUCCESS)
                    {
                        if (_barrier.ReadInputPort(SCALE_IN_I1))
                        {
                            Thread.Sleep(500);

                            _barrier.ShuttleOutputPort((byte.Parse(SCALE_IN_Q1.ToString())));

                            Thread.Sleep(500);

                            _barrier.ShuttleOutputPort((byte.Parse(SCALE_IN_Q1.ToString())));

                            Thread.Sleep(500);

                            _barrier.Close();

                            _logger.Info($"OpenBarrierScaleIn count={count} thanh cong");
                        }
                        else
                        {
                            _logger.Info($"OpenBarrierScaleIn count={count}: barrier dang mo");
                        }

                        isConnectSuccessed = true;
                    }
                    else
                    {
                        _logger.Info($"OpenBarrierScaleIn count={count}: Ket noi PLC khong thanh cong");

                        Thread.Sleep(1000);
                    }
                }

                return isConnectSuccessed;
            }
            catch (Exception ex)
            {
                _logger.Info($"OpenBarrierScaleIn Error: {ex.Message} == {ex.StackTrace} == {ex.InnerException}");
                return false;
            }
        }

        public bool CloseBarrierScaleIn()
        {
            var isConnectSuccessed = false;
            int count = 0;

            try
            {
                while (!isConnectSuccessed && count < 6)
                {
                    count++;

                    _logger.Info($@"CloseBarrierScaleIn: count={count}");

                    M221Result isConnected = _barrier.ConnectPLC(IP_ADDRESS);

                    if (isConnected == M221Result.SUCCESS)
                    {
                        if (!_barrier.ReadInputPort(SCALE_IN_I1))
                        {
                            Thread.Sleep(500);

                            _barrier.ShuttleOutputPort((byte.Parse(SCALE_IN_Q2.ToString())));

                            Thread.Sleep(500);

                            _barrier.ShuttleOutputPort((byte.Parse(SCALE_IN_Q2.ToString())));

                            Thread.Sleep(500);

                            _barrier.Close();

                            _logger.Info($"CloseBarrierScaleIn count={count} thanh cong");
                        }
                        else
                        {
                            _logger.Info($"CloseBarrierScaleIn count={count}: barrier dang dong");
                        }

                        isConnectSuccessed = true;
                    }
                    else
                    {
                        _logger.Info($"CloseBarrierScaleIn count={count}: Ket noi PLC khong thanh cong");

                        Thread.Sleep(1000);
                    }
                }

                return isConnectSuccessed;
            }
            catch (Exception ex)
            {
                _logger.Info($"CloseBarrierScaleIn Error: {ex.Message} == {ex.StackTrace} == {ex.InnerException}");
                return false;
            }
        }

        // Barrier chiều ra
        public bool OpenBarrierScaleOut()
        {
            var isConnectSuccessed = false;
            int count = 0;

            try
            {
                while (!isConnectSuccessed && count < 6)
                {
                    count++;

                    _logger.Info($@"OpenBarrierScaleOut: count={count}");

                    M221Result isConnected = _barrier.ConnectPLC(IP_ADDRESS);

                    if (isConnected == M221Result.SUCCESS)
                    {
                        if (_barrier.ReadInputPort(SCALE_OUT_I1))
                        {
                            Thread.Sleep(500);

                            _barrier.ShuttleOutputPort((byte.Parse(SCALE_OUT_Q1.ToString())));

                            Thread.Sleep(500);

                            _barrier.ShuttleOutputPort((byte.Parse(SCALE_OUT_Q1.ToString())));

                            Thread.Sleep(500);

                            _barrier.Close();

                            _logger.Info($"OpenBarrierScaleOut count={count} thanh cong");
                        }
                        else
                        {
                            _logger.Info($"OpenBarrierScaleOut count={count}: barrier dang mo");
                        }

                        isConnectSuccessed = true;
                    }
                    else
                    {
                        _logger.Info($"OpenBarrierScaleOut count={count}: Ket noi PLC khong thanh cong");

                        Thread.Sleep(1000);
                    }
                }

                return isConnectSuccessed;
            }
            catch (Exception ex)
            {
                _logger.Info($"OpenBarrierScaleOut Error: {ex.Message} == {ex.StackTrace} == {ex.InnerException}");
                return false;
            }
        }

        public bool CloseBarrierScaleOut()
        {
            var isConnectSuccessed = false;
            int count = 0;

            try
            {
                while (!isConnectSuccessed && count < 6)
                {
                    count++;

                    _logger.Info($@"CloseBarrierScaleOut: count={count}");

                    M221Result isConnected = _barrier.ConnectPLC(IP_ADDRESS);

                    if (isConnected == M221Result.SUCCESS)
                    {
                        if (!_barrier.ReadInputPort(SCALE_OUT_I1))
                        {
                            Thread.Sleep(500);

                            _barrier.ShuttleOutputPort((byte.Parse(SCALE_OUT_Q2.ToString())));

                            Thread.Sleep(500);

                            _barrier.ShuttleOutputPort((byte.Parse(SCALE_OUT_Q2.ToString())));

                            Thread.Sleep(500);

                            _barrier.Close();

                            _logger.Info($"CloseBarrierScaleOut count={count} thanh cong");
                        }
                        else
                        {
                            _logger.Info($"CloseBarrierScaleOut count={count}: barrier dang dong");
                        }

                        isConnectSuccessed = true;
                    }
                    else
                    {
                        _logger.Info($"CloseBarrierScaleOut count={count}: Ket noi PLC khong thanh cong");

                        Thread.Sleep(1000);
                    }
                }

                return isConnectSuccessed;
            }
            catch (Exception ex)
            {
                _logger.Info($"CloseBarrierScaleOut Error: {ex.Message} == {ex.StackTrace} == {ex.InnerException}");
                return false;
            }
        }
    }
}
