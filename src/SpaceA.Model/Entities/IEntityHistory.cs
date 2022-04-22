using System;

namespace SpaceA.Model.Entities
{
    public interface IEntityHistory<TMemberId>
    {
        int Rev { get; set; }

        DateTime CreatedDate { get; set; }

        DateTime ChangedDate { get; set; }

        TMemberId ChangerId { get; set; }
    }
}