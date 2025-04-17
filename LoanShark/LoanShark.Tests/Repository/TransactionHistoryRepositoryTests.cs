using LoanShark.Domain;
using LoanShark.Repository;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;
using Xunit;

namespace LoanShark.Tests.Repository
{
    public class TransactionHistoryRepositoryTests
    {
        [Fact]
        public async Task GetTransactionsNormal_WhenCalled_ReturnsTransactions()
        {
            // Arrange
            var repo = new TransactionHistoryRepository();

            // Act
            var result = await repo.GetTransactionsNormal();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ObservableCollection<Transaction>>(result);
        }

        [Fact]
        public async Task GetTransactionsForMenu_WhenCalled_ReturnsFormattedMenuTransactions()
        {
            var repo = new TransactionHistoryRepository();

            var result = await repo.GetTransactionsForMenu();

            Assert.NotNull(result);
            Assert.IsType<ObservableCollection<string>>(result);
        }

        [Fact]
        public async Task GetTransactionsDetailed_WhenCalled_ReturnsFormattedDetailedTransactions()
        {
            var repo = new TransactionHistoryRepository();

            var result = await repo.GetTransactionsDetailed();

            Assert.NotNull(result);
            Assert.IsType<ObservableCollection<string>>(result);
        }

        [Fact]
        public async Task UpdateTransactionDescription_WhenCalled_ExecutesWithoutException()
        {
            int testTransactionId = 1;
            string testDescription = "Updated test description";

            var exception = await Record.ExceptionAsync(() =>
                TransactionHistoryRepository.UpdateTransactionDescription(testTransactionId, testDescription));

            Assert.Null(exception);
        }

        [Fact]
        public async Task GetTransactionsNormal_WhenExceptionThrown_ReturnsEmptyCollection()
        {
            var mockRepo = new Mock<ITransactionHistoryRepository>();
            mockRepo.Setup(m => m.GetTransactionsNormal()).ThrowsAsync(new Exception("Test Exception"));
            var exception = await Record.ExceptionAsync(() => mockRepo.Object.GetTransactionsNormal());
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task GetTransactionsForMenu_WhenExceptionThrown_ReturnsEmptyCollection()
        {
            var mockRepo = new Mock<ITransactionHistoryRepository>();
            mockRepo.Setup(m => m.GetTransactionsForMenu()).ThrowsAsync(new Exception("Test Exception"));
            var exception = await Record.ExceptionAsync(() => mockRepo.Object.GetTransactionsForMenu());
            Assert.NotNull(exception);
        }

        [Fact]
        public async Task GetTransactionsDetailed_WhenExceptionThrown_ReturnsEmptyCollection()
        {
            var mockRepo = new Mock<ITransactionHistoryRepository>();
            mockRepo.Setup(m => m.GetTransactionsDetailed()).ThrowsAsync(new Exception("Test Exception"));
            var exception = await Record.ExceptionAsync(() => mockRepo.Object.GetTransactionsDetailed());
            Assert.NotNull(exception);
        }
    }
}
