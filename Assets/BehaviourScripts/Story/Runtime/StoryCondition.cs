public abstract class StoryCondition
{
    public bool IsTrue { get; protected set; }

    public abstract void initialize();
    public abstract void Dispose();
}