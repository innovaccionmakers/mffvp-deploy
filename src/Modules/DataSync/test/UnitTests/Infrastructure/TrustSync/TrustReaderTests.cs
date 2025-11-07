using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Linq;
using DataSync.Infrastructure.ConnectionFactory.Interfaces;
using DataSync.Infrastructure.TrustSync;
using Npgsql;

namespace DataSync.test.UnitTests.Infrastructure.TrustSync;

public class TrustReaderTests
{
    [Fact]
    public async Task ReadActiveAsync_FiltersByStatusesAndClosingDate()
    {
        // Arrange
        var closingDate = new DateTime(2024, 05, 10, 15, 30, 0, DateTimeKind.Utc);

        var records = new[]
        {
            new FakeTrustRecord(1, 77, closingDate.Date, 1, 1000m, 600m, 400m),
            new FakeTrustRecord(2, 77, closingDate.Date, 2, 2000m, 1200m, 800m),
            new FakeTrustRecord(3, 77, closingDate.Date.AddDays(-1), 1, 3000m, 1800m, 1200m),
            new FakeTrustRecord(4, 77, closingDate.Date, 3, 4000m, 2400m, 1600m),
            new FakeTrustRecord(5, 80, closingDate.Date, 1, 5000m, 3000m, 2000m),
            new FakeTrustRecord(6, 77, closingDate.Date.AddDays(-1), 2, 6000m, 3600m, 2400m)
        };

        var factory = new FakeTrustConnectionFactory();
        var reader = new TestTrustReader(factory, records);

        // Act
        var result = await reader.ReadActiveAsync(77, closingDate, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, row => Assert.Equal(closingDate.Date, row.ClosingDate));

        var ordered = result.OrderBy(row => row.TrustId).ToArray();
        Assert.Equal(1, ordered[0].TrustId);
        Assert.Equal(2, ordered[1].TrustId);
        Assert.Equal(3, ordered[2].TrustId);
    }

    private sealed record FakeTrustRecord(
        long Id,
        int PortfolioId,
        DateTime FechaActualizacion,
        int Estado,
        decimal SaldoTotal,
        decimal Capital,
        decimal RetencionContingente);

    private sealed class TestTrustReader : TrustReader
    {
        private readonly IReadOnlyList<FakeTrustRecord> _records;

        public TestTrustReader(ITrustConnectionFactory factory, IReadOnlyList<FakeTrustRecord> records)
            : base(factory)
        {
            _records = records;
        }

        protected override NpgsqlCommand CreateCommand(NpgsqlConnection connection, string commandText)
        {
            return new NpgsqlCommand(commandText);
        }

        protected override Task<DbDataReader> ExecuteReaderAsync(NpgsqlCommand command, CancellationToken cancellationToken)
        {
            var portfolioId = (int)command.Parameters["p"].Value!;
            var closingDate = ((DateTime)command.Parameters["closingDate"].Value!).Date;
            var activeStatus = Convert.ToInt32(command.Parameters["active"].Value!);
            var annulledStatus = Convert.ToInt32(command.Parameters["annulledByDebitNote"].Value!);

            var rows = _records
                .Where(record => record.PortfolioId == portfolioId)
                .Where(record => record.Estado == activeStatus ||
                                 (record.Estado == annulledStatus && record.FechaActualizacion.Date == closingDate))
                .Select(record => new object[]
                {
                    record.Id,
                    record.PortfolioId,
                    record.SaldoTotal,
                    record.Capital,
                    record.RetencionContingente
                })
                .ToList();

            return Task.FromResult<DbDataReader>(new FakeDataReader(rows));
        }
    }

    private sealed class FakeTrustConnectionFactory : ITrustConnectionFactory
    {
        public Task<NpgsqlConnection> CreateOpenAsync(CancellationToken ct)
        {
            return Task.FromResult(new NpgsqlConnection());
        }
    }

    private sealed class FakeDataReader : DbDataReader
    {
        private readonly IReadOnlyList<object[]> _rows;
        private int _index = -1;

