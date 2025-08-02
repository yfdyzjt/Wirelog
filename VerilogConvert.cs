using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using Terraria.ModLoader;

namespace Wirelog
{
    public static partial class Converter
    {
        private static void VerilogConvert()
        {
            Main.statusText = "convert verilog";
            var sb = new StringBuilder();
            sb.AppendLine($"""
                module Wiring (
                    input wire clk,
                    input wire reset,
                    input wire logic_reset,
                    input wire [{_inputsPortFound.Count - 1}:0] in,
                    output wire wiring_running,
                    output wire [{_outputsPortFound.Count - 1}:0] out,
                    output wire [31:0] in_width,
                    output wire [31:0] out_width
                );
                    wire [{_wires.Count - 1}:0] wires;
                    wire [{_lampsFound.Count - 1}:0] lamps;

                    assign wiring_running = |wires;

                    assign in_width = {_inputsPortFound.Count};
                    assign out_width = {_outputsPortFound.Count};
                """);

            sb.AppendLine("    // input port module");
            foreach (var inputPort in _inputsPortFound.Values)
            {
                sb.AppendLine(GetInputPortMoudleString(inputPort));
            }
            sb.AppendLine("    // output port module");
            foreach (var outputPort in _outputsPortFound.Values)
            {
                sb.AppendLine(GetOutputPortMoudleString(outputPort));
            }
            sb.AppendLine("    // lamp module");
            foreach (var lamp in _lampsFound.Values)
            {
                sb.AppendLine(GetLampMoudleString(lamp));
            }
            sb.AppendLine("    // gate module");
            foreach (var gate in _gatesFound.Values)
            {
                sb.AppendLine(GetGateMoudleString(gate));
            }

            sb.AppendLine($"""
                endmodule
                """);

            WriteVerilogToFile(sb.ToString());
        }

        private static void WriteVerilogToFile(string verilogCode)
        {
            var outputDir = System.IO.Path.Combine(ModLoader.ModPath, "WirelogModule");
            WriteVModules(outputDir);
            System.IO.File.WriteAllText(System.IO.Path.Combine(outputDir, "Wiring.v"), verilogCode);
        }

        private static string GetInputPortMoudleString(InputPort inputPort)
        {
            var moduleType = inputPort.OutputWires.Count == 1 ? "Single" : "Multi";
            var parameters = moduleType == "Multi" ? $" #(.OUTPUT_COUNT({inputPort.OutputWires.Count}))" : "";
            var moduleName = $"Input_{moduleType}{parameters}";

            var outputWires = GetWireNames(inputPort.OutputWires);

            return $"    {moduleName} i_{inputPort.Id} (.in(in[{inputPort.Id}]), .out({outputWires}));";
        }

        private static string GetOutputPortMoudleString(OutputPort outputPort)
        {
            return $"    Output_Single o_{outputPort.Id} (.clk(clk), .logic_reset(logic_reset), .in(wires[{outputPort.InputWire.Id}]), .out(out[{outputPort.Id}]));";
        }

        private static string GetLampMoudleString(Lamp lamp)
        {
            var moduleType = lamp.InputWires.Count is 0 or 1 ? "Single" : "Multi";
            var parameters = moduleType == "Multi" ? $" #(.INPUT_COUNT({lamp.InputWires.Count}))" : "";
            var moduleName = $"Lamp_{moduleType}_{lamp.Type}{parameters}";

            var inputWires = GetWireNames(lamp.InputWires);

            if (lamp.Type == LampType.Fault)
            {
                return $"    {moduleName} l_{lamp.Id} (.clk(clk), .in({inputWires}), .out(lamps[{lamp.Id}]));";
            }
            else
            {
                return $"    {moduleName} l_{lamp.Id} (.clk(clk), .reset(reset), .in({inputWires}), .out(lamps[{lamp.Id}]));";
            }
        }

        private static string GetGateMoudleString(Gate gate)
        {
            if (gate.Type == GateType.Fault)
            {
                var inputType = gate.InputLamps.Count - 1 == 1 ? "Single" : "Multi";
                var outputType = gate.OutputWires.Count == 1 ? "Single" : "Multi";

                var parameters = new List<string>();
                if (inputType == "Multi") parameters.Add($".INPUT_COUNT({gate.InputLamps.Count})");
                if (outputType == "Multi") parameters.Add($".OUTPUT_COUNT({gate.OutputWires.Count})");
                var parameterString = parameters.Count > 0 ? $" #({string.Join(", ", parameters)})" : "";

                var moduleName = $"Gate_{inputType}_{outputType}_{gate.Type}{parameterString}";

                var inputLamps = GetLampNames(gate.InputLamps.Where(gate => gate.Type != LampType.Fault).ToList());
                var inputFaultLamp = GetLampNames([gate.InputLamps.First(gate => gate.Type == LampType.Fault)]);
                var outputWires = GetWireNames(gate.OutputWires);

                return $"    {moduleName} g_{gate.Id} (.clk(clk), .logic_reset(logic_reset), .in({inputLamps}), .fault_in({inputFaultLamp}), .out({outputWires}));";
            }
            else
            {
                var inputType = gate.InputLamps.Count == 1 ? "Single" : "Multi";
                var outputType = gate.OutputWires.Count == 1 ? "Single" : "Multi";

                var parameters = new List<string>();
                if (inputType == "Multi") parameters.Add($".INPUT_COUNT({gate.InputLamps.Count})");
                if (outputType == "Multi") parameters.Add($".OUTPUT_COUNT({gate.OutputWires.Count})");
                var parameterString = parameters.Count > 0 ? $" #({string.Join(", ", parameters)})" : "";

                var moduleName = $"Gate_{inputType}_{outputType}_{gate.Type}{parameterString}";

                var inputLamps = GetLampNames(gate.InputLamps);
                var outputWires = GetWireNames(gate.OutputWires);

                return $"    {moduleName} g_{gate.Id} (.clk(clk), .logic_reset(logic_reset), .in({inputLamps}), .out({outputWires}));";
            }
        }

        private static string GetWireNames(ICollection<Wire> wires)
        {
            if (wires.Count == 0) return $"0";
            else if (wires.Count == 1) return $"wires[{wires.First().Id}]";
            return $"{{{string.Join(", ", wires.Select(w => $"wires[{w.Id}]"))}}}";
        }

        private static string GetLampNames(ICollection<Lamp> lamps)
        {
            if (lamps.Count == 1) return $"lamps[{lamps.First().Id}]";
            return $"{{{string.Join(", ", lamps.Select(l => $"lamps[{l.Id}]"))}}}";
        }
    }
}
