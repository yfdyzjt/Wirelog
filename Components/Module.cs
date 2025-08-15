using System.Collections.Generic;

namespace Wirelog
{
    public class Module
    {
        public long Hash { get; set; } // init;
        public int Id { get; set; }

        public HashSet<Gate> Gates { get; } = [];
        public HashSet<Lamp> Lamps { get; } = [];
        public HashSet<Wire> Wires { get; } = [];
        public HashSet<OutputPort> OutputPorts { get; } = [];
        public HashSet<InputPort> InputPorts { get; } = [];
        // public HashSet<ModuleInstance> InstanceModules { get; } = [];
        // Nested modules need to be supported.
    }

    public class ModuleInstance
    {
        public Module Module { get; init; }
        public int Id { get; set; }
        public Dictionary<InputPort, Wire> InputMapping { get; } = [];
        public Dictionary<OutputPort, Wire> OutputMapping { get; } = [];
    }
}