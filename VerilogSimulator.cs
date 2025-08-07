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
        private const string InputEventName = "TerrariaWiringSim_InputEvent";
        private const string OutputEventName = "TerrariaWiringSim_OutputEvent";
        private const string ShutdownEventName = "TerrariaWiringSim_ShutdownEvent";

        private const int IpcInputBufferSize = 8192;
        private const int IpcMaxOutputIdsPerSet = 65536;
        private const int IpcMaxOutputSets = 1024;

        private const int SimReadyOffset = 0;
        private const int InputWriteIdxOffset = SimReadyOffset + sizeof(int);
        private const int InputReadIdxOffset = InputWriteIdxOffset + sizeof(long);
        private const int InputBufferOffset = InputReadIdxOffset + sizeof(long);
        private const int OutputWriteIdxOffset = InputBufferOffset + IpcInputBufferSize * sizeof(int);
        private const int OutputReadIdxOffset = OutputWriteIdxOffset + sizeof(long);
        private const int OutputSetsOffset = OutputReadIdxOffset + sizeof(long);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct OutputSet
        {
            public int Count;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = IpcMaxOutputIdsPerSet)]
            public int[] Ids;

            public OutputSet()
            {
                Count = 0;
                Ids = new int[IpcMaxOutputIdsPerSet];
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct SharedMemoryLayout
        {
            [MarshalAs(UnmanagedType.I4)]
            public int SimReady;

            public long InputWriteIdx;
            public long InputReadIdx;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = IpcInputBufferSize)]
            public int[] InputBuffer;

            public long OutputWriteIdx;
            public long OutputReadIdx;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = IpcMaxOutputSets)]
            public OutputSet[] OutputSets;

            public SharedMemoryLayout()
            {
                SimReady = 0;
                InputWriteIdx = 0;
                InputReadIdx = 0;
                InputBuffer = new int[IpcInputBufferSize];
                OutputWriteIdx = 0;
                OutputReadIdx = 0;
                OutputSets = new OutputSet[IpcMaxOutputSets];
                for (int i = 0; i < IpcMaxOutputSets; i++)
                {
                    OutputSets[i] = new OutputSet();
                }
            }
        }

        private static Process _simProcess;
        private static MemoryMappedFile _mmf;
        private static MemoryMappedViewAccessor _accessor;
        private static EventWaitHandle _inputEvent;
        private static EventWaitHandle _outputEvent;
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
                        _inputEvent = EventWaitHandle.OpenExisting(InputEventName);
                        _outputEvent = EventWaitHandle.OpenExisting(OutputEventName);
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

                _inputEvent?.Dispose();
                _inputEvent = null;
                _outputEvent?.Dispose();
                _outputEvent = null;
                _shutdownEvent?.Dispose();
                _shutdownEvent = null;
            }
            catch (Exception ex)
            {
                Main.NewText($"Error during cleanup: {ex.Message}");
            }

            Main.NewText("Verilog simulator disconnected.");
        }

        public static List<int> SendInputAndWaitForOutput(int inputPortId)
        {
            if (_accessor == null || _inputEvent == null || _outputEvent == null)
            {
                Main.NewText("Simulator not connected.");
                return [];
            }

            if (_simProcess == null || _simProcess.HasExited)
            {
                Main.NewText("Simulator process is not running.");
                return [];
            }

            try
            {
                long writeIdx = _accessor.ReadInt64(InputWriteIdxOffset);

                _accessor.Write(InputBufferOffset + writeIdx % IpcInputBufferSize * sizeof(int), inputPortId);
                _accessor.Write(InputWriteIdxOffset, writeIdx + 1);

                _inputEvent.Set();

                bool signaled = _outputEvent.WaitOne(1000);
                if (!signaled)
                {
                    Main.NewText("Timeout waiting for simulator output.");
                    return [];
                }

                long outputReadIdx = _accessor.ReadInt64(OutputReadIdxOffset);
                long outputWriteIdx = _accessor.ReadInt64(OutputWriteIdxOffset);

                if (outputReadIdx >= outputWriteIdx)
                {
                    Main.NewText("No new output data available.");
                    return [];
                }

                long offset = OutputSetsOffset + outputReadIdx % IpcMaxOutputSets * (sizeof(int) + IpcMaxOutputIdsPerSet * sizeof(int));

                int count = _accessor.ReadInt32(offset);
                var outputIds = new List<int>();
                if (count > 0)
                {
                    var buffer = new int[count];
                    _accessor.ReadArray(offset + sizeof(int), buffer, 0, count);
                    outputIds.AddRange(buffer);
                }

                _accessor.Write(OutputReadIdxOffset, outputReadIdx + 1);

                return outputIds;
            }
            catch (Exception ex)
            {
                Main.NewText($"Error in SendInputAndWaitForOutput: {ex.Message}");
                Stop();
                return [];
            }
        }
    }
}
