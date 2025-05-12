using System.Data.Common;

namespace Common.SharedKernel.Application.Data;

public interface IDbConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync();
}