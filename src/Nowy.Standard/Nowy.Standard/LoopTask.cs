using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Nowy.Standard;

public sealed class LoopTask : IDisposable
{
    private CancellationTokenSource _cts_background_task = new();
    private Stopwatch? _step_started_stopwatch;
    private Stopwatch? _step_stopped_stopwatch;

    public TimeSpan? DurationSinceStart => this._step_started_stopwatch?.Elapsed;
    public TimeSpan? DurationSinceStop => this._step_stopped_stopwatch?.Elapsed;

    public void Dispose()
    {
        _cts_background_task?.Dispose();
        _cts_background_task = null;
    }

    private LoopTask(TimeSpan delay, Func<Task> func)
    {
        Task.Run(async () =>
        {
            try
            {
                while (!_cts_background_task.IsCancellationRequested)
                {
                    try
                    {
                        _step_started_stopwatch = Stopwatch.StartNew();
                        await func();
                    }
                    finally
                    {
                        _step_stopped_stopwatch = Stopwatch.StartNew();
                    }

                    await Task.Delay(delay).ConfigureAwait(false);
                }
            }
            finally
            {
                this.Dispose();
            }
        }).Forget();
    }

    public static LoopTask Create(TimeSpan delay, Func<Task> func)
    {
        return new LoopTask(delay, func);
    }

    public static LoopTask Create(TimeSpan delay, Action action)
    {
        return new LoopTask(delay, () =>
        {
            action();
            return Task.CompletedTask;
        });
    }
}
