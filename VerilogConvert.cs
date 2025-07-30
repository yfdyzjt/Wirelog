using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wirelog
{
    public partial class Converter
    {
        private static void VerilogConvert()
        {
            SetComponentId();

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

            sb.AppendLine("// input port module");
            foreach (var inputPort in _inputsPortFound.Values)
            {
                sb.AppendLine(GetInputPortMoudleString(inputPort));
            }
            sb.AppendLine("// output port module");
            foreach (var outputPort in _outputsPortFound.Values)
            {
                sb.AppendLine(GetOutputPortMoudleString(outputPort));
            }
            sb.AppendLine("// lamp module");
            foreach (var lamp in _lampsFound.Values)
            {
                sb.AppendLine(GetLampMoudleString(lamp));
            }
            sb.AppendLine("// gate module");
            foreach (var gate in _gatesFound.Values)
            {
                sb.AppendLine(GetGateMoudleString(gate));
            }

            sb.AppendLine($"""
                endmodule
                """);
        }

        private static void SetComponentId()
        {
            HashSet<InputPort> inputPorts = _inputsFound.Values.Select(input => input.InputPort).ToHashSet();
            HashSet<OutputPort> outputPorts = _outputsFound.Values.Select(output => output.OutputPort).ToHashSet();

            int wireId = 0;
            foreach (var wire in _wires)
            {
                wire.Id = wireId++;
            }
            int inputId = 0;
            foreach (var inputPort in inputPorts)
            {
                inputPort.Id = inputId++;
                _inputsPortFound.Add(inputPort.Id, inputPort);
            }
            int outputId = 0;
            foreach (var outputPort in outputPorts)
            {
                outputPort.Id = outputId++;
                _outputsPortFound.Add(outputPort.Id, outputPort);
            }
            int lampId = 0;
            foreach (var lamp in _lampsFound.Values)
            {
                lamp.Id = lampId++;
            }
            int gateId = 0;
            foreach (var gate in _gatesFound.Values)
            {
                gate.Id = gateId++;
            }
        }

        private static string GetInputPortMoudleString(InputPort inputPort)
        {
            string moduleName = "Input_" +
                (inputPort.OutputWires.Count == 1 ?
                $"Single" :
                $"Multi #( .OUTPUT_COUNT({inputPort.OutputWires.Count}) )");
            string outputWiresNames = inputPort.OutputWires.Count == 1
                ? $"wires[{inputPort.OutputWires.First().Id}]"
                : $"{{{string.Join(", ", inputPort.OutputWires.Select(w => $"wires[{w.Id}]"))}}}";

            return $"{moduleName} i_{inputPort.Id} (" +
                   $".in(in[{inputPort.Id}]), " +
                   $".out({outputWiresNames})" +
                   ");";
        }

        private static string GetOutputPortMoudleString(OutputPort outputPort)
        {
            return
                $"Output_Single o_{outputPort.Id} (" +
                $".clk(clk), " +
                $".logic_reset(logic_reset), " +
                $".in(wires[{outputPort.InputWire}]), " +
                $".out(out[{outputPort.Id}])" +
                $");";
        }

        private static string GetLampMoudleString(Lamp lamp)
        {
            string moduleName = "Lamp_" +
                (lamp.InputWires.Count == 1 ? "Single_" : "Multi_") +
                lamp.Type switch
                {
                    LampType.On => "On",
                    LampType.Off => "Off",
                    LampType.Fault => "Fault",
                    _ => "None"
                } +
                (lamp.InputWires.Count == 1 ?
                "" :
                $" #( .INPUT_COUNT({lamp.InputWires.Count}) )");
            string inputWiresNames = lamp.InputWires.Count == 1
                ? $"wires[{lamp.InputWires.First().Id}]"
                : $"{{{string.Join(", ", lamp.InputWires.Select(w => $"wires[{w.Id}]"))}}}";
            return $"{moduleName} l_{lamp.Id} (" +
                   $".clk(clk), " +
                   $".logic_reset(logic_reset), " +
                   $".in({inputWiresNames}), " +
                   $".out(lamps[{lamp.Id}])" +
                   ");";
        }

        private static string GetGateMoudleString(Gate gate)
        {
            string moduleName = "Gate_" +
                (gate.InputLamps.Count == 1 ? "Single_" : "Multi_") +
                (gate.OutputWires.Count == 1 ? "Single_" : "Multi_") +
                gate.Type switch
                {
                    GateType.AND => "AND",
                    GateType.OR => "OR",
                    GateType.NAND => "NAND",
                    GateType.NOR => "NOR",
                    GateType.XOR => "XOR",
                    GateType.XNOR => "XNOR",
                    _ => "None"
                };
            List<string> parameters = [];
            if (gate.InputLamps.Count != 1)
            {
                parameters.Add($".INPUT_COUNT({gate.InputLamps.Count})");
            }
            if (gate.OutputWires.Count != 1)
            {
                parameters.Add($".OUTPUT_COUNT({gate.OutputWires.Count})");
            }
            moduleName += $" #( {string.Join(", ", parameters)} )";
            string inputLampsNames = gate.InputLamps.Count == 1
                ? $"lamps[{gate.InputLamps.First().Id}]"
                : $"{{{string.Join(", ", gate.InputLamps.Select(l => $"lamps[{l.Id}]"))}}}";
            string outputWiresNames = gate.OutputWires.Count == 1
                ? $"wires[{gate.OutputWires.First().Id}]"
                : $"{{{string.Join(", ", gate.OutputWires.Select(w => $"wires[{w.Id}]"))}}}";
            return $"{moduleName} g_{gate.Id} (" +
                   $".clk(clk), " +
                   $".logic_reset(logic_reset), " +
                   $".in({inputLampsNames}), " +
                   $".out({outputWiresNames})" +
                   ");";
        }
    }
}
