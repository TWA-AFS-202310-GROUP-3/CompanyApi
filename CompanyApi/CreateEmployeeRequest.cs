namespace CompanyApi
{
    public class CreateEmployeeRequest
    {
        public CreateEmployeeRequest(string name, int salary)
        {
            Name = name;
            Salary = salary;
        }

        public string Name { get; set; }
        public int Salary { get; set; }
    }
}
