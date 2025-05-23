namespace Energycom.Analysis.Tests
{
    using Xunit;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ReadingParserTests
    {
        [Fact]
        public void TryParseValue_ParsesNumber()
        {
            string json = @"{ ""Value"": 42.5, ""Unit"": ""kwh"" }";
            var result = ReadingParser.TryParseValue(json, out var value);
            Assert.True(result);
            Assert.Equal(42.5, value, 2);
        }

        [Fact]
        public void TryParseValue_ParsesQuotedHex()
        {
            decimal original = 123.45M;
            var bits = decimal.GetBits(original);
            var bytes = bits.SelectMany(BitConverter.GetBytes).ToArray();
            var hex = BitConverter.ToString(bytes).Replace("-", "");
            string json = $@"{{ ""Value"": ""{hex}"" }}";
            var result = ReadingParser.TryParseValue(json, out var value);
            Assert.True(result);
            Assert.Equal((double)original, value, 2);
        }

        [Fact]
        public void TryParseValue_ParsesUnquotedHex()
        {
            decimal original = 77.77M;
            var bits = decimal.GetBits(original);
            var bytes = bits.SelectMany(BitConverter.GetBytes).ToArray();
            var hex = BitConverter.ToString(bytes).Replace("-", "");
            string json = $@"{{ ""Value"": ""{hex}"" }}";
            var result = ReadingParser.TryParseValue(json, out var value);
            Assert.True(result);
            Assert.Equal((double)original, value, 2);
        }

        [Fact]
        public void TryParseValue_ParsesStringNumber()
        {
            string json = @"{ ""Value"": ""123.45"" }";
            var result = ReadingParser.TryParseValue(json, out var value);
            Assert.True(result);
            Assert.Equal(123.45, value, 2);
        }

        [Fact]
        public void TryParseValue_ReturnsFalseOnMalformedJson()
        {
            string json = @"{ ""Value"": ";
            var result = ReadingParser.TryParseValue(json, out var value);
            Assert.False(result);
        }

        [Fact]
        public void TryParseValue_ReturnsFalseOnMissingValue()
        {
            string json = @"{ ""Unit"": ""kwh"" }";
            var result = ReadingParser.TryParseValue(json, out var value);
            Assert.False(result);
        }

        [Fact]
        public void TryParseValue_ReturnsFalseOnNonNumericString()
        {
            string json = @"{ ""Value"": ""not-a-number"" }";
            var result = ReadingParser.TryParseValue(json, out var value);
            Assert.False(result);
        }

        [Fact]
        public void TryParseValue_ReturnsFalseOnShortHex()
        {
            string json = @"{ ""Value"": ""ABCDEF"" }";
            var result = ReadingParser.TryParseValue(json, out var value);
            Assert.False(result);
        }

        [Fact]
        public void TryParseValue_ReturnsFalseOnNull()
        {
            string json = @"{ ""Value"": null }";
            var result = ReadingParser.TryParseValue(json, out var value);
            Assert.False(result);
        }
    }

    public class EnergyCalculatorTests
    {
        [Fact]
        public void CalculateNet_SumsAllValidValues()
        {
            decimal five = 5m;
            var bits = decimal.GetBits(five);
            var bytes = bits.SelectMany(BitConverter.GetBytes).ToArray();
            var hex = BitConverter.ToString(bytes).Replace("-", "");

            var readings = new List<DapperReading>
            {
                new DapperReading(Guid.NewGuid(), @"{ ""Value"": 10 }", DateTime.UtcNow, 1, "Meter1", "GroupA"),
                new DapperReading(Guid.NewGuid(), @"{ ""Value"": 20 }", DateTime.UtcNow, 2, "Meter2", "GroupB"),
                new DapperReading(Guid.NewGuid(), $@"{{ ""Value"": ""{hex}"" }}", DateTime.UtcNow, 3, "Meter3", "GroupC"),
            };
            var net = EnergyCalculator.CalculateNet(readings, out int skipped);
            Assert.Equal(35, net, 2);
            Assert.Equal(0, skipped);
        }

        [Fact]
        public void CalculateNet_SkipsInvalidReadings()
        {
            var readings = new List<DapperReading>
            {
                new DapperReading(Guid.NewGuid(), @"{ ""Value"": 10 }", DateTime.UtcNow, 1, "Meter1", "GroupA"),
                new DapperReading(Guid.NewGuid(), @"{ ""Value"": ""not-a-number"" }", DateTime.UtcNow, 2, "Meter2", "GroupB"),
            };
            var net = EnergyCalculator.CalculateNet(readings, out int skipped);
            Assert.Equal(10, net, 2);
            Assert.Equal(1, skipped);
        }

        [Fact]
        public void CalculateNet_ReturnsZeroOnEmptyList()
        {
            var readings = new List<DapperReading>();
            var net = EnergyCalculator.CalculateNet(readings, out int skipped);
            Assert.Equal(0, net);
            Assert.Equal(0, skipped);
        }

        [Fact]
        public void CalculateProduced_SumsOnlyPositive()
        {
            var readings = new List<DapperReading>
            {
                new DapperReading(Guid.NewGuid(), @"{ ""Value"": 10 }", DateTime.UtcNow, 1, "Meter1", "GroupA"),
                new DapperReading(Guid.NewGuid(), @"{ ""Value"": -5 }", DateTime.UtcNow, 2, "Meter2", "GroupB"),
                new DapperReading(Guid.NewGuid(), @"{ ""Value"": 0 }", DateTime.UtcNow, 3, "Meter3", "GroupC"),
            };
            var produced = EnergyCalculator.CalculateProduced(readings, out int skipped);
            Assert.Equal(10, produced, 2);
            Assert.Equal(0, skipped);
        }

        [Fact]
        public void CalculateConsumed_SumsAbsoluteNegatives()
        {
            var readings = new List<DapperReading>
            {
                new DapperReading(Guid.NewGuid(), @"{ ""Value"": -10 }", DateTime.UtcNow, 1, "Meter1", "GroupA"),
                new DapperReading(Guid.NewGuid(), @"{ ""Value"": 5 }", DateTime.UtcNow, 2, "Meter2", "GroupB"),
                new DapperReading(Guid.NewGuid(), @"{ ""Value"": -2.5 }", DateTime.UtcNow, 3, "Meter3", "GroupC"),
            };
            var consumed = EnergyCalculator.CalculateConsumed(readings, out int skipped);
            Assert.Equal(12.5, consumed, 2);
            Assert.Equal(0, skipped);
        }

        [Fact]
        public void CalculateProduced_ReturnsZeroIfNoPositive()
        {
            var readings = new List<DapperReading>
            {
                new DapperReading(Guid.NewGuid(), @"{ ""Value"": -10 }", DateTime.UtcNow, 1, "Meter1", "GroupA"),
                new DapperReading(Guid.NewGuid(), @"{ ""Value"": 0 }", DateTime.UtcNow, 2, "Meter2", "GroupB"),
            };
            var produced = EnergyCalculator.CalculateProduced(readings, out int skipped);
            Assert.Equal(0, produced, 2);
            Assert.Equal(0, skipped);
        }

        [Fact]
        public void CalculateConsumed_ReturnsZeroIfNoNegative()
        {
            var readings = new List<DapperReading>
            {
                new DapperReading(Guid.NewGuid(), @"{ ""Value"": 10 }", DateTime.UtcNow, 1, "Meter1", "GroupA"),
                new DapperReading(Guid.NewGuid(), @"{ ""Value"": 0 }", DateTime.UtcNow, 2, "Meter2", "GroupB"),
            };
            var consumed = EnergyCalculator.CalculateConsumed(readings, out int skipped);
            Assert.Equal(0, consumed, 2);
            Assert.Equal(0, skipped);
        }

        [Fact]
        public void CalculateNet_SkipsAllInvalid()
        {
            var readings = new List<DapperReading>
            {
                new DapperReading(Guid.NewGuid(), @"{ ""Value"": ""something but a number!!:D"" }", DateTime.UtcNow, 1, "Meter1", "GroupA"),
                new DapperReading(Guid.NewGuid(), @"{ ""Value"": """" }", DateTime.UtcNow, 2, "Meter2", "GroupB"),
            };
            var net = EnergyCalculator.CalculateNet(readings, out int skipped);
            Assert.Equal(0, net, 2);
            Assert.Equal(2, skipped);
        }
    }
}