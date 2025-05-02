
namespace Contributions.Application.Abstractions.Lookups
{
    public interface ILookupService
    {
        bool CodeExists(string table, string code);
        bool CodeIsActive(string table, string code);
        bool OriginRequiresCertification(string originCode);
    }
}
