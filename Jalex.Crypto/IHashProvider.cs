using System;
namespace Jalex.Crypto
{
    public interface IHashProvider : IDisposable
    {
        string GetHash(string text);
    }
}
