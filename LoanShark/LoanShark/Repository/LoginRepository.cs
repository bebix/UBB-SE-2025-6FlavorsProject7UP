using System;
using System.Data;
using System.Threading.Tasks;
using LoanShark.Data;
using Microsoft.Data.SqlClient;

namespace LoanShark.Repository
{
    public interface ILoginRepository
    {
        /// <summary>
        /// Retrieves user credentials for authentication
        /// </summary>
        /// <param name="email">The email of the user</param>
        /// <returns>A DataTable containing the user's hashed password and salt</returns>
        Task<DataTable> GetUserCredentials(string email);

        /// <summary>
        /// Retrieves user information after successful login
        /// </summary>
        /// <param name="email">The email of the user</param>
        /// <returns>A DataTable containing the user's information</returns>
        Task<DataTable> GetUserInfoAfterLogin(string email);

        /// <summary>
        /// Retrieves bank accounts for a specific user
        /// </summary>
        /// <param name="id_user">The ID of the user</param>
        /// <returns>A DataTable containing the user's bank accounts</returns>
        Task<DataTable> GetUserBankAccounts(int id_user);
    }

    public class LoginRepository : ILoginRepository
    {
        private readonly IDataLink dataLink;

        public LoginRepository(IDataLink dataLink)
        {
            this.dataLink = dataLink;
        }

        public LoginRepository() : this(DataLink.Instance)
        {
        }

        // Returns a DataTable with the user credentials, they will be accessible from dt.Rows[0]["hashed_password"] and dt.Rows[0]["passowrd_salt"]
        // If the user with the given email is not found, an exception will be thrown
        public async Task<DataTable> GetUserCredentials(string email)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@email", email)
                };
                var dataTable = await dataLink.ExecuteReader("GetUserCredentials", sqlParameters);

                if (dataTable.Rows.Count == 0)
                {
                    throw new Exception($"User with the email {email} does NOT exist.");
                }

                return dataTable;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting user credentials: {ex.Message}", ex);
            }
        }

        public async Task<DataTable> GetUserInfoAfterLogin(string email)
        {
            try
            {
                SqlParameter[] sqlParameters =
                {
                    new SqlParameter("@email", email)
                };
                var dataTable = await dataLink.ExecuteReader("GetUserInfoAfterLogin", sqlParameters);

                if (dataTable.Rows.Count == 0)
                {
                    throw new Exception($"User with the email {email} does NOT exist.");
                }

                return dataTable;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting user info: {ex.Message}", ex);
            }
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
            catch (Exception ex)
            {
                throw new Exception($"Error getting user bank accounts: {ex.Message}", ex);
            }
        }
    }
}
