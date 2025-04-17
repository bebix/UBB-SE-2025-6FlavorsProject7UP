using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanShark.Data;
using LoanShark.Repository;
using Microsoft.Data.SqlClient;
using Moq;
using Xunit;

namespace LoanShark.Tests.Repository
{
    public class LoginRepositoryTests
    {
        private readonly Mock<IDataLink> mockDataLink;
        private readonly ILoginRepository repository;

        public LoginRepositoryTests()
        {
            mockDataLink = new Mock<IDataLink>();
            repository = new LoginRepository(mockDataLink.Object);
        }

        [Fact]
        public async Task GetUserCredentials_ReturnsCorrectCredentials()
        {
            // Arrange
            string email = "test@example.com";
            var expectedTable = new DataTable();
            expectedTable.Columns.Add("hashed_password", typeof(string));
            expectedTable.Columns.Add("password_salt", typeof(string));
            expectedTable.Rows.Add("hashedpass123", "salt123");

            mockDataLink.Setup(dl => dl.ExecuteReader(
                "GetUserCredentials",
                It.Is<SqlParameter[]>(p => p.Length == 1 && (string)p[0].Value == email)))
                .ReturnsAsync(expectedTable);

            // Act
            var result = await repository.GetUserCredentials(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Rows.Count);
            var row = result.Rows[0];
            Assert.Equal("hashedpass123", row["hashed_password"]);
            Assert.Equal("salt123", row["password_salt"]);

            // Verify mock was called
            mockDataLink.Verify(dl => dl.ExecuteReader(
                "GetUserCredentials",
                It.Is<SqlParameter[]>(p => p.Length == 1 && (string)p[0].Value == email)),
                Times.Once);
        }

        [Fact]
        public async Task GetUserCredentials_WhenUserNotFound_ThrowsException()
        {
            // Arrange
            string email = "nonexistent@example.com";
            var emptyTable = new DataTable();
            emptyTable.Columns.Add("hashed_password", typeof(string));
            emptyTable.Columns.Add("password_salt", typeof(string));

            mockDataLink.Setup(dl => dl.ExecuteReader(
                "GetUserCredentials",
                It.Is<SqlParameter[]>(p => p.Length == 1 && (string)p[0].Value == email)))
                .ReturnsAsync(emptyTable);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                repository.GetUserCredentials(email));
            Assert.Contains("does NOT exist", exception.Message);

            // Verify mock was called
            mockDataLink.Verify(dl => dl.ExecuteReader(
                "GetUserCredentials",
                It.Is<SqlParameter[]>(p => p.Length == 1 && (string)p[0].Value == email)),
                Times.Once);
        }

        [Fact]
        public async Task GetUserInfoAfterLogin_ReturnsCorrectUserInfo()
        {
            // Arrange
            string email = "test@example.com";
            var expectedTable = new DataTable();
            expectedTable.Columns.Add("id_user", typeof(int));
            expectedTable.Columns.Add("cnp", typeof(string));
            expectedTable.Columns.Add("first_name", typeof(string));
            expectedTable.Columns.Add("last_name", typeof(string));
            expectedTable.Columns.Add("email", typeof(string));
            expectedTable.Columns.Add("phone_number", typeof(string));
            expectedTable.Rows.Add(1, "1234567890123", "John", "Doe", email, "0712345678");

            mockDataLink.Setup(dl => dl.ExecuteReader(
                "GetUserInfoAfterLogin",
                It.Is<SqlParameter[]>(p => p.Length == 1 && (string)p[0].Value == email)))
                .ReturnsAsync(expectedTable);

            // Act
            var result = await repository.GetUserInfoAfterLogin(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Rows.Count);
            var row = result.Rows[0];
            Assert.Equal(1, row["id_user"]);
            Assert.Equal("1234567890123", row["cnp"]);
            Assert.Equal("John", row["first_name"]);
            Assert.Equal("Doe", row["last_name"]);
            Assert.Equal(email, row["email"]);
            Assert.Equal("0712345678", row["phone_number"]);

            // Verify mock was called
            mockDataLink.Verify(dl => dl.ExecuteReader(
                "GetUserInfoAfterLogin",
                It.Is<SqlParameter[]>(p => p.Length == 1 && (string)p[0].Value == email)),
                Times.Once);
        }

        [Fact]
        public async Task GetUserInfoAfterLogin_WhenUserNotFound_ThrowsException()
        {
            // Arrange
            string email = "nonexistent@example.com";
            var emptyTable = new DataTable();
            emptyTable.Columns.Add("id_user", typeof(int));
            emptyTable.Columns.Add("cnp", typeof(string));
            emptyTable.Columns.Add("first_name", typeof(string));
            emptyTable.Columns.Add("last_name", typeof(string));
            emptyTable.Columns.Add("email", typeof(string));
            emptyTable.Columns.Add("phone_number", typeof(string));

            mockDataLink.Setup(dl => dl.ExecuteReader(
                "GetUserInfoAfterLogin",
                It.Is<SqlParameter[]>(p => p.Length == 1 && (string)p[0].Value == email)))
                .ReturnsAsync(emptyTable);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                repository.GetUserInfoAfterLogin(email));
            Assert.Contains("does NOT exist", exception.Message);

            // Verify mock was called
            mockDataLink.Verify(dl => dl.ExecuteReader(
                "GetUserInfoAfterLogin",
                It.Is<SqlParameter[]>(p => p.Length == 1 && (string)p[0].Value == email)),
                Times.Once);
        }

        [Fact]
        public async Task GetUserBankAccounts_ReturnsCorrectAccounts()
        {
            // Arrange
            int userId = 1;
            var expectedTable = new DataTable();
            expectedTable.Columns.Add("iban", typeof(string));
            expectedTable.Rows.Add("RO01SEUP0000000001");

            mockDataLink.Setup(dl => dl.ExecuteReader(
                "GetUserBankAccounts",
                It.Is<SqlParameter[]>(p => p.Length == 1 && (int)p[0].Value == userId)))
                .ReturnsAsync(expectedTable);

            // Act
            var result = await repository.GetUserBankAccounts(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Rows.Count);
            Assert.Equal("RO01SEUP0000000001", result.Rows[0]["iban"]);

            // Verify mock was called
            mockDataLink.Verify(dl => dl.ExecuteReader(
                "GetUserBankAccounts",
                It.Is<SqlParameter[]>(p => p.Length == 1 && (int)p[0].Value == userId)),
                Times.Once);
        }

        [Fact]
        public void LoginRepository_ParameterlessConstructor_ShouldCreateInstance()
        {
            // Act
            var repo = new LoginRepository();

            // Assert
            Assert.NotNull(repo);
        }
    }
}
