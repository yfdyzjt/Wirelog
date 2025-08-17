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
                    if (AreSubgraphSetIsomorphic(currentSubgraphSet, combination))
                    {
                        var nextSubgraphSet = currentSubgraphSet.Clone();
                        SubGraphSetAddExpansion(nextSubgraphSet, combination);

                        var resultFromPath = FindBestSubgraphSetExpansion(nextSubgraphSet, gateSignatures, searchableGates/*, processedGates*/);

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
                    var gateWires = gateInSubgraph.Wires;
                    neighborGates.UnionWith(gateWires.SelectMany(w => w.Lamps.Select(l => l.Gate)));
                    var lampWires = gateInSubgraph.Lamps.SelectMany(l => l.Wires);
                    neighborGates.UnionWith(lampWires.SelectMany(w => w.Gates));
                    /*
                    var wires = new HashSet<Wire>();
                    wires.UnionWith(gateInSubgraph.Lamps.SelectMany(l => l.Wires));
                    wires.UnionWith(gateInSubgraph.Wires);
                    neighborGates.UnionWith(wires.SelectMany(w => w.Lamps.Select(l => l.Gate)));
                    neighborGates.UnionWith(wires.SelectMany(w => w.Gates));
                    */
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
                newLabels[c] = GetArrayLongHash(neighborLabels);
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

        private static HashSet<object> GetModuleComponents(Module module)
        {
            var components = new HashSet<object>();
            components.UnionWith(module.Wires);
            components.UnionWith(module.Lamps);
            components.UnionWith(module.Gates);
            components.UnionWith(module.InputPorts);
            components.UnionWith(module.OutputPorts);
            return components;
        }

        private static Dictionary<object, long> GetModuleComponentHashs(Module module)
        {
            return RunHashingIteration(GetModuleComponents(module));
        }

        private static bool AreSubgraphSetIsomorphic(SubgraphSet subgraphSet, List<Gate> expansion)
        {
            var newSubgraphSet = subgraphSet.Clone();
            SubGraphSetAddExpansion(newSubgraphSet, expansion);

            long lastModuleHash = 0;
            for (var i = 0; i < subgraphSet.Subgraphs.Count; i++)
            {
                var module = CopySubGraphToModule(subgraphSet.Subgraphs[i]).module;
                var componentHashs = GetModuleComponentHashs(module);
                var moduleHash = GetArrayLongHash([.. componentHashs.Values.Order()]);
                if (i != 0 && moduleHash != lastModuleHash) return false;
                lastModuleHash = moduleHash;
            }
            return true;
        }

        private static int GetSubgraphSetScore(SubgraphSet subgraphSet)
        {
            return (subgraphSet.Gates.Count - 2) * (subgraphSet.Subgraphs.Count - 1);
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
            }
            subgraphSet.Gates.UnionWith(expansionGates);
        }

        private static (Module module,
            Dictionary<Wire, Wire> newWiresFound)
            CopySubGraphToModule(Subgraph subgraph)
        {
            var module = new Module();
            var newGatesFound = new Dictionary<Gate, Gate>();
            var newLampsFound = new Dictionary<Lamp, Lamp>();
            var newWiresFound = new Dictionary<Wire, Wire>();
            var wires = new HashSet<Wire>();

            foreach (var gate in subgraph.Gates)
            {
                var newGate = new Gate { Type = gate.Type };
                module.Gates.Add(newGate);
                newGatesFound[gate] = newGate;
                wires.UnionWith(gate.Wires);
                foreach (var lamp in gate.Lamps)
                {
                    var newLamp = new Lamp { Type = lamp.Type };
                    Link.Add(newLamp, newGate);
                    module.Lamps.Add(newLamp);
                    newLampsFound[lamp] = newLamp;
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
                }
                if (wire.OutputPorts.Count != 0 ||
                    wire.Lamps.Any(l => !newLampsFound.ContainsKey(l)))
                {
                    var newOutputPort = new OutputPort();
                    module.OutputPorts.Add(newOutputPort);
                    Link.Add(newWire, newOutputPort);
                }
                module.Wires.Add(newWire);
                newWiresFound[wire] = newWire;
            }

            return (module, newWiresFound);
        }

        private static Module CopySubGraphSetToModule(SubgraphSet subgraphSet)
        {
            return CopySubGraphToModule(subgraphSet.Subgraphs.First()).module;
        }

        private static Dictionary<long, List<object>> GetPortsHashMapByComponentHashes(Dictionary<object, long> moduleComponentHashes)
        {
            return moduleComponentHashes
                .Where(kvp => kvp.Key is InputPort || kvp.Key is OutputPort)
                .GroupBy(kvp => kvp.Value, kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        private static Dictionary<object, Wire> GetPortToWireMap(HashSet<Wire> wires)
        {
            var portToWireMap = new Dictionary<object, Wire>();
            foreach (var wire in wires)
            {
                foreach (var port in wire.InputPorts) portToWireMap[port] = wire;
                foreach (var port in wire.OutputPorts) portToWireMap[port] = wire;
            }
            return portToWireMap;
        }

        private static ModuleInstance CreateModuleInstanceFromSubgraph(
            Module module,
            Dictionary<long, List<object>> prototypePortsByHash,
            Subgraph subgraph)
        {
            var (subgraphModule, originalToAbstractWireMap) = CopySubGraphToModule(subgraph);
            var abstractToOriginalWireMap = originalToAbstractWireMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
            var abstractPortToWireMap = GetPortToWireMap(subgraphModule.Wires);

            var subgraphHashes = GetModuleComponentHashs(subgraphModule);
            var subgraphPortsByHash = GetPortsHashMapByComponentHashes(subgraphHashes);

            var instance = new ModuleInstance { Module = module };
            foreach (var (hash, subgraphPortList) in subgraphPortsByHash)
            {
                if (prototypePortsByHash.TryGetValue(hash, out var prototypePortList) &&
                    subgraphPortList.Count == prototypePortList.Count)
                {
                    for (int i = 0; i < subgraphPortList.Count; i++)
                    {
                        var subgraphPort = subgraphPortList[i];
                        var prototypePort = prototypePortList[i];

                        var abstractWire = abstractPortToWireMap[subgraphPort];
                        var originalWire = abstractToOriginalWireMap[abstractWire];

                        if (prototypePort is InputPort protoInput)
                        {
                            instance.InputMapping[protoInput] = originalWire;
                        }
                        else if (prototypePort is OutputPort protoOutput)
                        {
                            instance.OutputMapping[protoOutput] = originalWire;
                        }
                    }
                }
            }

            return instance;
        }

        private static void PruneModularizedComponents(HashSet<Gate> gatesToPrune, ModuleInstance relatedInstance)
        {
            var lampsToPrune = gatesToPrune
                .SelectMany(g => g.Lamps)
                .ToHashSet();

            var boundaryWires = relatedInstance.InputMapping.Values
                .Union(relatedInstance.OutputMapping.Values)
                .ToHashSet();
            var allWiresInvolved = gatesToPrune
                .SelectMany(g => g.Wires)
                .Union(lampsToPrune
                .SelectMany(l => l.Wires))
                .ToHashSet();
            var wiresToPrune = allWiresInvolved
                .Where(w => !boundaryWires.Contains(w))
                .ToHashSet();

            Link.Remove(gatesToPrune);
            foreach (var gate in gatesToPrune) _gatesFound.Remove(gate.Pos);
            Link.Remove(lampsToPrune);
            foreach (var lamp in lampsToPrune) _lampsFound.Remove(lamp.Pos);
            Link.Remove(wiresToPrune);
            foreach (var wire in wiresToPrune) _wires.Remove(wire);
        }

        private static void ReplaceSubgraphSetWithModuleInstances(SubgraphSet subgraphSet)
        {
            var module = CopySubGraphSetToModule(subgraphSet);
            _moduleDefinitions.Add(module);

            var moduleComponentHashes = GetModuleComponentHashs(module);
            var prototypePortsByHash = GetPortsHashMapByComponentHashes(moduleComponentHashes);

            foreach (var subgraph in subgraphSet.Subgraphs)
            {
                var instance = CreateModuleInstanceFromSubgraph(module, prototypePortsByHash, subgraph);
                _moduleInstances.Add(instance);

                PruneModularizedComponents(subgraph.Gates, instance);
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