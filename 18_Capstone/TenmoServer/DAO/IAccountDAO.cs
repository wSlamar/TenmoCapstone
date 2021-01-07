using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface IAccountDAO
    {
        Account GetAccount(int userId);
        void Transfer(int userId, int transferId, decimal amount);
        List<Transfer> GetTransfers(int userId);
        void RequestTransfer(int userId, int transferId, decimal amount);
        List<Transfer> GetRequests(int userId);
        void UpdateTransfer(int fromId, decimal amount, int transId, bool approve);
    }
}
