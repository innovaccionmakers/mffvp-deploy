namespace Associate.Application.Associates;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Application.Messaging;
using Associate.Integrations.Associates;

public class GetAssociatesQueryHandler : IQueryHandler<GetAssociatesQuery, IReadOnlyCollection<AssociateResponse>>
{
    private readonly List<AssociateResponse> _listAssociates =
    [
        new ("CC", "1014176356", "John Sebastian Rodriguez"),
        new ("CC", "1002003001", "Maria Fernanda Lopez"),
        new ("TI", "900123456", "Carlos Andres Perez"),
        new ("CC", "1020304050", "Ana Lucia Martinez"),
        new ("CE", "123456789", "Luis Alberto Gomez")
    ];

    public Task<Result<IReadOnlyCollection<AssociateResponse>>> Handle(GetAssociatesQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<AssociateResponse> result = _listAssociates;

        if (!string.IsNullOrWhiteSpace(request.IdentificationType))
        {
            result = result.Where(a => a.IdentificationType == request.IdentificationType);
        }

        if (!string.IsNullOrWhiteSpace(request.SearchBy) && !string.IsNullOrWhiteSpace(request.Text))
        {
            if (request.SearchBy.Equals("name", StringComparison.OrdinalIgnoreCase))
            {
                result = result.Where(a => a.FullName.Contains(request.Text, StringComparison.OrdinalIgnoreCase));
            }
            else if (request.SearchBy.Equals("identification", StringComparison.OrdinalIgnoreCase))
            {
                result = result.Where(a => a.Identification.Contains(request.Text, StringComparison.OrdinalIgnoreCase));
            }
        }

        return Task.FromResult(Result.Success<IReadOnlyCollection<AssociateResponse>>(result.ToList()));
    }
}