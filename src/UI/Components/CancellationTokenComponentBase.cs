using Microsoft.AspNetCore.Components;

namespace LANdalf.UI.Components {
    public abstract class CancellationTokenComponentBase : ComponentBase, IDisposable {
        private CancellationTokenSource? cancellationTokenSource;

        public CancellationToken CancellationToken => (cancellationTokenSource ??= new()).Token;

        public virtual void Dispose() {
            if (cancellationTokenSource != null) {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }
    }
}
