namespace SYSS8.OPF.Clean.WebUi.Services;

public sealed class UiOperations : IUiOperations
{
    private readonly IUiStatus _uiStatus;

    public UiOperations(IUiStatus uiStatus)
    {
        ArgumentNullException.ThrowIfNull(uiStatus);
        _uiStatus = uiStatus;
    }

    public async Task RunAsync(Func<CancellationToken, Task> work, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(work);

        _uiStatus.Busy(true);
        _uiStatus.SetError(null);

        try
        {
            await work(ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Cancellation is not an error - just clear busy state
        }
        catch (Exception ex)
        {
            _uiStatus.SetError(new UiError("Operation failed", ex.Message));
        }
        finally
        {
            _uiStatus.Busy(false);
        }
    }

    public async Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(work);

        _uiStatus.Busy(true);
        _uiStatus.SetError(null);

        try
        {
            return await work(ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Cancellation is not an error - just clear busy state and rethrow
            throw;
        }
        catch (Exception ex)
        {
            _uiStatus.SetError(new UiError("Operation failed", ex.Message));
            throw;
        }
        finally
        {
            _uiStatus.Busy(false);
        }
    }
}
