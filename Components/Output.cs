using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace Wirelog
{
    public class Output
    {
        private static readonly Action<Point16>[] _outputActivators = new Action<Point16>[Enum.GetValues(typeof(OutputType)).Length];

        static Output()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.Namespace == "Wirelog.Outputs")
                {
                    if (Enum.TryParse<OutputType>(type.Name, out var outputType))
                    {
                        var activateMethod = type.GetMethod("Activate", BindingFlags.Public | BindingFlags.Static, null, [typeof(Point16)], null);
                        if (activateMethod != null)
                        {
                            _outputActivators[(int)outputType] = (Action<Point16>)Delegate.CreateDelegate(typeof(Action<Point16>), activateMethod);
                        }
                    }
                }
            }
        }
        public static void Activate(OutputType type, Point16 pos)
        {
            _outputActivators[(int)type]?.Invoke(pos);
        }

        public OutputType Type { get; set; }
        public Point16 Pos { get; set; }
        public HashSet<OutputPort> OutputPorts { get; } = [];
        public static bool TryGetType(Tile tile, out OutputType type)
        {
            if (tile == null || !tile.HasTile) { type = OutputType.None; return false; }
            if (tile.HasActuator) { type = OutputType.Actuator; return true; }
            type = tile.TileType switch
            {
                TileID.Timers => OutputType.Timers,
                TileID.ConveyorBeltLeft or
                TileID.ConveyorBeltRight => OutputType.ConveyorBelts,
                ushort id when TileID.AmethystGemsparkOff <= id && id <= TileID.AmberGemspark
                => OutputType.Gemsparks,
                TileID.Chimney => OutputType.Chimney,
                TileID.SillyBalloonMachine => OutputType.SillyBalloonMachine,
                TileID.Detonator => OutputType.Detonator,
                TileID.Sundial or
                TileID.Moondial => OutputType.SunAndMoondial,
                TileID.AnnouncementBox => OutputType.AnnouncementBox,
                TileID.Fireplace => OutputType.Fireplace,
                TileID.Cannon when tile.TileFrameX % 72 is 0 => OutputType.CannonsLeft,
                TileID.Cannon when tile.TileFrameX % 72 is 54 => OutputType.CannonsRight,
                TileID.Cannon when tile.TileFrameX < 216 &&
                tile.TileFrameX % 72 is 18 or 36 => OutputType.CannonsShot,
                TileID.Cannon when tile.TileFrameX >= 216 &&
                tile.TileFrameX % 72 is 18 or 36 &&
                tile.TileFrameY % 54 is 0 or 18 => OutputType.PortalGunStationChange,
                TileID.Cannon when tile.TileFrameX >= 216 &&
                tile.TileFrameX % 72 is 18 or 36 &&
                tile.TileFrameY % 54 is 36 => OutputType.PortalGunStationShot,
                TileID.SnowballLauncher => OutputType.SnowballLauncher,
                TileID.Campfire => OutputType.Campfires,
                TileID.ActiveStoneBlock or
                TileID.InactiveStoneBlock => OutputType.ActiveStoneBlocks,
                TileID.TrapdoorOpen => OutputType.TrapdoorOpen,
                TileID.TrapdoorClosed => OutputType.TrapdoorClosed,
                TileID.TallGateOpen or
                TileID.TallGateClosed => OutputType.TallGates,
                TileID.OpenDoor => OutputType.OpenDoors,
                TileID.ClosedDoor => OutputType.ClosedDoors,
                TileID.Firework => OutputType.Fireworks,
                TileID.Toilets => OutputType.Toilets,
                TileID.Chairs when tile.TileFrameY / 40 is 1 or 20 => OutputType.Toilets,
                TileID.FireworksBox => OutputType.FireworksBox,
                TileID.FireworkFountain => OutputType.FireworkFountain,
                TileID.Teleporter => OutputType.Teleporter,
                TileID.Torches => OutputType.Torches,
                TileID.WireBulb => OutputType.WireBulb,
                TileID.HolidayLights => OutputType.HolidayLights,
                TileID.BubbleMachine => OutputType.BubbleMachine,
                TileID.FogMachine => OutputType.FogMachine,
                TileID.HangingLanterns => OutputType.HangingLanterns,
                TileID.Lamps => OutputType.Lamps,
                TileID.DiscoBall or
                TileID.ChineseLanterns or
                TileID.Candelabras or
                TileID.PlatinumCandelabra or
                TileID.PlasmaLamp => OutputType.Lights,
                TileID.VolcanoSmall => OutputType.VolcanoSmall,
                TileID.VolcanoLarge => OutputType.VolcanoLarge,
                TileID.Chandeliers => OutputType.Chandeliers,
                TileID.MinecartTrack when (30 <= tile.TileFrameX && tile.TileFrameX < 36) ||
                (0 <= tile.TileFrameX && tile.TileFrameX < 20 &&
                23 < tile.TileFrameX && tile.TileFrameX < 30 &&
                tile.TileFrameY != -1) => OutputType.MinecartTrack, // check
                TileID.Candles or
                TileID.PlatinumCandle or
                TileID.WaterCandle or
                TileID.PeaceCandle or
                TileID.ShadowCandle => OutputType.Candles,
                TileID.Lampposts => OutputType.Lampposts,
                TileID.Traps => OutputType.Traps,
                TileID.GeyserTrap => OutputType.GeyserTrap,
                TileID.MusicBoxes or
                TileID.Jackolanterns => OutputType.MusicBoxes,
                TileID.WaterFountain => OutputType.WaterFountain,
                TileID.LunarMonolith or
                TileID.BloodMoonMonolith or
                TileID.VoidMonolith or
                TileID.EchoMonolith or
                TileID.ShimmerMonolith => OutputType.Monoliths,
                TileID.PartyMonolith => OutputType.PartyMonolith,
                TileID.Explosives => OutputType.Explosives,
                TileID.LandMine => OutputType.LandMine,
                TileID.InletPump or
                TileID.OutletPump => OutputType.Pumps,
                TileID.BoulderStatue or
                TileID.MushroomStatue or
                TileID.CatBast => OutputType.Statues,
                TileID.Statues when !(tile.TileFrameX / 36 is 0 or 1 or 3 or 6 or 11 or 12 or 14 or 15 or 19 or
                20 or 21 or 22 or 24 or 25 or 26 or 29 or 31 or 32 or 33 or 36 or 38 or 39 or 43 or 44 or 45)
                => OutputType.Statues, // check
                TileID.Grate or
                TileID.GrateClosed => OutputType.Grates,
                TileID.PixelBox => OutputType.PixelBox,
                _ => OutputType.None,
            };
            return type != OutputType.None;
        }
        public static (int sizeX, int sizeY) GetSize(OutputType type) => type switch
        {
            OutputType.Lampposts => (1, 6),
            OutputType.TallGates => (1, 5),
            OutputType.SillyBalloonMachine or
            OutputType.SnowballLauncher or
            OutputType.Chandeliers or
            OutputType.PartyMonolith => (3, 3),
            OutputType.Fireplace or
            OutputType.Campfires or
            OutputType.BubbleMachine => (3, 2),
            OutputType.CannonsShot or
            OutputType.SunAndMoondial or
            OutputType.OpenDoors or
            OutputType.WaterFountain or
            OutputType.Monoliths or
            OutputType.Statues => (2, 3),
            OutputType.Teleporter => (3, 1),
            OutputType.CannonsRight or
            OutputType.CannonsLeft or
            OutputType.ClosedDoors or
            OutputType.Lamps => (1, 3),
            OutputType.PortalGunStationChange or
            OutputType.Chimney or
            OutputType.Detonator or
            OutputType.AnnouncementBox or
            OutputType.TrapdoorOpen or
            OutputType.FireworksBox or
            OutputType.FogMachine or
            OutputType.Lights or
            OutputType.VolcanoLarge or
            OutputType.MusicBoxes or
            OutputType.Pumps => (2, 2),
            OutputType.PortalGunStationShot or
            OutputType.TrapdoorClosed or
            OutputType.GeyserTrap => (2, 1),
            OutputType.Fireworks or
            OutputType.Toilets or
            OutputType.FireworkFountain or
            OutputType.HangingLanterns => (1, 2),
            _ => (1, 1),
        };
    }
    public enum OutputType
    {
        None,
        Actuator,
        Timers,
        ConveyorBelts,
        Gemsparks,
        Chimney,
        SillyBalloonMachine,
        Detonator,
        SunAndMoondial,
        AnnouncementBox,
        Fireplace,
        CannonsLeft,
        CannonsRight,
        CannonsShot,
        PortalGunStationShot,
        PortalGunStationChange,
        SnowballLauncher,
        Campfires,
        ActiveStoneBlocks,
        TrapdoorOpen,
        TrapdoorClosed,
        TallGates,
        OpenDoors,
        ClosedDoors,
        Fireworks,
        Toilets,
        FireworksBox,
        FireworkFountain,
        Teleporter,
        Torches,
        WireBulb,
        HolidayLights,
        BubbleMachine,
        FogMachine,
        HangingLanterns,
        Lamps,
        Lights,
        VolcanoSmall,
        VolcanoLarge,
        Chandeliers,
        MinecartTrack,
        Candles,
        Lampposts,
        Traps,
        GeyserTrap,
        MusicBoxes,
        WaterFountain,
        Monoliths,
        PartyMonolith,
        Explosives,
        LandMine,
        Pumps,
        Statues,
        Grates,
        PixelBox,
    }
}
