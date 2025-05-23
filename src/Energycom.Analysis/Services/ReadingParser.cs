using System.Text.Json;
using System.Text.RegularExpressions;

public static class ReadingParser
{
    public static bool TryParseValue(string rawJson, out double value)
    {
        value = 0;
        try
        {
            var fixedJson = Regex.Replace(
                rawJson,
                @"(""Value""\s*:\s*)([A-Fa-f0-9]{32})(\s*,)",
                "$1\"$2\"$3"
            );

            using var doc = JsonDocument.Parse(fixedJson);
            var root = doc.RootElement;
            if (root.TryGetProperty("Value", out var valueProp))
            {
                if (valueProp.ValueKind == JsonValueKind.Number)
                {
                    value = valueProp.GetDouble();
                    return true;
                }
                else if (valueProp.ValueKind == JsonValueKind.String)
                {
                    var hex = valueProp.GetString();
                    if (!string.IsNullOrEmpty(hex) && Regex.IsMatch(hex, @"^[0-9A-Fa-f]{32}$"))
                    {
                        var bytes = Enumerable.Range(0, 16)
                            .Select(i => Convert.ToByte(hex.Substring(i * 2, 2), 16))
                            .ToArray();
                        var bits = new int[4];
                        for (int i = 0; i < 4; i++)
                            bits[i] = BitConverter.ToInt32(bytes, i * 4);
                        decimal dec = new decimal(bits);
                        value = (double)dec;
                        return true;
                    }
                    if (double.TryParse(hex, out var dbl))
                    {
                        value = dbl;
                        return true;
                    }
                }
            }
        }
        catch
        {}
        return false;
    }
}
