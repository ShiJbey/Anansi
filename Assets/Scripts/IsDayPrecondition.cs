using Calypso;

public class IsDayPrecondition : IPrecondition
{
	public int Day { get; }

	public IsDayPrecondition(int day)
	{
		Day = day;
	}

	public bool CheckPrecondition(SimDateTime dateTime)
	{
		return dateTime.Day == Day;
	}
}
