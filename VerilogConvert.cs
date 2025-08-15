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
            var dir = System.IO.Path.Combine(ModLoader.ModPath, "WirelogModule");
            WriteCodeToFile(dir, "Wiring.v", GetTopModuleStringBuilder().ToString());
            foreach (var module in _moduleDefinitions.Values)
            {
                WriteCodeToFile(dir, $"Module_{module.Id}.v", GetModuleStringBuilder(module).ToString());
            }
            WriteVModules(dir);
        }

        private static void WriteCodeToFile(string dir, string name, string code)
        {
            System.IO.File.WriteAllText(System.IO.Path.Combine(dir, name), code);
        }

        private static StringBuilder GetTopModuleStringBuilder()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"""
                module Wiring (
                    input wire clk,
                    input wire reset,
                    input wire logic_reset,
                    input wire [{_inputPorts.Length - 1}:0] in,
                    output wire wiring_running,
                    output wire [{_outputPorts.Length - 1}:0] out,
                    output wire [31:0] in_width,
                    output wire [31:0] out_width
                );
                    assign in_width = {_inputPorts.Length};
                    assign out_width = {_outputPorts.Length};

                    {GetWiresStringBuilder(_wires.Count + _moduleInstances.Count, _lampsFound.Values.Count)}
                    {GetComponentsStringBuilder(_inputPorts, _outputPorts, _lampsFound.Values, _gatesFound.Values)}
                    {GetStringModuleInstancesBuilder(_wires.Count, _moduleInstances)}

                    endmodule
                """);

            return sb;
        }

        private static StringBuilder GetModuleStringBuilder(Module module)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"""
                module Module_{module.Id} (
                    input wire clk,
                    input wire reset,
                    input wire logic_reset,
                    input wire [{module.InputPorts.Count - 1}:0] in,
                    output wire wiring_running,
                    output wire [{module.OutputPorts.Count - 1}:0] out,
                );

                    {GetWiresStringBuilder(module.Wires.Count, module.Lamps.Count)}
                    {GetComponentsStringBuilder(module.InputPorts, module.OutputPorts, module.Lamps, module.Gates)}

                    endmodule
                """);

            return sb;
        }

        private static StringBuilder GetStringModuleInstancesBuilder(int wiresCount, ICollection<ModuleInstance> moduleInstances)
        {
            var sb = new StringBuilder();

            if (moduleInstances.Count > 0)
                sb.AppendLine($"// module instances: {moduleInstances.Count}");
            foreach (var moduleInstance in moduleInstances)
            {
                sb.AppendLine(GetModuleInstanceString(wiresCount, moduleInstance));
            }

            return sb;
        }

        private static StringBuilder GetWiresStringBuilder(int wiresCount, int lampsCount)
        {
            var sb = new StringBuilder();

            if (wiresCount > 0)
                sb.AppendLine($"// wires: {wiresCount} count");
            if (wiresCount > 0)
                sb.AppendLine($"wire [{wiresCount - 1}:0] wires;");
            if (lampsCount > 0)
                sb.AppendLine($"wire [{lampsCount - 1}:0] lamps;");
            if (wiresCount > 0)
                sb.AppendLine($"assign wiring_running = |wires;");
            else
                sb.AppendLine($"assign wiring_running = 1'b0;");

            return sb;
        }

        private static StringBuilder GetComponentsStringBuilder(
            ICollection<InputPort> inputPorts,
            ICollection<OutputPort> outputPorts,
            ICollection<Lamp> lamps,
            ICollection<Gate> gates)
        {
            var sb = new StringBuilder();
            if (inputPorts.Count > 0)
                sb.AppendLine($"// input port components: {inputPorts.Count} count");
            foreach (var inputPort in inputPorts)
            {
                sb.AppendLine(GetInputPortString(inputPort));
            }
            if (outputPorts.Count > 0)
                sb.AppendLine($"// output port components: {outputPorts.Count} count");
            foreach (var outputPort in outputPorts)
            {
                sb.AppendLine(GetOutputPortString(outputPort));
            }
            if (lamps.Count > 0)
                sb.AppendLine($"// lamp components: {lamps.Count} count");
            foreach (var lamp in lamps)
            {
                sb.AppendLine(GetLampString(lamp));
            }
            if (gates.Count > 0)
                sb.AppendLine($"// gate components: {gates.Count} count");
            foreach (var gate in gates)
            {
                sb.AppendLine(GetGateString(gate));
            }
            return sb;
        }

        private static string GetModuleInstanceString(int wiresCount, ModuleInstance moduleInstance)
        {
            var inputWires = GetWireNames(moduleInstance.InputMapping.Values);
            var outputWires = GetWireNames(moduleInstance.OutputMapping.Values);
            var runningWires = (wiresCount + moduleInstance.Id).ToString();
            var connections = $".clk(clk), .reset(reset), .logic_reset(logic_reset), .in({inputWires}), .wiring_running(wires[{runningWires}]), .out({outputWires})";
            var moduleName = $"Module_{moduleInstance.Module.Id}";
            return BuildComponentInstanceString(moduleName, "m", moduleInstance.Id.ToString(), connections);
        }

        private static string GetInputPortString(InputPort inputPort)
        {
            var moduleType = GetComponentTypeString(inputPort.Wires.Count);
            var parameters = new List<string>();
            if (moduleType == "Multi") parameters.Add($".OUTPUT_COUNT({inputPort.Wires.Count})");
            var parameterString = BuildParameterString(parameters);
            var moduleName = $"Input_{moduleType}{parameterString}";

            var outputWires = GetWireNames(inputPort.Wires);
            var connections = $".in(in[{inputPort.Id}]), .out({outputWires})";

            return BuildComponentInstanceString(moduleName, "i", inputPort.Id.ToString(), connections);
        }

        private static string GetOutputPortString(OutputPort outputPort)
        {
            var connections = $".clk(clk), .logic_reset(logic_reset), .in(wires[{outputPort.Wire.Id}]), .out(out[{outputPort.Id}])";
            return BuildComponentInstanceString("Output_Single", "o", outputPort.Id.ToString(), connections);
        }

        private static string GetLampString(Lamp lamp)
        {
            var moduleType = GetComponentTypeString(lamp.Wires.Count);
            var parameters = new List<string>();
            if (moduleType == "Multi") parameters.Add($".INPUT_COUNT({lamp.Wires.Count})");
            var parameterString = BuildParameterString(parameters);
            var moduleName = $"Lamp_{moduleType}_{lamp.Type}{parameterString}";

            var inputWires = GetWireNames(lamp.Wires);
            var clockReset = lamp.Type == LampType.Fault ? ".clk(clk)" : ".clk(clk), .reset(reset)";
            var connections = $"{clockReset}, .in({inputWires}), .out(lamps[{lamp.Id}])";

            return BuildComponentInstanceString(moduleName, "l", lamp.Id.ToString(), connections);
        }

        private static string GetGateString(Gate gate)
        {
            var inputType = GetComponentTypeString(gate.Type == GateType.Fault ? gate.Lamps.Count - 1 : gate.Lamps.Count);
            var outputType = GetComponentTypeString(gate.Wires.Count);

            var randSeed = Main.rand.Next(1, 0xFFF);
            var parameters = new List<string>();
            if (inputType == "Multi") parameters.Add($".INPUT_COUNT({(gate.Type == GateType.Fault ? gate.Lamps.Count - 1 : gate.Lamps.Count)})");
            if (outputType == "Multi") parameters.Add($".OUTPUT_COUNT({gate.Wires.Count})");
            if (gate.Type == GateType.Fault && inputType == "Multi") parameters.Add($".RAND_SEED({randSeed})");
            var parameterString = BuildParameterString(parameters);

            var moduleName = (gate.Type != GateType.Fault && inputType == "Single") ?
                $"Gate_{inputType}_{outputType}{parameterString}" :
                $"Gate_{inputType}_{outputType}_{gate.Type}{parameterString}";

            var connections = "";
            var outputWires = GetWireNames(gate.Wires);
            if (gate.Type == GateType.Fault)
            {
                var inputLamps = GetLampNames(gate.Lamps.Where(gate => gate.Type != LampType.Fault).ToHashSet());
                var inputFaultLamp = GetLampNames([gate.Lamps.First(gate => gate.Type == LampType.Fault)]);
                var clockReset = inputType == "Multi" ? ".clk(clk), .reset(reset), .logic_reset(logic_reset)" : ".clk(clk), .logic_reset(logic_reset)";
                connections = $"{clockReset}, .in({inputLamps}), .fault_in({inputFaultLamp}), .out({outputWires})";
            }
            else
            {
                var inputLamps = GetLampNames(gate.Lamps);
                connections = $".clk(clk), .logic_reset(logic_reset), .in({inputLamps}), .out({outputWires})";
            }
            return BuildComponentInstanceString(moduleName, "g", gate.Id.ToString(), connections);
        }

        private static string GetComponentTypeString(int count)
        {
            return count is 0 or 1 ? "Single" : "Multi";
        }

        private static string BuildParameterString(List<string> parameters)
        {
            return parameters.Count > 0 ? $" #({string.Join(", ", parameters)})" : "";
        }

        private static string BuildComponentInstanceString(string moduleName, string instancePrefix, string id, string connections)
        {
            return $"    {moduleName} {instancePrefix}_{id} ({connections});";
        }

        private static string GetWireNames(ICollection<Wire> wires)
        {
            if (wires.Count == 0) return $"1'b0";
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
