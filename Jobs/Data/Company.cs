namespace Jobs.Data
{
    public class Company
    {
        public Company()
        {
            SuperVisors = new List<SuperVisor>();
        }
        public string Name { get; set; }
        public long Id { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Username { get; set; }
        public virtual ICollection<SuperVisor> SuperVisors { get; set; }
    }
}
