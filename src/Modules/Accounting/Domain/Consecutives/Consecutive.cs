using Common.SharedKernel.Domain;

namespace Accounting.Domain.Consecutives;

public class Consecutive : Entity
{
    public long ConsecutiveId { get; private set; }
    public string Nature { get; private set; }
    public string SourceDocument { get; private set; }
    public int Number { get; private set; }

    private Consecutive()
    {
    }

    public static Result<Consecutive> Create(
        string nature,
        string sourceDocument,
        int number)
    {
        var consecutive = new Consecutive
        {
            ConsecutiveId = default,
            Nature = nature,
            SourceDocument = sourceDocument,
            Number = number
        };
        return Result.Success(consecutive);
    }

    public void UpdateDetails(
        string nature,
        string sourceDocument,
        int number)
    {
        Nature = nature;
        SourceDocument = sourceDocument;
        Number = number;
    }
}
