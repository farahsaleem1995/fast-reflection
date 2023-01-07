namespace FastReflection;

public class RequestHandler
{
	public string Name { get; set; }

	public string Do()
	{
		return "Done";
	}

	public Task<int> Handle(int param, CancellationToken cancellation)
	{
		return Task.FromResult(param);
	}
}