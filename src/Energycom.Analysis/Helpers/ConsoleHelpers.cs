namespace Energycom.Analysis.Helpers
{
    public static class ConsoleHelpers
    {
        public static string AnsiColor(string text, string colorCode)
        {
            return $"\u001b[{colorCode}m{text}\u001b[0m";
        }
        public static void PrintSideBySide(IList<string> left, IList<string> right, int gap = 4)
        {
            int maxLines = Math.Max(left.Count, right.Count);
            for (int i = 0; i < maxLines; i++)
            {
                string l = i < left.Count ? left[i] : new string(' ', left[0].Length);
                string r = i < right.Count ? right[i] : "";
                Console.WriteLine(l + new string(' ', gap) + r);
            }
        }
    }
}
