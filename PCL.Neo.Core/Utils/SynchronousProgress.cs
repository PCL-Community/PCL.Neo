namespace PCL.Neo.Core.Utils;

public class SynchronousProgress<T>(Action<T> action) : IProgress<T>
{
    public void Report(T value)
    {
        action(value);
    }
}
