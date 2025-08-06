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

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct OutputSet
        {
            public int Count;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = IpcMaxOutputIdsPerSet)]
            public int[] Ids;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct SharedMemoryLayout
        {
            public bool SimReady;
            public bool ClientConnected;

            public long InputWriteIdx;
            public long InputReadIdx;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = IpcInputBufferSize)]
            public int[] InputBuffer;

            public long OutputWriteIdx;
            public long OutputReadIdx;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = IpcMaxOutputSets)]
            public OutputSet[] OutputSets;
        }

        private static Process _simProcess;
        private static MemoryMappedFile _mmf;
        private static MemoryMappedViewAccessor _accessor;
        private static EventWaitHandle _inputEvent;
        private static EventWaitHandle _outputEvent;
        private static EventWaitHandle _shutdownEvent;

        private static long _outputReadIdx = 0;

        public static bool IsRunning => _simProcess != null && !_simProcess.HasExited;

        public static void Start()
        {
            if (_simProcess != null && !_simProcess.HasExited) return;

            Main.statusText = "Waiting for verilog simulator to connect";
            var simPath = Path.Combine(ModLoader.ModPath, "VWiring.exe");
            while (!File.Exists(simPath))
            {
                Thread.Sleep(1000);
            }

            try
            {
                _shutdownEvent = new EventWaitHandle(false, EventResetMode.ManualReset, ShutdownEventName);
                _simProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = simPath,
                    Arguments = "--ipc",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                _mmf = MemoryMappedFile.OpenExisting(SharedMemName, MemoryMappedFileRights.ReadWrite);
                _accessor = _mmf.CreateViewAccessor();

                SpinWait.SpinUntil(() =>
                {
                    _accessor.Read(0, out bool simReady);
                    return simReady;
                }, 5000); 

                _accessor.Read(0, out bool isReady);
                if (!isReady)
                {
                    throw new TimeoutException("Simulator did not become ready in time.");
                }

                _inputEvent = EventWaitHandle.OpenExisting(InputEventName);
                _outputEvent = EventWaitHandle.OpenExisting(OutputEventName);

                _accessor.Write(8, true);

                Main.NewText("Verilog simulator connected.");
            }
            catch (Exception e)
            {
                Main.NewText($"Failed to start or connect to simulator: {e.Message}");
                Stop();
                return;
            }
            return;
        }

        public static void Stop()
        {
            _shutdownEvent?.Set();

            if (_simProcess != null && !_simProcess.HasExited)
            {
                if (!_simProcess.WaitForExit(2000))
                {
                    _simProcess.Kill();
                }
            }
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
            Main.NewText("Verilog simulator disconnected.");
        }

        public static List<int> SendInputAndWaitForOutput(int inputPortId)
        {
            if (_accessor == null || _inputEvent == null || _outputEvent == null)
            {
                Main.NewText("Simulator not connected.");
                return [];
            }

            long writeIdx = _accessor.ReadInt64(16);
            _accessor.Write(32 + (writeIdx % IpcInputBufferSize) * 4, inputPortId);
            _accessor.Write(16, writeIdx + 1);
            _inputEvent.Set();

            bool signaled = _outputEvent.WaitOne(1000);
            if (!signaled)
            {
                Main.NewText("Timeout waiting for simulator output.");
                return [];
            }

            long currentOutputWriteIdx = _accessor.ReadInt64(32 + IpcInputBufferSize * 4);
            if (_outputReadIdx >= currentOutputWriteIdx)
            {
                return [];
            }

            long currentReadIdxInRing = _outputReadIdx % IpcMaxOutputSets;
            long offset = 32 + IpcInputBufferSize * 4 + 16 + currentReadIdxInRing * Marshal.SizeOf<OutputSet>();

            _accessor.Read(offset, out OutputSet resultSet);

            var outputIds = new List<int>();
            if (resultSet.Count > 0)
            {
                outputIds.AddRange(resultSet.Ids.AsSpan(0, resultSet.Count).ToArray());
            }

            _outputReadIdx++;
            _accessor.Write(32 + IpcInputBufferSize * 4 + 8, _outputReadIdx);

            return outputIds;
        }
    }
}
