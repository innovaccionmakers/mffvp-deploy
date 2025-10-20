using Common.SharedKernel.Domain;

namespace Operations.Application.Abstractions.External;

public interface ITrustUpdater
{
    Task<Result> AnnulByDebitNoteAsync(long clientOperationId, CancellationToken cancellationToken);
}
