using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Cassandra;
using NLog;

namespace Jalex.Repository.Cassandra
{
    /// <summary>
    /// This class ensures that only one session per keyspace is created in order to take advantage of the driver's connection pooling
    /// </summary>
    internal static class CassandraSessionPool
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        private const string _defaultKeyspaceSettingName = "cassandra-keyspace";
        private const string _defaultContactsSettingName = "cassandra-contacts";
        private const string _defaultContactsPort = "cassandra-port";
        private const int _defaultPort = 9042;

        private static readonly ConcurrentDictionary<string, ISession> _keyspaceToSession = new ConcurrentDictionary<string, ISession>();

        internal static IEnumerable<string> Contacts { get; set; }

        internal static ISession GetSession(string keyspace = null)
        {
            if (keyspace == null)
            {
                keyspace = ConfigurationManager.AppSettings[_defaultKeyspaceSettingName] ?? Environment.GetEnvironmentVariable(@"DEFAULT_CASSANDRA_KEYSPACE");
            }

            if (string.IsNullOrEmpty(keyspace))
            {
                throw new InvalidOperationException("Must specify Cassandra keyspace by providing a value in the Keyspace property or populating the " + _defaultKeyspaceSettingName + " app setting");
            }

            var session = _keyspaceToSession.GetOrAdd(keyspace, createSessionForKeyspace);
            return session;
        }

        internal static void DestroySessionForKeyspace(string keyspace)
        {
            ISession session;
            var removed = _keyspaceToSession.TryRemove(keyspace, out session);
            if (removed)
            {
                session.Cluster.Shutdown();
            }
        }

        internal static void DestroyAllSessions()
        {
            var keyspaces = _keyspaceToSession.Keys.ToArray();
            foreach (var keyspace in keyspaces)
            {
                DestroySessionForKeyspace(keyspace);
            }
        }

        private static ISession createSessionForKeyspace(string keyspace)
        {
            string[] contacts = getContacts();
            var retryPolicy = new CassandraRetryPolicy(3, TimeSpan.Zero);

            Builder builder = Cluster.Builder();

            builder.AddContactPoints(contacts)
                   .WithPort(getCassandraPort())
                   .WithRetryPolicy(retryPolicy);

            Cluster cluster = builder.Build();

            ISession session = cluster.Connect();

            try
            {
                session.ChangeKeyspace(keyspace);
            }
            catch
            {
                session.CreateKeyspace(keyspace);
                session.ChangeKeyspace(keyspace);
            }

            return session;
        }

        private static int getCassandraPort()
        {
            string contactPortString = ConfigurationManager.AppSettings[_defaultContactsPort];
            int port;
            if (int.TryParse(contactPortString, out port)) 
                return port;
            return _defaultPort;
        }

        private static string[] getContacts()
        {
            if (Contacts != null)
            {
                return Contacts.ToArray();
            }

            string contactSettingsString = ConfigurationManager.AppSettings[_defaultContactsSettingName];

            if (string.IsNullOrEmpty(contactSettingsString))
            {
                _logger.Warn("Could not retrieve list of Cassandra contacts from application settings key " + _defaultContactsSettingName + ". Defaulting to localhost");
                contactSettingsString = "localhost";
            }

            var contacts = contactSettingsString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            return contacts;
        }
    }
}
