﻿using System;
using System.Collections.Generic;
using System.Linq;
using Merchello.Core;
using Merchello.Core.Models;
using Merchello.Core.Models.Interfaces;
using Merchello.Web.Cache;
using Umbraco.Core;

namespace Merchello.Web.Shipping.Gateway.FlatRate
{
    public class ShipRateTable : IShipRateTable
    {
        private readonly List<IShipRateTier> _shipRateTiers;
        private readonly Guid _shipMethodKey;

        public ShipRateTable(Guid shipMethodKey)
            : this(shipMethodKey, new List<IShipRateTier>())
        { }

        internal ShipRateTable(Guid shipMethodKey, IEnumerable<IShipRateTier> rows)
        {
            var shipRateTiers = rows as IShipRateTier[] ?? rows.ToArray();

            Mandate.ParameterCondition(shipMethodKey != Guid.Empty, "shipMethodKey");
            Mandate.ParameterNotNull(shipRateTiers, "rows");
            

            _shipMethodKey = shipMethodKey;
            _shipRateTiers = new List<IShipRateTier>();
            _shipRateTiers.AddRange(shipRateTiers);

        }

        /// <summary>
        /// Retrieves 
        /// </summary>
        /// <param name="shipMethodKey">The 'unique' ShipMethodKey of the <see cref="IShipMethod"/> associated</param>
        /// <returns></returns>
        public static ShipRateTable GetShipRateTable(Guid shipMethodKey)
        {
            var context = MerchelloContext.Current;

            return (ShipRateTable)context.Cache
                .RequestCache.GetCacheItem(CacheKeys.ShipRateTableCacheKey(shipMethodKey), 
                () => GetShipRateTable(MerchelloContext.Current, shipMethodKey));
        }

        internal static ShipRateTable GetShipRateTable(IMerchelloContext merchelloContext, Guid shipMethodKey)
        {
            var rows = merchelloContext.Services.GatewayProviderService.GetShipRateTiersByShipMethodKey(shipMethodKey);
            return new ShipRateTable(shipMethodKey, rows);
        }

        /// <summary>
        /// The 'unique' ShipMethodKey of the ship method associated with the <see cref="IShipRateTable"/>
        /// </summary>
        public Guid ShipMethodKey {
            get
            {
                return _shipMethodKey;
            }
        }

        /// <summary>
        /// Adds a rate tier row to the ship rate table
        /// </summary>
        /// <param name="rangeLow">The lowest qualifying value defining the range</param>
        /// <param name="rangeHigh">The highest qualifying value defining the range</param>
        /// <param name="rate">The rate or cost assoicated with the range</param>
        /// <remarks>
        /// Requires a call to Save() to persist
        /// </remarks>
        public void AddRow(decimal rangeLow, decimal rangeHigh, decimal rate)
        {
            AddRow(new ShipRateTier(_shipMethodKey)
                    {
                        RangeLow = rangeLow,
                        RangeHigh = rangeHigh,
                        Rate = rate
                    });
        }

        /// <summary>
        /// Adds a rate tier row to the ship rate table
        /// </summary>
        /// <param name="shipRateTier"></param>
        /// <remarks>
        /// Requires a call to Save() to persist
        /// </remarks>
        internal void AddRow(IShipRateTier shipRateTier)
        {
            if (!ValidateRateTier(ref shipRateTier)) return;
                        
            // TODO : Refactor this validation
            if (!Rows.Any())
            {
                shipRateTier.RangeLow = 0;
                _shipRateTiers.Add(shipRateTier);
            }
            else
            {
                // confirm there is not already a matching tier
                if(Rows.FirstOrDefault(x => x.RangeLow == shipRateTier.RangeLow && x.RangeHigh == shipRateTier.RangeHigh) != null) return;
                
                // find the insertion point
                var index = Rows.IndexOf(Rows.Where(y => y.RangeHigh >= shipRateTier.RangeLow).OrderBy(z => z.RangeLow).FirstOrDefault());
                if (index < 0)
                {
                    shipRateTier.RangeLow = Rows.Last().RangeHigh;
                    AddRow(shipRateTier);
                    return;
                }

                // not found or at the end of the table
                if (index < 0 || index == Rows.IndexOf(Rows.Last()))
                {
                    shipRateTier.RangeLow = Rows.Last().RangeHigh;
                    if (shipRateTier.RangeHigh <= shipRateTier.RangeLow) return;
                    _shipRateTiers.Add(shipRateTier);
                }
                else // insert in the middle of the table
                {
                    // verify that inserting this tier will not create a span encapsulating another tier
                    if (shipRateTier.RangeHigh >= _shipRateTiers[index + 1].RangeHigh) return;
                    if (shipRateTier.RangeLow <= _shipRateTiers[index].RangeLow) return;

                    // match the range low to range high in the following tier
                    _shipRateTiers[index + 1].RangeLow = shipRateTier.RangeHigh;

                    // verify that the high value at the current index is equal to the low value of the tier to be insert
                    _shipRateTiers[index].RangeHigh = shipRateTier.RangeLow;

                    _shipRateTiers.Insert(index + 1, shipRateTier);

                }
            }
 
        }

        private static bool ValidateRateTier(ref IShipRateTier shipRateTier)
        {
            if (shipRateTier.RangeLow < 0 || shipRateTier.RangeHigh < 0) return false;
            if (shipRateTier.RangeLow == shipRateTier.RangeHigh) return false;
            if (shipRateTier.RangeHigh > shipRateTier.RangeLow) return true;

            var temp = shipRateTier.RangeLow;
            shipRateTier.RangeLow = shipRateTier.RangeHigh;
            shipRateTier.RangeHigh = temp;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shipRateTier"></param>
        /// <remarks>
        /// Requires a call to save to persist
        /// </remarks>
        public void DeleteRow(IShipRateTier shipRateTier)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Persists the rate table to the database and refreshes the runtime cache
        /// </summary>
        public void Save()
        {
            Save(MerchelloContext.Current, this);
        }

        internal static void Save(IMerchelloContext merchelloContext, IShipRateTable rateTable)
        {
            var cache = merchelloContext.Cache.RequestCache;

            // clear the current cached item
            cache.ClearCacheItem(CacheKeys.ShipRateTableCacheKey(rateTable.ShipMethodKey));

            // persist and enter into cache
            merchelloContext.Services.GatewayProviderService.Save(rateTable.Rows);
            cache.GetCacheItem(CacheKeys.ShipRateTableCacheKey(rateTable.ShipMethodKey), () => rateTable);   
        }

        /// <summary>
        /// Gets the decimal rate associated with the range
        /// </summary>
        /// <param name="rangeValue">The value within a range used to determine which rate to return</param>
        /// <returns>A decimal rate or zero (0) if not found</returns>
        public decimal GetRate(decimal rangeValue)
        {
            var rateTier = Rows.FirstOrDefault(x => x.RangeLow < rangeValue && x.RangeHigh >= rangeValue);
            return rateTier == null ? 0M : rateTier.Rate;
        }

        /// <summary>
        /// The rows of the rate table
        /// </summary>
        public IEnumerable<IShipRateTier> Rows
        {
            get { return _shipRateTiers.OrderBy(x => x.RangeLow); }

        }
    }
}