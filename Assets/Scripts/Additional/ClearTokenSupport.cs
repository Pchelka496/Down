using System.Threading;

namespace Additional
{
    public static class ClearTokenSupport
    {
        public static void ClearToken(ref CancellationTokenSource cts)
        {
            if (cts == null) return;

            if (!cts.IsCancellationRequested)
            {
                cts.Cancel();
            }

            cts.Dispose();
            cts = null;
        }
    }
}