using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public class AccountSqlDAO : IAccountDAO
    {
        private readonly string connectionString;
        public AccountSqlDAO(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public Account GetAccount(int userId)
        {
            Account returnAccount = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT account_id, user_id, balance FROM accounts WHERE user_id = @userId", conn);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows && reader.Read())
                    {
                        returnAccount = GetAccountFromReader(reader);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }
            return returnAccount;
        }

        public void Transfer(int userId, int transferId, decimal amount)
        {
            int int1 = userId;
            int int2 = transferId;
            decimal dec3 = amount;

            Account acc = GetAccount(userId);

            if (acc.Balance >= amount)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        SqlCommand cmd = new SqlCommand(@"

                        Begin Transaction 
                        INSERT into transfers(transfer_type_id, transfer_status_id, account_from, account_to, amount) Values (2, 2, @fromId, @toId, @amount)
                        UPDATE accounts set balance = balance - @amount where account_id = @fromId
                        UPDATE accounts set balance = balance + @amount where account_id = @toId
                        COMMIT Transaction

                        ", conn);
                        cmd.Parameters.AddWithValue("@fromId", userId);
                        cmd.Parameters.AddWithValue("@toId", transferId);
                        cmd.Parameters.AddWithValue("@amount", amount);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException)
                {
                    throw;
                }
            } else
            {
                Console.WriteLine("You don't have enough money!");
            }
        }

        public void RequestTransfer(int userId, int transferId, decimal amount)
        {
            int int1 = userId;
            int int2 = transferId;
            decimal dec3 = amount;

            Account acc = GetAccount(userId);

            if (acc.Balance >= amount)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        SqlCommand cmd = new SqlCommand(@"

                        INSERT into transfers(transfer_type_id, transfer_status_id, account_from, account_to, amount) Values (1, 1, @fromId, @toId, @amount)

                        ", conn);
                        cmd.Parameters.AddWithValue("@toId", userId);
                        cmd.Parameters.AddWithValue("@fromId", transferId);
                        cmd.Parameters.AddWithValue("@amount", amount);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException)
                {
                    throw;
                }
            }
            else
            {
                Console.WriteLine("You don't have enough money!");
            }
        }

        public List<Transfer> GetTransfers(int userId)
        {
            List<Transfer> returnTransfers = new List<Transfer>();
            
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(@"
                        select transfer_id, u.username AS 'from', ur.username AS 'to', amount
	                        from transfers t
	                        JOIN accounts a on a.account_id = t.account_from
	                        JOIN users u on u.user_id = t.account_from
	                        JOIN users ur on ur.user_id = t.account_to
                            Where (t.account_from = @userid OR t.account_to = @userid) AND transfer_status_id = 2
                        ", conn);
                    cmd.Parameters.AddWithValue("@userid", userId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Transfer t = GetTransferFromReader(reader);
                            returnTransfers.Add(t);
                        }
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return returnTransfers;
        }

        public Transfer GetTransferFromReader(SqlDataReader reader)
        {
            Transfer tr = new Transfer
            {
                TransferId = Convert.ToInt32(reader["transfer_id"]),
                FromName = Convert.ToString(reader["from"]),
                ToName = Convert.ToString(reader["to"]),
                Amount = Convert.ToDecimal(reader["amount"]),
            };

            return tr;
        }

        public List<Transfer> GetRequests(int userId)
        {
            List<Transfer> returnTransfers = new List<Transfer>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(@"
                        select transfer_id,transfer_status_id, u.username AS 'from', ur.username AS 'to', amount
	                        from transfers t
	                        JOIN accounts a on a.account_id = t.account_from
	                        JOIN users u on u.user_id = t.account_from
	                        JOIN users ur on ur.user_id = t.account_to
	                        Where t.account_from = @userid AND transfer_status_id = 1
                        ", conn);
                    cmd.Parameters.AddWithValue("@userid", userId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Transfer t = GetTransferFromReader(reader);
                            returnTransfers.Add(t);
                        }
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return returnTransfers;
        }

        public void UpdateTransfer(int fromId, decimal amount, int transId, bool approve)
        {
            //int int1 = toId;
            int int2 = fromId;
            decimal dec3 = amount;
            int int3 = transId;
            bool approved = approve;

            Account acc = GetAccount(fromId);

            if (acc.Balance >= amount)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        if (approve)
                        {
                        SqlCommand cmd = new SqlCommand(@"
                            BEGIN TRANSACTION
                            UPDATE transfers set transfer_status_id = 2 where transfer_id = @transID
                            UPDATE accounts set balance = balance - @amount where account_id = @fromId
                            UPDATE accounts set balance = balance + @amount where account_id = (Select account_to from transfers where transfer_id = @transID)
                            COMMIT TRANSACTION

                        ", conn);
                        cmd.Parameters.AddWithValue("@fromId", fromId);
                    //    cmd.Parameters.AddWithValue("@toId", toId);
                        cmd.Parameters.AddWithValue("@amount", amount);
                        cmd.Parameters.AddWithValue("@transID", transId);
                        cmd.ExecuteNonQuery();
                        }
                        if (!approve)
                        {
                            SqlCommand cmd = new SqlCommand(@"
                            UPDATE transfers set transfer_status_id = 3 where transfer_id = @transID
                        ", conn);
                            cmd.Parameters.AddWithValue("@transID", transId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (SqlException)
                {
                    throw;
                }
            }
            else
            {
                Console.WriteLine("You don't have enough money!");
            }
        }

        public Account GetAccountFromReader(SqlDataReader reader)
        {
            Account acc = new Account
            {
                AccountId = Convert.ToInt32(reader["account_id"]),
                UserId = Convert.ToInt32(reader["user_id"]),
                Balance = Convert.ToDecimal(reader["balance"])
            };

            return acc;
        }

    }
}
