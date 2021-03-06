using System;
using System.Collections.Generic;
using System.Linq;
using FChoice.Foundation;
using FChoice.Foundation.Clarify;

namespace Dovetail.SDK.Bootstrap.Clarify.Extensions
{
    public static class ClarifyDataRowExtensions
    {
        public static int DatabaseIdentifier(this ClarifyDataRow row)
        {
            return Convert.ToInt32(row.UniqueID);
        }

        public static string AsString(this ClarifyDataRow row, string fieldName)
        {
            return row[fieldName] == DBNull.Value ? null : row[fieldName].ToString();
        }

        public static int AsInt(this ClarifyDataRow row, string fieldName)
        {
            return row[fieldName] == DBNull.Value ? 0 : Convert.ToInt32(row[fieldName]);
        }

        public static DateTime AsDateTime(this ClarifyDataRow row, string fieldName)
        {
            return row[fieldName] == DBNull.Value ? FCGeneric.MIN_DATE : Convert.ToDateTime(row[fieldName].ToString());
        }

        public static IEnumerable<ClarifyDataRow> DataRows(this ClarifyGeneric generic)
        {
            return generic.Rows.Cast<ClarifyDataRow>();
        }
    }
}