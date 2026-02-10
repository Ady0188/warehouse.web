namespace Warehouse.Web.Orders
{
    internal class Outbox
    {
        public long Id { get; private set; }
        public string StoreName { get; private set; }
        public string UserName { get; private set; }
        public short Method { get; private set; }
        public long ObjectId { get; private set; }
        public string ObjectName { get; private set; }
        public string? OldData { get; private set; }
        public string NewData { get; private set; }
        public DateTime CreateDate { get; private set; } = DateTime.Now;

        private Outbox() { }

        public static Outbox FromEvent(
            string storeName, string userName,
            short method, long objectId,
            string objectName, string? oldData, string newData)
        => new()
        {
            StoreName = storeName,
            UserName = userName,
            Method = method,
            ObjectId = objectId,
            ObjectName = objectName,
            OldData = oldData,
            NewData = newData
        };
    }
}
