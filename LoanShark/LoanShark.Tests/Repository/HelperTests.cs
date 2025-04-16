using LoanShark.Domain;
using LoanShark.Helper;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LoanShark.Tests.Repository
{
    public class HelperTests
    {
        [Fact]
        public void BankAccountDisplayConverter_WithName_ReturnsName()
        {
            var converter = new BankAccountDisplayConverter();
            var account = new BankAccount("RO49AAAA", "EUR", 1000, false, 1, "Main Account", 500, 200, 5);

            var result = converter.Convert(account, null, null, null);

            Assert.Equal("Main Account", result);
        }

        [Fact]
        public void BankAccountDisplayConverter_WithoutName_ReturnsIbanAndCurrency()
        {
            var converter = new BankAccountDisplayConverter();
            var account = new BankAccount("RO49AAAA", "EUR", 1000, false, 1, "", 500, 200, 5);

            var result = converter.Convert(account, null, null, null);

            Assert.Equal("RO49AAAA (EUR)", result);
        }

        [Fact]
        public void ListEmptyToVisibilityConverter_WhenZero_ReturnsVisible()
        {
            var converter = new ListEmptyToVisibilityConverter();

            var result = converter.Convert(0, null, null, null);

            Assert.Equal(Visibility.Visible, result);
        }

        [Fact]
        public void ListEmptyToVisibilityConverter_WhenNonZero_ReturnsCollapsed()
        {
            var converter = new ListEmptyToVisibilityConverter();

            var result = converter.Convert(3, null, null, null);

            Assert.Equal(Visibility.Collapsed, result);
        }

        [Fact]
        public void ListNonEmptyToVisibilityConverter_WhenZero_ReturnsCollapsed()
        {
            var converter = new ListNonEmptyToVisibilityConverter();

            var result = converter.Convert(0, null, null, null);

            Assert.Equal(Visibility.Collapsed, result);
        }

        [Fact]
        public void ListNonEmptyToVisibilityConverter_WhenNonZero_ReturnsVisible()
        {
            var converter = new ListNonEmptyToVisibilityConverter();

            var result = converter.Convert(5, null, null, null);

            Assert.Equal(Visibility.Visible, result);
        }

        [Fact]
        public void RelayCommand_Execute_CallsAction()
        {
            bool wasCalled = false;
            var command = new RelayCommand(() => wasCalled = true);

            command.Execute(null);

            Assert.True(wasCalled);
        }

        [Fact]
        public void RelayCommand_CanExecute_ReturnsTrue()
        {
            var command = new RelayCommand(() => { });

            var result = command.CanExecute(null);

            Assert.True(result);
        }
    }


}
