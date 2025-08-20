using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;

namespace Wirelog
{
    public static partial class Converter
    {
        private static void HashModules()
        {
            var gateSignatures = new Dictionary<Gate, long>();
            foreach (var gate in _gatesFound.Values)
            {
                gateSignatures[gate] = CalculateGateSignature(gate);
            }

            var seedGroups = gateSignatures
                .GroupBy(kvp => kvp.Value)
                .Where(g => g.Count() > 1)
                .Select(g => g.Select(kvp => kvp.Key).ToList())
                .OrderBy(g => g.Count)
                .ToList();
            var searchableGates = seedGroups
                    .SelectMany(g => g)
                    .ToHashSet();

            var allFoundSubgraphSet = new List<SubgraphSet>();

            for (var i = 0; i < seedGroups.Count; i++)
            {
                if (i % Math.Max(1, seedGroups.Count / 100) == 0)
                    Main.statusText = $"hash modules {1f * i / seedGroups.Count:P1}";

                var seedGroup = seedGroups[i];
                var initialSubgraphSet = new SubgraphSet([.. seedGroup]);
                if (initialSubgraphSet.Subgraphs.Count < 2) continue;

                var bestSubgraphSet = FindBestSubgraphSetExpansion(initialSubgraphSet, searchableGates, gateSignatures);

                if (bestSubgraphSet.Gates.Count > initialSubgraphSet.Gates.Count)
                {
                    allFoundSubgraphSet.Add(bestSubgraphSet);
                }
            }

            Main.statusText = $"resolve hash module conflicts";
            var finalSubgraphSets = ResolveSubgraphSetsConflicts(allFoundSubgraphSet);

            Main.statusText = $"create hash module instances";
            foreach (var subgraphSet in finalSubgraphSets)
            {
                ReplaceSubgraphSetWithModuleInstances(subgraphSet);
            }
        }

        private static long GetArrayLongHash(ICollection<long> hashs)
        {
            unchecked
            {
                long result = 17;
                foreach (var hash in hashs)
                {
                    result = result * 23 + hash;
                }
                return result;
            }
        }

        private static long CalculateGateSignature(Gate gate)
        {
            long gateHash = $"G:{gate.Type},W:{gate.Wires.Count}".GetHashCode();

            var lampHashs = gate.Lamps
                .Select(lamp => $"L:{lamp.Type},W:{lamp.Wires.Count}")
                .OrderBy(s => s)
                .Select(s => s.GetHashCode())
                .ToList();

            return GetArrayLongHash([gateHash, .. lampHashs]);
        }

        private static SubgraphSet FindBestSubgraphSetExpansion(
            SubgraphSet currentSubgraphSet,
            HashSet<Gate> searchableGates,
            Dictionary<Gate, long> gateSignatures)
        {
            SubgraphSet bestFound = currentSubgraphSet;
            var queue = new Queue<SubgraphSet>();

            currentSubgraphSet.Subgraphs.ForEach(UpdateSubgraphHashs);
            queue.Enqueue(currentSubgraphSet);

            var visitedSetHashs = new HashSet<long>([currentSubgraphSet.Hash]);

            while (queue.Count > 0)
            {
                var dequeuedSet = queue.Dequeue();

                if (GetSubgraphSetScore(dequeuedSet) > GetSubgraphSetScore(bestFound))
                {
                    bestFound = dequeuedSet;
                }

                var expansionSets = FindExpansionSets(dequeuedSet, searchableGates, visitedSetHashs, gateSignatures);
                expansionSets.ForEach(queue.Enqueue);
            }

            return bestFound;
        }

