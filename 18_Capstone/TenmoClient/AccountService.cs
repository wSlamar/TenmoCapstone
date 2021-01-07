using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using TenmoClient.Data;
using TenmoServer.Controllers;
using TenmoServer.DAO;
using TenmoServer.Models;

namespace TenmoClient
{
    public class AccountService
    {
        private static API_Account acc = new API_Account();

        public static void SetAccount(API_Account account)
        {
            acc = account;
        }
        public static decimal GetBalance()
        {
            return acc.Balance;
        }
    }
}
