using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reports.Domain.Deposits
{
    public class DepositsRequest
    {
        public DateTime ProcessDate { get; set; }
        public bool IsValid()
        {
            return ProcessDate != default;
        }
    }
}
