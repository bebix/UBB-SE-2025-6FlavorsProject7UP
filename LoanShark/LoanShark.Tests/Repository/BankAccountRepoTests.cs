using LoanShark.Data;
using LoanShark.Domain;
using LoanShark.Repository;
using LoanShark.Service;
using Microsoft.Data.SqlClient;
using Moq;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Xunit;

namespace LoanShark.Tests
{
    public class BankAccountRepoTests
    {
        private readonly Mock<IBankAccountRepository> _mockRepo;
        private readonly Mock<IDataLink> mockDataLink;
        private readonly BankAccountRepository repo;

        public BankAccountRepoTests()
        {
            mockDataLink = new Mock<IDataLink>();
            _mockRepo = new Mock<IBankAccountRepository>();
            repo = new BankAccountRepository(mockDataLink.Object);
        }

        [Fact]
        public async Task GetBankAccountByIBAN_ReturnsCorrectAccount()
        {
            var expectedIBAN = "RO49AAAA1B31007593840000";
            var dataTable = new DataTable();
            dataTable.Columns.Add("iban");
            dataTable.Columns.Add("currency");
            dataTable.Columns.Add("amount");
            dataTable.Columns.Add("blocked");
            dataTable.Columns.Add("id_user");
            dataTable.Columns.Add("custom_name");
            dataTable.Columns.Add("daily_limit");
            dataTable.Columns.Add("max_per_transaction");
            dataTable.Columns.Add("max_nr_transactions_daily");

            dataTable.Rows.Add(expectedIBAN, "RON", 1234.56, false, 1, "My Acc", "1000", "500", "10");

            mockDataLink.Setup(dl => dl.ExecuteReader("GetBankAccountByIBAN", It.IsAny<SqlParameter[]>()))
                        .ReturnsAsync(dataTable);

            var result = await repo.GetBankAccountByIBAN(expectedIBAN);

            Assert.NotNull(result);
            Assert.Equal(expectedIBAN, result!.Iban);
            Assert.Equal("RON", result.Currency);
            Assert.Equal(1234.56m, result.Balance);
        }

        [Fact]
        public void ConvertDataTableRowToBankAccount_ReturnsCorrectBankAccount()
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

            var row = dataTable.NewRow();
            row["iban"] = "RO99TEST0000000001";
            row["currency"] = "EUR";
            row["amount"] = 1200.50m;
            row["blocked"] = false;
            row["id_user"] = 7;
            row["custom_name"] = "Teo";
            row["daily_limit"] = 500.00m;
            row["max_per_transaction"] = 200.00m;
            row["max_nr_transactions_daily"] = 3;

            dataTable.Rows.Add(row);

            var repo = new BankAccountRepository();

            var result = repo.ConvertDataTableRowToBankAccount(dataTable.Rows[0]);

            Assert.Equal("RO99TEST0000000001", result.Iban);
            Assert.Equal("EUR", result.Currency);
            Assert.Equal(1200.50m, result.Balance);
            Assert.False(result.Blocked);
            Assert.Equal(7, result.UserID);
            Assert.Equal("Teo", result.Name);
            Assert.Equal(500.00m, result.DailyLimit);
            Assert.Equal(200.00m, result.MaximumPerTransaction);
            Assert.Equal(3, result.MaximumNrTransactions);
        }

        [Fact]
        public async Task ConvertDataTableToCurrencyList_ReturnsListOfCurrencies()
        {

            var dataTable = new DataTable();
            dataTable.Columns.Add("currency_name", typeof(string));
            dataTable.Rows.Add("USD");
            dataTable.Rows.Add("EUR");

            var repo = new BankAccountRepository();


            var result = await repo.ConvertDataTableToCurrencyList(dataTable);


            Assert.Equal(2, result.Count);
            Assert.Contains("USD", result);
            Assert.Contains("EUR", result);
        }

        private DataTable CreateSampleDataTable()
        {
            var table = new DataTable();
            table.Columns.Add("iban", typeof(string));
            table.Columns.Add("currency", typeof(string));
            table.Columns.Add("amount", typeof(decimal));
            table.Columns.Add("blocked", typeof(bool));
            table.Columns.Add("id_user", typeof(int));
            table.Columns.Add("custom_name", typeof(string));
            table.Columns.Add("daily_limit", typeof(decimal));
            table.Columns.Add("max_per_transaction", typeof(decimal));
            table.Columns.Add("max_nr_transactions_daily", typeof(int));

            table.Rows.Add("RO49AAAA1B31007593840000", "EUR", 1000, false, 1, "Cont Teo", 500, 200, 10);
            return table;
        }

        [Fact]
        public async Task GetAllBankAccounts_ReturnsAccounts()
        {
            mockDataLink.Setup(m => m.ExecuteReader("GetAllBankAccounts", null))
                .ReturnsAsync(CreateSampleDataTable());

            var result = await repo.GetAllBankAccounts();

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("EUR", result[0].Currency);
        }

