namespace SYSS8.OPF.Clean.WebUi.Services;

public interface IUiOperations
{
    /// <summary>
    /// Kör en UI-åtgärd med konsekvent Busy/Error-hantering.
    /// Avbryt via CancellationToken (t.ex. vid Cancel-knapp).
    /// </summary>
    Task RunAsync(Func<CancellationToken, Task> work, CancellationToken ct = default);

    /// <summary>
    /// Variant som returnerar ett värde.
    /// </summary>
    Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken ct = default);
}
