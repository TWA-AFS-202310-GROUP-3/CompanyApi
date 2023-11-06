namespace CompanyApi
{
    public class EmployeeRequest
    {
        public string Name { get; set; }

        public EmployeeRequest(string name)
        {
            Name = name;
        }
    }
}
