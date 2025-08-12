using System.Collections.Generic;

namespace Wirelog
{
    public class InputPort
    {
        public int Id { get; set; }
        public HashSet<Input> Inputs { get; } = [];
        public HashSet<Wire> Wires { get; } = [];
    }
}
