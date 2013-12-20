using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Jalex.Logging.Loggers;

namespace Jalex.Logging
{
    public static class LogManager
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ILogger GetCurrentClassLogger()
        {
            var stackFrame = new StackFrame(1, false);
            return GetLoggerForType(stackFrame.GetMethod().DeclaringType);
        }

        /// <summary>
        /// This property allows overwriting loggers return by the factory. Note that this single instance of a logger will be returned for ALL classes.
        /// If this property is set after a class has already obtained a logger, it will have no effect.
        /// </summary>
        public static ILogger OverwriteLogger { get; set; }

        /// <summary>
        ///     Gets a logger for the given class type.
        /// </summary>
        /// <param name="type">The type of object that wants to do logging.</param>
        /// <returns>A logger.</returns>
        /// <remarks>
        ///     The type param should be the type of the class that needs to do logging. For example, if you were doing logging
        ///     within
        ///     a controller called HomeController, you would pass typeof(HomeController) as the type to this method.
        /// </remarks>
        public static ILogger GetLoggerForType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return OverwriteLogger ?? new NLogLogger(type);
        }
    }
}
