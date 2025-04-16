using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using LoanShark.Data;

namespace LoanShark.Repository
{
    public interface IMainPageRepository
    {
        /// <summary>
        /// Retrieves all bank accounts for a specific user
        /// </summary>
        /// <param name="id_user">The ID of the user</param>
        /// <returns>A DataTable containing the user's bank accounts</returns>
        Task<DataTable> GetUserBankAccounts(int id_user);

        /// <summary>
        /// Retrieves the balance and currency for a specific bank account
        /// </summary>
        /// <param name="iban">The IBAN of the bank account</param>
        /// <returns>A tuple containing the balance (decimal) and currency (string)</returns>
        Task<Tuple<decimal, string>> GetBankAccountBalanceByUserIban(string iban);
    }

    public class MainPageRepository : IMainPageRepository
    {
        private readonly IDataLink dataLink;

        public MainPageRepository(IDataLink dataLink)
        {
            this.dataLink = dataLink;
        }

        public MainPageRepository() : this(DataLink.Instance)
        {
        }

        public async Task<DataTable> GetUserBankAccounts(int id_user)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@id_user", id_user)
                };
                return await dataLink.ExecuteReader("GetUserBankAccounts", sqlParameters);
            }
            catch (Exception)
            {
                return new DataTable();
            }
        }

        public async Task<Tuple<decimal, string>> GetBankAccountBalanceByUserIban(string iban)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@iban", iban)
                };
                DataTable dataTable = await dataLink.ExecuteReader("GetBankAccountBalanceByIban", sqlParameters);

                if (dataTable == null || dataTable.Rows.Count == 0)
                {
                    return new Tuple<decimal, string>(0m, string.Empty);
                }

                var row = dataTable.Rows[0];
                decimal amount = row["amount"] != DBNull.Value ? Convert.ToDecimal(row["amount"]) : 0m;
                string currency = row["currency"] != DBNull.Value ? row["currency"].ToString() ?? string.Empty : string.Empty;

                return new Tuple<decimal, string>(amount, currency);
            }
            catch (Exception)
            {
                return new Tuple<decimal, string>(0m, string.Empty);
            }
        }
    }
}
