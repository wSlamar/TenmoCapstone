using System;
using System.Collections.Generic;
using System.Text;

namespace TenmoClient.Data
{
    public class API_Account
    {
        public int UserId { get; set; }
        public int AccountId { get; set; }
        public decimal Balance { get; set; }
    }

    public class Transfer
    {
        public int UserId { get; set; }
        public int TransferId { get; set; }
        public decimal Amount { get; set; }
        public string FromName { get; set; } 
        public string ToName { get; set; }
        public bool Status { get; set; }
        public int TransferIdColum { get; set; }
     //   public int ToId { get; set; }
    }

}
