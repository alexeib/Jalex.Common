using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Jalex.Logging;

namespace Jalex.Authentication.DynamoDB
{
    public class Dynamo : IDisposable
    {
        protected static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        internal AmazonDynamoDBClient client;
        protected string idPropertyName;
        public bool Init(string idname)
        {
            idPropertyName = idname;
            bool res = false;
            try
            {
                var config = new AmazonDynamoDBConfig();
                config.ServiceURL = System.Configuration.ConfigurationManager.AppSettings["AWSServiceURL"];
                client = new AmazonDynamoDBClient(config);
                res = true;
            }
            catch (AmazonDynamoDBException ex) { Log.ErrorException("AWSDynamo", ex); }
            catch (AmazonServiceException ex) { Log.ErrorException("AWSDynamo", ex); }
            catch (Exception ex) { Log.ErrorException("AWSDynamo", ex); }

            return res;
        }

        public void Dispose() 
        { 
            if (client != null) client.Dispose(); 
        }

        #region Tenbin
        public string CreateEmailToGuidTable()
        {
            try
            {
                string tableName = "EmailGuid";
                var request = new CreateTableRequest
                {
                    TableName = tableName,
                    AttributeDefinitions = new List<AttributeDefinition>()
                    {
                        new AttributeDefinition
                        {
                            AttributeName = "Email",
                            AttributeType = "S"
                        }
                    },
                    KeySchema = new List<KeySchemaElement>()
                    {
                        new KeySchemaElement
                        {
                            AttributeName = "Email",
                            KeyType = "HASH"
                        }
                    },
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 2,
                        WriteCapacityUnits = 1
                    }
                };
                return client.CreateTable(request).ToString();
            }
            catch (AmazonDynamoDBException ex) { Log.ErrorException("AWSDynamo", ex); }
            catch (AmazonServiceException ex) { Log.ErrorException("AWSDynamo", ex); }
            return null;
        }
        public string CreatePersonTable()
        {
            try
            {
                string tableName = "Person";
                var request = new CreateTableRequest
                {
                    TableName = tableName,
                    AttributeDefinitions = new List<AttributeDefinition>()
                    {
                        new AttributeDefinition
                        {
                            AttributeName = "ID",
                            AttributeType = "S"
                        }
                    },
                    KeySchema = new List<KeySchemaElement>()
                    {
                        new KeySchemaElement
                        {
                            AttributeName = "ID",
                            KeyType = "HASH"
                        }
                    },
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 2,
                        WriteCapacityUnits = 1
                    }
                };
                return client.CreateTable(request).ToString();
            }
            catch (AmazonDynamoDBException ex) { Log.ErrorException("AWSDynamo", ex); }
            catch (AmazonServiceException ex) { Log.ErrorException("AWSDynamo", ex); }
            return null;
        }
        public IDictionary<string, DynamoDBEntry> GetPersonByEmail(string email)
        {
            string ID = GetPersonID(email);
            return ID == null ? null : GetPerson(ID);
        }
        public IDictionary<string, DynamoDBEntry> GetPerson(string ID)
        {
            Table profileTable = Table.LoadTable(client, "Person");
            return profileTable.GetItem(ID);
        }
        public IEnumerable<IDictionary<string, DynamoDBEntry>> BatchGetPerson(IEnumerable<string> IDs)
        {
            Table profileTable = Table.LoadTable(client, "Person");
            var batch = profileTable.CreateBatchGet();
            foreach (string ID in IDs)
                batch.AddKey(ID);
            batch.Execute();
            List<IDictionary<string, DynamoDBEntry>> results = new List<IDictionary<string, DynamoDBEntry>>();
            foreach (var doc in batch.Results)
            {
                Dictionary<string, string> nis = GetNIProfiles(doc[idPropertyName], Assets["Kaytto"]);
                if (nis != null)
                {
                    List<string> niprofiles = new List<string>();
                    foreach (var k in nis.Keys)
                    {
                        niprofiles.Add(k);
                        doc.Add(k, nis[k]);
                    }
                    doc.Add("NIProfiles", niprofiles);
                }
                results.Add(doc as IDictionary<string, DynamoDBEntry>);
            }
            return results;
        }
        public string GetPersonID(string email)
        {
            Table profileTable = Table.LoadTable(client, "EmailGuid");
            GetItemOperationConfig config = new GetItemOperationConfig()
            {
                AttributesToGet = new List<string>() { "ID" }
            };
            Document prof = profileTable.GetItem(email, config);
            if (prof == null)
                return null;
            else
                return prof["ID"];
        }
        public void BatchInsertPerson(IEnumerable<Dictionary<string, DynamoDBEntry>> persons)
        {
            foreach (var pers in persons)
                InsertPerson(pers);
        }
        public void InsertPerson(Dictionary<string, DynamoDBEntry> person)
        {
            if (!person.ContainsKey("email"))
                throw new Exception("Email property does not exist");
            if (!person.ContainsKey(idPropertyName))
                throw new Exception("ID property does not exist");
            string email = person["email"];
            if (string.IsNullOrEmpty(email))
                throw new Exception("Email property is empty");

            if (person.ContainsKey("NIProfiles"))
            {
                List<string> niprofiles = person["NIProfiles"].AsListOfString();
                person.Remove("NIProfiles");
                foreach (string ni in niprofiles)
                {
                    CreateNIProfile(person[idPropertyName], Assets["Kaytto"], ni, person[ni]);
                    person.Remove(ni);
                }
            }

            Table emailTable = Table.LoadTable(client, "EmailGuid");
            Document pers = new Document();
            pers["Email"] = email;
            pers["ID"] = person[idPropertyName];
            emailTable.PutItem(pers);

            Table profileTable = Table.LoadTable(client, "Person");
            pers = new Document(person);
            pers["ID"] = person[idPropertyName];
            pers["createTimeStamp"] = DateTime.UtcNow.ToString("yyyyMMddHHmmssK");
            pers["modifyTimeStamp"] = pers["createTimeStamp"];
            profileTable.PutItem(pers);
        }
        public void DeleteByEmail(string email)
        {
            string id = GetPersonID(email);
            if (!string.IsNullOrEmpty(id))
            {
                Table profileTable = Table.LoadTable(client, "EmailGuid");
                Table personTable = Table.LoadTable(client, "Person");
                profileTable.DeleteItem(email);
                personTable.DeleteItem(id);
            }
        }
        public void Update(Dictionary<string, DynamoDBEntry> person)
        {
            if (!person.ContainsKey("email"))
                throw new Exception("Email property does not exist");
            if (!person.ContainsKey(idPropertyName))
                throw new Exception("ID property does not exist");

            if (person.ContainsKey("NIProfiles"))
            {
                List<string> niprofiles = person["NIProfiles"].AsListOfString();
                person.Remove("NIProfiles");
                foreach (string ni in niprofiles)
                {
                    CreateNIProfile(person[idPropertyName], Assets["Kaytto"], ni, person[ni]);
                    person.Remove(ni);
                }
            }

            Table personTable = Table.LoadTable(client, "Person");
            GetItemOperationConfig config = new GetItemOperationConfig()
            {
                AttributesToGet = new List<string>() { "createTimeStamp", "email" }
            };
            Document prof = personTable.GetItem(person[idPropertyName].AsString(), config);
            Document pers = new Document(person);
            pers["ID"] = person[idPropertyName];
            pers["createTimeStamp"] = prof["createTimeStamp"];
            pers["modifyTimeStamp"] = DateTime.UtcNow.ToString("yyyyMMddHHmmssK");
            personTable.PutItem(pers);
        }
        public List<IDictionary<string, DynamoDBEntry>> ScanPerson(int limit = 100)
        {
            Table profileTable = Table.LoadTable(client, "Person");
            //ScanFilter scanFilter = new ScanFilter();
            //scanFilter.AddCondition("ForumId", ScanOperator.Equal, forumId);
            //scanFilter.AddCondition("Tags", ScanOperator.Contains, "rangekey");

            ScanOperationConfig config = new ScanOperationConfig()
            {
                Limit = limit
                //AttributesToGet = new List<string> { "Subject", "Message" },
                //Filter = scanFilter
            };

            Search search = profileTable.Scan(config);
            List<IDictionary<string, DynamoDBEntry>> documentList = new List<IDictionary<string,DynamoDBEntry>>();
            do
            {
                documentList.AddRange(search.GetNextSet().Select(d => (IDictionary<string, DynamoDBEntry>)d).ToList());
            } while (!search.IsDone);

            return documentList;
        }
        #endregion

        #region BMIdm
        private static Lazy<Dictionary<string, int>> _assets = new Lazy<Dictionary<string, int>>(() =>
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            try
            {
                using (Dynamo dynamo = new Dynamo())
                {
                    dynamo.Init(null);
                    Table assetTable = Table.LoadTable(dynamo.client, "Asset");
                    Search search = assetTable.Scan(new ScanFilter());
                    List<Document> documentSet = search.GetNextSet();
                    foreach (var doc in documentSet)
                    {
                        int v;
                        if (int.TryParse(doc["ID"], out v))
                            dict.Add(doc["Name"], v);
                    }
                }
            }
            catch (Exception ex) { Log.ErrorException("AWSDynamo", ex); }
            return dict;
        });
        public static Dictionary<string, int> Assets
        {
            get
            {
                try
                {
                    return _assets.Value;
                }
                catch (Exception ex) { Log.Error("ListAssets Exception: {0}", ex.ToString()); }
                return new Dictionary<string, int>();
            }
        }

        public string CreateAssetTable()
        {
            try
            {
                string tableName = "Asset";
                var request = new CreateTableRequest
                {
                    TableName = tableName,
                    AttributeDefinitions = new List<AttributeDefinition>()
                    {
                        new AttributeDefinition
                        {
                            AttributeName = "ID",
                            AttributeType = "N"
                        }
                    },
                    KeySchema = new List<KeySchemaElement>()
                    {
                        new KeySchemaElement
                        {
                            AttributeName = "ID",
                            KeyType = "HASH"
                        }
                    },
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 2,
                        WriteCapacityUnits = 1
                    }
                };
                return client.CreateTable(request).ToString();
            }
            catch (AmazonDynamoDBException ex) { Log.ErrorException("AWSDynamo", ex); }
            catch (AmazonServiceException ex) { Log.ErrorException("AWSDynamo", ex); }
            return null;
        }

        public string CreateUserTable()
        {
            try
            {
                string tableName = "ProfileUser";
                var request = new CreateTableRequest
                {
                    TableName = tableName,
                    AttributeDefinitions = new List<AttributeDefinition>()
                    {
                        new AttributeDefinition
                        {
                            AttributeName = "BMIdpUID",
                            AttributeType = "S"
                        },
                        new AttributeDefinition
                        {
                            AttributeName = "AssetID",
                            AttributeType = "N"
                        }
                    },
                    KeySchema = new List<KeySchemaElement>()
                    {
                        new KeySchemaElement
                        {
                            AttributeName = "BMIdpUID",
                            KeyType = "HASH"
                        },
                        new KeySchemaElement
                        {
                            AttributeName = "AssetID",
                            KeyType = "RANGE"
                        }
                    },
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 2,
                        WriteCapacityUnits = 1
                    }
                };
                return client.CreateTable(request).ToString();
            }
            catch (AmazonDynamoDBException ex) { Log.ErrorException("AWSDynamo", ex); }
            catch (AmazonServiceException ex) { Log.ErrorException("AWSDynamo", ex); }
            return null;
        }

        public string CreateDataTable()
        {
            try
            {
                string tableName = "ProfileData";
                var request = new CreateTableRequest
                {
                    TableName = tableName,
                    AttributeDefinitions = new List<AttributeDefinition>()
                    {
                        new AttributeDefinition
                        {
                            AttributeName = "DataID",
                            AttributeType = "S"
                        },
                        new AttributeDefinition
                        {
                            AttributeName = "Name",
                            AttributeType = "S"
                        }
                    },
                    KeySchema = new List<KeySchemaElement>()
                    {
                        new KeySchemaElement
                        {
                            AttributeName = "DataID",
                            KeyType = "HASH"
                        },
                        new KeySchemaElement
                        {
                            AttributeName = "Name",
                            KeyType = "RANGE"
                        }
                    },
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 2,
                        WriteCapacityUnits = 1
                    }
                };
                return client.CreateTable(request).ToString();
            }
            catch (AmazonDynamoDBException ex) { Log.ErrorException("AWSDynamo", ex); }
            catch (AmazonServiceException ex) { Log.ErrorException("AWSDynamo", ex); }
            return null;
        }

        public void UploadAssets()
        {
            try
            {
                Table assetTable = Table.LoadTable(client, "Asset");
                var doc1 = new Document();
                doc1["ID"] = 101;
                doc1["Name"] = "TSN";
                assetTable.PutItem(doc1);

                doc1 = new Document();
                doc1["ID"] = 102;
                doc1["Name"] = "Bravo";
                assetTable.PutItem(doc1);

                doc1 = new Document();
                doc1["ID"] = 201;
                doc1["Name"] = "Kaytto";
                assetTable.PutItem(doc1);
            }
            catch (AmazonDynamoDBException ex) { Log.ErrorException("AWSDynamo", ex); }
            catch (AmazonServiceException ex) { Log.ErrorException("AWSDynamo", ex); }
        }

        private string CreateUserProfile(string uID, int assetID)
        {
            try
            {
                string id = Guid.NewGuid().ToString();
                Table profileTable = Table.LoadTable(client, "ProfileUser");
                var doc1 = new Document();
                doc1["BMIdpUID"] = uID;
                doc1["AssetID"] = assetID;
                doc1["DataID"] = id;
                doc1["Created"] = DateTime.UtcNow.ToString("s");
                profileTable.PutItem(doc1);
                return id;
            }
            catch (AmazonDynamoDBException ex) { Log.ErrorException("AWSDynamo", ex); }
            catch (AmazonServiceException ex) { Log.ErrorException("AWSDynamo", ex); }
            return null;
        }

        public bool CreateNIProfile(string uID, int assetID, string name, string data)
        {
            if (!Assets.ContainsValue(assetID))
                return false;
            try
            {
                string id = null;
                Table profileTable = Table.LoadTable(client, "ProfileUser");
                //QueryFilter filter = new QueryFilter("AssetID", QueryOperator.Equal, assetID);
                //Search search = profileTable.Query(uID, filter);
                //List<Document> documentSet = search.GetNextSet();
                //if (documentSet.Count == 0)
                //    id = CreateUserProfile(uID, assetID);
                //else
                //    id = documentSet[0]["DataID"];
                GetItemOperationConfig config = new GetItemOperationConfig()
                {
                    AttributesToGet = new List<string>() { "DataID" }
                    //ConsistentRead = true
                };
                Document prof = profileTable.GetItem(uID, assetID, config);
                if (prof != null)
                    id = prof["DataID"];
                if (string.IsNullOrEmpty(id))
                    id = CreateUserProfile(uID, assetID);
                if (string.IsNullOrEmpty(id)) return false;

                Table dataTable = Table.LoadTable(client, "ProfileData");
                var doc1 = new Document();
                doc1["DataID"] = id;
                doc1["Name"] = name;
                doc1["Data"] = data;
                doc1["Created"] = DateTime.UtcNow.ToString("s");
                dataTable.PutItem(doc1);
                return true;
            }
            catch (AmazonDynamoDBException ex) { Log.ErrorException("AWSDynamo", ex); }
            catch (AmazonServiceException ex) { Log.ErrorException("AWSDynamo", ex); }
            return false;
        }

        public string GetNIProfile(string uID, int assetID, string name, out DateTime? created)
        {
            created = null;
            try
            {
                string id = GetUserDataID(uID, assetID);
                if (id == null) return null;

                Table dataTable = Table.LoadTable(client, "ProfileData");
                GetItemOperationConfig config = new GetItemOperationConfig()
                {
                    AttributesToGet = new List<string>() { "Data", "Created" }
                };
                Document data = dataTable.GetItem(id, name, config);
                if (data != null)
                {
                    created = DateTime.ParseExact(data["Created"], "s", null);
                    return data["Data"];
                }
            }
            catch (AmazonDynamoDBException ex) { Log.ErrorException("AWSDynamo", ex); }
            catch (AmazonServiceException ex) { Log.ErrorException("AWSDynamo", ex); }
            return null;
        }

        public Dictionary<string, string> GetNIProfiles(string uID, int assetID)
        {
            try
            {
                string id = GetUserDataID(uID, assetID);
                if (id == null) return null;

                Dictionary<string, string> dict = new Dictionary<string, string>();
                Table dataTable = Table.LoadTable(client, "ProfileData");
                QueryOperationConfig config = new QueryOperationConfig()
                {
                    Filter = new QueryFilter("DataID", QueryOperator.Equal, id),
                    AttributesToGet = new List<string>() { "Name", "Data" },
                    Select = SelectValues.SpecificAttributes
                };
                Search search = dataTable.Query(config);
                List<Document> documentSet = search.GetNextSet();
                if (documentSet == null) return null;
                foreach (var doc in documentSet)
                    dict.Add(doc["Name"], doc["Data"]);
                return dict;
            }
            catch (AmazonDynamoDBException ex) { Log.ErrorException("AWSDynamo", ex); }
            catch (AmazonServiceException ex) { Log.ErrorException("AWSDynamo", ex); }
            return null;
        }

        public string GetUserDataID(string uID, int assetID)
        {
            Table profileTable = Table.LoadTable(client, "ProfileUser");
            GetItemOperationConfig config = new GetItemOperationConfig()
            {
                AttributesToGet = new List<string>() { "DataID" }
            };
            Document prof = profileTable.GetItem(uID, assetID, config);
            if (prof == null) 
                return null;
            else 
                return prof["DataID"];
        }
        #endregion
    }
}