        private static List<SubgraphSet> FindExpansionSets(
            SubgraphSet subgraphSet,
            HashSet<Gate> searchableGates,
            HashSet<long> visitedSetHashs,
            Dictionary<Gate, long> gateSignatures)
        {
            var expansionSets = new List<SubgraphSet>();
            if (subgraphSet.Subgraphs.Count == 0) return expansionSets;

            var potentialExpansions = new List<(Subgraph original, Subgraph expanded, Gate addedGate)>();
            foreach (var subgraph in subgraphSet.Subgraphs)
            {
                var boundaryWires = subgraph.Module.Wires
                    .Where(w => w.InputPorts.Count + w.OutputPorts.Count != 0)
                    .Select(w => (Wire)subgraph.CompoundsFound[w])
                    .ToHashSet();

                var boundaryGates = boundaryWires
                    .SelectMany(wire => new HashSet<Gate>([.. wire.Gates, .. wire.Lamps.Select(l => l.Gate)])
                    .Where(gate => searchableGates.Contains(gate) && !subgraphSet.Gates.Contains(gate)))
                    .ToHashSet();

                foreach (var boundaryGate in boundaryGates)
                {
                    var expansionSubgraph = subgraph.Clone();
                    expansionSubgraph.Gates.Add(boundaryGate);
                    UpdateSubgraphHashs(expansionSubgraph);
                    potentialExpansions.Add((subgraph, expansionSubgraph, boundaryGate));
                }
            }

            var disambiguatedExpansions = new List<Subgraph>();

            foreach (var groupFromOriginal in potentialExpansions.GroupBy(p => p.original))
            {
                foreach (var collisionSet in groupFromOriginal.GroupBy(p => p.expanded.Hash))
                {
                    var expansionsToDisambiguate = collisionSet.ToList();

                    expansionsToDisambiguate.Sort((p1, p2) =>
                    {
                        var g1 = p1.addedGate;
                        var g2 = p2.addedGate;
                        var sigCompare = gateSignatures[g1].CompareTo(gateSignatures[g2]);
                        if (sigCompare != 0) return sigCompare;
                        var posCompare = g1.Pos.X.CompareTo(g2.Pos.X);
                        if (posCompare != 0) return posCompare;
                        return g1.Pos.Y.CompareTo(g2.Pos.Y);
                    });

                    var lastPos = expansionsToDisambiguate.First().addedGate.Pos;
                    long lastSignature = gateSignatures[expansionsToDisambiguate.First().addedGate];

                    for (var i = 0; i < expansionsToDisambiguate.Count; i++)
                    {
                        var (original, expanded, addedGate) = expansionsToDisambiguate[i];
                        var canonicalSignature = gateSignatures[addedGate];

                        if (lastSignature != canonicalSignature) lastPos = addedGate.Pos;

                        var relPos = new Point16(addedGate.Pos.X - lastPos.X, addedGate.Pos.Y - lastPos.Y);
                        var canonicalHash = GetArrayLongHash(new[] { expanded.Hash, canonicalSignature, relPos.GetHashCode() });

                        lastPos = addedGate.Pos;
                        lastSignature = canonicalSignature;
                        expanded.Hash = canonicalHash;
                        disambiguatedExpansions.Add(expanded);
                    }
                }
            }

            var finalGroups = disambiguatedExpansions.GroupBy(s => s.Hash);

            foreach (var finalGroup in finalGroups)
            {
                if (finalGroup.Count() < 2) continue;

                var newSubgraphs = finalGroup.Select(p => p).ToList();
                var newSet = new SubgraphSet(newSubgraphs);

                if (visitedSetHashs.Contains(newSet.Hash)) continue;

                visitedSetHashs.Add(newSet.Hash);
                expansionSets.Add(newSet);
            }

            return expansionSets;
        }

        private static Dictionary<object, long> InitializeLabels(HashSet<object> components)
        {
            var labels = new Dictionary<object, long>();
            foreach (var c in components)
            {
                long label = c switch
                {
                    Wire w => 1,
                    InputPort i => 2,
                    OutputPort o => 3,
                    Gate g => (long)g.Type << 8,
                    Lamp l => (long)l.Type << 16,
                    _ => 0
                };
                labels[c] = label;
            }
            return labels;
        }

        private static IEnumerable<object> GetComponentNeighbors(object c)
        {
            return c switch
            {
                Wire w => w.Gates.Cast<object>().Concat(w.Lamps).Concat(w.InputPorts).Concat(w.OutputPorts),
                Gate g => g.Lamps.Cast<object>().Concat(g.Wires),
                Lamp l => l.Wires.Cast<object>().Concat([l.Gate]),
                InputPort i => i.Wires,
                OutputPort o => [o.Wire],
                _ => []
            };
        }

        private static Dictionary<object, long> ComponentWlIteration(Dictionary<object, long> currentLabels, HashSet<object> components)
        {
            var newLabels = new Dictionary<object, long>();
            foreach (var c in components)
            {
                var neighborLabels = new List<long>();
                var neighbors = GetComponentNeighbors(c);
                foreach (var neighbor in neighbors)
                {
                    if (currentLabels.TryGetValue(neighbor, out var label))
                    {
                        neighborLabels.Add(label);
                    }
                }

                neighborLabels.Sort();
                newLabels[c] = GetArrayLongHash([currentLabels[c], .. neighborLabels]);
            }
            return newLabels;
        }

        private static Dictionary<object, long> RunHashingIteration(HashSet<object> components)
        {
            var labels = InitializeLabels(components);

            int iterations = components.Count;
            for (var i = 0; i < iterations + 1; i++)
            {
                labels = ComponentWlIteration(labels, components);
            }

            return labels;
        }

