namespace CourseApp.ServiceLayer.Utilities.Result;

public class Result : IResult
{
    public bool Success {  get; }

    public string Message { get; }

    public Result(bool Success)
    {
        Success = Success;        
    }
    public Result(bool isSuccess,string message):this(isSuccess)
    {
        Message = message;
    }
}
