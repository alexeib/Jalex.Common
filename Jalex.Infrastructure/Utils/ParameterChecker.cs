using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Jalex.Infrastructure.Extensions;

namespace Jalex.Infrastructure.Utils
{
    public class ParameterChecker
    {
        [DebuggerNonUserCode]
        public static void CheckForEmptyCollection(IEnumerable collection, string name)
        {
            CheckForNull(collection, name);

            if (!collection.OfType<object>().Any())
            {
                throw new ArgumentException("Collection is empty.", name);
            }
        }

        [DebuggerNonUserCode]
        public static void CheckForNull(object parameter, string name)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        [DebuggerNonUserCode]
        public static void CheckForNullOrEmpty(string parameter, string name)
        {
            CheckForNull(parameter, name);
            if (string.IsNullOrWhiteSpace(parameter))
            {
                throw new ArgumentException();
            }
        }

        [DebuggerNonUserCode]
        public static void CheckForVoid(Expression<Func<object>> expression, string message = null, bool checkForWhiteSpace = true, bool checkForEmptyCollection = true)
        {
            object @object = expression.Compile()();

            // simple check
            if (@object == null)
            {
                message = message ?? new ArgumentNullException().Message;
                throw new ArgumentNullException(expression.GetParameterName(), message);
            }

            // string checking
            if (@object is string)
            {
                if (checkForWhiteSpace ? string.IsNullOrWhiteSpace((string)@object) : string.IsNullOrEmpty((string)@object))
                {
                    message = message ?? "Null or white space string.";
                    throw new ArgumentException(message, expression.GetParameterName());
                }
            }

            // collection checking
            if (checkForEmptyCollection && @object is IEnumerable && !(@object is string))
            {
                IEnumerator enumerator = ((IEnumerable)@object).GetEnumerator();
                if (!enumerator.MoveNext())
                {
                    message = message ?? "Collection has no items.";
                    throw new ArgumentException(message, expression.GetParameterName());
                }
            }
        }
    }
}
