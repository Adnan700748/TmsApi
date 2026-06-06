public class EnrollmentWorker
{
    private readonly IEnrollmentService _service;

    public EnrollmentWorker(IEnrollmentService service)
    {
        _service = service;
    }

    public void ProcessBatch()
    {
        
    }
}