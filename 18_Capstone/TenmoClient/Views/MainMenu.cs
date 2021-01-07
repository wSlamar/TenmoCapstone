using MenuFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TenmoClient.Data;
using TenmoServer.Controllers;
using TenmoServer.DAO;
using TenmoServer.Models;

namespace TenmoClient.Views
{
    public class MainMenu : ConsoleMenu
    {
        public MainMenu()
        {
            AddOption("View your current balance", ViewBalance)
                .AddOption("View your past transfers", ViewTransfers)
                .AddOption("View your pending requests", ViewRequests)
                .AddOption("Send TE bucks", SendTEBucks)
                .AddOption("Request TE bucks", RequestTEBucks)
                .AddOption("Log in as different user", Logout)
                .AddOption("Exit", Exit);
        }

        protected override void OnBeforeShow()
        {
            Console.WriteLine($"TE Account Menu for User: {UserService.GetUserName()}");
        }

        private MenuOptionResult ViewBalance()
        {
            AuthService authService = new AuthService();
            decimal d = authService.GetBalance();
            Console.WriteLine($"Your current account balance is: {d:c}");
            return MenuOptionResult.WaitAfterMenuSelection;
        }

        private MenuOptionResult ViewTransfers()
        {
            Data.Transfer transfer = new Data.Transfer();
            AuthService authService = new AuthService();
            transfer.UserId = UserService.GetUserId();
            List<Data.Transfer> list = authService.ReturnTransfers();

            Console.WriteLine("____________________________________________");
            Console.WriteLine("Transfer");
            Console.WriteLine("ID\tFrom/To\t\t\tAmount");
            Console.WriteLine("____________________________________________");
            string currentUser = UserService.GetUserName();
            foreach (Data.Transfer l in list)
            {
                string displayName;
                displayName = (currentUser == l.ToName) ? $"From:\t{l.FromName}" : $"To:  \t{l.ToName}";
                Console.WriteLine($"{l.TransferId}\t{displayName} \t\t${l.Amount}");
            }
            Console.WriteLine("____________________________________________");
            Console.WriteLine();
            Console.Write("Please enter transfer ID to view details (0 to cancel): ");
            string input = Console.ReadLine();
            if (input == "0")
            {
                return MenuOptionResult.DoNotWaitAfterMenuSelection;
            }
            if (input.Length == 0)
            {
                Console.WriteLine("You must enter an ID or enter 0 to cancel!");
                Console.WriteLine("Hit 'Enter' to continue.");
                return MenuOptionResult.WaitAfterMenuSelection;
            }
            int id = Convert.ToInt32(input);
            bool found = false;
            foreach (Data.Transfer l in list)
            {
                if (l.TransferId == id)
                {
                    Console.WriteLine("____________________________________________");
                    Console.WriteLine("Transfer Details");
                    Console.WriteLine("____________________________________________");
                    Console.WriteLine();
                    Console.WriteLine($"Id: { l.TransferId}");
                    Console.WriteLine($"From: {l.FromName}");
                    Console.WriteLine($"To: {l.ToName}");
                    string type;
                    type = (currentUser == l.ToName) ? "Receive" : "Send";
                    Console.WriteLine($"Type: {type}");
                    Console.WriteLine($"Status: Approved");
                    Console.WriteLine($"Amount: ${l.Amount}");
                    found = true;
                }
            }
            if (!found)
            {
                Console.WriteLine("The ID you entered was not valid!");
                Console.WriteLine("Hit 'Enter' to continue.");
            }

            return MenuOptionResult.WaitAfterMenuSelection;
        }

        private MenuOptionResult ViewRequests()
        {
            Data.Transfer transfer = new Data.Transfer();
            AuthService authService = new AuthService();
            transfer.UserId = UserService.GetUserId();
            List<Data.Transfer> list = authService.ReturnRequests();

            Console.WriteLine("____________________________________________");
            Console.WriteLine("Pending Transfers");
            Console.WriteLine("ID\tTo\t\t\tAmount");
            Console.WriteLine("____________________________________________");
            string currentUser = UserService.GetUserName();
            foreach (Data.Transfer l in list)
            {
                Console.WriteLine($"{l.TransferId}\t{l.ToName} \t\t${l.Amount}");
            }
            Console.WriteLine("____________________________________________");
            Console.WriteLine();
            Console.Write("Please enter transfer ID to approve/reject (0 to cancel): ");
            string input = Console.ReadLine();
            if (input == "0")
            {
                return MenuOptionResult.DoNotWaitAfterMenuSelection;
            }
            if (input.Length == 0)
            {
                Console.WriteLine("You must enter an ID or enter 0 to cancel!");
                Console.WriteLine("Hit 'Enter' to continue.");
                return MenuOptionResult.WaitAfterMenuSelection;
            }
            int id = Convert.ToInt32(input);
            bool found = false;
            foreach (Data.Transfer l in list)
            {
                if (l.TransferId == id)
                {
                    Console.WriteLine("____________________________________________");
                    Console.WriteLine("Transfer Details");
                    Console.WriteLine("____________________________________________");
                    Console.WriteLine();
                    Console.WriteLine($"Id: { l.TransferId}");
                    Console.WriteLine($"To: {l.ToName}");
                    Console.WriteLine($"Status: Pending");
                    Console.WriteLine($"Amount: ${l.Amount}");
                    transfer.Amount = l.Amount;
                    transfer.ToName = l.ToName;
                    transfer.TransferIdColum = l.TransferId;
                    transfer.TransferId = l.TransferId;
                    found = true;

                    // TransferId and TransferIdcolum names need to be changed
                    // TransferId is getting passed as 0 when going through controller????

                }
            }
            if (!found)
            {
                Console.WriteLine("The ID you entered was not valid!");
                Console.WriteLine("Hit 'Enter' to continue.");
                return MenuOptionResult.WaitAfterMenuSelection;
            }
            Console.WriteLine("____________________________________________");
            Console.WriteLine("1: Approve");
            Console.WriteLine("2: Reject");
            Console.WriteLine("0: Don't approve or reject");
            Console.WriteLine("------");
            Console.WriteLine("Please choose an option");
            string reader = Console.ReadLine();

            if (reader == "0")
            {
                Console.WriteLine("Transfer will remain pending.");
                return MenuOptionResult.WaitAfterMenuSelection;
            }
            if (reader == "1")
            {
                
                transfer.UserId = UserService.GetUserId();
                transfer.Status = true;
                authService.UpdateTransfer(transfer);
                Console.WriteLine("The transfer has been approved. The amount will be deducted from your account.");
                return MenuOptionResult.WaitAfterMenuSelection;
            }
            if (reader == "2")
            {
                transfer.UserId = UserService.GetUserId();
                transfer.Status = false;
                authService.UpdateTransfer(transfer);
                Console.WriteLine("The transfer has been rejected. The amount will not be deducted from your account.");
                return MenuOptionResult.WaitAfterMenuSelection;
            }
            if (reader != "0" || reader != "1" || reader != "2")
            {
                Console.WriteLine("You did not enter a given choice.");
                return MenuOptionResult.WaitAfterMenuSelection;
            }

            return MenuOptionResult.WaitAfterMenuSelection;
        }

