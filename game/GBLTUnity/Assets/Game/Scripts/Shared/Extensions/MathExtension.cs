namespace Shared.Extension
{
    public static class MathExtension
    {
        public static bool IsBetweenRange(this float thisValue, float lesser, float greater)
        {
            return thisValue >= lesser && thisValue <= greater;
        }
    }
}