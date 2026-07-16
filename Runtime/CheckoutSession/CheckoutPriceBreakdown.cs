using System.Collections.Generic;
using System.Globalization;
using Galleon.Checkout.Shared;

namespace Galleon.Checkout
{
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Tax Mode

    /// <summary>
    /// Debug-only override for how the client interprets <see cref="TaxItem.inclusive"/>.
    ///
    /// DontOverride is the only value that ever runs for a real user :
    /// it is the config default ("dont override"), and the value only changes if a human
    /// explicitly picks another option in the debug tool window.
    ///
    /// Overriding is display-only and cannot change what is charged :
    /// ChargeRequest carries a session_id, never an amount, so the server always charges
    /// the session it priced itself, regardless of what this client draws on screen.
    /// </summary>
    public enum TaxMode
    {
        DontOverride,   // use TaxItem.inclusive exactly as the server sent it
        ForceInclusive, // pretend every tax is     inclusive -> already inside the subtotal
        ForceAdded,     // pretend every tax is not inclusive -> added on top of the subtotal
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////// Price Breakdown

    /// <summary>
    /// The single place that decides what money the checkout UI is allowed to draw.
    ///
    /// Two kinds of tax, per the wire contract in CheckoutSharedModels :
    ///     inclusive = true    the tax is ALREADY part of the subtotal      -> do NOT add it again  (PL, most of EU)
    ///     inclusive = false   the tax sits OUTSIDE the subtotal            -> add it on top        (US, CA)
    ///
    /// Before this type existed, the UI summed every tax unconditionally, which turned
    /// Poland's tax-inclusive 29.99 PLN into a displayed 35.60 PLN while the server
    /// charged 29.99. See CheckoutPriceBreakdownTests.
    /// </summary>
    public class CheckoutPriceBreakdown
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Types

        public enum TotalSourceKind
        {
            Server,     // total came from price_data.total_price          -> authoritative, matches the charge
            Computed,   // total came from subtotal + added taxes          -> server prices were missing
            Override,   // total came from subtotal + added taxes          -> a debug tax_mode override is active
        }

        public class TaxLine
        {
            public string  Name;
            public decimal Amount;
            public bool    IsInclusive;

            /// <summary> Row label. Inclusive taxes are marked so subtotal/tax/total read coherently on screen. </summary>
            public string  DisplayName => IsInclusive ? $"{Name} (incl.)" : Name;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Constants

        /// <summary> Half a minor unit. Below this, server and client are considered to agree. </summary>
        private const decimal AGREEMENT_TOLERANCE = 0.005m;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Members

        public decimal         SubTotal        ;   // the line-item price. Contains InclusiveTax already.
        public decimal         InclusiveTax    ;   // sum of taxes already inside SubTotal.       Never added to Total.
        public decimal         AddedTax        ;   // sum of taxes outside SubTotal.              Always added to Total.
        public decimal         Total           ;   // what the user pays.

        public List<TaxLine>   Lines           = new();
        public TotalSourceKind TotalSource     ;
        public TaxMode         Mode            ;

        /// <summary> True when the server's own (total - subtotal) disagrees with the taxes we classified as added. </summary>
        public bool            HasMismatch     ;
        public decimal         ServerAddedTax  ;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Properties

        public bool            HasTaxes        => Lines.Count > 0;
        public bool            HasAddedTax     => AddedTax     > 0m;
        public bool            HasInclusiveTax => InclusiveTax > 0m;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Build

