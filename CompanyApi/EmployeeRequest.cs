namespace CompanyApi
{
    public class EmployeeRequest
    {
        public EmployeeRequest(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
