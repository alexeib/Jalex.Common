using System;

namespace Jalex.Scripting
{
    public interface IScriptExecutor : IDisposable
    {
        TClass CreateClass<TClass>(string scriptLocation, string className, params object[] constructorArgs)
            where TClass : class;

        TResult CallMethod<TResult>(string scriptLocation, string methodName, params object[] args);
    }
}