namespace Tombatron.Results.Analyzers
{
    public partial class ResultTypeSwitchSuppressor
    {
        private const string SuppressorId = "TBTRA901";
        private const string SuppressedId = "CS8509";
        private const string Justification = "Switch expression is exhaustive for Tombatron.Results.Result type, otherwise there's an error.";
    }
}
