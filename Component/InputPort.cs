using System.Collections.Generic;

namespace Wirelog
{
    public class InputPort
    {
        public HashSet<Input> Inputs { get; } = [];
        public HashSet<Wire> OutputWires { get; } = [];
    }
}
