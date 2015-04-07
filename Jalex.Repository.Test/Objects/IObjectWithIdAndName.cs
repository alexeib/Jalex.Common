using System;

namespace Jalex.Repository.Test.Objects
{
    public interface IObjectWithIdAndName
    {
        Guid Id { get; set; }
        string Name { get; set; }
    }
}
