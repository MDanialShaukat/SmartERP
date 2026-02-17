using System;
using System.Globalization;
using System.Windows.Data;

namespace SmartERP.UI.Converters
{
    /// <summary>
    /// Formats the Payment Method column in the Billing grid:
    ///   Cash   → "Cash - {RecoveryPersonName}"  (or just "Cash" when no person assigned)
    ///   Other  → the payment method as-is
    ///
    /// Expects two bindings in order:
    ///   [0] PaymentMethod  (string)
    ///   [1] RecoveryPerson.PersonName  (string?, nullable nav-prop)
    /// </summary>
    public class PaymentMethodDisplayConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var method = values.Length > 0 ? values[0] as string : null;
            var recoveryName = values.Length > 1 ? values[1] as string : null;

            if (string.IsNullOrWhiteSpace(method))
                return string.Empty;

            if (method == "Cash")
                return string.IsNullOrWhiteSpace(recoveryName)
                    ? "Cash"
                    : $"Cash - {recoveryName}";

            return method;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
