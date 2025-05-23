public static class EnergyCalculator
{
    public static double CalculateNet(IEnumerable<DapperReading> readings, out int skipped)
    {
        double net = 0;
        skipped = 0;
        foreach (var reading in readings)
        {
            if (ReadingParser.TryParseValue(reading.RawJson, out var value))
                net += value;
            else
                skipped++;
        }
        return net;
    }

    public static double CalculateProduced(IEnumerable<DapperReading> readings, out int skipped)
    {
        double produced = 0;
        skipped = 0;
        foreach (var reading in readings)
        {
            if (ReadingParser.TryParseValue(reading.RawJson, out var value))
            {
                if (value > 0)
                    produced += value;
            }
            else
            {
                skipped++;
            }
        }
        return produced;
    }

    public static double CalculateConsumed(IEnumerable<DapperReading> readings, out int skipped)
    {
        double consumed = 0;
        skipped = 0;
        foreach (var reading in readings)
        {
            if (ReadingParser.TryParseValue(reading.RawJson, out var value))
            {
                if (value < 0)
                    consumed += Math.Abs(value);
            }
            else
            {
                skipped++;
            }
        }
        return consumed;
    }
}
