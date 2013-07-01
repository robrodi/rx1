namespace Rx1
{
    public static class Extensions
    {
        public static bool IsTrue(this bool? value)
        {
            return value.HasValue && value.Value;
        }
    }
}