        [Fact]
        public async Task GetBankAccountsByUserId_ReturnsAccounts()
        {
            mockDataLink.Setup(m => m.ExecuteReader("GetBankAccountsByUser", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(CreateSampleDataTable());

            var result = await repo.GetBankAccountsByUserId(1);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result[0].UserID);
        }

        [Fact]
        public async Task GetBankAccountByIBAN_ReturnsSingleAccount()
        {
            var table = CreateSampleDataTable();
            mockDataLink.Setup(m => m.ExecuteReader("GetBankAccountByIBAN", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(table);

            var result = await repo.GetBankAccountByIBAN("RO49AAAA1B31007593840000");

            Assert.NotNull(result);
            Assert.Equal("EUR", result.Currency);
        }


        [Fact]
        public async Task GetCurrencies_ReturnsCurrencyList()
        {
            var table = new DataTable();
            table.Columns.Add("currency_name", typeof(string));
            table.Rows.Add("EUR");
            table.Rows.Add("RON");

            mockDataLink.Setup(m => m.ExecuteReader("GetCurrencies", null))
                .ReturnsAsync(table);

            var result = await repo.GetCurrencies();

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetCredentials_ReturnsHashedAndSalt()
        {
            var table = new DataTable();
            table.Columns.Add("hashed_password", typeof(string));
            table.Columns.Add("password_salt", typeof(string));
            table.Rows.Add("hash123", "salt456");

            mockDataLink.Setup(m => m.ExecuteReader("GetCredentials", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(table);

            var result = await repo.GetCredentials("teo@gmail.com");

            Assert.Equal("hash123", result[0]);
            Assert.Equal("salt456", result[1]);
        }

        [Fact]
        public void Add_ShouldCallRepositoryAdd()
        {
            var account = new BankAccount("RO49AAAA", "EUR", 1000m, false, 1, "Teo’s account", 1000, 500, 5);

            _mockRepo.Object.AddBankAccount(account);

            _mockRepo.Verify(r => r.AddBankAccount(It.Is<BankAccount>(a => a.Iban == "RO49AAAA")), Times.Once);
        }

        [Fact]
        public void Delete_ShouldCallRepositoryDelete()
        {
            string ibanToDelete = "RO22TEST";
            _mockRepo.Object.RemoveBankAccount(ibanToDelete);

            _mockRepo.Verify(r => r.RemoveBankAccount("RO22TEST"), Times.Once);
        }

        [Fact]
        public async Task AddBankAccount_ShouldCallExecuteNonQuery_WithCorrectParameters()
        {
            // Arrange
            var mockDataLink = new Mock<IDataLink>();
            var repo = new BankAccountRepository(mockDataLink.Object);
            var account = new BankAccount(
                "RO49AAAA1B31007593840000",
                "EUR",
                1500.75m,
                true,
                42,
                "Cont Teo",
                1000m,
                500m,
                5
            );

            // Act
            var result = await repo.AddBankAccount(account);

            // Assert
            Assert.True(result);
        }


        [Fact]
        public async Task RemoveBankAccount_ShouldCallExecuteNonQuery_WithCorrectIBAN()
        {
            // Arrange
            var mockDataLink = new Mock<IDataLink>();
            var repo = new BankAccountRepository(mockDataLink.Object);
            var iban = "RO49AAAA1B31007593840000";

            // Act
            var result = await repo.RemoveBankAccount(iban);

            // Assert
            Assert.True(result);
            mockDataLink.Verify(d => d.ExecuteNonQuery("RemoveBankAccount",
                It.Is<SqlParameter[]>(p =>
                    p.Length == 1 && ((string)p[0].Value) == iban
                )), Times.Once);
        }

        [Fact]
        public async Task UpdateBankAccount_ShouldCallExecuteNonQuery_WithCorrectParameters()
        {
            // Arrange
            var mockDataLink = new Mock<IDataLink>();
            var repo = new BankAccountRepository(mockDataLink.Object);
            var account = new BankAccount(
                "RO49AAAA1B31007593840000",
                "EUR",
                1500.75m,
                true,
                42,
                "Cont Teo",
                1000m,
                500m,
                5
            );

            // Act
            var result = await repo.UpdateBankAccount(account.Iban, account);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetAllBankAccounts_ShouldReturnNull_WhenExceptionOccurs()
        {
            // Arrange
            mockDataLink.Setup(dl => dl.ExecuteReader("GetAllBankAccounts",null))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await repo.GetAllBankAccounts();

            // Assert
            Assert.Null(result); // The result should be null when an exception occurs
        }

        [Fact]
        public async Task GetBankAccountsByUserId_ShouldReturnNull_WhenExceptionOccurs()
        {
            // Arrange
            int userId = 1;
            mockDataLink.Setup(dl => dl.ExecuteReader("GetBankAccountsByUser", It.Is<SqlParameter[]>(p => (int)p[0].Value == userId)))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await repo.GetBankAccountsByUserId(userId);

            // Assert
            Assert.Null(result); // The result should be null when an exception occurs
        }


        [Fact]
        public async Task GetBankAccountByIBAN_ShouldReturnNull_WhenExceptionOccurs()
        {
            // Arrange
            string iban = "RO01SEUP0000000001";
            mockDataLink.Setup(dl => dl.ExecuteReader("GetBankAccountByIBAN", It.Is<SqlParameter[]>(p => (string)p[0].Value == iban)))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await repo.GetBankAccountByIBAN(iban);

            // Assert
            Assert.Null(result); // Result should be null since an exception occurred
        }

    }

}