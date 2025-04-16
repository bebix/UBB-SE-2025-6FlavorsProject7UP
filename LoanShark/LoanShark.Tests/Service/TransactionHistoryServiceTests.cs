using LoanShark.Domain;
using LoanShark.Repository;
using LoanShark.Service;
using Moq;
using System.Collections.ObjectModel;
using Xunit;

namespace LoanShark.Tests
{
    public class TransactionHistoryServiceTests
    {
        private string testIban = "RO01TEST1234567890";

        private ObservableCollection<Transaction> GetMockTransactions()
        {
            return new ObservableCollection<Transaction>
            {
                new Transaction
                {
                    SenderIban = testIban,
                    ReceiverIban = "OTHER",
                    TransactionType = "Transfer",
                    TransactionDatetime = new DateTime(2023, 5, 1)
                },
                new Transaction
                {
                    SenderIban = "OTHER",
                    ReceiverIban = "OTHER",
                    TransactionType = "Payment",
                    TransactionDatetime = new DateTime(2023, 6, 1)
                }
            };
        }

        [Fact]
        public async Task RetrieveForMenu_WhenIbanMatches_ReturnsFilteredList()
        {
            var mockRepo = new Mock<ITransactionHistoryRepository>();
            mockRepo.Setup(r => r.GetTransactionsNormal()).ReturnsAsync(GetMockTransactions());
            var service = new TransactionHistoryService(mockRepo.Object, testIban);

            var result = await service.RetrieveForMenu();

            Assert.Single(result);
        }

        [Fact]
        public async Task FilterByTypeForMenu_WhenTypeProvided_ReturnsMatchingType()
        {
            var mockRepo = new Mock<ITransactionHistoryRepository>();
            mockRepo.Setup(r => r.GetTransactionsNormal()).ReturnsAsync(GetMockTransactions());
            var service = new TransactionHistoryService(mockRepo.Object, testIban);

            var result = await service.FilterByTypeForMenu("Transfer");

            Assert.Single(result);
        }

        [Fact]
        public async Task FilterByTypeForMenu_WhenTypeEmpty_ReturnsAll()
        {
            var mockRepo = new Mock<ITransactionHistoryRepository>();
            mockRepo.Setup(r => r.GetTransactionsNormal()).ReturnsAsync(GetMockTransactions());
            var service = new TransactionHistoryService(mockRepo.Object, testIban);

            var result = await service.FilterByTypeForMenu("");

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task FilterByTypeDetailed_WhenTypeProvidedAndMatches_ReturnsDetailedMatching()
        {
            var mockRepo = new Mock<ITransactionHistoryRepository>();
            mockRepo.Setup(r => r.GetTransactionsNormal()).ReturnsAsync(GetMockTransactions());
            var service = new TransactionHistoryService(mockRepo.Object, testIban);

            var result = await service.FilterByTypeDetailed("Transfer");

            Assert.Single(result);
        }

        [Fact]
        public async Task FilterByTypeDetailed_WhenTypeEmpty_ReturnsAllDetailed()
        {
            var mockRepo = new Mock<ITransactionHistoryRepository>();
            mockRepo.Setup(r => r.GetTransactionsNormal()).ReturnsAsync(GetMockTransactions());
            var service = new TransactionHistoryService(mockRepo.Object, testIban);

            var result = await service.FilterByTypeDetailed("");

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task SortByDate_WhenAscending_ReturnsSortedAsc()
        {
            var transactions = new ObservableCollection<Transaction>
            {
                 new Transaction { SenderIban = testIban, TransactionDatetime = new DateTime(2023, 6, 1) },
                 new Transaction { SenderIban = testIban, TransactionDatetime = new DateTime(2023, 5, 1) }
            };

            var mockRepo = new Mock<ITransactionHistoryRepository>();
            mockRepo.Setup(r => r.GetTransactionsNormal()).ReturnsAsync(transactions);

            var service = new TransactionHistoryService(mockRepo.Object, testIban);

            var result = await service.SortByDate("Ascending");

            Assert.True(result[0].Contains("2023-05-01") || result[0] == transactions[1].TostringForMenu());
        }


        [Fact]
        public async Task SortByDate_WhenDescending_ReturnsSortedDesc()
        {
            var transactions = new ObservableCollection<Transaction>
    {
        new Transaction { SenderIban = testIban, TransactionDatetime = new DateTime(2023, 5, 1) },
        new Transaction { SenderIban = testIban, TransactionDatetime = new DateTime(2023, 6, 1) }
    };

            var mockRepo = new Mock<ITransactionHistoryRepository>();
            mockRepo.Setup(r => r.GetTransactionsNormal()).ReturnsAsync(transactions);

            var service = new TransactionHistoryService(mockRepo.Object, testIban);

            var result = await service.SortByDate("Descending");

            Assert.True(result[0].Contains("2023-06-01") || result[0] == transactions[1].TostringForMenu());
        }

        [Fact]
        public async Task SortByDate_WhenInvalidOrder_ReturnsNull()
        {
            var mockRepo = new Mock<ITransactionHistoryRepository>();
            mockRepo.Setup(r => r.GetTransactionsNormal()).ReturnsAsync(GetMockTransactions());
            var service = new TransactionHistoryService(mockRepo.Object, testIban);

            var result = await service.SortByDate("Invalid");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetTransactionByMenuString_WhenMenuStringMatches_ReturnsTransaction()
        {
            var mockRepo = new Mock<ITransactionHistoryRepository>();
            var transactions = GetMockTransactions();
            var target = transactions[0];
            mockRepo.Setup(r => r.GetTransactionsNormal()).ReturnsAsync(transactions);
            var service = new TransactionHistoryService(mockRepo.Object, testIban);

            var result = await service.GetTransactionByMenuString(target.TostringForMenu());

            Assert.Equal(target, result);
        }

        [Fact]
        public async Task GetTransactionTypeCounts_WhenCalled_ReturnsGroupedCounts()
        {
            var mockRepo = new Mock<ITransactionHistoryRepository>();
            mockRepo.Setup(r => r.GetTransactionsNormal()).ReturnsAsync(GetMockTransactions());
            var service = new TransactionHistoryService(mockRepo.Object, testIban);

            var result = await service.GetTransactionTypeCounts();

            Assert.Single(result);
            Assert.True(result.ContainsKey("Transfer"));
        }
    }
}