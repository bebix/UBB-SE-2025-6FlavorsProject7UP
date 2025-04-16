using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Moq;
using LoanShark.Data;
using LoanShark.Repository;
using Xunit;
using LoanShark.Domain;
using LoanShark.Service;
using System.Data;

namespace LoanShark.Tests.Repository
{
    public class LoanRepositoryTests
    {
        private readonly Mock<IDataLink> dataLinkMock;
        private readonly LoanRepository loanRepository;

        public LoanRepositoryTests()
        {
            dataLinkMock = new Mock<IDataLink>();
            loanRepository = new LoanRepository(dataLinkMock.Object);
        }

        [Fact]
        public async Task CreateLoan_ShouldReturnLoanWithId_WhenSuccessful()
        {
            var loan = new Loan(0, 1, 1000, "USD", DateTime.Now, null, 5, 12, "unpaid");

            dataLinkMock.Setup(dl => dl.ExecuteNonQuery(
                It.IsAny<string>(), It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(1)
                .Callback<string, SqlParameter[]>((_, parameters) =>
                {
                    var outputParam = Array.Find(parameters, p => p.ParameterName == "@id_loan");
                    outputParam.Value = 42;
                });

            var result = await loanRepository.CreateLoan(loan);

            Assert.NotNull(result);
            Assert.Equal(42, result!.LoanID);
        }


        [Fact]
        public async Task GetAllLoans_ShouldReturnListOfLoans_WhenDataIsReturned()
        {

            var sampleTable = new DataTable();
            var expectedLoansCount = 1;
            sampleTable.Columns.Add("id_loan", typeof(int));
            sampleTable.Columns.Add("id_user", typeof(int));
            sampleTable.Columns.Add("amount", typeof(decimal));
            sampleTable.Columns.Add("currency", typeof(string));
            sampleTable.Columns.Add("date_taken", typeof(DateTime));
            sampleTable.Columns.Add("date_paid", typeof(DateTime));
            sampleTable.Columns.Add("tax_percentage", typeof(decimal));
            sampleTable.Columns.Add("number_months", typeof(int));
            sampleTable.Columns.Add("loan_state", typeof(string));

            sampleTable.Rows.Add(1, 2, 1000, "USD", DateTime.Now, DBNull.Value, 5, 12, "unpaid");

            dataLinkMock
                .Setup(dl => dl.ExecuteReader("GetAllLoans", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(sampleTable);

            var loans = await loanRepository.GetAllLoans();
            var loanCount = loans.Count();

            Assert.Equal(expectedLoansCount, loanCount);
            Assert.Single(loans);
            Assert.Equal(1, loans[0].LoanID);
        }

        [Fact]
        public async Task GetLoansByUserId_ShouldReturnLoans_WhenDataExists()
        {
            var sampleTable = new DataTable();
            var userId = 2;
            sampleTable.Columns.Add("id_loan", typeof(int));
            sampleTable.Columns.Add("id_user", typeof(int));
            sampleTable.Columns.Add("amount", typeof(decimal));
            sampleTable.Columns.Add("currency", typeof(string));
            sampleTable.Columns.Add("date_taken", typeof(DateTime));
            sampleTable.Columns.Add("date_paid", typeof(DateTime));
            sampleTable.Columns.Add("tax_percentage", typeof(decimal));
            sampleTable.Columns.Add("number_months", typeof(int));
            sampleTable.Columns.Add("loan_state", typeof(string));

            sampleTable.Rows.Add(2, userId, 3000, "USD", DateTime.Now, DBNull.Value, 5, 12, "unpaid");
            sampleTable.Rows.Add(3, userId, 4000, "USD", DateTime.Now, DBNull.Value, 5, 12, "unpaid");
            
            dataLinkMock
                .Setup(dl => dl.ExecuteReader("GetLoansByUserId", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(sampleTable);

            var loans = await loanRepository.GetLoansByUserId(userId);
            var expectedCount = 2;

            Assert.Equal(2, loans[0].LoanID);
            Assert.Equal(userId, loans[0].UserID);
            Assert.Equal(loans.Count(),expectedCount);
        }
        [Fact]
        public async Task GetLoansByUserId_ShouldReturnEmptyList_WhenNoDataReturned()
        {
            int userId = 1;

            dataLinkMock
                .Setup(dl => dl.ExecuteReader("GetLoansByUserId", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(new DataTable()); 

            var loans = await loanRepository.GetLoansByUserId(userId);

            Assert.Empty(loans);
        }

        [Fact]
        public async Task GetLoansByUserId_ShouldReturnEmptyList_WhenExceptionOccurs()
        {
            int userId = 1;
            dataLinkMock
                .Setup(dl => dl.ExecuteReader("GetLoansByUserId", It.IsAny<SqlParameter[]>()))
                .ThrowsAsync(new Exception("Database error"));

            var loans = await loanRepository.GetLoansByUserId(userId);

            Assert.Empty(loans);
        }
        [Fact]
        public async Task GetLoanById_ShouldReturnLoan_WhenLoanExists()
        {
            var sampleTable = new DataTable();
            var loanId = 1;
            sampleTable.Columns.Add("id_loan", typeof(int));
            sampleTable.Columns.Add("id_user", typeof(int));
            sampleTable.Columns.Add("amount", typeof(decimal));
            sampleTable.Columns.Add("currency", typeof(string));
            sampleTable.Columns.Add("date_taken", typeof(DateTime));
            sampleTable.Columns.Add("date_paid", typeof(DateTime));
            sampleTable.Columns.Add("tax_percentage", typeof(decimal));
            sampleTable.Columns.Add("number_months", typeof(int));
            sampleTable.Columns.Add("loan_state", typeof(string));

            sampleTable.Rows.Add(loanId, 2, 1000, "USD", DateTime.Now, DBNull.Value, 5, 12, "unpaid");

            dataLinkMock
                .Setup(dl => dl.ExecuteReader("GetLoanById", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(sampleTable);

            var loan = await loanRepository.GetLoanById(loanId);

            Assert.NotNull(loan);
            Assert.Equal(loanId, loan?.LoanID);
        }

        [Fact]
        public async Task GetLoanById_ShouldReturnNull_WhenLoanDoesNotExist()
        {

            int loanId = 1;
            dataLinkMock
                .Setup(dl => dl.ExecuteReader("GetLoanById", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(new DataTable()); 

            var loan = await loanRepository.GetLoanById(loanId);

            Assert.Null(loan);
        }

        [Fact]
        public async Task GetLoanById_ShouldReturnNull_WhenExceptionOccurs()
        {
            int loanId = 1;
            dataLinkMock
                .Setup(dl => dl.ExecuteReader("GetLoanById", It.IsAny<SqlParameter[]>()))
                .ThrowsAsync(new Exception("Database error"));

            var loan = await loanRepository.GetLoanById(loanId);

            Assert.Null(loan);
        }
        [Fact]
        public async Task UpdateLoan_ShouldReturnTrue_WhenUpdateIsSuccessful()
        {
            var loan = new Loan(1, 1, 1000, "USD", DateTime.Now, null, 5, 12, "unpaid");
            var parameters = new SqlParameter[]
                {
                    new SqlParameter("@id_loan", loan.LoanID),
                    new SqlParameter("@amount", loan.Amount),
                    new SqlParameter("@currency", loan.Currency),
                    new SqlParameter("@date_deadline", loan.DateDeadline),
                    new SqlParameter("@date_paid", loan.DatePaid),
                    new SqlParameter("@tax_percentage", loan.TaxPercentage),
                    new SqlParameter("@loan_state", loan.State)
                };

            dataLinkMock
                .Setup(dl => dl.ExecuteNonQuery("UpdateLoan", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(1); 

            var result = await loanRepository.UpdateLoan(loan);

            Assert.True(result);
        }

        [Fact]
        public async Task UpdateLoan_ShouldReturnFalse_WhenNoRowsAffected()
        {
            var loan = new Loan(1, 1, 1000, "USD", DateTime.Now, null, 5, 12, "unpaid");

            dataLinkMock
                .Setup(dl => dl.ExecuteNonQuery("UpdateLoan", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(0); 

            var result = await loanRepository.UpdateLoan(loan);

            Assert.False(result);
        }

        [Fact]
        public async Task UpdateLoan_ShouldReturnFalse_WhenExceptionOccurs()
        {

            var loan = new Loan(1, 1, 1000, "USD", DateTime.Now, null, 5, 12, "unpaid");

            dataLinkMock
                .Setup(dl => dl.ExecuteNonQuery("UpdateLoan", It.IsAny<SqlParameter[]>()))
                .ThrowsAsync(new Exception("Database error"));

            var result = await loanRepository.UpdateLoan(loan);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteLoan_ShouldReturnTrue_WhenDeletionIsSuccessful()
        {
            int loanId = 1;
            var parameters = new SqlParameter[] { new SqlParameter("@id_loan", loanId) };

            dataLinkMock
                .Setup(dl => dl.ExecuteNonQuery("DeleteLoan", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(1); 

            var result = await loanRepository.DeleteLoan(loanId);

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteLoan_ShouldReturnFalse_WhenNoRowsAffected()
        {
            int loanId = 1;

            dataLinkMock
                .Setup(dl => dl.ExecuteNonQuery("DeleteLoan", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(0); 

            var result = await loanRepository.DeleteLoan(loanId);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteLoan_ShouldReturnFalse_WhenExceptionOccurs()
        {
            int loanId = 1;

            dataLinkMock
                .Setup(dl => dl.ExecuteNonQuery("DeleteLoan", It.IsAny<SqlParameter[]>()))
                .ThrowsAsync(new Exception("Database error"));

            var result = await loanRepository.DeleteLoan(loanId);

            Assert.False(result);
        }

        [Fact]
        public async Task GetBankAccountsByUserId_ReturnsCorrectAccounts()
        {
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

            dataTable.Rows.Add("IBAN123", "USD", 2000m, false, 1, "Test Account", 500m, 1000m, 5);

            dataLinkMock.Setup(d => d.ExecuteReader("GetBankAccountsByUserId", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(dataTable);

            var repo = new LoanRepository(dataLinkMock.Object);

            var result = await repo.GetBankAccountsByUserId(userId);

            Assert.Single(result);
            Assert.Equal("IBAN123", result[0].Iban);

        }

        [Fact]
        public async Task GetBankAccountByIBAN_ReturnsCorrectAccount()
        {
            string iban = "IBAN123";

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

            dataTable.Rows.Add("IBAN123", "USD", 1500m, false, 1, "Savings", 300m, 1000m, 4);

            dataLinkMock.Setup(dl => dl.ExecuteReader("GetBankAccountByIBAN", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(dataTable);

            var repo = new LoanRepository(dataLinkMock.Object);

            var result = await repo.GetBankAccountByIBAN(iban);

            Assert.NotNull(result);
            Assert.Equal("IBAN123", result.Iban);
            Assert.Equal("USD", result.Currency);
          
        }
        [Fact]
        public async Task UpdateBankAccountBalance_ReturnsTrue_WhenUpdateSucceeds()
        {
            string iban = "IBAN123";
            decimal amount = 1000m;

            dataLinkMock.Setup(dl => dl.ExecuteNonQuery("UpdateBankAccountBalance", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(1);

            var repo = new LoanRepository(dataLinkMock.Object);

            var result = await repo.UpdateBankAccountBalance(iban, amount);

            Assert.True(result);
        }
        [Fact]
        public async Task UpdateBankAccountBalance_ReturnsFalse_WhenExceptionThrown()
        {
            string iban = "IBAN123";
            decimal amount = 1000m;

            dataLinkMock.Setup(dl => dl.ExecuteNonQuery("UpdateBankAccountBalance", It.IsAny<SqlParameter[]>()))
                .ThrowsAsync(new Exception("Database error"));

            var repo = new LoanRepository(dataLinkMock.Object);
            var result = await repo.UpdateBankAccountBalance(iban, amount);
            Assert.False(result);
        }

        [Fact]
        public async Task GetAllCurrencyExchanges_ReturnsCurrencyList_WhenDataAvailable()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("from_currency");
            dataTable.Columns.Add("to_currency");
            dataTable.Columns.Add("rate", typeof(decimal));

            var row = dataTable.NewRow();
            row["from_currency"] = "USD";
            row["to_currency"] = "EUR";
            row["rate"] = 0.85m;
            dataTable.Rows.Add(row);

            dataLinkMock.Setup(dl => dl.ExecuteReader("GetAllCurrencyExchanges", It.IsAny<SqlParameter[]>()))
                .ReturnsAsync(dataTable);

            var repo = new LoanRepository(dataLinkMock.Object);

            var result = await repo.GetAllCurrencyExchanges();

            Assert.Single(result);
            Assert.Equal("USD", result[0].FromCurrency);
            Assert.Equal("EUR", result[0].ToCurrency);
            Assert.Equal(0.85m, result[0].ExchangeRate);
        }
        [Fact]
        public async Task GetAllCurrencyExchanges_ReturnsEmptyList_WhenExceptionThrown()
        {
            dataLinkMock.Setup(dl => dl.ExecuteReader("GetAllCurrencyExchanges", It.IsAny<SqlParameter[]>()))
                .ThrowsAsync(new Exception("Some DB error"));

            var repo = new LoanRepository(dataLinkMock.Object);

            var result = await repo.GetAllCurrencyExchanges();

            Assert.Empty(result);
        }






    }

}
