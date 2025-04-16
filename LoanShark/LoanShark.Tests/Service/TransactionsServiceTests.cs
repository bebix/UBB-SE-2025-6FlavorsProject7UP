using LoanShark.Domain;
using LoanShark.Repository;
using LoanShark.Service;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LoanShark.Tests.Service
{
    public class TransactionsServiceTests
    {
        private Mock<ITransactionsRepository> _mockRepo;
        private TransactionsService _service;

        public TransactionsServiceTests()
        {
            _mockRepo = new Mock<ITransactionsRepository>();
            _service = new TransactionsService(_mockRepo.Object);
        }

        [Fact]
        public async Task AddTransaction_ShouldSucceed_WhenValid()
        {
            var sender = new BankAccount("IBAN1", "USD", 1000m, false, 1, "Main", 2000m, 500m, 10);
            var receiver = new BankAccount("IBAN2", "USD", 300m, false, 2, "Savings", 2000m, 500m, 10);
            decimal amount = 200m;

            _mockRepo.Setup(r => r.GetBankAccountByIBAN("IBAN1")).ReturnsAsync(sender);
            _mockRepo.Setup(r => r.GetBankAccountByIBAN("IBAN2")).ReturnsAsync(receiver);
            _mockRepo.Setup(r => r.AddTransaction(It.IsAny<Transaction>())).Returns(Task.FromResult(true));
            _mockRepo.Setup(r => r.UpdateBankAccountBalance("IBAN1", 800m)).Returns(Task.FromResult(true));
            _mockRepo.Setup(r => r.UpdateBankAccountBalance("IBAN2", 500m)).Returns(Task.FromResult(true));

            var result = await _service.AddTransaction("IBAN1", "IBAN2", amount, "Test");

            Assert.Equal("Transaction successful!", result);
        }

        [Fact]
        public async Task AddTransaction_ShouldFail_WhenBlocked()
        {
            var sender = new BankAccount("IBAN1", "USD", 1000m, true, 1, "Main", 2000m, 500m, 10);
            var receiver = new BankAccount("IBAN2", "USD", 300m, false, 2, "Savings", 2000m, 500m, 10);

            _mockRepo.Setup(r => r.GetBankAccountByIBAN("IBAN1")).ReturnsAsync(sender);
            _mockRepo.Setup(r => r.GetBankAccountByIBAN("IBAN2")).ReturnsAsync(receiver);

            var result = await _service.AddTransaction("IBAN1", "IBAN2", 100m);

            Assert.Equal("Sender account is blocked.", result);
        }

        [Fact]
        public async Task AddTransaction_ShouldFail_WhenInsufficientFunds()
        {
            var sender = new BankAccount("IBAN1", "USD", 100m, false, 1, "Main", 2000m, 500m, 10);
            var receiver = new BankAccount("IBAN2", "USD", 300m, false, 2, "Savings", 2000m, 500m, 10);

            _mockRepo.Setup(r => r.GetBankAccountByIBAN("IBAN1")).ReturnsAsync(sender);
            _mockRepo.Setup(r => r.GetBankAccountByIBAN("IBAN2")).ReturnsAsync(receiver);

            var result = await _service.AddTransaction("IBAN1", "IBAN2", 200m);

            Assert.Equal("Insufficient funds.", result);
        }

        [Fact]
        public async Task AddTransaction_ShouldFail_WhenExchangeRateUnavailable()
        {
            var sender = new BankAccount("IBAN1", "USD", 1000m, false, 1, "Main", 2000m, 500m, 10);
            var receiver = new BankAccount("IBAN2", "EUR", 300m, false, 2, "Savings", 2000m, 500m, 10);

            _mockRepo.Setup(r => r.GetBankAccountByIBAN("IBAN1")).ReturnsAsync(sender);
            _mockRepo.Setup(r => r.GetBankAccountByIBAN("IBAN2")).ReturnsAsync(receiver);
            _mockRepo.Setup(r => r.GetExchangeRate("USD", "EUR")).ReturnsAsync(-1m);

            var result = await _service.AddTransaction("IBAN1", "IBAN2", 100m);

            Assert.Equal("Exchange rate not available.", result);
        }

        [Fact]
        public async Task TakeLoanTransaction_ShouldSucceed()
        {
            var account = new BankAccount("IBAN123", "USD", 100m, false, 1, "LoanAcct", 5000m, 1000m, 10);
            decimal loanAmount = 400m;

            _mockRepo.Setup(r => r.GetBankAccountByIBAN("IBAN123")).ReturnsAsync(account);
            _mockRepo.Setup(r => r.UpdateBankAccountBalance("IBAN123", 500m)).Returns(Task.FromResult(true));
            _mockRepo.Setup(r => r.AddTransaction(It.IsAny<Transaction>())).Returns(Task.FromResult(true));

            var result = await _service.TakeLoanTransaction("IBAN123", loanAmount);

            Assert.Equal("Loan credited to your account!", result);
        }

        [Fact]
        public async Task PayLoanTransaction_ShouldSucceed()
        {
            var account = new BankAccount("IBAN123", "USD", 500m, false, 1, "LoanAcct", 5000m, 1000m, 10);
            decimal payAmount = 300m;

            _mockRepo.Setup(r => r.GetBankAccountByIBAN("IBAN123")).ReturnsAsync(account);
            _mockRepo.Setup(r => r.GetBankAccountByIBAN("RO90BANK0000000000000005")).ReturnsAsync(account);

            _mockRepo.Setup(r => r.UpdateBankAccountBalance("IBAN123", 200m)).Returns(Task.FromResult(true));
            _mockRepo.Setup(r => r.AddTransaction(It.IsAny<Transaction>())).Returns(Task.FromResult(true));

            var result = await _service.PayLoanTransaction("IBAN123", payAmount);

            Assert.Equal("Loan payment successful!", result);
        }

        [Fact]
        public async Task PayLoanTransaction_ShouldFail_WhenInsufficientFunds()
        {
            //_mockRepo.Reset();
            var account = new BankAccount("IBAN123", "USD", 100m, false, 1, "LoanAcct", 5000m, 1000m, 10);
            decimal payAmount = 200m;

            _mockRepo.Setup(r => r.GetBankAccountByIBAN("IBAN123")).ReturnsAsync(account);
            _mockRepo.Setup(r => r.GetBankAccountByIBAN("RO90BANK0000000000000005")).ReturnsAsync(account);

            var result = await _service.PayLoanTransaction("IBAN123", payAmount);

            Assert.Equal("Insufficient funds.", result);
        }

        [Fact]
        public async Task GetAllCurrencyExchangeRates_ReturnsRates_WhenSuccessful()
        {
            // Arrange
            var expectedRates = new List<CurrencyExchange>
            {
                new CurrencyExchange("USD", "EUR", 0.9m),
                new CurrencyExchange("EUR", "RON", 4.95m)
            };

            _mockRepo.Setup(repo => repo.GetAllCurrencyExchangeRates())
                     .ReturnsAsync(expectedRates);

            // Act
            var result = await _service.GetAllCurrencyExchangeRates();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("USD", result[0].FromCurrency);
            Assert.Equal("RON", result[1].ToCurrency);
        }

        [Fact]
        public async Task GetAllCurrencyExchangeRates_ThrowsException_WhenRepositoryFails()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetAllCurrencyExchangeRates())
                     .ThrowsAsync(new Exception("Database connection error"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(async () => await _service.GetAllCurrencyExchangeRates());

            Assert.Contains("Error retrieving currency exchange rates", ex.Message);
            Assert.Contains("Database connection error", ex.InnerException?.Message);
        }

        [Fact]
        public async Task AddTransaction_ShouldFail_WhenIBANsAreEmpty()
        {
            var result = await _service.AddTransaction("", "", 100m);
            Assert.Equal("Receiver IBANs must be provided.", result);
        }

        [Fact]
        public async Task AddTransaction_ShouldFail_WhenAmountIsNonPositive()
        {
            var result = await _service.AddTransaction("IBAN1", "IBAN2", 0m);
            Assert.Equal("Invalid transaction amount. Must be greater than zero.", result);
        }


        [Fact]
        public async Task AddTransaction_ShouldFail_WhenSenderEqualsReceiver()
        {
            var result = await _service.AddTransaction("IBAN1", "IBAN1", 100m);
            Assert.Equal("Cannot send money to the same account.", result);
        }


        [Fact]
        public async Task AddTransaction_ShouldFail_WhenTransactionExceedsMaxLimit()
        {
            var sender = new BankAccount("IBAN1", "USD", 2000m, false, 1, "Main", 2000m, 500m, 10);
            var receiver = new BankAccount("IBAN2", "USD", 300m, false, 2, "Savings", 2000m, 500m, 10);

            _mockRepo.Setup(r => r.GetBankAccountByIBAN("IBAN1")).ReturnsAsync(sender);
            _mockRepo.Setup(r => r.GetBankAccountByIBAN("IBAN2")).ReturnsAsync(receiver);

            var result = await _service.AddTransaction("IBAN1", "IBAN2", 600m);

            Assert.Equal("Transaction exceeds maximum limit per transaction (500).", result);
        }


        [Fact]
        public async Task TakeLoanTransaction_ShouldFail_WhenIbanIsEmpty()
        {
            var result = await _service.TakeLoanTransaction("", 200m);
            Assert.Equal("IBAN must be provided.", result);
        }


        [Fact]
        public async Task TakeLoanTransaction_ShouldFail_WhenAmountIsInvalid()
        {
            var result = await _service.TakeLoanTransaction("IBAN123", 0m);
            Assert.Equal("Invalid loan amount. Must be greater than zero.", result);
        }


        [Fact]
        public async Task TakeLoanTransaction_ShouldFail_WhenAccountDoesNotExist()
        {
            _mockRepo.Setup(r => r.GetBankAccountByIBAN("IBAN123")).ReturnsAsync((BankAccount?)null);

            var result = await _service.TakeLoanTransaction("IBAN123", 100m);

            Assert.Equal("Bank account does not exist.", result);
        }


        [Fact]
        public async Task TakeLoanTransaction_ShouldFail_WhenAccountIsBlocked()
        {
            var account = new BankAccount("IBAN123", "USD", 100m, true, 1, "LoanAcct", 5000m, 1000m, 10);
            _mockRepo.Setup(r => r.GetBankAccountByIBAN("IBAN123")).ReturnsAsync(account);

            var result = await _service.TakeLoanTransaction("IBAN123", 100m);

            Assert.Equal("Bank account is blocked.", result);
        }


        [Fact]
        public async Task PayLoanTransaction_ShouldFail_WhenIbanIsEmpty()
        {
            var result = await _service.PayLoanTransaction("", 100m);
            Assert.Equal("IBAN must be provided.", result);
        }


        [Fact]
        public async Task PayLoanTransaction_ShouldFail_WhenAmountIsInvalid()
        {
            var result = await _service.PayLoanTransaction("IBAN123", 0m);
            Assert.Equal("Invalid payment amount. Must be greater than zero.", result);
        }


        [Fact]
        public async Task PayLoanTransaction_ShouldFail_WhenUserAccountDoesNotExist()
        {
            _mockRepo.Setup(r => r.GetBankAccountByIBAN("IBAN123")).ReturnsAsync((BankAccount?)null);

            var result = await _service.PayLoanTransaction("IBAN123", 100m);

            Assert.Equal("User account does not exist.", result);
        }


        [Fact]
        public async Task PayLoanTransaction_ShouldFail_WhenBankAccountIsMissing()
        {
            var user = new BankAccount("IBAN123", "USD", 500m, false, 1, "LoanAcct", 2000m, 500m, 10);
            _mockRepo.Setup(r => r.GetBankAccountByIBAN("IBAN123")).ReturnsAsync(user);
            _mockRepo.Setup(r => r.GetBankAccountByIBAN("RO90BANK0000000000000005")).ReturnsAsync((BankAccount?)null);

            var result = await _service.PayLoanTransaction("IBAN123", 100m);

            Assert.Equal("Bank account does not exist.", result);
        }


        [Fact]
        public async Task PayLoanTransaction_ShouldFail_WhenUserAccountBlocked()
        {
            var account = new BankAccount("IBAN123", "USD", 500m, true, 1, "LoanAcct", 2000m, 500m, 10);
            _mockRepo.Setup(r => r.GetBankAccountByIBAN("IBAN123")).ReturnsAsync(account);
            _mockRepo.Setup(r => r.GetBankAccountByIBAN("RO90BANK0000000000000005")).ReturnsAsync(account);

            var result = await _service.PayLoanTransaction("IBAN123", 100m);

            Assert.Equal("User account is blocked.", result);
        }
    }
}
