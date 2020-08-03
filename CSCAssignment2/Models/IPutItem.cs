using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CSCAssignment2.Models
{
    public interface IPutItem
    {
        Task AddNewEntry(string subscriptionId, string invoiceId, int userId);
    }
}
