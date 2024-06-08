namespace Anansi
{
    public interface IPrecondition
    {
        public bool CheckPrecondition(SimDateTime dateTime);
    }
}
