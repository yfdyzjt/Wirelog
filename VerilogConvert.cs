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
                    assign wiring_running = |wires;
                    assign in_width = {_inputsPortFound.Count};
                    assign out_width = {_outputsPortFound.Count};
                """);
            if (_wires.Count > 0) sb.Append($"    wire [{_wires.Count - 1}:0] wires;");
            if (_lampsFound.Count > 0) sb.Append($"    wire [{_lampsFound.Count - 1}:0] lamps;");
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
            var moduleType = GetModuleTypeString(inputPort.OutputWires.Count);
            var parameters = new List<string>();
            if (moduleType == "Multi") parameters.Add($".OUTPUT_COUNT({inputPort.OutputWires.Count})");
            var parameterString = BuildParameterString(parameters);
            var moduleName = $"Input_{moduleType}{parameterString}";

            var outputWires = GetWireNames(inputPort.OutputWires);
            var connections = $".in(in[{inputPort.Id}]), .out({outputWires})";

            return BuildModuleInstanceString(moduleName, "i", inputPort.Id.ToString(), connections);
        }

        private static string GetOutputPortMoudleString(OutputPort outputPort)
        {
            var connections = $".clk(clk), .logic_reset(logic_reset), .in(wires[{outputPort.InputWire.Id}]), .out(out[{outputPort.Id}])";
            return BuildModuleInstanceString("Output_Single", "o", outputPort.Id.ToString(), connections);
        }

        private static string GetLampMoudleString(Lamp lamp)
        {
            var moduleType = GetModuleTypeString(lamp.InputWires.Count);
            var parameters = new List<string>();
            if (moduleType == "Multi") parameters.Add($".INPUT_COUNT({lamp.InputWires.Count})");
            var parameterString = BuildParameterString(parameters);
            var moduleName = $"Lamp_{moduleType}_{lamp.Type}{parameterString}";

            var inputWires = GetWireNames(lamp.InputWires);
            var clockReset = lamp.Type == LampType.Fault ? ".clk(clk)" : ".clk(clk), .reset(reset)";
            var connections = $"{clockReset}, .in({inputWires}), .out(lamps[{lamp.Id}])";

            return BuildModuleInstanceString(moduleName, "l", lamp.Id.ToString(), connections);
        }

        private static string GetGateMoudleString(Gate gate)
        {
            var inputType = GetModuleTypeString(gate.Type == GateType.Fault ? gate.InputLamps.Count - 1 : gate.InputLamps.Count);
            var outputType = GetModuleTypeString(gate.OutputWires.Count);

            var randSeed = Main.rand.Next(1, 0xFFF);
            var parameters = new List<string>();
            if (inputType == "Multi") parameters.Add($".INPUT_COUNT({(gate.Type == GateType.Fault ? gate.InputLamps.Count - 1 : gate.InputLamps.Count)})");
            if (outputType == "Multi") parameters.Add($".OUTPUT_COUNT({gate.OutputWires.Count})");
            if (gate.Type == GateType.Fault && inputType == "Multi") parameters.Add($".RAND_SEED({randSeed})");
            var parameterString = BuildParameterString(parameters);

            var moduleName = (gate.Type != GateType.Fault && inputType == "Single") ?
                $"Gate_{inputType}_{outputType}{parameterString}" :
                $"Gate_{inputType}_{outputType}_{gate.Type}{parameterString}";

            var connections = "";
            var outputWires = GetWireNames(gate.OutputWires);
            if (gate.Type == GateType.Fault)
            {
                var inputLamps = GetLampNames(gate.InputLamps.Where(gate => gate.Type != LampType.Fault).ToList());
                var inputFaultLamp = GetLampNames([gate.InputLamps.First(gate => gate.Type == LampType.Fault)]);
                var clockReset = inputType == "Multi" ? ".clk(clk), .reset(reset), .logic_reset(logic_reset)" : ".clk(clk), .logic_reset(logic_reset)";
                connections = $"{clockReset}, .in({inputLamps}), .fault_in({inputFaultLamp}), .out({outputWires})";
            }
            else
            {
                var inputLamps = GetLampNames(gate.InputLamps);
                connections = $".clk(clk), .logic_reset(logic_reset), .in({inputLamps}), .out({outputWires})";
            }
            return BuildModuleInstanceString(moduleName, "g", gate.Id.ToString(), connections);
        }

        private static string GetModuleTypeString(int count)
        {
            return count is 0 or 1 ? "Single" : "Multi";
        }

        private static string BuildParameterString(List<string> parameters)
        {
            return parameters.Count > 0 ? $" #({string.Join(", ", parameters)})" : "";
        }

        private static string BuildModuleInstanceString(string moduleName, string instancePrefix, string id, string connections)
        {
            return $"    {moduleName} {instancePrefix}_{id} ({connections});";
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
