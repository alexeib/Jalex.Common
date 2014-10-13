using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;

namespace Jalex.Infrastructure.Utils
{
    public class Guard
    {
        [DebuggerNonUserCode]
        public static void AgainstEmpty(IEnumerable collection, string name)
        {
            AgainstNull(collection, name);

            if (!collection.OfType<object>().Any())
            {
                throw new ArgumentException("Collection is empty.", name);
            }
        }

        [DebuggerNonUserCode]
        public static void AgainstNull(object parameter, string name)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        [DebuggerNonUserCode]
        public static void AgainstNullOrEmpty(string parameter, string name)
        {
            AgainstNull(parameter, name);
            if (string.IsNullOrWhiteSpace(parameter))
            {
                throw new ArgumentException();
            }
        }
    }
}
