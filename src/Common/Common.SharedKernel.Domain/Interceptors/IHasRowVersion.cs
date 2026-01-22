namespace Common.SharedKernel.Domain.Interceptors;

public interface IHasRowVersion
{
    long RowVersion { get; set; }
}
