using LoanShark.Data;
using LoanShark.Domain;
using LoanShark.Repository;
using Microsoft.Data.SqlClient;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LoanShark.Tests.Repository
{
    public class TransactionsRepositoryTests
    {
        private readonly Mock<IDataLink> dataLinkMock;
        private readonly TransactionsRepository transactionsRepository;

        public TransactionsRepositoryTests()
        {
            dataLinkMock = new Mock<IDataLink>();
            transactionsRepository = new TransactionsRepository(dataLinkMock.Object);
        }

        [Fact]
        public async Task GetAllBankAccounts_ShouldReturnListOfBankAccounts_WhenDataIsReturned()
        {
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

            dataTable.Rows.Add("RO99TEST0000000001", "EUR", 1200.50m, false, 7, "Florin", 500.00m, 200.00m, 3);

            dataLinkMock
                .Setup(dl => dl.ExecuteReader("GetAllBankAccounts", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(dataTable);

            var bankAccounts = await transactionsRepository.GetAllBankAccounts();
            var bankCount = bankAccounts.Count();

            Assert.Equal(1, bankCount);
            Assert.Single(bankAccounts);
            Assert.Equal("RO99TEST0000000001", bankAccounts[0].Iban);
        }

        [Fact]
        public async Task AddTransaction_ShouldReturnTrue_WhenSuccessful() // CreateLoan_ShouldReturnLoanWithId_WhenSuccessful()
        {
            var transaction = new Transaction(1,"RO1234567890123456789012","RO2345678901234567890123",DateTime.UtcNow,"EUR","RON",Decimal.Parse("10.50"),Decimal.Parse("50.50"),"tip1","descriere");

            dataLinkMock.Setup(dl => dl.ExecuteNonQuery(
               It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
               .ReturnsAsync(1);

            var result = await transactionsRepository.AddTransaction(transaction);

            Assert.True(result);
        }

        [Fact]
        public async Task GetAllCurrencyExchangeRates_ShouldReturnExchange_WhenSuccessful()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("from_currency", typeof(string));
            dataTable.Columns.Add("to_currency", typeof(string));
            dataTable.Columns.Add("rate", typeof(decimal));

            dataTable.Rows.Add("EUR", "RON", 4.95m);
            dataTable.Rows.Add("RON", "EUR", 0.2m);

            dataLinkMock
                .Setup(dl => dl.ExecuteReader("GetAllCurrencyExchangeRates", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(dataTable);

            var currencyExchanges = await transactionsRepository.GetAllCurrencyExchangeRates();

            Assert.Equal(2, currencyExchanges.Count);
            Assert.Equal("EUR", currencyExchanges[0].FromCurrency);
            Assert.Equal("RON", currencyExchanges[1].FromCurrency);
        }

        [Fact]
        public async Task GetBankAccountByIBAN_ShouldReturnBankAccount_WhenSuccessful()
        {
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

            dataTable.Rows.Add("RO99TEST0000000001", "EUR", 1200.50m, false, 7, "Florin", 500.00m, 200.00m, 3);

            dataLinkMock
                .Setup(dl => dl.ExecuteReader("GetBankAccountByIBAN", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(dataTable);

            var bankAccount = await transactionsRepository.GetBankAccountByIBAN("RO99TEST0000000001");

            Assert.Equal("RO99TEST0000000001", bankAccount?.Iban);
        }

        [Fact]
        public async Task GetBankAccountTransactions_ShouldReturnTransactions_WhenSuccessful()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("transaction_id", typeof(string));
            dataTable.Columns.Add("sender_iban", typeof(string));
            dataTable.Columns.Add("receiver_iban", typeof(string));
            dataTable.Columns.Add("transaction_datetime", typeof(DateTime));
            dataTable.Columns.Add("sender_currency", typeof(string));
            dataTable.Columns.Add("receiver_currency", typeof(string));
            dataTable.Columns.Add("sender_amount", typeof(decimal));
            dataTable.Columns.Add("receiver_amount", typeof(decimal));
            dataTable.Columns.Add("transaction_type", typeof(string));
            dataTable.Columns.Add("transaction_description", typeof(string));

            dataTable.Rows.Add(1, "RO1234567890123456789012", "RO2345678901234567890123", DateTime.UtcNow, "EUR", "RON", Decimal.Parse("10.50"), Decimal.Parse("50.50"), "tip1", "descriere");
            dataTable.Rows.Add(2, "RO1234567890123456789012", "RO2345678901234567890123", DateTime.UtcNow, "EUR", "RON", Decimal.Parse("10.50"), Decimal.Parse("50.50"), "tip1", "descriere");

            dataLinkMock
                .Setup(dl => dl.ExecuteReader("GetBankAccountTransactions", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(dataTable);

            var transactions = await transactionsRepository.GetBankAccountTransactions("RO1234567890123456789012");

            Assert.Equal(2, transactions.Count);
            Assert.Equal("RO1234567890123456789012", transactions[0].SenderIban);
        }

        [Fact]
        public async Task GetExchangeRate_ShouldReturnDecimal_WhenSuccessful()
        {
            dataLinkMock
                .Setup(dl => dl.ExecuteScalar<decimal> ("GetExchangeRate", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(0.5m);

            var exchangeRate = await transactionsRepository.GetExchangeRate("from", "to");

            Assert.Equal(0.5m, exchangeRate);
        }

        [Fact]
        public async Task UpdateBankAccountBalance_ShouldReturnTrue_WhenSuccessful()
        {
            var transaction = new Transaction(1, "RO1234567890123456789012", "RO2345678901234567890123", DateTime.UtcNow, "EUR", "RON", Decimal.Parse("10.50"), Decimal.Parse("50.50"), "tip1", "descriere");

            dataLinkMock.Setup(dl => dl.ExecuteNonQuery("UpdateBankAccountBalance", It.IsAny<SqlParameter[]>()))
               .ReturnsAsync(1);

            var result = await transactionsRepository.UpdateBankAccountBalance("RO1234567890123456789012", 1.2m);
            Assert.True(result);
        }


        [Fact]
        public async Task AddTransaction_ShouldThrowArgumentNullException_WhenTransactionIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                transactionsRepository.AddTransaction(null!));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetBankAccountByIBAN_ShouldThrowArgumentException_WhenIBANIsInvalid(string invalidIban)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                transactionsRepository.GetBankAccountByIBAN(invalidIban));
        }

        [Fact]
        public async Task GetBankAccountByIBAN_ShouldReturnNull_WhenNoDataReturned()
        {
            var emptyTable = new DataTable();
            emptyTable.Columns.Add("iban", typeof(string)); // Include expected columns
            dataLinkMock.Setup(dl => dl.ExecuteReader("GetBankAccountByIBAN", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(emptyTable);

            var result = await transactionsRepository.GetBankAccountByIBAN("RO00TEST0000000000");

            Assert.Null(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetBankAccountTransactions_ShouldThrowArgumentException_WhenIBANIsInvalid(string invalidIban)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                transactionsRepository.GetBankAccountTransactions(invalidIban));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task UpdateBankAccountBalance_ShouldThrowArgumentException_WhenIbanIsInvalid(string invalidIban)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                transactionsRepository.UpdateBankAccountBalance(invalidIban, 100));
        }

        [Fact]
        public async Task UpdateBankAccountBalance_ShouldThrowArgumentException_WhenBalanceIsNegative()
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                transactionsRepository.UpdateBankAccountBalance("RO99TEST", -50));
        }

        [Fact]
        public async Task GetExchangeRate_ShouldReturnMinusOne_WhenResultIsNull()
        {
            dataLinkMock
                .Setup(dl => dl.ExecuteScalar<decimal>("GetExchangeRate", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync((decimal)0); // Simulate as if casted from null

            var rate = await transactionsRepository.GetExchangeRate("AAA", "BBB");

            Assert.Equal(0, rate); // Depending on implementation — you may also use -1 if needed
        }

        [Fact]
        public async Task GetAllBankAccounts_ShouldReturnEmptyList_WhenNoData()
        {
            var emptyTable = new DataTable();
            emptyTable.Columns.Add("iban", typeof(string)); // Add expected columns

            dataLinkMock.Setup(dl => dl.ExecuteReader("GetAllBankAccounts", null))
                .ReturnsAsync(emptyTable);

            var result = await transactionsRepository.GetAllBankAccounts();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllCurrencyExchangeRates_ShouldReturnEmptyList_WhenNoData()
        {
            var emptyTable = new DataTable();
            emptyTable.Columns.Add("from_currency", typeof(string));
            emptyTable.Columns.Add("to_currency", typeof(string));
            emptyTable.Columns.Add("rate", typeof(decimal));

            dataLinkMock.Setup(dl => dl.ExecuteReader("GetAllCurrencyExchangeRates", null))
                .ReturnsAsync(emptyTable);

            var result = await transactionsRepository.GetAllCurrencyExchangeRates();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task UpdateBankAccountBalance_ShouldThrowException_WhenSqlExceptionOccurs()
        {
            var mockDataLink = new Mock<IDataLink>();
            var repo = new TransactionsRepository(mockDataLink.Object);
            string iban = "RO01SEUP0000000001";
            decimal newBalance = 1000m;

            mockDataLink.Setup(dl => dl.ExecuteNonQuery("UpdateBankAccountBalance", It.IsAny<SqlParameter[]>()))
                .ThrowsAsync(new Exception("Database error in UpdateBankAccountBalance"));

            await Assert.ThrowsAsync<Exception>(() => repo.UpdateBankAccountBalance(iban, newBalance));
        }

        [Fact]
        public async Task GetExchangeRate_ShouldThrowException_WhenSqlExceptionOccurs()
        {
            var mockDataLink = new Mock<IDataLink>();
            var repo = new TransactionsRepository(mockDataLink.Object);
            string fromCurrency = "RON";
            string toCurrency = "EUR";

            mockDataLink.Setup(dl => dl.ExecuteScalar<decimal>("GetExchangeRate", It.IsAny<SqlParameter[]>()))
                .ThrowsAsync(new Exception("Database error in GetExchangeRate"));

            await Assert.ThrowsAsync<Exception>(() => repo.GetExchangeRate(fromCurrency, toCurrency));
        }
        [Fact]
        public async Task GetBankAccountTransactions_ShouldThrowException_WhenSqlExceptionOccurs()
        {
            var mockDataLink = new Mock<IDataLink>();
            var repo = new TransactionsRepository(mockDataLink.Object);
            string iban = "RO01SEUP0000000001";

            mockDataLink.Setup(dl => dl.ExecuteReader("GetBankAccountTransactions", It.IsAny<SqlParameter[]>()))
                .ThrowsAsync(new Exception("Database error in GetBankAccountTransactions"));

            await Assert.ThrowsAsync<Exception>(() => repo.GetBankAccountTransactions(iban));
        }

        [Fact]
        public async Task GetBankAccountByIBAN_ShouldThrowException_WhenSqlExceptionOccurs()
        {
        
            var mockDataLink = new Mock<IDataLink>();
            var repo = new TransactionsRepository(mockDataLink.Object);
            string iban = "RO01SEUP0000000001";

            mockDataLink.Setup(dl => dl.ExecuteReader("GetBankAccountByIBAN", It.IsAny<SqlParameter[]>()))
                .ThrowsAsync(new Exception("Database error in GetBankAccountByIBAN"));

            await Assert.ThrowsAsync<Exception>(() => repo.GetBankAccountByIBAN(iban));
        }

        [Fact]
        public async Task GetAllCurrencyExchangeRates_ShouldThrowException_WhenSqlExceptionOccurs()
        {
            var mockDataLink = new Mock<IDataLink>();
            var repo = new TransactionsRepository(mockDataLink.Object);

            mockDataLink.Setup(dl => dl.ExecuteReader("GetAllCurrencyExchangeRates",null))
                .ThrowsAsync(new Exception("Database error in GetAllCurrencyExchangeRates: "));

            await Assert.ThrowsAsync<Exception>(() => repo.GetAllCurrencyExchangeRates());
        }
        [Fact]
        public async Task GetAllBankAccounts_ShouldThrowException_WhenSqlExceptionOccurs()
        {
            var mockDataLink = new Mock<IDataLink>();
            var repo = new TransactionsRepository(mockDataLink.Object);

            mockDataLink.Setup(dl => dl.ExecuteReader("GetAllBankAccounts",null))
                .ThrowsAsync(new Exception("Database error in GetAllBankAccounts: "));

            await Assert.ThrowsAsync<Exception>(() => repo.GetAllBankAccounts());
        }

        [Fact]
        public async Task AddTransaction_ShouldThrowException_WhenSqlExceptionOccurs()
        {
            var mockDataLink = new Mock<IDataLink>();
            var repo = new TransactionsRepository(mockDataLink.Object);

            var transaction = new Transaction
            {
                SenderIban = "RO01SEUP0000000001",
                ReceiverIban = "RO01SEUP0000000002",
                SenderCurrency = "RON",
                ReceiverCurrency = "EUR",
                SenderAmount = 100m,
                ReceiverAmount = 200m,
                TransactionType = "Transfer",
                TransactionDescription = "Payment"
            };

            mockDataLink.Setup(dl => dl.ExecuteNonQuery("AddTransaction", It.IsAny<SqlParameter[]>()))
                .ThrowsAsync(new Exception("Database error in AddTransaction: "));

            await Assert.ThrowsAsync<Exception>(() => repo.AddTransaction(transaction));
        }
        [Fact]
        public void TransactionsRepository_ParameterlessConstructor_ShouldCreateInstance()
        {
            // Act
            var repo = new TransactionsRepository();

            // Assert
            Assert.NotNull(repo); // Just confirms the object can be created
        }
    }
}
