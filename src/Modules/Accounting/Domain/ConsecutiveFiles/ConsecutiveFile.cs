using Common.SharedKernel.Domain;

namespace Accounting.Domain.ConsecutiveFiles;

public class ConsecutiveFile : Entity
{
    public long Id { get; private set; }
    public DateTime GenerationDate { get; private set; }
    public int Consecutive { get; private set; }
    public DateTime CurrentDate { get; private set; }

    private ConsecutiveFile()
    {
    }

    public static Result<ConsecutiveFile> Create(
        DateTime generationDate,
        int consecutive,
        DateTime currentDate)
    {
        var consecutiveFile = new ConsecutiveFile
        {
            Id = default,
            GenerationDate = generationDate,
            Consecutive = consecutive,
            CurrentDate = currentDate
        };
        return Result.Success(consecutiveFile);
    }

    public void UpdateDetails(
        DateTime generationDate,
        int consecutive,
        DateTime currentDate)
    {
        GenerationDate = generationDate;
        Consecutive = consecutive;
        CurrentDate = currentDate;
    }
}

