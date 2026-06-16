public class EnrollmentWorker
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EnrollmentWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task ProcessBatch()
    {
         using var scope = _scopeFactory.CreateScope();

         var svc = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();

         var enrollments = await svc.GetAllAsync();

    }
}
public record EnrollmentRecord(
    string Id,
    string StudentId,
    string CourseCode,
    DateTime EnrolledAt);