        /// <summary>
        /// Builds the breakdown from a priced session.
        /// </summary>
        /// <param name="priceData">        price_data as returned by /session. May be null.                        </param>
        /// <param name="taxes">            the session's per-tax data, keyed by tax name. May be null or empty.    </param>
        /// <param name="fallbackSubTotal"> used only when the server sent no usable prices (e.g. product amount).  </param>
        /// <param name="mode">             debug override. DontOverride in every real-user code path.              </param>
        public static CheckoutPriceBreakdown Build(PriceData                            priceData
                                                  ,IReadOnlyDictionary<string, TaxItem> taxes
                                                  ,decimal                              fallbackSubTotal
                                                  ,TaxMode                              mode = TaxMode.DontOverride)
        {
            var result = new CheckoutPriceBreakdown();
            result.Mode = mode;

            //////////////////////////////////////// 1. classify every tax by its (possibly overridden) inclusive flag

            if (taxes != null)
            {
                foreach (var pair in taxes)
                {
                    if (pair.Value == null)
                        continue;

                    var isInclusive = ResolveInclusive(pair.Value, mode);

                    result.Lines.Add(new TaxLine
                                     {
                                         Name        = pair.Key,
                                         Amount      = pair.Value.tax_amount,
                                         IsInclusive = isInclusive,
                                     });

                    if (isInclusive) result.InclusiveTax += pair.Value.tax_amount;
                    else             result.AddedTax     += pair.Value.tax_amount;
                }
            }

            //////////////////////////////////////// 2. resolve subtotal + total

            var hasServerPrices = priceData             != null
                               && priceData.subtotal_price > 0m
                               && priceData.total_price    > 0m;

            // The server is the authority: it prices the session and charges it without ever
            // asking the client for an amount. Its (total - subtotal) is coherent by construction,
            // so we display it and use our own classification only to cross-check.
            if (hasServerPrices && mode == TaxMode.DontOverride)
            {
                result.SubTotal       = priceData.subtotal_price;
                result.Total          = priceData.total_price;
                result.TotalSource    = TotalSourceKind.Server;
                result.ServerAddedTax = priceData.total_price - priceData.subtotal_price;
                result.HasMismatch    = Abs(result.ServerAddedTax - result.AddedTax) > AGREEMENT_TOLERANCE;
            }
            // No usable server prices, or a debug override is deliberately in play.
            // Fall back to arithmetic that at least respects the inclusive flag.
            else
            {
                result.SubTotal       = hasServerPrices ? priceData.subtotal_price : fallbackSubTotal;
                result.Total          = result.SubTotal + result.AddedTax;
                result.TotalSource    = mode == TaxMode.DontOverride ? TotalSourceKind.Computed
                                                                     : TotalSourceKind.Override;
                result.ServerAddedTax = hasServerPrices ? priceData.total_price - priceData.subtotal_price : 0m;
                result.HasMismatch    = false;
            }

            return result;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// Methods

        /// <summary> The effective inclusive flag for a tax, after applying any debug override. </summary>
        public static bool ResolveInclusive(TaxItem tax, TaxMode mode)
        {
            switch (mode)
            {
                case TaxMode.ForceInclusive : return true;
                case TaxMode.ForceAdded     : return false;
                default                     : return tax != null && tax.inclusive;
            }
        }

        /// <summary> Parses the "tax_mode" config value. Anything unrecognised means "dont override". </summary>
        public static TaxMode ParseTaxMode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return TaxMode.DontOverride;

            switch (value.Trim().ToLowerInvariant())
            {
                case "inclusive" : return TaxMode.ForceInclusive;
                case "added"     : return TaxMode.ForceAdded;
                default          : return TaxMode.DontOverride;
            }
        }

        /// <summary> Serialises a TaxMode back to its config value. </summary>
        public static string ToConfigValue(TaxMode mode)
        {
            switch (mode)
            {
                case TaxMode.ForceInclusive : return "inclusive";
                case TaxMode.ForceAdded     : return "added";
                default                     : return "dont override";
            }
        }

        private static decimal Abs(decimal value) => value < 0m ? -value : value;

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////// ToString

        override public string ToString()
        {
            var c = CultureInfo.InvariantCulture;
            return $"PriceBreakdown | subtotal:{SubTotal.ToString(c)} "
                 + $"inclusive_tax:{InclusiveTax.ToString(c)} "
                 + $"added_tax:{AddedTax.ToString(c)} "
                 + $"total:{Total.ToString(c)} "
                 + $"source:{TotalSource} "
                 + $"mode:{Mode}"
                 + (HasMismatch ? $" | MISMATCH server_added_tax:{ServerAddedTax.ToString(c)} vs classified_added_tax:{AddedTax.ToString(c)}" : "");
        }
    }
}
