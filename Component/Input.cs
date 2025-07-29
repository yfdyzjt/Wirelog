using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace Wirelog
{
    public class Input
    {
        public InputType Type { get; set; }
        public Point16 Pos { get; set; }
        public static bool TryGetType(Tile tile, out InputType type)
        {
            if (tile == null || !tile.HasTile) { type = InputType.None; return false; }
            type = tile.TileType switch
            {
                TileID.PressurePlates => InputType.PressurePlates,
                TileID.MinecartTrack when 20 <= tile.TileFrameX && tile.TileFrameX < 24 => InputType.PressurePlateTrack,
                TileID.LogicSensor => InputType.LogicSensor,
                TileID.WeightedPressurePlate => InputType.WeightedPressurePlate,
                TileID.ProjectilePressurePad => InputType.ProjectilePressurePad,
                TileID.GolfHole => InputType.GolfHole,
                TileID.GemLocks => InputType.GemLocks,
                TileID.Switches => InputType.Switches,
                TileID.Timers => InputType.Timers,
                TileID.FakeContainers or TileID.FakeContainers2 => InputType.FakeContainers,
                TileID.Containers2 when tile.TileFrameX / 36 is 4 => InputType.DeadMansChest,
                TileID.Lever => InputType.Lever,
                TileID.Detonator => InputType.Detonator,
                _ => InputType.None,
            };
            return type != InputType.None;
        }
        public static (int sizeX, int sizeY) GetSize(InputType type) => type switch
        {
            InputType.GemLocks => (3, 3),
            InputType.Lever or
            InputType.Detonator or
            InputType.DeadMansChest or
            InputType.FakeContainers => (2, 2),
            _ => (1, 1),
        };

    }
    public enum InputType
    {
        None,
        PressurePlates,
        PressurePlateTrack,
        LogicSensor,
        WeightedPressurePlate,
        ProjectilePressurePad,
        GolfHole,
        GemLocks,
        Switches,
        Timers,
        FakeContainers,
        DeadMansChest,
        Lever,
        Detonator,
    }
}
