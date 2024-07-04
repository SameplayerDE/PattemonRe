using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace TestRender;

public enum TimeOfDay
{
    Night,
    Morning,
    Day
}

public struct TimePeriod
{
    public TimeSpan From;
    public TimeSpan To;
    public string Name;
    public TimeOfDay TimeOfDay;
}

public class TimePeriodChangedEventArgs(TimePeriod previousPeriod, TimePeriod currentPeriod) : EventArgs
{
    public TimePeriod PreviousPeriod { get; } = previousPeriod;
    public TimePeriod CurrentPeriod { get; } = currentPeriod;
}

public class WorldTimeManager
{
    public static TimePeriod Morning = new() { From = new TimeSpan(4, 0, 0), To = new TimeSpan(9, 59, 59), Name = "Morning", TimeOfDay = TimeOfDay.Morning};
    public static TimePeriod Day = new() { From = new TimeSpan(10, 0, 0), To = new TimeSpan(16, 59, 59), Name = "Day", TimeOfDay = TimeOfDay.Day};
    public static TimePeriod Evening = new() { From = new TimeSpan(17, 0, 0), To = new TimeSpan(19, 59, 59), Name = "Evening", TimeOfDay = TimeOfDay.Day};
    public static TimePeriod Night = new() { From = new TimeSpan(20, 0, 0), To = new TimeSpan(3, 59, 59), Name = "Night", TimeOfDay = TimeOfDay.Night};

    public event EventHandler<TimePeriodChangedEventArgs> TimePeriodChanged;
    
    private readonly List<TimePeriod> _timePeriods;
    private TimeSpan _lastCheck;
    private TimeSpan _checkPeriod = TimeSpan.FromSeconds(1);
    
    public TimePeriod CurrentPeriod { get; private set; }
    public TimeSpan CurrentTime { get; private set; }

    public WorldTimeManager()
    {
        _timePeriods = [Morning, Day, Evening, Night];
    }

    public void Update(GameTime gameTime)
    {
        if (gameTime.TotalGameTime - _lastCheck <= _checkPeriod)
        {
            return;
        }
        _lastCheck = gameTime.TotalGameTime;
        CurrentTime = DateTime.Now.TimeOfDay;
        CheckAndTriggerEvents();
    }

    private void CheckAndTriggerEvents()
    {
        foreach (var period in _timePeriods)
        {
            if (!IsTimeInPeriod(CurrentTime, period))
            {
                continue;
            }
            if (CurrentPeriod.Name != period.Name)
            {
                var previousPeriod = CurrentPeriod;
                CurrentPeriod = period;
                OnTimePeriodChanged(previousPeriod, CurrentPeriod);
            }
            break;
        }
    }

    private static bool IsTimeInPeriod(TimeSpan currentTime, TimePeriod period)
    {
        if (period.From <= period.To)
        {
            return currentTime >= period.From && currentTime <= period.To;
        }

        return currentTime >= period.From || currentTime <= period.To;
    }

    protected virtual void OnTimePeriodChanged(TimePeriod previousPeriod, TimePeriod currentPeriod)
    {
        TimePeriodChanged?.Invoke(this, new TimePeriodChangedEventArgs(previousPeriod, currentPeriod));
    }
}