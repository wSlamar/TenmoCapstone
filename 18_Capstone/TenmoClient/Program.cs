using System;
using System.Collections.Generic;
using TenmoClient.Data;
using TenmoClient.Views;
using TenmoServer.DAO;

namespace TenmoClient
{
    class Program
    {

        static void Main(string[] args)
        {
            AuthService authService = new AuthService();
            
            new LoginRegisterMenu(authService).Show();

            Console.WriteLine("\r\nThank you for using TEnmo!!!\r\n");
        }
    }
}
