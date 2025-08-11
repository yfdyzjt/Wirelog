using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;
using Terraria;
using Terraria.ModLoader;

namespace Wirelog
{
    public static class VerilogSimulator
    {
        private const string SharedMemName = "TerrariaWiringSim_SharedMem";
        private const string FrameSyncEventName = "TerrariaWiringSim_FrameSyncEvent";
        private const string ShutdownEventName = "TerrariaWiringSim_ShutdownEvent";

        private const int IpcMaxInputRleSize = 8192;
        private const int IpcMaxOutputBatchSize = 65536;

        private static readonly int SimReadyOffset = Marshal.OffsetOf<SharedMemoryLayout>(nameof(SharedMemoryLayout.SimReady)).ToInt32();
        private static readonly int FrameSyncReadyOffset = Marshal.OffsetOf<SharedMemoryLayout>(nameof(SharedMemoryLayout.FrameSyncReady)).ToInt32();
        private static readonly int ShutdownOffset = Marshal.OffsetOf<SharedMemoryLayout>(nameof(SharedMemoryLayout.Shutdown)).ToInt32();
        private static readonly int InputRleCountOffset = Marshal.OffsetOf<SharedMemoryLayout>(nameof(SharedMemoryLayout.InputRleCount)).ToInt32();
        private static readonly int InputRleIdsOffset = Marshal.OffsetOf<SharedMemoryLayout>(nameof(SharedMemoryLayout.InputRleIds)).ToInt32();
        private static readonly int InputRleCountsOffset = Marshal.OffsetOf<SharedMemoryLayout>(nameof(SharedMemoryLayout.InputRleCounts)).ToInt32();
        private static readonly int OutputCountOffset = Marshal.OffsetOf<SharedMemoryLayout>(nameof(SharedMemoryLayout.OutputCount)).ToInt32();
        private static readonly int OutputIdsOffset = Marshal.OffsetOf<SharedMemoryLayout>(nameof(SharedMemoryLayout.OutputIds)).ToInt32();

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct SharedMemoryLayout
        {
            public int SimReady;
            public int FrameSyncReady;
            public int Shutdown;

            public int InputRleCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = IpcMaxInputRleSize)]
            public int[] InputRleIds;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = IpcMaxInputRleSize)]
            public int[] InputRleCounts;

            public int OutputCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = IpcMaxOutputBatchSize)]
            public int[] OutputIds;
        }

        private static Process _simProcess;
        private static MemoryMappedFile _mmf;
        private static MemoryMappedViewAccessor _accessor;
        private static EventWaitHandle _frameSyncEvent;
        private static EventWaitHandle _shutdownEvent;

        public static bool IsRunning => _simProcess != null && !_simProcess.HasExited;

        public static void Start()
        {
            if (IsRunning) return;

            Main.statusText = "Waiting for verilog simulator to connect.";
            var simPath = Path.Combine(ModLoader.ModPath, "VWiring.exe");
            while (!File.Exists(simPath))
            {
                Thread.Sleep(1000);
            }

            try
            {
                Main.statusText = "Simulator process start.";
                _shutdownEvent = new EventWaitHandle(false, EventResetMode.ManualReset, ShutdownEventName);

                var psi = new ProcessStartInfo
                {
                    FileName = simPath,
                    Arguments = "--ipc",
                    UseShellExecute = false,
                    CreateNoWindow = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                };

                _simProcess = new Process
                {
                    StartInfo = psi,
                    EnableRaisingEvents = true
                };

                _simProcess.Start();

                Thread.Sleep(1000);

                Main.statusText = "Connect to shared memory.";
                int retryCount = 0;
                bool connected = false;
                while (retryCount < 10 && !connected)
                {
                    try
                    {
                        _mmf = MemoryMappedFile.OpenExisting(SharedMemName, MemoryMappedFileRights.ReadWrite);
                        _accessor = _mmf.CreateViewAccessor();
                        connected = true;
                    }
                    catch
                    {
                        retryCount++;
                        Thread.Sleep(500);
                    }
                }

                if (!connected)
                {
                    throw new TimeoutException("Failed to connect to shared memory after multiple attempts.");
                }

                SpinWait.SpinUntil(() =>
                {
                    _accessor.Read(SimReadyOffset, out int simReady);
                    return simReady != 0;
                }, 5000);

                _accessor.Read(SimReadyOffset, out int isReady);
                if (isReady == 0)
                {
                    throw new TimeoutException("Simulator did not become ready in time.");
                }

                retryCount = 0;
                bool eventsConnected = false;
                while (retryCount < 10 && !eventsConnected)
                {
                    try
                    {
                        _frameSyncEvent = EventWaitHandle.OpenExisting(FrameSyncEventName);
                        eventsConnected = true;
                    }
                    catch
                    {
                        retryCount++;
                        Thread.Sleep(500);
                    }
                }

                if (!eventsConnected)
                {
                    throw new TimeoutException("Failed to connect to event handles after multiple attempts.");
                }

                Main.NewText("Verilog simulator connected.");
            }
            catch (Exception e)
            {
                Main.NewText($"Failed to start or connect to simulator: {e.Message}");
                Stop();
                return;
            }
        }

        public static void Stop()
        {
            if (_accessor != null)
            {
                try
                {
                    _accessor.Write(ShutdownOffset, 1);
                }
                catch { }
            }

            try
            {
                _shutdownEvent?.Set();
            }
            catch { }

            if (IsRunning)
            {
                try
                {
                    if (!_simProcess.WaitForExit(3000))
                    {
                        _simProcess.Kill();
                        _simProcess.WaitForExit(1000);
                    }
                }
                catch { }
            }

            try
            {
                _simProcess?.Dispose();
                _simProcess = null;

                _accessor?.Dispose();
                _accessor = null;
                _mmf?.Dispose();
                _mmf = null;

                _frameSyncEvent?.Dispose();
                _frameSyncEvent = null;
                _shutdownEvent?.Dispose();
                _shutdownEvent = null;
            }
            catch (Exception ex)
            {
                Main.NewText($"Error during cleanup: {ex.Message}");
            }

            Main.NewText("Verilog simulator disconnected.");
        }

        private static readonly List<int> _currentFrameInputIds = new(IpcMaxInputRleSize);
        private static readonly List<int> _currentFrameInputCounts = new(IpcMaxInputRleSize);
        private static readonly List<int> _lastFrameOutputs = new(IpcMaxOutputBatchSize);

        public static List<int> LastFrameOutputs => _lastFrameOutputs;

        public static void EnqueueInput(int inputPortId)
        {
            if (_accessor == null)
                return;
            int n = _currentFrameInputIds.Count;
            if (n > 0 && _currentFrameInputIds[n - 1] == inputPortId)
            {
                int newCnt = _currentFrameInputCounts[n - 1] + 1;
                if (newCnt < 0) newCnt = int.MaxValue;
                _currentFrameInputCounts[n - 1] = newCnt;
                return;
            }
            if (n < IpcMaxInputRleSize)
            {
                _currentFrameInputIds.Add(inputPortId);
                _currentFrameInputCounts.Add(1);
            }
        }

        public static void FrameSync()
        {
            if (_accessor == null || _frameSyncEvent == null)
            {
                return;
            }
            if (_simProcess == null || _simProcess.HasExited)
            {
                return;
            }

            try
            {
                _lastFrameOutputs.Clear();

                int frameReady = _accessor.ReadInt32(FrameSyncReadyOffset);
                if (frameReady != 0)
                {
                    int outCount = _accessor.ReadInt32(OutputCountOffset);
                    if (outCount > 0)
                    {
                        var tmp = new int[outCount];
                        _accessor.ReadArray(OutputIdsOffset, tmp, 0, outCount);
                        _lastFrameOutputs.AddRange(tmp);
                    }

                    _accessor.Write(FrameSyncReadyOffset, 0);
                }

                int rleCount = _currentFrameInputIds.Count;
                _accessor.Write(InputRleCountOffset, rleCount);
                if (rleCount > 0)
                {
                    _accessor.WriteArray(InputRleIdsOffset, _currentFrameInputIds.ToArray(), 0, rleCount);
                    _accessor.WriteArray(InputRleCountsOffset, _currentFrameInputCounts.ToArray(), 0, rleCount);
                }

                _frameSyncEvent.Set();

                _currentFrameInputIds.Clear();
                _currentFrameInputCounts.Clear();
            }
            catch (Exception ex)
            {
                Main.NewText($"Error in FrameSync: {ex.Message}");
                Stop();
            }
        }
    }
}