        public FakeDataReader(IReadOnlyList<object[]> rows)
        {
            _rows = rows;
        }

        public override object this[int ordinal] => GetValue(ordinal);

        public override object this[string name] => GetValue(GetOrdinal(name));

        public override int FieldCount => 5;

        public override bool HasRows => _rows.Count > 0;

        public override bool IsClosed => false;

        public override int RecordsAffected => 0;

        public override int Depth => 0;

        public override bool Read()
        {
            _index++;
            return _index < _rows.Count;
        }

        public override Task<bool> ReadAsync(CancellationToken cancellationToken)
            => Task.FromResult(Read());

        public override object GetValue(int ordinal)
        {
            return _rows[_index][ordinal];
        }

        public override int GetValues(object[] values)
        {
            var currentRow = _rows[_index];
            var length = Math.Min(values.Length, currentRow.Length);
            Array.Copy(currentRow, values, length);
            return length;
        }

        public override string GetName(int ordinal)
        {
            return ordinal switch
            {
                0 => "id",
                1 => "portafolio_id",
                2 => "saldo_total",
                3 => "capital",
                4 => "retencion_contingente",
                _ => throw new IndexOutOfRangeException()
            };
        }

        public override int GetOrdinal(string name)
        {
            return name switch
            {
                "id" => 0,
                "portafolio_id" => 1,
                "saldo_total" => 2,
                "capital" => 3,
                "retencion_contingente" => 4,
                _ => throw new IndexOutOfRangeException(name)
            };
        }

        public override string GetDataTypeName(int ordinal) => GetFieldType(ordinal).Name;

        public override Type GetFieldType(int ordinal)
        {
            if (_rows.Count == 0)
            {
                return typeof(object);
            }

            var value = _rows[0][ordinal];
            return value?.GetType() ?? typeof(object);
        }

        public override bool GetBoolean(int ordinal) => Convert.ToBoolean(GetValue(ordinal));

        public override byte GetByte(int ordinal) => Convert.ToByte(GetValue(ordinal));

        public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
            => throw new NotSupportedException();

        public override char GetChar(int ordinal) => Convert.ToChar(GetValue(ordinal));

        public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
            => throw new NotSupportedException();

        public override Guid GetGuid(int ordinal)
        {
            var value = GetValue(ordinal);
            return value switch
            {
                Guid guid => guid,
                string str when Guid.TryParse(str, out var parsed) => parsed,
                _ => throw new InvalidCastException()
            };
        }

        public override short GetInt16(int ordinal) => Convert.ToInt16(GetValue(ordinal));

        public override int GetInt32(int ordinal) => Convert.ToInt32(GetValue(ordinal));

        public override long GetInt64(int ordinal) => Convert.ToInt64(GetValue(ordinal));

        public override float GetFloat(int ordinal) => Convert.ToSingle(GetValue(ordinal));

        public override double GetDouble(int ordinal) => Convert.ToDouble(GetValue(ordinal));

        public override string GetString(int ordinal) => Convert.ToString(GetValue(ordinal))!;

        public override decimal GetDecimal(int ordinal) => Convert.ToDecimal(GetValue(ordinal));

        public override DateTime GetDateTime(int ordinal)
        {
            var value = GetValue(ordinal);
            return value switch
            {
                DateTime dt => dt,
                DateOnly dateOnly => dateOnly.ToDateTime(TimeOnly.MinValue),
                IConvertible convertible => convertible.ToDateTime(null),
                _ => throw new InvalidCastException()
            };
        }

        public override bool IsDBNull(int ordinal)
        {
            var value = GetValue(ordinal);
            return value is null || Convert.IsDBNull(value);
        }

        public override int VisibleFieldCount => FieldCount;

        public override bool NextResult() => false;

        public override Task<bool> NextResultAsync(CancellationToken cancellationToken)
            => Task.FromResult(false);

        public override void Close()
        {
        }

        public override DataTable GetSchemaTable() => throw new NotSupportedException();

        public override IEnumerator GetEnumerator()
        {
            return ((IEnumerable)_rows).GetEnumerator();
        }
    }
}
