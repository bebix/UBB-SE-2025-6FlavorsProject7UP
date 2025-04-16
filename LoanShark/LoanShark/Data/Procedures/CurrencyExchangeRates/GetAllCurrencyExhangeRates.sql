create or alter procedure GetAllCurrencyExchangeRates
as
begin
	select * from currency_exchange_rates
end