        private static Dictionary<object, long> GetModuleComponentHashs(Module module)
        {
            var components = new HashSet<object>();
            components.UnionWith(module.Wires);
            components.UnionWith(module.Lamps);
            components.UnionWith(module.Gates);
            components.UnionWith(module.InputPorts);
            components.UnionWith(module.OutputPorts);
            return RunHashingIteration(components);
        }

        private static void UpdateSubgraphHashs(Subgraph subgraph)
        {
            var (module, compoundsFound) = CopySubGraphToModule(subgraph);
            subgraph.Module = module;
            subgraph.CompoundsFound = compoundsFound;
            subgraph.ComponentHashs = GetModuleComponentHashs(module);
            subgraph.Hash = GetArrayLongHash([.. subgraph.ComponentHashs.Values.Order()]);
        }


        private static double GetSubgraphSetScore(SubgraphSet subgraphSet)
        {
            var module = subgraphSet.Subgraphs.First().Module;
            var portCount = module.InputPorts.Count + module.OutputPorts.Count;
            var allCount = module.Gates.Count + module.Lamps.Count + module.Wires.Count + portCount;
            return (subgraphSet.Gates.Count - 2) * (subgraphSet.Subgraphs.Count - 1) + 
                (1 - (double)portCount / allCount);
        }

        private static List<SubgraphSet> ResolveSubgraphSetsConflicts(List<SubgraphSet> subgraphSets)
        {
            subgraphSets.Sort((a, b) => GetSubgraphSetScore(b).CompareTo(GetSubgraphSetScore(a)));

            var finalModules = new List<SubgraphSet>();
            var usedGates = new HashSet<Gate>();

            foreach (var subgraphSet in subgraphSets)
            {
                if (subgraphSet.Gates.Overlaps(usedGates)) continue;
                finalModules.Add(subgraphSet);
                usedGates.UnionWith(subgraphSet.Gates);
            }

            return finalModules;
        }

        private static (Module module, Dictionary<object, object> compoundsFound)
            CopySubGraphToModule(Subgraph subgraph)
        {
            var module = new Module();
            var newGatesFound = new Dictionary<Gate, Gate>();
            var newLampsFound = new Dictionary<Lamp, Lamp>();
            var compoundsFound = new Dictionary<object, object>();
            var wires = new HashSet<Wire>();

            foreach (var gate in subgraph.Gates)
            {
                var newGate = new Gate { Type = gate.Type };
                module.Gates.Add(newGate);
                newGatesFound[gate] = newGate;
                compoundsFound[newGate] = gate;
                wires.UnionWith(gate.Wires);
                foreach (var lamp in gate.Lamps)
                {
                    var newLamp = new Lamp { Type = lamp.Type };
                    Link.Add(newLamp, newGate);
                    module.Lamps.Add(newLamp);
                    newLampsFound[lamp] = newLamp;
                    compoundsFound[newLamp] = lamp;
                    wires.UnionWith(lamp.Wires);
                }
            }

            foreach (var wire in wires)
            {
                var newWire = new Wire { Type = wire.Type };
                foreach (var newLamp in wire.Lamps
                    .Where(newLampsFound.ContainsKey)
                    .Select(l => newLampsFound[l]))
                {
                    Link.Add(newWire, newLamp);
                }
                foreach (var newGate in wire.Gates
                    .Where(newGatesFound.ContainsKey)
                    .Select(g => newGatesFound[g]))
                {
                    Link.Add(newWire, newGate);
                }
                if (wire.InputPorts.Count != 0 ||
                    wire.Gates.Any(g => !newGatesFound.ContainsKey(g)))
                {
                    var newInputPort = new InputPort();
                    module.InputPorts.Add(newInputPort);
                    Link.Add(newWire, newInputPort);
                    compoundsFound[newInputPort] = wire;
                }
                if ((wire.OutputPorts.Count != 0 ||
                    wire.Lamps.Any(l => !newLampsFound.ContainsKey(l))) &&
                    wire.InputPorts.Count == 0 &&
                    wire.Gates.All(newGatesFound.ContainsKey))
                {
                    var newOutputPort = new OutputPort();
                    module.OutputPorts.Add(newOutputPort);
                    Link.Add(newWire, newOutputPort);
                    compoundsFound[newOutputPort] = wire;
                }
                module.Wires.Add(newWire);
                compoundsFound[newWire] = wire;
            }

            return (module, compoundsFound);
        }

