using System;
using System.Data;
using System.Threading.Tasks;
using LoanShark.Data;
using LoanShark.Repository;
using Microsoft.Data.SqlClient;
using Moq;
using Xunit;

namespace LoanShark.Tests.Repository
{
    public class MainPageRepositoryTests
    {
        private readonly Mock<IDataLink> mockDataLink;
        private readonly MainPageRepository repository;

        public MainPageRepositoryTests()
        {
            mockDataLink = new Mock<IDataLink>();
            repository = new MainPageRepository(mockDataLink.Object);
        }

        [Fact]
        public async Task GetUserBankAccounts_ReturnsCorrectDataTable()
        {
            // Arrange
            int userId = 1;
            var expectedTable = new DataTable();
            expectedTable.Columns.Add("iban", typeof(string));
            expectedTable.Columns.Add("currency", typeof(string));
            expectedTable.Columns.Add("amount", typeof(decimal));
            expectedTable.Columns.Add("blocked", typeof(bool));
            expectedTable.Columns.Add("id_user", typeof(int));
            expectedTable.Columns.Add("custom_name", typeof(string));
            expectedTable.Columns.Add("daily_limit", typeof(decimal));
            expectedTable.Columns.Add("max_per_transaction", typeof(decimal));
            expectedTable.Columns.Add("max_nr_transactions_daily", typeof(int));

            expectedTable.Rows.Add(
                "RO01SEUP0000000001",
                "RON",
                1000m,
                false,
                userId,
                "Test Account",
                2000m,
                500m,
                10
            );

            mockDataLink.Setup(dl => dl.ExecuteReader(
                "GetUserBankAccounts",
                It.Is<SqlParameter[]>(p => p.Length == 1 && (int)p[0].Value == userId)))
                .ReturnsAsync(expectedTable);

            // Act
            var result = await repository.GetUserBankAccounts(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Rows.Count);
            var row = result.Rows[0];
            Assert.Equal("RO01SEUP0000000001", row["iban"]);
            Assert.Equal("RON", row["currency"]);
            Assert.Equal(1000m, row["amount"]);
            Assert.False((bool)row["blocked"]);
            Assert.Equal(userId, row["id_user"]);
            Assert.Equal("Test Account", row["custom_name"]);
            Assert.Equal(2000m, row["daily_limit"]);
            Assert.Equal(500m, row["max_per_transaction"]);
            Assert.Equal(10, row["max_nr_transactions_daily"]);

            // Verify the mock was called
            mockDataLink.Verify(dl => dl.ExecuteReader(
                "GetUserBankAccounts",
                It.Is<SqlParameter[]>(p => p.Length == 1 && (int)p[0].Value == userId)),
                Times.Once);
        }

        [Fact]
        public async Task GetUserBankAccounts_WhenNoAccounts_ReturnsEmptyDataTable()
        {
            // Arrange
            int userId = 1;
            var emptyTable = new DataTable();
            emptyTable.Columns.Add("iban", typeof(string));

            mockDataLink.Setup(dl => dl.ExecuteReader(
                "GetUserBankAccounts",
                It.Is<SqlParameter[]>(p => p.Length == 1 && (int)p[0].Value == userId)))
                .ReturnsAsync(emptyTable);

            // Act
            var result = await repository.GetUserBankAccounts(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Rows);
        }

        [Fact]
        public async Task GetBankAccountBalanceByUserIban_ReturnsCorrectBalance()
        {
            // Arrange
            string iban = "RO01SEUP0000000001";
            var dataTable = new DataTable();
            dataTable.Columns.Add("amount", typeof(decimal));
            dataTable.Columns.Add("currency", typeof(string));
            dataTable.Rows.Add(1000m, "RON");

            mockDataLink.Setup(dl => dl.ExecuteReader(
                "GetBankAccountBalanceByIban",
                It.Is<SqlParameter[]>(p => p.Length == 1 && (string)p[0].Value == iban)))
                .ReturnsAsync(dataTable);

            // Act
            var result = await repository.GetBankAccountBalanceByUserIban(iban);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1000m, result.Item1);
            Assert.Equal("RON", result.Item2);

            // Verify the mock was called
            mockDataLink.Verify(dl => dl.ExecuteReader(
                "GetBankAccountBalanceByIban",
                It.Is<SqlParameter[]>(p => p.Length == 1 && (string)p[0].Value == iban)),
                Times.Once);
        }

        [Fact]
        public async Task GetBankAccountBalanceByUserIban_WhenNoData_ReturnsZeroAndEmptyString()
        {
            // Arrange
            string iban = "INVALID_IBAN";
            var emptyTable = new DataTable();
            emptyTable.Columns.Add("amount", typeof(decimal));
            emptyTable.Columns.Add("currency", typeof(string));
            emptyTable.Rows.Add(DBNull.Value, DBNull.Value);

            mockDataLink.Setup(dl => dl.ExecuteReader(
                "GetBankAccountBalanceByIban",
                It.Is<SqlParameter[]>(p => p.Length == 1 && (string)p[0].Value == iban)))
                .ReturnsAsync(emptyTable);

            // Act
            var result = await repository.GetBankAccountBalanceByUserIban(iban);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0m, result.Item1);
            Assert.Equal(string.Empty, result.Item2);

            // Verify the mock was called
            mockDataLink.Verify(dl => dl.ExecuteReader(
                "GetBankAccountBalanceByIban",
                It.Is<SqlParameter[]>(p => p.Length == 1 && (string)p[0].Value == iban)),
                Times.Once);
        }
    }
} 