using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Threading.Tasks.Dataflow;
using CancellationTokenSource = System.Threading.CancellationTokenSource;

namespace PCL.Neo.Core.Download;

public class Downloader(int degreeOfParallelism = 8) : IDisposable
{
    public bool IsCancelled => _cancellationTokenSource.IsCancellationRequested;

    private readonly HttpClient _client = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly List<DownloadReceipt> _downloadReceipts = [];
    private readonly ConcurrentQueue<(DateTime, long)> _transferRateRecords = [];
    private readonly int _maxTransferRateRecordSize = degreeOfParallelism * 256;

    private IDisposable? _transferRateSubscription;

    public double Progress
    {
        get
        {
            var a = _downloadReceipts.Sum(x => x.Size);
            var b = _downloadReceipts.Sum(x => x.TotalSize != 0 ? x.TotalSize : x.Integrity?.ExpectedSize ?? 0);
            return b == 0 ? 0 : (double)a / b;
        }
    }

    public long TransferRate => _transferRateRecords
        .Where(x => DateTime.Now - x.Item1 <= TimeSpan.FromSeconds(1))
        .Sum(x => x.Item2);

    public async Task Download<T>(T receipts) where T : IEnumerable<DownloadReceipt>
    {
        var immutableReceipts = receipts.ToImmutableArray();
        _downloadReceipts.AddRange(immutableReceipts);

        _transferRateSubscription?.Dispose();
        _transferRateSubscription = immutableReceipts.Select(r =>
                Observable.FromEvent<Action<DownloadReceipt, long>, long>(
                    h => (_, x) => h(x),
                    h => r.OnDeltaSizeChanged += h,
                    h => r.OnDeltaSizeChanged -= h))
            .Merge()
            .Buffer(TimeSpan.FromMilliseconds(10))
            .Subscribe(list =>
            {
                _transferRateRecords.Enqueue((DateTime.Now, list.Sum()));
                while (_transferRateRecords.Count >= _maxTransferRateRecordSize)
                {
                    _transferRateRecords.TryDequeue(out _);
                }
            });

        var receiptExecutor = new ActionBlock<DownloadReceipt>(
            async r =>
                await r.DownloadAsync(_client, false, _cancellationTokenSource.Token),
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = degreeOfParallelism,
                CancellationToken = _cancellationTokenSource.Token
            });

        foreach (var r in immutableReceipts)
        {
            receiptExecutor.Post(r);
        }

        receiptExecutor.Complete();

        try
        {
            await receiptExecutor.Completion.WaitAsync(CancellationToken.None);
        }
        catch (OperationCanceledException) {}
    }

    public void Cancel()
    {
        _cancellationTokenSource.Cancel();
    }

    public async Task CancelAsync()
    {
        await _cancellationTokenSource.CancelAsync();
    }

    public void Dispose()
    {
        _client.Dispose();
        _cancellationTokenSource.Dispose();
        _transferRateSubscription?.Dispose();
    }
}
