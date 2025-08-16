using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace Wirelog
{
    public static partial class Converter
    {
        private static void HashModules()
        {
            Main.statusText = "hashing components";

            var componentHashes = new Dictionary<object, long>();
            RunHashingIteration(
                _wires,
                _inputPorts,
                _outputPorts,
                _lampsFound.Values,
                _gatesFound.Values,
                ref componentHashes);

            Main.statusText = "finding optimal modules";
            var gateGroups = _gatesFound.Values
                    .GroupBy(g => componentHashes[g])
                    .Where(g => g.Count() > 1)
                    .ToList();
            var searchableGates = gateGroups
                    .SelectMany(g => g)
                    .ToHashSet();
            if (gateGroups.Count == 0) return;

            BuildOptimalModules(gateGroups, searchableGates, componentHashes);
        }

        private static void RunHashingIteration(
            ICollection<Wire> wires,
            ICollection<InputPort> inputPorts,
            ICollection<OutputPort> outputPorts,
            ICollection<Lamp> lamps,
            ICollection<Gate> gates,
            ref Dictionary<object, long> componentHashes)
        {
            const int MaxHashIterations = 1000;

            InitializeHashes(wires, inputPorts, outputPorts, lamps, gates, componentHashes);
            CompressionNextHashes(ref componentHashes);
            int iterations = 0;
            while (true)
            {
                var newHashes = ComputeNextHashes(wires, inputPorts, outputPorts, lamps, gates, componentHashes);
                CompressionNextHashes(ref newHashes);
                if (HashesStabilized(newHashes, componentHashes))
                {
                    Main.statusText = $"Hash stabilization converge after {iterations} iterations.";
                    break;
                }
                else if (iterations >= MaxHashIterations)
                {
                    Main.statusText = $"Hash stabilization did not converge after {iterations} iterations.";
                    break;
                }
                componentHashes = newHashes;
                iterations++;
            }
        }

        private static void CompressionNextHashes(ref Dictionary<object, long> hashes)
        {
            var uniqueHashes = hashes.Values.Distinct().OrderBy(h => h).ToList();
            var compressionMap = uniqueHashes
                .Select((hash, index) => new { hash, index })
                .ToDictionary(p => p.hash, p => (long)p.index);
            var compressedHashes = new Dictionary<object, long>();
            foreach (var kvp in hashes)
            {
                compressedHashes[kvp.Key] = compressionMap[kvp.Value];
            }
            hashes = compressedHashes;
        }

        private static void InitializeHashes(
            ICollection<Wire> wires,
            ICollection<InputPort> inputPorts,
            ICollection<OutputPort> outputPorts,
            ICollection<Lamp> lamps,
            ICollection<Gate> gates,
            Dictionary<object, long> componentHashes)
        {
            componentHashes.Clear();
            foreach (var wire in wires) componentHashes[wire] = 0;
            foreach (var inputPort in inputPorts) componentHashes[inputPort] = 1;
            foreach (var outputPort in outputPorts) componentHashes[outputPort] = 2;
            foreach (var lamp in lamps) componentHashes[lamp] = (long)lamp.Type + 3;
            foreach (var gate in gates) componentHashes[gate] = (long)gate.Type + 7;
        }

        private static Dictionary<object, long> ComputeNextHashes(
            ICollection<Wire> wires,
            ICollection<InputPort> inputPorts,
            ICollection<OutputPort> outputPorts,
            ICollection<Lamp> lamps,
            ICollection<Gate> gates,
            Dictionary<object, long> componentHashes)
        {
            var nextHashes = new Dictionary<object, long>();

            foreach (var inputPort in inputPorts)
            {
                var code = new HashCode();
                code.Add(componentHashes[inputPort]);
                var wireHashes = inputPort.Wires
                    .Where(wires.Contains)
                    .Select(w => componentHashes[w])
                    .OrderBy(h => h);
                foreach (var h in wireHashes) code.Add(h);
                nextHashes[inputPort] = code.ToHashCode();
            }

            foreach (var outputPort in outputPorts)
            {
                var code = new HashCode();
                code.Add(componentHashes[outputPort]);
                if (wires.Contains(outputPort.Wire))
                    code.Add(componentHashes[outputPort.Wire]);
                nextHashes[outputPort] = code.ToHashCode();
            }

            foreach (var gate in gates)
            {
                var code = new HashCode();
                code.Add(componentHashes[gate]);
                var lampHashes = gate.Lamps
                    .Where(lamps.Contains)
                    .Select(l => componentHashes[l])
                    .OrderBy(h => h);
                foreach (var h in lampHashes) code.Add(h);
                var wireHashes = gate.Wires
                    .Where(wires.Contains)
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
                    .Where(wires.Contains)
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
                    inputPorts.Contains(source)))
                    code.Add(componentHashes[source]);
                var sinkHashes = wire.Lamps
                    .Where(lamps.Contains)
                    .Select(l => componentHashes[l])
                    .Concat(wire.OutputPorts
                    .Where(outputPorts.Contains)
                    .Select(p => componentHashes[p]))
                    .OrderBy(h => h);
                foreach (var h in sinkHashes) code.Add(h);

                nextHashes[wire] = code.ToHashCode();
            }

            return nextHashes;
        }

        private static bool HashesStabilized(Dictionary<object, long> newHashes, Dictionary<object, long> oldHashes)
        {
            if (newHashes.Count != oldHashes.Count) return false;
            foreach (var kvp in newHashes)
            {
                if (!oldHashes.TryGetValue(kvp.Key, out var oldHash) || oldHash != kvp.Value)
                {
                    return false;
                }
            }
            return true;
        }

        private static void BuildOptimalModules(List<IGrouping<long, Gate>> gateGroups, HashSet<Gate> searchableGates, Dictionary<object, long> componentHashes)
        {
            var allCandidateGroups = FindAllCandidateGroups(gateGroups, searchableGates, componentHashes);
            if (allCandidateGroups.Count == 0) return;

            var bestCombination = FindBestCombination(allCandidateGroups, 0, []).bestCombination;

            foreach (var moduleGroup in bestCombination)
            {
                var prototype = moduleGroup.First();
                if (prototype.Count < 2) continue;

                var module = DefineModuleFromGateSet(prototype).module;
                var (moduleHash, moduleComponentHashes) = ComputeModuleHash(module);
                module.Hash = moduleHash;
                var prototypePortsByHash = GetPortsHashMapByComponentHashes(moduleComponentHashes);

                var instances = new List<ModuleInstance>();
                foreach (var subgraphGates in moduleGroup)
                {
                    var instance = CreateModuleInstanceFromSubgraph(module, prototypePortsByHash, subgraphGates);
                    instances.Add(instance);
                }

                if (instances.Count > 1)
                {
                    _moduleDefinitions.Add(moduleHash, module);
                    foreach (var instance in instances)
                    {
                        _moduleInstances.Add(instance);
                        PruneModularizedComponents(instance.Module.Gates, instance);
                    }
                }
            }
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

        private static (
            Module module,
            Dictionary<Gate, Gate> newGatesFound,
            Dictionary<Lamp, Lamp> newLampsFound,
            Dictionary<Wire, Wire> newWiresFound)
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
                ref componentHashes);
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

        private static List<List<HashSet<Gate>>> FindAllCandidateGroups(List<IGrouping<long, Gate>> gateGroups, HashSet<Gate> searchableGates, Dictionary<object, long> componentHashes)
        {
            var allGroups = new List<List<HashSet<Gate>>>();
            var processedSeeds = new HashSet<Gate>();

            foreach (var seedGateGroup in gateGroups)
            {
                foreach (var seedGate in seedGateGroup)
                {
                    if (processedSeeds.Contains(seedGate)) continue;

                    var potentialPeers = seedGateGroup.Where(g => g != seedGate);
                    var (prototype, matches) = FindMatchesForSeed(seedGate, potentialPeers, searchableGates, componentHashes);

                    if (matches.Count > 0)
                    {
                        var newGroup = new List<HashSet<Gate>> { prototype };
                        newGroup.AddRange(matches);
                        allGroups.Add(newGroup);

                        processedSeeds.UnionWith(prototype);
                        foreach (var match in matches)
                        {
                            processedSeeds.UnionWith(match);
                        }
                    }
                }
            }

            return allGroups;
        }

        private static (HashSet<Gate> prototype, List<HashSet<Gate>> matches) FindMatchesForSeed(Gate seed, IEnumerable<Gate> potentialPeers, HashSet<Gate> searchableGates, Dictionary<object, long> componentHashes)
        {
            var prototype = new HashSet<Gate> { seed };

            var matchesWithMappings = new List<(HashSet<Gate> match, Dictionary<Gate, Gate> mapping)>();
            foreach (var peerSeed in potentialPeers)
            {
                matchesWithMappings.Add((new HashSet<Gate> { peerSeed }, new Dictionary<Gate, Gate> { { seed, peerSeed } }));
            }

            bool changed;
            do
            {
                changed = false;
                var frontier = new HashSet<Gate>(prototype
                    .SelectMany(GetConnectedGates)
                    .Where(g => !prototype.Contains(g) && searchableGates.Contains(g)));
                if (frontier.Count == 0) break;

                Gate bestGateToExpand = null;
                List<(HashSet<Gate> match, Dictionary<Gate, Gate> mapping)> bestVerifiedMatches = null;

                foreach (var gateToExpand in frontier)
                {
                    var currentVerifiedMatches = new List<(HashSet<Gate> match, Dictionary<Gate, Gate> mapping)>();
                    foreach (var (match, mapping) in matchesWithMappings)
                    {
                        var (success, expandedMatch, newMapping) = TryExpandMatch(prototype, match, mapping, gateToExpand, searchableGates, componentHashes);
                        if (success) currentVerifiedMatches.Add((expandedMatch, newMapping));
                    }

                    if (currentVerifiedMatches.Count > 0 && (bestVerifiedMatches == null || currentVerifiedMatches.Count > bestVerifiedMatches.Count))
                    {
                        bestGateToExpand = gateToExpand;
                        bestVerifiedMatches = currentVerifiedMatches;
                    }
                }

                if (bestGateToExpand != null)
                {
                    prototype.Add(bestGateToExpand);
                    matchesWithMappings = bestVerifiedMatches;
                    changed = true;
                }

            } while (changed);

            var finalMatches = matchesWithMappings.Select(m => m.match).ToList();
            return (prototype, finalMatches);
        }

        private static (bool success, HashSet<Gate> expandedMatch, Dictionary<Gate, Gate> newMapping) TryExpandMatch(
            HashSet<Gate> prototype,
            HashSet<Gate> matchToExpand,
            Dictionary<Gate, Gate> mapping,
            Gate prototypeExpansionGate,
            HashSet<Gate> searchableGates,
            Dictionary<object, long> componentHashes)
        {
            var protoNeighbors = GetConnectedGates(prototypeExpansionGate).Where(prototype.Contains).ToHashSet();

            var matchFrontier = matchToExpand
                .SelectMany(GetConnectedGates)
                .Where(g => !matchToExpand.Contains(g) && searchableGates.Contains(g))
                .ToHashSet();

            var expansionCandidates = matchFrontier
                .Where(g => componentHashes[g] == componentHashes[prototypeExpansionGate])
                .ToList();

            foreach (var candidate in expansionCandidates)
            {
                var matchNeighbors = GetConnectedGates(candidate)
                    .Where(matchToExpand.Contains)
                    .ToHashSet();

                if (protoNeighbors.Count != matchNeighbors.Count) continue;

                var mappedProtoNeighbors = protoNeighbors
                    .Select(n => mapping[n])
                    .ToHashSet();
                if (!mappedProtoNeighbors.SetEquals(matchNeighbors)) continue;

                var newMatch = new HashSet<Gate>(matchToExpand) { candidate };
                var newMapping = new Dictionary<Gate, Gate>(mapping) { [prototypeExpansionGate] = candidate };
                return (true, newMatch, newMapping);
            }

            return (false, null, null);
        }

        private static (List<List<HashSet<Gate>>> bestCombination, int bestScore) FindBestCombination(
            List<List<HashSet<Gate>>> allGroups,
            int startIndex,
            HashSet<Gate> usedGates)
        {
            if (startIndex >= allGroups.Count)
            {
                return (new List<List<HashSet<Gate>>>(), 0);
            }

            var (combinationWithout, scoreWithout) = FindBestCombination(allGroups, startIndex + 1, usedGates);

            var currentGroup = allGroups[startIndex];
            var prototype = currentGroup.First();
            var instances = currentGroup;

            bool canInclude = !instances.SelectMany(g => g).Any(usedGates.Contains);

            if (canInclude)
            {
                var newUsedGates = new HashSet<Gate>(usedGates);
                foreach (var instance in instances) newUsedGates.UnionWith(instance);

                var (combinationWith, scoreWith) = FindBestCombination(allGroups, startIndex + 1, newUsedGates);

                int currentScore = (instances.Count - 1) * (prototype.Count - 1);
                scoreWith += currentScore;

                if (scoreWith > scoreWithout)
                {
                    combinationWith.Add(currentGroup);
                    return (combinationWith, scoreWith);
                }
            }

            return (combinationWithout, scoreWithout);
        }
    }
}