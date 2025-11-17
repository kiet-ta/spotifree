using Spotifree.IServices;
using System.Windows.Threading;

namespace Spotifree.Services;

public class FocusTimerService : IFocusTimerService
{
    private DispatcherTimer _timer;
    private TimeSpan _remainingTime;

    public event Action<TimeSpan> TimerTick;

    public event Action TimerFinished;

    public bool IsRunning => _timer.IsEnabled;

    public FocusTimerService()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += OnTimerTick;
    }

    public void StartTimer(TimeSpan duration)
    {
        _remainingTime = duration;
        _timer.Start();
        TimerTick?.Invoke(_remainingTime);
    }

    public void StopTimer()
    {
        _timer.Stop();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        _remainingTime = _remainingTime.Subtract(TimeSpan.FromSeconds(1));
        TimerTick?.Invoke(_remainingTime);

        if (_remainingTime <= TimeSpan.Zero)
        {
            _timer.Stop();
            TimerFinished?.Invoke();
        }
    }
}