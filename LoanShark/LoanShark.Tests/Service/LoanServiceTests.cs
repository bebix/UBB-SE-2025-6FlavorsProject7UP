using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using LoanShark.Repository;
using LoanShark.Service;
using LoanShark.Domain;
using Xunit;
using Windows.System;
using Moq.Protected;
namespace LoanShark.Tests.Service
{
    public class LoanServiceTests
    {

        private readonly Mock<ILoanRepository> loanRepositoryMock;
        private readonly LoanService loanService;

        public LoanServiceTests()
        {
            loanRepositoryMock = new Mock<ILoanRepository>();
            loanService = new LoanService(loanRepositoryMock.Object);

        }



        [Fact]
        public async Task GetUserLoans_ReturnsLoansForUser()
        {
            int userId = 1;
            var loans = new List<Loan>
            {
                new Loan(1, userId, 5000, "USD", DateTime.Now, null, 5, 12, "unpaid"),
                new Loan(2, userId, 3000, "USD", DateTime.Now, null, 3, 6, "paid")
            };

            loanRepositoryMock.Setup(r => r.GetLoansByUserId(userId)).ReturnsAsync(loans);
            var result = await loanService.GetUserLoans(userId);

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetUnpaidUserLoans_ReturnsOnlyUnpaid()
        {
            int userId = 1;
            var loans = new List<Loan>
            {
                new Loan(1, userId, 5000, "USD", DateTime.Now, null, 5, 12, "unpaid"),
                new Loan(2, userId, 3000, "USD", DateTime.Now, null, 3, 6, "paid")
            };

            loanRepositoryMock.Setup(r => r.GetLoansByUserId(userId)).ReturnsAsync(loans);

            var result = await loanService.GetUnpaidUserLoans(userId);

            Assert.Single(result);
            Assert.Equal("unpaid", result.First().State);
        }

        [Theory]
        [InlineData(0, 12, "Invalid loan amount")]
        [InlineData(5000, 5, "Invalid loan duration")]
        [InlineData(200, 24, "success")]
        public void ValidateLoanRequest_ReturnsAppropriateMessage(decimal amount, int months, string expected)
        {
            var result = loanService.ValidateLoanRequest(amount, months);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateAmountToPay_CorrectlyCalculates()
        {
            decimal amount = 1000;
            decimal tax = 12;

            var result = loanService.CalculateAmountToPay(amount, tax);

            Assert.Equal(1120, result);
        }

        [Fact]
        public async Task ConvertCurrency_ReturnsConvertedAmount()
        {
            var rates = new List<CurrencyExchange>
            {
                new CurrencyExchange("USD", "EUR", 0.9M)
            };

            loanRepositoryMock.Setup(r => r.GetAllCurrencyExchanges()).ReturnsAsync(rates);

            var result = await loanService.ConvertCurrency(100, "USD", "EUR");

            Assert.Equal(90, result);
        }

        [Fact]
        public async Task GetFormattedBankAccounts_ReturnsCorrectFormat()
        {
            int userId = 1;
            var accounts = new List<BankAccount>
            {
                new BankAccount("IBAN123", "USD", 2000, false, userId, "accountName", 200, 100, 10)
            };

            loanRepositoryMock.Setup(r => r.GetBankAccountsByUserId(userId)).ReturnsAsync(accounts);

            var result = await loanService.GetFormattedBankAccounts(userId);

            Assert.Single(result);
            Assert.Equal("IBAN123 - USD - 2000", result.First());
        }

        [Fact]
        public async Task GetLoanById_ReturnsCorrectLoan()
        {

            var loans = new List<Loan>
            {
                new Loan(10, 1, 1000, "USD", DateTime.Now, null, 10, 6, "unpaid"),
                new Loan(20, 1, 1500, "USD", DateTime.Now, null, 12, 12, "paid")
            };

            loanRepositoryMock.Setup(r => r.GetAllLoans()).ReturnsAsync(loans);

            var result = await loanService.GetLoanById(20);

            Assert.NotNull(result);
            Assert.Equal(20, result.LoanID);
        }

        [Fact]
        public async Task CheckSufficientFunds_ReturnsTrue_WhenEnoughBalance()
        {
            int userId = 1;
            var account = new BankAccount("IBAN123", "USD", 2000, false, userId, "accountName", 200, 100, 10);

            loanRepositoryMock.Setup(r => r.GetBankAccountsByUserId(userId))
                .ReturnsAsync(new List<BankAccount> { account });

            var result = await loanService.CheckSufficientFunds(userId, "IBAN123", 1000, "USD");

            Assert.True(result);
        }

        [Fact]
        public async Task CheckSufficientFunds_ReturnsFalse_WhenNotEnoughBalance()
        {
            int userId = 1;
            var account = new BankAccount("IBAN123", "USD", 200, false, userId, "accountName", 200, 100, 10);

            loanRepositoryMock.Setup(r => r.GetBankAccountsByUserId(userId))
                .ReturnsAsync(new List<BankAccount> { account });

            var result = await loanService.CheckSufficientFunds(userId, "IBAN123", 1000, "USD");

            Assert.False(result);
        }

        [Fact]
        public async Task PayLoanAsync_LoanNotFoundOrAlreadyPaid_ReturnsError()
        {
            int userId = 1;
            int loanId = 100;
            string iban = "IBAN123";

            loanRepositoryMock.Setup(r => r.GetAllLoans()).ReturnsAsync(new List<Loan>());

            var result = await loanService.PayLoanAsync(userId, loanId, iban);

            Assert.Equal("Loan not found or already paid", result);
        }

        [Fact]
        public async Task PayLoanAsync_BankAccountNotFound_ReturnsError()
        {
            var loan = new Loan(100, 1, 1000, "USD", DateTime.Now, null, 1, 1, "unpaid");

            loanRepositoryMock.Setup(r => r.GetAllLoans()).ReturnsAsync(new List<Loan> { loan });
            loanRepositoryMock.Setup(r => r.GetBankAccountByIBAN(It.IsAny<string>())).ReturnsAsync((BankAccount?)null);

            var result = await loanService.PayLoanAsync(1, loan.LoanID, "IBAN123");

            Assert.Equal("Bank account not found", result);
        }

        [Theory]
        [InlineData(12, 12)]
        [InlineData(0, 0)]
        public void CalculateTaxPercentage_ReturnsExpectedValue(int months, decimal expected)
        {

            var result = loanService.CalculateTaxPercentage(months);

            Assert.Equal(expected, result);
        }
        [Fact]
        public void GetCurrencyRate_ReturnsMatchingRate_WhenFound()
        {
            var rates = new List<CurrencyExchange>
            {
                new CurrencyExchange("USD", "EUR", 0.9m),
                new CurrencyExchange("USD", "JPY", 110m)
            };


            var result = loanService.GetCurrencyRate(rates, "USD", "EUR");

            Assert.NotNull(result);
            Assert.Equal("USD", result!.FromCurrency);
            Assert.Equal("EUR", result.ToCurrency);
            Assert.Equal(0.9m, result.ExchangeRate);
        }

        [Fact]
        public void GetCurrencyRate_ReturnsNull_WhenNoMatch()
        {
            var rates = new List<CurrencyExchange>
            {
                new CurrencyExchange("USD", "JPY", 110m)
            };

            var result = loanService.GetCurrencyRate(rates, "USD", "EUR");

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateBankAccount_UpdatesBalance_WhenValid()
        {
            var account = new BankAccount("IBAN123", "USD", 1000, false, 1, "acc", 0, 0, 0);

            loanRepositoryMock.Setup(r => r.GetBankAccountByIBAN("IBAN123")).ReturnsAsync(account);
            loanRepositoryMock.Setup(r => r.UpdateBankAccountBalance("IBAN123", 900)).ReturnsAsync(true);

            await loanService.UpdateBankAccount(1, "IBAN123", 100, "USD");

            loanRepositoryMock.Verify(r => r.UpdateBankAccountBalance("IBAN123", 900), Times.Once);
        }

        [Fact]
        public async Task UpdateBankAccount_ThrowsException_WhenAccountNotFound()
        {

            loanRepositoryMock.Setup(r => r.GetBankAccountByIBAN("IBAN123")).ReturnsAsync((BankAccount?)null);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                loanService.UpdateBankAccount(1, "IBAN123", 100, "USD"));
        }



        [Fact]
        public async Task UpdateBankAccount_ThrowsException_WhenCurrencyMismatch()
        {
            var account = new BankAccount("IBAN123", "EUR", 1000, false, 1, "acc", 1, 0, 0);
            loanRepositoryMock.Setup(r => r.GetBankAccountByIBAN("IBAN123")).ReturnsAsync(account);

            await Assert.ThrowsAsync<ArgumentException>(() =>
                loanService.UpdateBankAccount(1, "IBAN123", 100, "USD"));
        }

        [Fact]
        public async Task UpdateBankAccount_ThrowsException_WhenUpdateFails()
        {
            var account = new BankAccount("IBAN123", "USD", 1000, false, 1, "acc", 0, 0, 0);

            loanRepositoryMock.Setup(r => r.GetBankAccountByIBAN("IBAN123")).ReturnsAsync(account);
            loanRepositoryMock.Setup(r => r.UpdateBankAccountBalance("IBAN123", 900)).ReturnsAsync(false);

            await Assert.ThrowsAsync<Exception>(() =>
                loanService.UpdateBankAccount(1, "IBAN123", 100, "USD"));
        }

        [Fact]
        public async Task TakeLoanAsync_ShouldReturnLoan_WhenParametersAreValid()
        {
            // Arrange
            int userId = 1;
            decimal amount = 1000m;
            string currency = "EUR";
            string iban = "RO49AAAA1B31007593840000";
            int months = 12;
            decimal expectedTax = 0.2m;

            var expectedLoan = new Loan(10, userId, amount, currency, DateTime.Now, null, expectedTax, months,
                "unpaid");

            loanRepositoryMock
                .Setup(repo => repo.CreateLoan(It.IsAny<Loan>()))
                .ReturnsAsync(expectedLoan);

            // Act
            var result = await loanService.TakeLoanAsync(userId, amount, currency, iban, months);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedLoan.LoanID, result!.LoanID);
            Assert.Equal(userId, result.UserID);
            Assert.Equal(currency, result.Currency);
            Assert.Equal("unpaid", result.State);
        }

        [Theory]
        [InlineData(0, 12)] // Invalid amount
        [InlineData(1000, 1)] // Invalid months
        public async Task TakeLoanAsync_ShouldThrowArgumentException_WhenParametersAreInvalid(decimal amount,
            int months)
        {
            // Arrange
            int userId = 1;
            string currency = "EUR";
            string iban = "RO49AAAA1B31007593840000";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                loanService.TakeLoanAsync(userId, amount, currency, iban, months));

            Assert.Equal("Invalid loan parameters", exception.Message);
        }

