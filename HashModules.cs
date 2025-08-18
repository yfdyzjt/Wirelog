using System.Collections.Generic;
using System.Linq;
using Terraria;

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
            // var processedGates = new HashSet<Gate>();

            foreach (var seedGroup in seedGroups)
            {
                // var initialSubgraphSet = new SubgraphSet(seedGroup.Where(g => !processedGates.Contains(g)).ToList());
                var initialSubgraphSet = new SubgraphSet([.. seedGroup]);
                if (initialSubgraphSet.Subgraphs.Count < 2) continue;

                var bestSubgraphSet = FindBestSubgraphSetExpansion(initialSubgraphSet, gateSignatures, searchableGates/*, processedGates*/);

                if (bestSubgraphSet.Gates.Count > initialSubgraphSet.Gates.Count)
                {
                    allFoundSubgraphSet.Add(bestSubgraphSet);
                    // processedGates.UnionWith(bestSubgraphSet.Gates);
                }
            }

            var finalSubgraphSets = ResolveSubgraphSetsConflicts(allFoundSubgraphSet);

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
            Dictionary<Gate, long> gateSignatures,
            HashSet<Gate> searchableGates/*,
            HashSet<Gate> processedGates*/)
        {
            SubgraphSet bestFound = currentSubgraphSet;

            var boundaryGateGroups = FindSubgraphSetBoundaryGates(currentSubgraphSet, searchableGates, gateSignatures/*, processedGates*/);

            foreach (var group in boundaryGateGroups)
            {
                var gatesByInstance = group.ToLookup(kvp => kvp.Key, kvp => kvp.Value);
                var combinations = GetCombinations(gatesByInstance.Select(g => g.ToList()).ToList());

                foreach (var combination in combinations)
                {
                    var newSubgraphSet = currentSubgraphSet.Clone();
                    SubGraphSetAddExpansion(newSubgraphSet, combination);

                    if (AreSubgraphSetIsomorphic(newSubgraphSet))
                    {
                        var resultFromPath = FindBestSubgraphSetExpansion(newSubgraphSet, gateSignatures, searchableGates/*, processedGates*/);

                        if (GetSubgraphSetScore(resultFromPath) > GetSubgraphSetScore(bestFound))
                        {
                            bestFound = resultFromPath;
                        }
                    }
                }
            }

            return bestFound;
        }

        private static List<IGrouping<long, KeyValuePair<Subgraph, Gate>>>
            FindSubgraphSetBoundaryGates(
            SubgraphSet subgraphSet,
            HashSet<Gate> searchableGates,
            Dictionary<Gate, long> gateSignatures/*,
            HashSet<Gate> processedGates*/)
        {
            var boundaryGates = new List<KeyValuePair<Subgraph, Gate>>();
            foreach (var subgraph in subgraphSet.Subgraphs)
            {
                var neighborGates = new HashSet<Gate>();
                foreach (var gateInSubgraph in subgraph.Gates)
                {
                    /*
                    var gateWires = gateInSubgraph.Wires;
                    neighborGates.UnionWith(gateWires.SelectMany(w => w.Lamps.Select(l => l.Gate)));
                    var lampWires = gateInSubgraph.Lamps.SelectMany(l => l.Wires);
                    neighborGates.UnionWith(lampWires.SelectMany(w => w.Gates));
                    */
                    var wires = new HashSet<Wire>();
                    wires.UnionWith(gateInSubgraph.Lamps.SelectMany(l => l.Wires));
                    wires.UnionWith(gateInSubgraph.Wires);
                    neighborGates.UnionWith(wires.SelectMany(w => w.Lamps.Select(l => l.Gate)));
                    neighborGates.UnionWith(wires.SelectMany(w => w.Gates));
                }
                foreach (var neighborGate in neighborGates)
                {
                    if (searchableGates.Contains(neighborGate) &&
                        !subgraphSet.Gates.Contains(neighborGate) /*&&
                        !processedGates.Contains(neighborGate)*/)
                    {
                        boundaryGates.Add(new KeyValuePair<Subgraph, Gate>(subgraph, neighborGate));
                    }
                }
            }
            return boundaryGates
                .Distinct()
                .GroupBy(kvp => gateSignatures[kvp.Value])
                .Where(g => g.Select(kvp => kvp.Key).Distinct().Count() == subgraphSet.Subgraphs.Count)
                .ToList();
        }

        private static List<List<Gate>> GetCombinations(List<List<Gate>> gatesByInstance)
        {
            var result = new List<List<Gate>>();
            if (gatesByInstance.Count == 0) return result;

            void GenerateCombinations(int instanceIndex, List<Gate> currentCombination)
            {
                if (instanceIndex == gatesByInstance.Count)
                {
                    result.Add(new List<Gate>(currentCombination));
                    return;
                }

                foreach (var gate in gatesByInstance[instanceIndex])
                {
                    currentCombination.Add(gate);
                    GenerateCombinations(instanceIndex + 1, currentCombination);
                    currentCombination.RemoveAt(currentCombination.Count - 1);
                }
            }

            GenerateCombinations(0, []);
            return result;
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

        private static bool AreSubgraphSetIsomorphic(SubgraphSet subgraphSet)
        {
            long lastModuleHash = 0;
            for (var i = 0; i < subgraphSet.Subgraphs.Count; i++)
            {
                var componentHashs = subgraphSet.Subgraphs[i].ComponentHashs;
                var moduleHash = GetArrayLongHash([.. componentHashs.Values.Order()]);
                if (i != 0 && moduleHash != lastModuleHash) return false;
                lastModuleHash = moduleHash;
            }
            return true;
        }

        private static double GetSubgraphSetScore(SubgraphSet subgraphSet)
        {
            var module = subgraphSet.Subgraphs.First().Module;
            var portCount = module.InputPorts.Count + module.OutputPorts.Count;
            var allCount = module.Gates.Count + module.Lamps.Count + module.Wires.Count + portCount;
            return (subgraphSet.Gates.Count - 2) * (subgraphSet.Subgraphs.Count - 1) + (1 - (double)portCount / allCount);
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

        private static void SubGraphSetAddExpansion(SubgraphSet subgraphSet, List<Gate> expansionGates)
        {
            for (int i = 0; i < subgraphSet.Subgraphs.Count; i++)
            {
                subgraphSet.Subgraphs[i].Gates.Add(expansionGates[i]);
                var (module, compoundsFound) = CopySubGraphToModule(subgraphSet.Subgraphs[i]);
                subgraphSet.Subgraphs[i].Module = module;
                subgraphSet.Subgraphs[i].CompoundsFound = compoundsFound;
                subgraphSet.Subgraphs[i].ComponentHashs = GetModuleComponentHashs(module);
            }
            subgraphSet.Gates.UnionWith(expansionGates);
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
            public List<Subgraph> Subgraphs { get; }
            public HashSet<Gate> Gates { get; }

            public SubgraphSet(List<Gate> seedGates)
            {
                Subgraphs = seedGates.Select(g => new Subgraph(g)).ToList();
                Gates = new HashSet<Gate>(seedGates);
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
            public Gate InitialSeed { get; }
            public HashSet<Gate> Gates { get; }
            public Module Module { get; set; }
            public Dictionary<object, object> CompoundsFound { get; set; }
            public Dictionary<object, long> ComponentHashs { get; set; }

            public Subgraph(Gate seed)
            {
                InitialSeed = seed;
                Gates = [seed];
                (Module, CompoundsFound) = CopySubGraphToModule(this);
                ComponentHashs = GetModuleComponentHashs(Module);
            }

            private Subgraph(Subgraph other)
            {
                InitialSeed = other.InitialSeed;
                Gates = new HashSet<Gate>(other.Gates);
                (Module, CompoundsFound) = CopySubGraphToModule(other);
                ComponentHashs = GetModuleComponentHashs(Module);
            }

            public Subgraph Clone() => new(this);
        }
    }
}