        private MenuOptionResult SendTEBucks()
        {
            Data.Transfer transfer = new Data.Transfer();
            AuthService authService = new AuthService();
            List<API_User> newList = GetList();

            Console.Write("Enter ID of user you are sending to (0 to cancel): ");
            string input = Console.ReadLine();
            if (input.Length == 0)
            {
                Console.WriteLine("You must enter an ID or enter 0 to cancel!");
                Console.WriteLine("Hit 'Enter' to continue.");
                return MenuOptionResult.WaitAfterMenuSelection;
            }
            transfer.TransferId = Convert.ToInt32(input);

            if (transfer.TransferId == 0)
            {
                Console.WriteLine("The transaction was canceled!");
                Console.WriteLine("Hit 'Enter' to continue.");
                return MenuOptionResult.WaitAfterMenuSelection;
            }

            if (transfer.TransferId > newList.Count)
            {
                Console.WriteLine("This user does not exist!");
                Console.WriteLine("Hit 'Enter' to continue.");
                return MenuOptionResult.WaitAfterMenuSelection;
            }

            if (UserService.GetUserId() == transfer.TransferId)
            {
                Console.WriteLine("You can not send money to yourself!");
                Console.WriteLine("Hit 'Enter' to continue.");
                return MenuOptionResult.WaitAfterMenuSelection;
            }

            Console.Write("Enter amount: ");
            transfer.Amount = Convert.ToDecimal(Console.ReadLine());
            transfer.UserId = UserService.GetUserId();

            authService.Transfer(transfer);

            if (transfer.Amount > authService.GetBalance())
            {
                Console.WriteLine("You don't have enough money in your account!");
            }
            else
            {
                Console.WriteLine("Success! Transaction was completed!");
            }

            return MenuOptionResult.WaitAfterMenuSelection;
        }

        private MenuOptionResult RequestTEBucks()
        {
            Data.Transfer transfer = new Data.Transfer();
            AuthService authService = new AuthService();
            List<API_User> newList = GetList();

            Console.Write("Enter ID of user you are requesting from (0 to cancel): ");
            string input = Console.ReadLine();
            if (input.Length == 0)
            {
                Console.WriteLine("You must enter an ID or enter 0 to cancel!");
                Console.WriteLine("Hit 'Enter' to continue.");
                return MenuOptionResult.WaitAfterMenuSelection;
            }
            transfer.TransferId = Convert.ToInt32(input);

            if (transfer.TransferId == 0)
            {
                Console.WriteLine("The transaction was canceled!");
                Console.WriteLine("Hit 'Enter' to continue.");
                return MenuOptionResult.WaitAfterMenuSelection;
            }

            if (transfer.TransferId > newList.Count)
            {
                Console.WriteLine("This user does not exist!");
                Console.WriteLine("Hit 'Enter' to continue.");
                return MenuOptionResult.WaitAfterMenuSelection;
            }

            if (UserService.GetUserId() == transfer.TransferId)
            {
                Console.WriteLine("You can not request money from yourself!");
                Console.WriteLine("Hit 'Enter' to continue.");
                return MenuOptionResult.WaitAfterMenuSelection;
            }

            Console.Write("Enter amount: ");
            transfer.Amount = Convert.ToDecimal(Console.ReadLine());
            transfer.UserId = UserService.GetUserId();

            authService.RequestTransfer(transfer);

            Console.WriteLine("Success! Request was sent!");

            return MenuOptionResult.WaitAfterMenuSelection;


        }
        private List<API_User> GetList()
        {
            //Data.Transfer transfer = new Data.Transfer();
            AuthService authService = new AuthService();
            List<API_User> list = authService.ReturnUsers();

            Console.WriteLine("________________________________");
            Console.WriteLine("Users ID\t  Name");

            foreach (API_User l in list)
            {
                if (l.UserId != UserService.GetUserId())
                {
                    Console.WriteLine($"{l.UserId}\t      {l.Username}");
                }
            }
            Console.WriteLine("________________________________");
            Console.WriteLine();

            return list;
        }

        private MenuOptionResult Logout()
        {
            UserService.SetLogin(new API_User()); //wipe out previous login info
            return MenuOptionResult.CloseMenuAfterSelection;
        }

    }
}
