namespace TechWrite.Web.Services;

public interface IRateLimitService
{
    bool IsRateLimited(string ipAddress);
    void RecordSubmission(string ipAddress);
}
