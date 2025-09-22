using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customers.Domain.People
{
    public sealed class PeopleByIdentifications
    {
        public PeopleByIdentifications(string identification, string name, string fullName)
        {
            Identification = identification;
            DocumentType = name;
            FullName = fullName;
        }

        public string Identification { get;  set; }
        public string DocumentType { get;  set; }
        public string FullName { get;  set; }
    }
}
