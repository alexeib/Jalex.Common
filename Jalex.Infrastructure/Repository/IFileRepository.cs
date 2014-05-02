using System.IO;
using Jalex.Infrastructure.Objects;

namespace Jalex.Infrastructure.Repository
{
    public interface IFileRepository
    {
        OperationResult<string> Create(string fileName, Stream fileStream);
        OperationResult<Stream> GetById(string id);
        OperationResult<Stream> GetByFileName(string fileName);
        OperationResult DeleteById(string id);
        OperationResult DeleteByFileName(string fileName);        
    }
}
