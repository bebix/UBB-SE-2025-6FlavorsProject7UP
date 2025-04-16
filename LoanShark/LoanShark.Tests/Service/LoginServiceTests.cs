using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanShark.Domain;
using LoanShark.Service;
using Moq;
using Xunit;
using LoanShark.Repository;

namespace LoanShark.Tests.Service
{
    public class LoginServiceTests
    {
        private readonly Mock<ILoginRepository> mockRepo;
        private readonly LoginService loginService;

        public LoginServiceTests()
        {
            mockRepo = new Mock<ILoginRepository>();
            loginService = new LoginService(mockRepo.Object);
        }

        [Fact]
        public async Task ValidateUserCredentials_WithValidCredentials_ReturnsTrue()
        {
            // Arrange
            string email = "test@example.com";
            string password = "Password123!";
            var hashedPassword = new HashedPassword(password);

            var credentialsTable = new DataTable();
            credentialsTable.Columns.Add("hashed_password", typeof(string));
            credentialsTable.Columns.Add("password_salt", typeof(string));
            credentialsTable.Rows.Add(hashedPassword.GetHashedPassword(), hashedPassword.GetSalt());

            mockRepo.Setup(r => r.GetUserCredentials(email))
                   .ReturnsAsync(credentialsTable);

            // Act
            var result = await loginService.ValidateUserCredentials(email, password);

            // Assert
            Assert.True(result);
            mockRepo.Verify(r => r.GetUserCredentials(email), Times.Once);
        }

        [Fact]
        public async Task ValidateUserCredentials_WithInvalidPassword_ReturnsFalse()
        {
            // Arrange
            string email = "test@example.com";
            string correctPassword = "Password123!";
            string wrongPassword = "WrongPassword123!";
            var hashedPassword = new HashedPassword(correctPassword);

            var credentialsTable = new DataTable();
            credentialsTable.Columns.Add("hashed_password", typeof(string));
            credentialsTable.Columns.Add("password_salt", typeof(string));
            credentialsTable.Rows.Add(hashedPassword.GetHashedPassword(), hashedPassword.GetSalt());

            mockRepo.Setup(r => r.GetUserCredentials(email))
                   .ReturnsAsync(credentialsTable);

            // Act
            var result = await loginService.ValidateUserCredentials(email, wrongPassword);

            // Assert
            Assert.False(result);
            mockRepo.Verify(r => r.GetUserCredentials(email), Times.Once);
        }

        [Fact]
        public async Task ValidateUserCredentials_WithNonexistentUser_ReturnsFalse()
        {
            // Arrange
            string email = "nonexistent@example.com";
            string password = "Password123!";

            mockRepo.Setup(r => r.GetUserCredentials(email))
                   .ThrowsAsync(new Exception("User not found"));

            // Act
            var result = await loginService.ValidateUserCredentials(email, password);

            // Assert
            Assert.False(result);
            mockRepo.Verify(r => r.GetUserCredentials(email), Times.Once);
        }

        [Fact]
        public async Task InstantiateUserSessionAfterLogin_SetsCorrectUserData()
        {
            // Arrange
            string email = "test@example.com";
            int userId = 1;

            var userInfoTable = new DataTable();
            userInfoTable.Columns.Add("id_user", typeof(int));
            userInfoTable.Columns.Add("cnp", typeof(string));
            userInfoTable.Columns.Add("first_name", typeof(string));
            userInfoTable.Columns.Add("last_name", typeof(string));
            userInfoTable.Columns.Add("email", typeof(string));
            userInfoTable.Columns.Add("phone_number", typeof(string));
            userInfoTable.Rows.Add(userId, "1234567890123", "John", "Doe", email, "0712345678");

            var bankAccountsTable = new DataTable();
            bankAccountsTable.Columns.Add("iban", typeof(string));
            bankAccountsTable.Rows.Add("RO01SEUP0000000001");

            mockRepo.Setup(r => r.GetUserInfoAfterLogin(email))
                   .ReturnsAsync(userInfoTable);
            mockRepo.Setup(r => r.GetUserBankAccounts(userId))
                   .ReturnsAsync(bankAccountsTable);

            // Act
            await loginService.InstantiateUserSessionAfterLogin(email);

            // Assert
            Assert.Equal("1", UserSession.Instance.GetUserData("id_user"));
            Assert.Equal("1234567890123", UserSession.Instance.GetUserData("cnp"));
            Assert.Equal("John", UserSession.Instance.GetUserData("first_name"));
            Assert.Equal("Doe", UserSession.Instance.GetUserData("last_name"));
            Assert.Equal(email, UserSession.Instance.GetUserData("email"));
            Assert.Equal("0712345678", UserSession.Instance.GetUserData("phone_number"));
            Assert.Equal("RO01SEUP0000000001", UserSession.Instance.GetUserData("current_bank_account_iban"));

            mockRepo.Verify(r => r.GetUserInfoAfterLogin(email), Times.Once);
            mockRepo.Verify(r => r.GetUserBankAccounts(userId), Times.Once);
        }

        [Fact]
        public async Task InstantiateUserSessionAfterLogin_WithNoAccounts_SetsEmptyIban()
        {
            // Arrange
            string email = "test@example.com";
            int userId = 1;

            var userInfoTable = new DataTable();
            userInfoTable.Columns.Add("id_user", typeof(int));
            userInfoTable.Columns.Add("cnp", typeof(string));
            userInfoTable.Columns.Add("first_name", typeof(string));
            userInfoTable.Columns.Add("last_name", typeof(string));
            userInfoTable.Columns.Add("email", typeof(string));
            userInfoTable.Columns.Add("phone_number", typeof(string));
            userInfoTable.Rows.Add(userId, "1234567890123", "John", "Doe", email, "0712345678");

            var emptyBankAccountsTable = new DataTable();
            emptyBankAccountsTable.Columns.Add("iban", typeof(string));

            mockRepo.Setup(r => r.GetUserInfoAfterLogin(email))
                   .ReturnsAsync(userInfoTable);
            mockRepo.Setup(r => r.GetUserBankAccounts(userId))
                   .ReturnsAsync(emptyBankAccountsTable);

            // Act
            await loginService.InstantiateUserSessionAfterLogin(email);

            // Assert
            Assert.Equal(string.Empty, UserSession.Instance.GetUserData("current_bank_account_iban"));
        }
    }
}
