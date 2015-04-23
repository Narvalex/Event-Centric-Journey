namespace Journey.EventSourcing.ReadModeling
{
    public class TableInfo
    {
        public TableInfo(string tableName, string schemaName, bool hasIdentityColumn)
        {
            this.TableName = tableName;
            this.SchemaName = schemaName;
            this.HasIdentityColumn = hasIdentityColumn;
        }

        public string TableName { get; private set; }
        public string SchemaName { get; private set; }
        public bool HasIdentityColumn { get; private set; }
    }
}
