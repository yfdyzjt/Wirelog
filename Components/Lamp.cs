using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Wirelog
{
    public class Lamp
    {
        public int Id { get; set; }
        public LampType Type { get; set; }
        public Point16 Pos { get; set; }
        public HashSet<Wire> Wires { get; } = [];
        public Gate Gate { get; set; }
        public static bool TryGetType(Tile tile, out LampType type)
        {
            if (tile == null || !tile.HasTile || tile.HasActuator) { type = LampType.None; return false; }
            type = tile.TileType switch
            {
                TileID.LogicGateLamp when tile.TileFrameX is 0 * 18 => LampType.Off,
                TileID.LogicGateLamp when tile.TileFrameX is 1 * 18 => LampType.On,
                TileID.LogicGateLamp when tile.TileFrameX is 2 * 18 => LampType.Fault,
                _ => LampType.None,
            };
            return type != LampType.None;
        }
    }
    public enum LampType
    {
        None,
        On,
        Off,
        Fault,
    }
}
