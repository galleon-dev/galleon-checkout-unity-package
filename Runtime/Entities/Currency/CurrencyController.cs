using System;
using System.Globalization;
using System.Linq;

namespace Galleon.Checkout
{
    public class CurrencyController : Entity
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Lifecycle

        public Step Initialize()
        => 
            new Step(name   : "initialize_currency"
                    ,tags   : new[] { "init" }
                    ,action : async s =>
                    {
                        // Initialize currency controller
                    });

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Public API

        public string FormatMoney(decimal amount, string currency = null)
        {
            var currencyCode    = ResolveCurrencyCode(currency);
            var amountText      = amount.ToString("0.00", CultureInfo.CurrentCulture);
            var showCurrencySign = CHECKOUT.Globals.ShowCurrencySign;

            // Code mode keeps a separator (e.g. "USD 1.00").
            if (!showCurrencySign)
                return string.IsNullOrEmpty(currencyCode) ? amountText : $"{currencyCode} {amountText}";

            // Sign mode has no separator (e.g. "$1.00").
            var currencySign = GetCurrencySign(currencyCode);
            return string.IsNullOrEmpty(currencySign) ? amountText : $"{currencySign}{amountText}";
        }

        public string GetCurrencySign(string currency)
        {
            if (string.IsNullOrEmpty(currency))
                return currency;

            var currencyCode = currency.ToUpperInvariant();
            var configKey = $"currency_sign_{currencyCode}";

            // Runtime config override has highest priority (e.g. currency_sign_USD = "!").
            var configuredSign = CHECKOUT.Config.GetString(configKey, defaultValue: null);
            if (!string.IsNullOrEmpty(configuredSign))
                return configuredSign;

            // Fast-path map for common currencies.
            var knownSign = currencyCode switch
            {
                "USD" => "$",
                "EUR" => "€",
                "GBP" => "£",
                "JPY" => "¥",
                "CNY" => "¥",
                "HKD" => "HK$",
                "SGD" => "S$",
                "NZD" => "NZ$",
                "INR" => "₹",
                "CAD" => "C$",
                "AUD" => "A$",
                "CHF" => "CHF ",
                "KRW" => "₩",
                "VND" => "₫",
                "THB" => "฿",
                "PHP" => "₱",
                "IDR" => "Rp ",
                "MYR" => "RM ",
                "TWD" => "NT$",
                "BRL" => "R$",
                "MXN" => "MX$",
                "ARS" => "$",
                "CLP" => "$",
                "COP" => "$",
                "PEN" => "S/",
                "RUB" => "₽",
                "TRY" => "₺",
                "ILS" => "₪",
                "SEK" => "kr ",
                "NOK" => "kr ",
                "DKK" => "kr ",
                "ISK" => "kr ",
                "PLN" => "zł ",
                "CZK" => "Kč ",
                "HUF" => "Ft ",
                "RON" => "lei ",
                "BGN" => "лв ",
                "UAH" => "₴",
                "ZAR" => "R ",
                "AED" => "د.إ ",
                "SAR" => "﷼",
                "QAR" => "ر.ق ",
                "KWD" => "د.ك ",
                "BHD" => ".د.ب ",
                "OMR" => "ر.ع. ",
                "EGP" => "E£",
                "MAD" => "د.م. ",
                "NGN" => "₦",
                _     => null
            };

            if (!string.IsNullOrEmpty(knownSign))
                return knownSign;

            // Fallback: derive sign from region metadata by ISO currency code.
            try
            {
                var region = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                                        .Where(culture => !string.IsNullOrEmpty(culture.Name))
                                        .Select(culture =>
                                        {
                                            try { return new RegionInfo(culture.Name); }
                                            catch { return null; }
                                        })
                                        .FirstOrDefault(r => r != null && r.ISOCurrencySymbol.Equals(currencyCode, StringComparison.OrdinalIgnoreCase));

                if (region != null && !region.CurrencySymbol.Equals(currencyCode, StringComparison.OrdinalIgnoreCase))
                    return region.CurrencySymbol;
            }
            catch
            {
                // Fallback to currency code
            }

            return currency;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Helper Methods

        private string ResolveCurrencyCode(string currency)
        {
            if (!string.IsNullOrEmpty(currency))
                return currency.ToUpperInvariant();

            return CHECKOUT.Session?.SelectedProduct?.Currency?.ToUpperInvariant();
        }
    }
}