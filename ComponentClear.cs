namespace Wirelog
{
    public static partial class Converter
    {
        private static void PreClear()
        {
            _inputsFound.Clear();
            _outputsFound.Clear();
            _gatesFound.Clear();
            _lampsFound.Clear();
            _wires.Clear();
            _inputPorts = null;
            _outputPorts = null;
            _moduleDefinitions.Clear();
            _moduleInstances.Clear();

            InputsPortFound.Clear();
        }

        private static void PostClear()
        {
            Link.Remove(_wires);
            _wires.Clear();
            Link.Remove(_lampsFound.Values);
            _lampsFound.Clear();
            Link.Remove(_gatesFound.Values);
            _gatesFound.Clear();
            Link.Remove(_moduleInstances);
            _moduleInstances.Clear();
            Link.Remove(_moduleDefinitions);
            _moduleDefinitions.Clear();

            _outputsFound.Clear();
            _inputPorts = null;
        }
    }
}
