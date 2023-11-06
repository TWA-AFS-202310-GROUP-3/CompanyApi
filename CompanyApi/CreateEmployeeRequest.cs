namespace CompanyApi
{
    public class CreateEmployeeRequest
    {
        public CreateEmployeeRequest(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