        [Fact]
        public async Task PayLoanAsync_InsufficientFunds_ReturnsError()
        {
            var loan = new Loan(123, 1, 1000, "USD", DateTime.Now, null, 0.1m, 12, "unpaid");
            var account = new BankAccount("IBAN123", "USD", 1000, false, 1, "acc", 0, 0, 0);

            loanRepositoryMock.Setup(r => r.GetAllLoans()).ReturnsAsync(new List<Loan> { loan });
            loanRepositoryMock.Setup(r => r.GetBankAccountByIBAN("IBAN123")).ReturnsAsync(account);
            loanRepositoryMock.Setup(r => r.GetBankAccountsByUserId(1)).ReturnsAsync(new List<BankAccount> { account });

            // Force CheckSufficientFunds to return false via low balance
            account.Balance = 100;

            var result = await loanService.PayLoanAsync(1, 123, "IBAN123");

            Assert.Equal("Insufficient funds", result);
        }

        [Fact]
        public async Task PayLoanAsync_UpdateBankAccountFails_ReturnsError()
        {
            var loan = new Loan(123, 1, 1000, "USD", DateTime.Now, null, 0.1m, 12, "unpaid");
            loan.Amount = 1100;

            var account = new BankAccount("IBAN123", "USD", 2000, false, 1, "acc", 0, 0, 0);

            loanRepositoryMock.Setup(r => r.GetAllLoans()).ReturnsAsync(new List<Loan> { loan });
            loanRepositoryMock.Setup(r => r.GetBankAccountByIBAN("IBAN123")).ReturnsAsync(account);
            loanRepositoryMock.Setup(r => r.GetBankAccountsByUserId(1)).ReturnsAsync(new List<BankAccount> { account });

            // Simulate exception on update
            loanRepositoryMock
                .Setup(r => r.UpdateBankAccountBalance(It.IsAny<string>(), It.IsAny<decimal>()))
                .ReturnsAsync(true);
            var result = await loanService.PayLoanAsync(1, 123, "IBAN123");

            Assert.Equal("Update loan failed", result);
        }

        


    }
}
