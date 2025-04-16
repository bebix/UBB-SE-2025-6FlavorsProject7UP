using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanShark.Repository;
using LoanShark.Service;
using Moq;
using Xunit;

namespace LoanShark.Tests.Service
{
    public class MainPageServiceTests
    {
        private readonly Mock<IMainPageRepository> mockRepo;
        private readonly MainPageService mainPageService;

        public MainPageServiceTests()
        {
            mockRepo = new Mock<IMainPageRepository>();
            mainPageService = new MainPageService(mockRepo.Object);
        }

        [Fact]
        public async Task GetUserBankAccounts_ReturnsCorrectBankAccounts()
        {
            // Arrange
            int userId = 1;
            var dataTable = new DataTable();
            dataTable.Columns.Add("iban", typeof(string));
            dataTable.Columns.Add("currency", typeof(string));
            dataTable.Columns.Add("amount", typeof(decimal));
            dataTable.Columns.Add("blocked", typeof(bool));
            dataTable.Columns.Add("id_user", typeof(int));
            dataTable.Columns.Add("custom_name", typeof(string));
            dataTable.Columns.Add("daily_limit", typeof(decimal));
            dataTable.Columns.Add("max_per_transaction", typeof(decimal));
            dataTable.Columns.Add("max_nr_transactions_daily", typeof(int));

            dataTable.Rows.Add(
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

            mockRepo.Setup(r => r.GetUserBankAccounts(userId))
                   .ReturnsAsync(dataTable);

            // Act
            var result = await mainPageService.GetUserBankAccounts(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            var bankAccount = result[0];
            Assert.Equal("RO01SEUP0000000001", bankAccount.Iban);
            Assert.Equal("RON", bankAccount.Currency);
            Assert.Equal(1000m, bankAccount.Balance);
            Assert.False(bankAccount.Blocked);
            Assert.Equal(userId, bankAccount.UserID);
            Assert.Equal("Test Account", bankAccount.Name);
            Assert.Equal(2000m, bankAccount.DailyLimit);
            Assert.Equal(500m, bankAccount.MaximumPerTransaction);
            Assert.Equal(10, bankAccount.MaximumNrTransactions);

            // Verify the mock was called
            mockRepo.Verify(r => r.GetUserBankAccounts(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserBankAccounts_WhenNoAccounts_ReturnsEmptyCollection()
        {
            // Arrange
            int userId = 1;
            var emptyDataTable = new DataTable();
            emptyDataTable.Columns.Add("iban", typeof(string));
            emptyDataTable.Columns.Add("currency", typeof(string));
            emptyDataTable.Columns.Add("amount", typeof(decimal));
            emptyDataTable.Columns.Add("blocked", typeof(bool));
            emptyDataTable.Columns.Add("id_user", typeof(int));
            emptyDataTable.Columns.Add("custom_name", typeof(string));
            emptyDataTable.Columns.Add("daily_limit", typeof(decimal));
            emptyDataTable.Columns.Add("max_per_transaction", typeof(decimal));
            emptyDataTable.Columns.Add("max_nr_transactions_daily", typeof(int));

            mockRepo.Setup(r => r.GetUserBankAccounts(userId))
                   .ReturnsAsync(emptyDataTable);

            // Act
            var result = await mainPageService.GetUserBankAccounts(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            // Verify the mock was called
            mockRepo.Verify(r => r.GetUserBankAccounts(userId), Times.Once);
        }

        [Fact]
        public async Task GetBankAccountBalanceByUserIban_ReturnsCorrectBalance()
        {
            // Arrange
            string iban = "RO01SEUP0000000001";
            var expectedBalance = new Tuple<decimal, string>(1000m, "RON");

            mockRepo.Setup(r => r.GetBankAccountBalanceByUserIban(iban))
                   .ReturnsAsync(expectedBalance);

            // Act
            var result = await mainPageService.GetBankAccountBalanceByUserIban(iban);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedBalance.Item1, result.Item1);
            Assert.Equal(expectedBalance.Item2, result.Item2);

            // Verify the mock was called
            mockRepo.Verify(r => r.GetBankAccountBalanceByUserIban(iban), Times.Once);
        }

        [Fact]
        public async Task GetBankAccountBalanceByUserIban_WhenAccountNotFound_ThrowsException()
        {
            // Arrange
            string iban = "INVALID_IBAN";
            var expectedException = new Exception("Account not found");

            mockRepo.Setup(r => r.GetBankAccountBalanceByUserIban(iban))
                   .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                mainPageService.GetBankAccountBalanceByUserIban(iban));

            Assert.Equal(expectedException.Message, exception.Message);

            // Verify the mock was called
            mockRepo.Verify(r => r.GetBankAccountBalanceByUserIban(iban), Times.Once);
        }
    }
}
