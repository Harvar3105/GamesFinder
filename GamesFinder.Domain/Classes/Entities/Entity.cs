namespace GamesFinder.Domain.Entities;

public abstract class Entity
{
    public Guid? Id = Guid.NewGuid();
    public DateTime CreatedAt = DateTime.Now.ToUniversalTime();
    public DateTime UpdatedAt = DateTime.Now.ToUniversalTime();
}