using HotChocolate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounting.Presentation.DTOs
{
    public record class AccountingAccountsDto(
        [property: GraphQLName("cuenta")] string Account,
        [property: GraphQLName("nombre")] string Name
        );
}