        private static ModuleInstance CreateModuleInstanceFromSubgraph(Subgraph prototypeSubgraph, Subgraph instanceSubgraph)
        {
            var portToWireMap = instanceSubgraph.CompoundsFound
                .Where(kv => kv.Key is InputPort or OutputPort)
                .ToDictionary(kv => kv.Key, kv => (Wire)kv.Value);
            var prototypePortsByHash = prototypeSubgraph.ComponentHashs
                .Where(kv => kv.Key is InputPort or OutputPort)
                .GroupBy(kvp => kvp.Value, kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.ToList());
            var instancePortsByHash = instanceSubgraph.ComponentHashs
                .Where(kv => kv.Key is InputPort or OutputPort)
                .GroupBy(kvp => kvp.Value, kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.ToList());
            var instance = new ModuleInstance { Module = prototypeSubgraph.Module };
            foreach (var (hash, instancePortList) in instancePortsByHash)
            {
                if (prototypePortsByHash.TryGetValue(hash, out var prototypePortList))
                {
                    for (int i = 0; i < instancePortList.Count; i++)
                    {
                        var instancePort = instancePortList[i];
                        var prototypePort = prototypePortList[i];
                        var originalWire = portToWireMap[instancePort];
                        if (prototypePort is InputPort protoInputPort)
                        {
                            instance.InputPortMap[protoInputPort] = originalWire;
                        }
                        else if (prototypePort is OutputPort protoOutputPort)
                        {
                            instance.OutputPortMap[protoOutputPort] = originalWire;
                        }
                    }
                }
            }
            return instance;
        }

        private static void PruneModularizedComponents(Subgraph instanceSubgraph)
        {
            var compoundsFound = instanceSubgraph.CompoundsFound;
            var lampsToPrune = instanceSubgraph.Module.Lamps.Select(l => (Lamp)compoundsFound[l]).ToList();
            var gatesToPrune = instanceSubgraph.Module.Gates.Select(g => (Gate)compoundsFound[g]).ToList();
            var wiresToPrune = instanceSubgraph.Module.Wires
                .Where(w => w.InputPorts.Count == 0 && w.OutputPorts.Count == 0)
                .Select(w => (Wire)compoundsFound[w]).ToList();

            Link.Remove(lampsToPrune);
            foreach (var lamp in lampsToPrune) _lampsFound.Remove(lamp.Pos);
            Link.Remove(gatesToPrune);
            foreach (var gate in gatesToPrune) _gatesFound.Remove(gate.Pos);
            Link.Remove(wiresToPrune);
            foreach (var wire in wiresToPrune) _wires.Remove(wire);
        }

        private static void ReplaceSubgraphSetWithModuleInstances(SubgraphSet subgraphSet)
        {
            var prototypeSubgraph = subgraphSet.Subgraphs.First();
            _moduleDefinitions.Add(prototypeSubgraph.Module);
            foreach (var instanceSubgraph in subgraphSet.Subgraphs)
            {
                var instance = CreateModuleInstanceFromSubgraph(prototypeSubgraph, instanceSubgraph);
                _moduleInstances.Add(instance);
                PruneModularizedComponents(instanceSubgraph);
            }
        }

        private class SubgraphSet
        {
            public long Hash => GetArrayLongHash(Subgraphs.Select(s => s.Hash).ToList());
            public List<Subgraph> Subgraphs { get; }
            public HashSet<Gate> Gates { get; }

            public SubgraphSet(List<Gate> seedGates)
            {
                Subgraphs = seedGates.Select(g => new Subgraph(g)).ToList();
                Gates = new HashSet<Gate>(seedGates);
            }

            public SubgraphSet(List<Subgraph> subgraphs)
            {
                Subgraphs = [.. subgraphs];
                Gates = new HashSet<Gate>(subgraphs.SelectMany(s => s.Gates));
            }

            private SubgraphSet(SubgraphSet other)
            {
                Subgraphs = other.Subgraphs.Select(s => s.Clone()).ToList();
                Gates = new HashSet<Gate>(other.Gates);
            }

            public SubgraphSet Clone() => new(this);
        }

        private class Subgraph
        {
            public long Hash { get; set; }
            public Gate InitialSeed { get; }
            public HashSet<Gate> Gates { get; }
            public Module Module { get; set; }
            public Dictionary<object, object> CompoundsFound { get; set; }
            public Dictionary<object, long> ComponentHashs { get; set; }

            public Subgraph(Gate seed)
            {
                InitialSeed = seed;
                Gates = [seed];
            }

            private Subgraph(Subgraph other)
            {
                InitialSeed = other.InitialSeed;
                Gates = new HashSet<Gate>(other.Gates);
            }

            public Subgraph Clone() => new(this);
        }
    }
}