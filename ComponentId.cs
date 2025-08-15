using System.Collections.Generic;
using System.Linq;

namespace Wirelog
{
    public static partial class Converter
    {
        private static void SetComponentsAndModulesId()
        {
            SetComponentsId(_wires, _inputPorts, _outputPorts, _lampsFound.Values, _gatesFound.Values);
            SetModulesId(_moduleDefinitions.Values, _moduleInstances);
        }

        private static void SetComponentsId(
            ICollection<Wire> wires,
            ICollection<InputPort> inputPorts,
            ICollection<OutputPort> outputPorts,
            ICollection<Lamp> lamps,
            ICollection<Gate> gates)
        {
            int wireId = 0;
            foreach (var wire in wires)
            {
                wire.Id = wireId++;
            }
            int lampId = 0;
            foreach (var lamp in lamps)
            {
                lamp.Id = lampId++;
            }
            int gateId = 0;
            foreach (var gate in gates)
            {
                gate.Id = gateId++;
            }
            int inputId = 0;
            foreach (var inputPort in inputPorts)
            {
                inputPort.Id = inputId++;
            }
            int outputId = 0;
            foreach (var outputPort in outputPorts)
            {
                outputPort.Id = outputId++;
            }
        }

        private static void SetModulesId(
            ICollection<Module> modules,
            ICollection<ModuleInstance> moduleInstances)
        {
            int moduleInstanceId = 0;
            foreach (var moduleInstance in moduleInstances)
            {
                moduleInstance.Id = moduleInstanceId++;
            }
            int moduleId = 0;
            foreach (var module in modules)
            {
                module.Id = moduleId++;
                SetComponentsId(module.Wires, module.InputPorts, module.OutputPorts, module.Lamps, module.Gates);
            }
        }

        private static void SetPortsData()
        {
            HashSet<InputPort> inputPorts = _inputsFound.Values
                .Select(input => input.InputPort)
                .ToHashSet();
            HashSet<OutputPort> outputPorts = _outputsFound.Values
                .SelectMany(output => output.OutputPorts)
                .ToHashSet();

            foreach (var input in _inputsFound.Values)
            {
                InputsPortFound.Add(input.Pos, input.InputPort);
            }
            int inputId = 0;
            foreach (var inputPort in inputPorts)
            {
                inputPort.Id = inputId++;
                _inputPorts[inputPort.Id] = inputPort;
            }
            int outputId = 0;
            foreach (var outputPort in outputPorts)
            {
                outputPort.Id = outputId++;
                _outputPorts[outputPort.Id] = outputPort;
            }
        }
    }
}
