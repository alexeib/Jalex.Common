using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using IronPython.Compiler;
using IronPython.Hosting;
using Jalex.Infrastructure.Logging;
using Jalex.Logging;
using Magnum.Extensions;
using Microsoft.Scripting.Hosting;

namespace Jalex.Scripting.Python
{
    public class PythonExecutor : IScriptExecutor
    {
        private readonly IEnumerable<Type> _typesToPreload;
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private readonly Lazy<ScriptEngine> _engine;
        private readonly ConcurrentDictionary<string, CompiledCode> _compiledScripts;

        public PythonExecutor()
            : this(Enumerable.Empty<Type>())
        {
        }

        public PythonExecutor(IEnumerable<Type> typesToPreload)
        {
            if (typesToPreload == null) throw new ArgumentNullException("typesToPreload");
            _typesToPreload = typesToPreload;

            _engine = new Lazy<ScriptEngine>(createEngine);

            _compiledScripts = new ConcurrentDictionary<string, CompiledCode>();
        }

        #region Implementation of IScriptExecutor

        public TClass CreateClass<TClass>(string scriptLocation, string className, params object[] constructorArgs) where TClass : class
        {
            var classObj = getVariableFromScript(scriptLocation, className);
            if (constructorArgs.Length == 0)
            {
                return classObj();
            }
            return classObj(constructorArgs);
        }

        public TResult CallMethod<TResult>(string scriptLocation, string methodName, params object[] args)
        {
            var methodObj = getVariableFromScript(scriptLocation, methodName);
            if (args.Length == 0)
            {
                return methodObj();
            }
            return methodObj(args);
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (_engine.IsValueCreated)
                {
                    _engine.Value.Runtime.Shutdown();
                }
            }
            catch (Exception e)
            {
                // swallow exception in finalization stage   
                _logger.ErrorException(e, "Exception when shutting down python engine!");
            }
        }

        #endregion

        private ScriptEngine createEngine()
        {
            var options = new Dictionary<string, object>();
#if DEBUG
            options["Debug"] = true;
#endif
            options["LightweightScopes"] = true;

            var engine = IronPython.Hosting.Python.CreateEngine(options);
            _typesToPreload.Select(t => t.Assembly)
                           .Distinct()
                           .Each(assembly => engine.Runtime.LoadAssembly(assembly));

            var libPath = ConfigurationManager.AppSettings["ptyon-lib"] ?? @"C:\Program Files (x86)\IronPython 2.7\Lib";

            var sp = engine.GetSearchPaths();
            sp.Add(libPath);
            engine.SetSearchPaths(sp);

            return engine;
        }

        private CompiledCode compileScript(string location)
        {
            var source = _engine.Value.CreateScriptSourceFromFile(location);

            var opts = (PythonCompilerOptions)_engine.Value.GetCompilerOptions();
            //opts.Module &= ~ModuleOptions.Optimized;
            return source.Compile(opts);
        }

        private ScriptScope createScope()
        {
            var scope = _engine.Value.CreateScope();

            if (_typesToPreload.Any())
            {
                scope.ImportModule("clr");
                _typesToPreload.Each(t => _engine.Value.Execute(string.Format("from {0} import {1}", t.Namespace, t.Name), scope));
            }

            return scope;
        }

        private dynamic getVariableFromScript(string scriptLocation, string variableName)
        {
            var compiledScript = _compiledScripts.GetOrAdd(scriptLocation, compileScript);
            var scope = createScope();

            compiledScript.Execute(scope);

            dynamic variable = scope.GetVariable(variableName);
            return variable;
        }
    }
}
