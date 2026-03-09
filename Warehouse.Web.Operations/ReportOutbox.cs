namespace Warehouse.Web.Operations;

internal class ReportOutbox
{
    public long Id { get; private set; }
    public string Payload { get; private set; } = string.Empty;
    public DateTime CreateDate { get; private set; } = DateTime.UtcNow;
    public DateTime? ProcessedDate { get; private set; }
    public int AttemptCount { get; private set; }
    public string? LastError { get; private set; }

    private ReportOutbox() { }

    public static ReportOutbox FromPayload(string payload) =>
        new()
        {
            Payload = payload
        };

    public void MarkProcessed() => ProcessedDate = DateTime.UtcNow;

    public void MarkFailed(string error)
    {
        AttemptCount++;
        LastError = error;
    }
}
