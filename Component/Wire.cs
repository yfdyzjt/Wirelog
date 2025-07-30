using System.Collections.Generic;
using Terraria;

namespace Wirelog
{
    public class Wire
    {
        public HashSet<InputPort> InputPorts { get; } = [];
        public HashSet<OutputPort> OutputPorts { get; } = [];
        public HashSet<Lamp> Lamps { get; } = [];
        public HashSet<Gate> Gates { get; } = [];

        public static bool HasWire(Tile tile)
        {
            return tile.RedWire || tile.BlueWire || tile.GreenWire || tile.YellowWire;
        }

        public static bool HasWire(Tile tile, WireType wireType)
        {
            return wireType switch
            {
                WireType.Red => tile.RedWire,
                WireType.Blue => tile.BlueWire,
                WireType.Green => tile.GreenWire,
                WireType.Yellow => tile.YellowWire,
                _ => false,
            };
        }
    }

    public enum WireType
    {
        Red,
        Blue,
        Green,
        Yellow,
    }
}