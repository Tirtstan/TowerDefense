using System;

public static class FormatUtils
{
    /// <summary>
    /// Formats the given time in seconds into a string representation.
    /// </summary>
    /// <param name="time">The time in seconds.</param>
    /// <returns>A formatted string representing the time.</returns>
    /// <remarks>This method formats the time as "MM:SS".</remarks>
    public static string FormatTime(float time)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);
        return string.Format("{0:D2}:{1:D2}", (int)timeSpan.TotalMinutes, timeSpan.Seconds);
    }
}
