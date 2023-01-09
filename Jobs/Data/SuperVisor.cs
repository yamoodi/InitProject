namespace Jobs.Data
{
    public class SuperVisor
    {
        public string Username { get; set; }
        public long Id { get; set; }
        public long CompanyId { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
