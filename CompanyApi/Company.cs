namespace CompanyApi
{
    public class Company
    {
        public Company(string name)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            Employees = new List<Employee>();
        }

        public List<Employee> Employees { get; set; }

        public string Id { get; set; }

        public string Name { get; set; }
    }
}
