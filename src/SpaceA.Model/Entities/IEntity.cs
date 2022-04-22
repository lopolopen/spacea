namespace SpaceA.Model
{
    public interface IEntity<TId>
    {
        TId Id { get; set; }
    }

    public interface IEntity<TId1, TId2>
    {
        TId1 Id1 { get; set; }

        TId2 Id2 { get; set; }
    }
}