using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace Wirelog
{
    public class Gate
    {
        public GateType Type { get; set; }
        public HashSet<Lamp> InputLamps { get; } = [];
        public HashSet<Wire> OutputWires { get; } = [];
        public static bool TryGetType(Tile tile, out GateType type)
        {
            if (tile == null || !tile.HasTile) { type = GateType.None; return false; }
            type = tile.TileType switch
            {
                TileID.LogicGate when tile.TileFrameX is 0 * 18 or 1 * 18 && 
                tile.TileFrameY is 0 * 18 => GateType.AND,
                TileID.LogicGate when tile.TileFrameX is 0 * 18 or 1 * 18 && 
                tile.TileFrameY is 1 * 18 => GateType.OR,
                TileID.LogicGate when tile.TileFrameX is 0 * 18 or 1 * 18 && 
                tile.TileFrameY is 2 * 18 => GateType.NAND,
                TileID.LogicGate when tile.TileFrameX is 0 * 18 or 1 * 18 && 
                tile.TileFrameY is 3 * 18 => GateType.NOR,
                TileID.LogicGate when tile.TileFrameX is 0 * 18 or 1 * 18 && 
                tile.TileFrameY is 4 * 18 => GateType.XOR,
                TileID.LogicGate when tile.TileFrameX is 0 * 18 or 1 * 18 && 
                tile.TileFrameY is 5 * 18 => GateType.XNOR,
                TileID.LogicGate when tile.TileFrameX is 2 * 18 => GateType.Fault,
                _ => GateType.None,
            };
            return type != GateType.None;
        }
    }
    public enum GateType
    {
        None,
        AND,
        NAND,
        OR,
        NOR,
        XOR,
        XNOR,
        Fault,
    }
}
