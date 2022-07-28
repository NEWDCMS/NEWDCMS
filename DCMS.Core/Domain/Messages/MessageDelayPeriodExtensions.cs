using System;
namespace DCMS.Core.Domain.Messages
{

    public static class MessageDelayPeriodExtensions
    {

        public static int ToHours(this MessageDelayPeriod period, int value)
        {
            switch (period)
            {
                case MessageDelayPeriod.Hours:
                    return value;
                case MessageDelayPeriod.Days:
                    return value * 24;
                default:
                    throw new ArgumentOutOfRangeException(nameof(period));
            }
        }
    }
}