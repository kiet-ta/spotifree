using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spotifree.IServices;

public interface IFocusTimerService
{
    event Action<TimeSpan> TimerTick;
    event Action TimerFinished;
    bool IsRunning { get; }
    void StartTimer(TimeSpan duration);
    void StopTimer();
}
