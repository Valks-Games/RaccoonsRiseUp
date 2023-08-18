namespace RRU;

public static class Utils
{
    /// <summary>
    /// Show two decimal places if the amount is less than or equal to
    /// 10, otherwise if greater than 10 do not show any decimals.
    /// </summary>
    public static string RoundGameValue(double amount) => amount <= 10 ?
                $"{amount:0.00}" : $"{amount:0}";
}
