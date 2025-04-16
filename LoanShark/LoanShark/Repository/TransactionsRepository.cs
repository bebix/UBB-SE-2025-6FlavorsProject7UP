using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using LoanShark.Domain;
using LoanShark.Data;
using Microsoft.Data.SqlClient;

namespace LoanShark.Repository
{
    public class TransactionsRepository : ITransactionsRepository
    {
        private readonly IDataLink dataLink;
        public TransactionsRepository(IDataLink dataLink)
        {
            this.dataLink = dataLink;
        }

        public TransactionsRepository()
        {
        }

        public async Task<bool> AddTransaction(Transaction transaction)
        {
            try
            {
                if (transaction == null)
                {
                    throw new ArgumentNullException(nameof(transaction), "Transaction cannot be null");
                }

                SqlParameter[] parameters =
                {
                    new SqlParameter("@SenderIBAN", transaction.SenderIban),
                    new SqlParameter("@ReceiverIBAN", transaction.ReceiverIban),
                    new SqlParameter("@SenderCurrency", transaction.SenderCurrency),
                    new SqlParameter("@ReceiverCurrency", transaction.ReceiverCurrency),
                    new SqlParameter("@SenderAmount", transaction.SenderAmount),
                    new SqlParameter("@ReceiverAmount", transaction.ReceiverAmount),
                    new SqlParameter("@TransactionType", transaction.TransactionType),
                    new SqlParameter("@description", string.IsNullOrWhiteSpace(transaction.TransactionDescription) ? DBNull.Value : transaction.TransactionDescription)
                };

                await dataLink.ExecuteNonQuery("AddTransaction", parameters);
                return true;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error in AddTransaction: {ex.Message}", ex);
            }
        }

        public async Task<List<BankAccount>> GetAllBankAccounts()
        {
            try
            {
                List<BankAccount> bankAccounts = new List<BankAccount>();
                DataTable result = await dataLink.ExecuteReader("GetAllBankAccounts");

                foreach (DataRow row in result.Rows)
                {
                    bankAccounts.Add(new BankAccount(
                        row["iban"].ToString() ?? string.Empty,
                        row["currency"].ToString() ?? string.Empty,
                        Convert.ToDecimal(row["amount"]),
                        Convert.ToBoolean(row["blocked"]),
                        Convert.ToInt32(row["id_user"]),
                        row["custom_name"]?.ToString() ?? string.Empty,
                        Convert.ToDecimal(row["daily_limit"]),
                        Convert.ToDecimal(row["max_per_transaction"]),
                        Convert.ToInt32(row["max_nr_transactions_daily"])));
                }

                return bankAccounts;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error in GetAllBankAccounts: {ex.Message}", ex);
            }
        }

        public async Task<List<CurrencyExchange>> GetAllCurrencyExchangeRates()
        {
            try
            {
                List<CurrencyExchange> exchangeRates = new List<CurrencyExchange>();
                DataTable result = await dataLink.ExecuteReader("GetAllCurrencyExchangeRates");

                foreach (DataRow row in result.Rows)
                {
                    exchangeRates.Add(new CurrencyExchange(
                        row["from_currency"].ToString() ?? string.Empty,
                        row["to_currency"].ToString() ?? string.Empty,
                        Convert.ToDecimal(row["rate"])));
                }

                return exchangeRates;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error in GetAllCurrencyExchangeRates: {ex.Message}", ex);
            }
        }

        public async Task<BankAccount?> GetBankAccountByIBAN(string iban)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(iban))
                {
                    throw new ArgumentException("IBAN cannot be empty.", nameof(iban));
                }
                SqlParameter[] parameters = { new SqlParameter("@IBAN", iban) };
                DataTable result = await dataLink.ExecuteReader("GetBankAccountByIBAN", parameters);

                if (result.Rows.Count == 0)
                {
                    return null;
                }

                DataRow row = result.Rows[0];

                return new BankAccount(
                    row["iban"].ToString() ?? string.Empty,
                    row["currency"].ToString() ?? string.Empty,
                    Convert.ToDecimal(row["amount"]),
                    Convert.ToBoolean(row["blocked"]),
                    Convert.ToInt32(row["id_user"]),
                    row["custom_name"]?.ToString() ?? string.Empty,
                    Convert.ToDecimal(row["daily_limit"]),
                    Convert.ToDecimal(row["max_per_transaction"]),
                    Convert.ToInt32(row["max_nr_transactions_daily"]));
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error in GetBankAccountByIBAN: {ex.Message}", ex);
            }
        }

        public async Task<List<Transaction>> GetBankAccountTransactions(string iban)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(iban))
                {
                    throw new ArgumentException("IBAN cannot be empty.", nameof(iban));
                }

                SqlParameter[] parameters = { new SqlParameter("@IBAN", iban) };
                DataTable result = await dataLink.ExecuteReader("GetBankAccountTransactions", parameters);

                List<Transaction> transactions = new List<Transaction>();

                foreach (DataRow row in result.Rows)
                {
                    transactions.Add(new Transaction(
                        Convert.ToInt32(row["transaction_id"]),
                        row["sender_iban"].ToString() ?? string.Empty,
                        row["receiver_iban"].ToString() ?? string.Empty,
                        Convert.ToDateTime(row["transaction_datetime"]),
                        row["sender_currency"].ToString() ?? string.Empty,
                        row["receiver_currency"].ToString() ?? string.Empty,
                        Convert.ToDecimal(row["sender_amount"]),
                        Convert.ToDecimal(row["receiver_amount"]),
                        row["transaction_type"].ToString() ?? string.Empty,
                        row["transaction_description"].ToString() ?? string.Empty));
                }

                return transactions;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error in GetBankAccountTransactions: {ex.Message}", ex);
            }
        }

        public async Task<decimal> GetExchangeRate(string fromCurrency, string toCurrency)
        {
            try
            {
                SqlParameter[] parameters =
                {
                    new SqlParameter("@FromCurrency", fromCurrency),
                    new SqlParameter("@ToCurrency", toCurrency)
                };

                object result = await dataLink.ExecuteScalar<decimal>("GetExchangeRate", parameters);
                return result != null ? Convert.ToDecimal(result) : -1;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error in GetExchangeRate: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateBankAccountBalance(string iban, decimal newBalance)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(iban))
                {
                    throw new ArgumentException("IBAN must be provided.", nameof(iban));
                }

                if (newBalance < 0)
                {
                    throw new ArgumentException("Balance cannot be negative.");
                }

                SqlParameter[] parameters =
                {
                    new SqlParameter("@iban", iban),
                    new SqlParameter("@amount", newBalance)
                };

                await dataLink.ExecuteNonQuery("UpdateBankAccountBalance", parameters);
                return true;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Database error in UpdateBankAccountBalance: {ex.Message}", ex);
            }
        }
    }
    public interface ITransactionsRepository
    {
        Task<bool> AddTransaction(Transaction transaction);

        Task<List<BankAccount>> GetAllBankAccounts();

        Task<List<CurrencyExchange>> GetAllCurrencyExchangeRates();

        Task<BankAccount?> GetBankAccountByIBAN(string iban);

        Task<List<Transaction>> GetBankAccountTransactions(string iban);
        Task<decimal> GetExchangeRate(string fromCurrency, string toCurrency);

        Task<bool> UpdateBankAccountBalance(string iban, decimal newBalance);
    }
}
