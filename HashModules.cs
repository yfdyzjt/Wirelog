using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace Wirelog
{
    public static partial class Converter
    {
        // Add limited step transformation equivalence detection.
        private static Dictionary<object, long> _componentHashes;

        private const int MaxModuleSearchLevel = 5;
        private const int MaxHashIterations = 100;
        private const int MaxGateSearchLevel = 128;

        private static void HashModules()
        {

            for (var level = 0; level < MaxModuleSearchLevel; level++)
            {
                Main.statusText = $"Hashing modules: Level {level + 1}";

                RunHashingIteration(
                    _wires,
                    _inputPorts,
                    _outputPorts,
                    _lampsFound.Values,
                    _gatesFound.Values,
                    _moduleInstances,
                    _componentHashes);

                var gateGroups = _gatesFound.Values
                    .GroupBy(g => _componentHashes[g])
                    .Where(g => g.Count() > 1)
                    .ToList();
                if (gateGroups.Count == 0) break;

                var modulesCount = CreateModulesFromGroups(gateGroups);
                if (modulesCount == 0) break;
            }
        }

        private static void RunHashingIteration(
            ICollection<Wire> wires,
            ICollection<InputPort> inputPorts,
            ICollection<OutputPort> outputPorts,
            ICollection<Lamp> lamps,
            ICollection<Gate> gates,
            ICollection<ModuleInstance> moduleInstances,
            Dictionary<object, long> componentHashes)
        {
            InitializeHashes(wires, inputPorts, outputPorts, lamps, gates, moduleInstances, componentHashes);
            int iterations = 0;
            while (true)
            {
                var newHashes = ComputeNextHashes(wires, inputPorts, outputPorts, lamps, gates, moduleInstances, componentHashes);
                if (HashesStabilized(newHashes))
                {
                    _componentHashes = newHashes;
                    break;
                }
                else
                {
                    _componentHashes = newHashes;
                }
                iterations++;
                if (iterations > MaxHashIterations)
                {
                    Main.statusText = $"Hash stabilization did not converge after {MaxHashIterations} iterations.";
                    break;
                }
            }
        }

        private static int CreateModulesFromGroups(List<IGrouping<long, Gate>> gateGroups)
        {
            int createdModules = 0;
            var processedGates = new HashSet<Gate>();

            foreach (var group in gateGroups)
            {
                var candidateGates = group.Where(g => !processedGates.Contains(g)).ToHashSet();
                if (candidateGates.Count < 2) continue;

                var subgraphs = FindMaximalIsomorphicSubgraphs(candidateGates);
                if (subgraphs.Count < 2 || subgraphs[0].Count < 2) continue;

                var (module, _, _, _) = DefineModuleFromGateSet(subgraphs[0]);
                if (module == null) continue;

                var (moduleHash, moduleComponentHashes) = ComputeModuleHash(module);
                if (_moduleDefinitions.ContainsKey(moduleHash)) continue;

                _moduleDefinitions.Add(moduleHash, module);
                module.Hash = moduleHash;
                createdModules++;
                processedGates.UnionWith(subgraphs.SelectMany(s => s));

                var prototypePortsByHash = GetPortsHashMapByComponentHashes(moduleComponentHashes);
                foreach (var subgraph in subgraphs)
                {
                    var instance = CreateModuleInstanceFromSubgraph(module, prototypePortsByHash, subgraph);
                    _moduleInstances.Add(instance);
                    PruneModularizedComponents(subgraph, instance);
                }
            }

            return createdModules;
        }

        private static HashSet<Gate> GetConnectedGates(Gate gate)
        {
            var connectedGates = new HashSet<Gate>();

            foreach (var wire in gate.Wires)
            {
                connectedGates
                    .UnionWith(wire.Gates
                    .Where(g => _gatesFound.ContainsKey(g.Pos)));
            }
            foreach (var lamp in gate.Lamps)
            {
                foreach (var wire in lamp.Wires)
                {
                    connectedGates
                        .UnionWith(wire.Gates
                        .Where(g => _gatesFound.ContainsKey(g.Pos)));
                }
            }
            connectedGates.Remove(gate);

            return connectedGates;
        }

        private static (Module, Dictionary<Gate, Gate>, Dictionary<Lamp, Lamp>, Dictionary<Wire, Wire>)
            DefineModuleFromGateSet(HashSet<Gate> moduleGates)
        {
            var module = new Module();
            var newGatesFound = new Dictionary<Gate, Gate>();
            var newLampsFound = new Dictionary<Lamp, Lamp>();
            var newWiresFound = new Dictionary<Wire, Wire>();

            foreach (var gate in moduleGates)
            {
                var newGate = new Gate { Type = gate.Type };
                module.Gates.Add(newGate);
                newGatesFound[gate] = newGate;
                foreach (var lamp in gate.Lamps)
                {
                    var newLamp = new Lamp { Type = lamp.Type };
                    Link.Add(newLamp, newGate);
                    module.Lamps.Add(newLamp);
                    newLampsFound[lamp] = newLamp;
                }
            }

            foreach (var gate in moduleGates)
            {
                var wires = new HashSet<Wire>();
                wires.UnionWith(gate.Wires);
                wires.UnionWith(gate.Lamps.SelectMany(l => l.Wires));
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
                        Link.Add(newWire, newInputPort);
                    }
                    if (wire.OutputPorts.Count != 0 ||
                        wire.Lamps.Any(l => !newLampsFound.ContainsKey(l)))
                    {
                        var newOutputPort = new OutputPort();
                        Link.Add(newWire, newOutputPort);
                    }
                    newWiresFound[wire] = newWire;
                    module.Wires.Add(newWire);
                }
            }

            return (module, newGatesFound, newLampsFound, newWiresFound);
        }

        private static (long, Dictionary<object, long>) ComputeModuleHash(Module module)
        {
            var componentHashes = new Dictionary<object, long>();

            RunHashingIteration(
                module.Wires,
                module.InputPorts,
                module.OutputPorts,
                module.Lamps,
                module.Gates,
                [],
                componentHashes);
            var sortedHashes = componentHashes.Values.OrderBy(h => h).ToList();

            var code = new HashCode();
            foreach (var hash in sortedHashes) code.Add(hash);
            return (code.ToHashCode(), componentHashes);
        }

        private static Dictionary<long, List<object>> GetPortsHashMapByComponentHashes(Dictionary<object, long> moduleComponentHashes)
        {
            return moduleComponentHashes
                .Where(kvp => kvp.Key is InputPort || kvp.Key is OutputPort)
                .GroupBy(kvp => kvp.Value, kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        private static ModuleInstance CreateModuleInstanceFromSubgraph(Module module, Dictionary<long, List<object>> prototypePortsByHash, HashSet<Gate> subgraphGates)
        {
            var (subgraphModule, _, _, originalToAbstractWireMap) = DefineModuleFromGateSet(subgraphGates);
            var (_, subgraphHashes) = ComputeModuleHash(subgraphModule);
            var subgraphPortsByHash = GetPortsHashMapByComponentHashes(subgraphHashes);

            var abstractToOriginalWireMap = originalToAbstractWireMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);
            var abstractPortToWireMap = new Dictionary<object, Wire>();
            foreach (var wire in subgraphModule.Wires)
            {
                foreach (var port in wire.InputPorts) abstractPortToWireMap[port] = wire;
                foreach (var port in wire.OutputPorts) abstractPortToWireMap[port] = wire;
            }

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

        private static void InitializeHashes(
            ICollection<Wire> wires,
            ICollection<InputPort> inputPorts,
            ICollection<OutputPort> outputPorts,
            ICollection<Lamp> lamps,
            ICollection<Gate> gates,
            ICollection<ModuleInstance> moduleInstances,
            Dictionary<object, long> componentHashes)
        {
            componentHashes.Clear();
            foreach (var wire in wires) componentHashes[wire] = 3;
            foreach (var inputPort in inputPorts) componentHashes[inputPort] = 1;
            foreach (var outputPort in outputPorts) componentHashes[outputPort] = 2;
            foreach (var lamp in lamps) componentHashes[lamp] = (long)lamp.Type;
            foreach (var gate in gates) componentHashes[gate] = (long)gate.Type;
            foreach (var instance in moduleInstances) componentHashes[instance] = instance.Module.Hash;
        }

        private static Dictionary<object, long> ComputeNextHashes(
            ICollection<Wire> wires,
            ICollection<InputPort> inputPorts,
            ICollection<OutputPort> outputPorts,
            ICollection<Lamp> lamps,
            ICollection<Gate> gates,
            ICollection<ModuleInstance> moduleInstances,
            Dictionary<object, long> componentHashes)
        {
            var nextHashes = new Dictionary<object, long>(componentHashes);

            foreach (var inputPort in inputPorts)
            {
                var code = new HashCode();
                code.Add(componentHashes[inputPort]);
                var wireHashes = inputPort.Wires
                    .Where(w => wires.Contains(w))
                    .Select(w => componentHashes[w])
                    .OrderBy(h => h);
                foreach (var h in wireHashes) code.Add(h);
                nextHashes[inputPort] = code.ToHashCode();
            }

            foreach (var outputPort in outputPorts)
            {
                var code = new HashCode();
                if (wires.Contains(outputPort.Wire))
                    code.Add(componentHashes[outputPort.Wire]);
                nextHashes[outputPort] = code.ToHashCode();
            }

            foreach (var gate in gates)
            {
                var code = new HashCode();
                code.Add(componentHashes[gate]);
                var lampHashes = gate.Lamps
                    .Where(l => lamps.Contains(l))
                    .Select(l => componentHashes[l])
                    .OrderBy(h => h);
                foreach (var h in lampHashes) code.Add(h);
                var wireHashes = gate.Wires
                    .Where(w => wires.Contains(w))
                    .Select(w => componentHashes[w])
                    .OrderBy(h => h);
                foreach (var h in wireHashes) code.Add(h);
                nextHashes[gate] = code.ToHashCode();
            }

            foreach (var lamp in lamps)
            {
                var code = new HashCode();
                code.Add(componentHashes[lamp]);
                if (gates.Contains(lamp.Gate))
                    code.Add(componentHashes[lamp.Gate]);
                var wireHashes = lamp.Wires
                    .Where(w => wires.Contains(w))
                    .Select(w => componentHashes[w])
                    .OrderBy(h => h);
                foreach (var h in wireHashes) code.Add(h);
                nextHashes[lamp] = code.ToHashCode();
            }

            foreach (var wire in wires)
            {
                var code = new HashCode();
                code.Add(componentHashes[wire]);

                var source = (object)wire.Gates.FirstOrDefault() ??
                    wire.InputPorts.FirstOrDefault();
                if (source != null &&
                    (gates.Contains(source) ||
                    inputPorts.Contains(source) ||
                    moduleInstances.Contains(source)))
                {
                    code.Add(componentHashes[source]);
                }

                var sinkHashes = wire.Lamps
                    .Where(l => lamps.Contains(l))
                    .Select(l => componentHashes[l])
                    .Concat(wire.OutputPorts
                    .Where(p => outputPorts.Contains(p))
                    .Select(p => componentHashes[p]))
                    .OrderBy(h => h);
                foreach (var h in sinkHashes) code.Add(h);

                nextHashes[wire] = code.ToHashCode();
            }

            foreach (var instance in moduleInstances)
            {
                var code = new HashCode();
                code.Add(componentHashes[instance]);

                var inputWireHashes = instance.InputMapping.Values
                    .Where(w => wires.Contains(w))
                    .Select(w => componentHashes[w])
                    .OrderBy(h => h);
                foreach (var h in inputWireHashes) code.Add(h);

                var outputWireHashes = instance.OutputMapping.Values
                    .Where(w => wires.Contains(w))
                    .Select(w => componentHashes[w])
                    .OrderBy(h => h);
                foreach (var h in outputWireHashes) code.Add(h);

                nextHashes[instance] = code.ToHashCode();
            }

            return nextHashes;
        }

        private static bool HashesStabilized(Dictionary<object, long> newHashes)
        {
            if (newHashes.Count != _componentHashes.Count) return false;
            foreach (var kvp in newHashes)
            {
                if (!_componentHashes.TryGetValue(kvp.Key, out var oldHash) || oldHash != kvp.Value)
                {
                    return false;
                }
            }
            return true;
        }

        private static List<HashSet<Gate>> FindMaximalIsomorphicSubgraphs(HashSet<Gate> gateGroup)
        {
            var bestOverallGrouping = new List<HashSet<Gate>>();
            var assignedGates = new HashSet<Gate>();

            var sortedGates = gateGroup.OrderBy(g => GetConnectedGates(g).Count).ToList();

            foreach (var seedGate in sortedGates)
            {
                if (assignedGates.Contains(seedGate)) continue;

                var bestGroupingForSeed = FindBestGroupingForSeed(seedGate, gateGroup, assignedGates);
                if (bestGroupingForSeed.Count == 0) continue;

                bestOverallGrouping.AddRange(bestGroupingForSeed);
                assignedGates.UnionWith(bestGroupingForSeed.SelectMany(g => g));
            }
            return bestOverallGrouping;
        }

        private static List<HashSet<Gate>> FindBestGroupingForSeed(Gate seedGate, HashSet<Gate> gateGroup, HashSet<Gate> assignedGates)
        {
            var bestGrouping = new List<HashSet<Gate>>();
            double bestScore = -1;

            var potentialPeers = gateGroup.Where(g => g != seedGate && !assignedGates.Contains(g)).ToList();

            for (int k = 2; k <= potentialPeers.Count + 1; k++)
            {
                foreach (var combination in GetCombinations(potentialPeers, k - 1))
                {
                    var seeds = new List<Gate> { seedGate };
                    seeds.AddRange(combination);

                    var initialSubgraphs = seeds.Select(s => new HashSet<Gate> { s }).ToList();
                    var initialAssigned = new HashSet<Gate>(assignedGates);
                    foreach (var seed in seeds) initialAssigned.Add(seed);

                    var allExpansions = FindAllExpansions(initialSubgraphs, initialAssigned, MaxGateSearchLevel);

                    foreach (var expansion in allExpansions)
                    {
                        var currentScore = EvaluateGrouping(expansion);
                        if (currentScore > bestScore)
                        {
                            bestScore = currentScore;
                            bestGrouping.Clear();
                            bestGrouping.AddRange(expansion);
                        }
                    }
                }
            }

            return bestGrouping;
        }

        private static List<List<HashSet<Gate>>> FindAllExpansions(List<HashSet<Gate>> currentSubgraphs, HashSet<Gate> assignedGates, int remainingSteps)
        {
            if (remainingSteps == 0) return [currentSubgraphs];

            var frontiers = currentSubgraphs.Select(sg =>
            new HashSet<Gate>(sg
                .SelectMany(GetConnectedGates)
                .Where(adj => !assignedGates.Contains(adj) && _componentHashes[adj] != 0)))
                .ToList();
            if (frontiers.Any(f => f.Count == 0)) return [currentSubgraphs];

            var commonHashes = frontiers
                .Select(f => new HashSet<long>(f.Select(g => _componentHashes[g])))
                .Aggregate((a, b) => { a.IntersectWith(b); return a; });
            if (commonHashes.Count == 0) return [currentSubgraphs];

            var allExpansions = new List<List<HashSet<Gate>>>();
            foreach (var hash in commonHashes)
            {
                var candidateLists = frontiers
                    .Select(f => f.Where(g => _componentHashes[g] == hash).ToList())
                    .ToList();
                if (candidateLists.Any(l => l.Count == 0)) continue;

                foreach (var combination in GetCartesianProduct(candidateLists))
                {
                    var newSubgraphs = new List<HashSet<Gate>>();
                    var newAssignedGates = new HashSet<Gate>(assignedGates);
                    bool combinationValid = true;

                    for (int i = 0; i < currentSubgraphs.Count; i++)
                    {
                        var newSubgraph = new HashSet<Gate>(currentSubgraphs[i]);
                        var gateToAdd = combination[i];
                        if (newAssignedGates.Contains(gateToAdd)) { combinationValid = false; break; }

                        newSubgraph.Add(gateToAdd);
                        newSubgraphs.Add(newSubgraph);
                        newAssignedGates.Add(gateToAdd);
                    }

                    if (combinationValid)
                    {
                        var furtherExpansions = FindAllExpansions(newSubgraphs, newAssignedGates, remainingSteps - 1);
                        allExpansions.AddRange(furtherExpansions);
                    }
                }
            }

            if (allExpansions.Count == 0) return [currentSubgraphs];
            else return allExpansions;
        }

        private static double EvaluateGrouping(List<HashSet<Gate>> grouping)
        {
            if (grouping == null) return 0;
            int k = grouping.Count;
            int size = grouping[0].Count;
            if (k <= 1 || size <= 1) return 0;
            return (double)k * size;
        }

        private static IEnumerable<List<T>> GetCartesianProduct<T>(List<List<T>> lists)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = [[]];
            return lists.Aggregate(
                emptyProduct,
                (accumulator, sequence) =>
                    from accseq in accumulator
                    from item in sequence
                    select accseq.Concat([item])
            ).Select(x => x.ToList());
        }

        private static IEnumerable<IEnumerable<T>> GetCombinations<T>(IEnumerable<T> items, int k)
        {
            if (k == 0)
            {
                yield return Enumerable.Empty<T>();
            }
            else
            {
                int count = 0;
                foreach (var item in items)
                {
                    count++;
                    foreach (var combination in GetCombinations(items.Skip(count), k - 1))
                    {
                        yield return new[] { item }.Concat(combination);
                    }
                }
            }
        }
    }
}