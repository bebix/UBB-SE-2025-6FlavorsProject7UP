CREATE OR ALTER PROCEDURE GetBankAccountBalanceByIban
    @iban NVARCHAR(50)
AS
BEGIN
    SELECT amount, currency FROM bank_accounts WHERE iban=@iban;
END
