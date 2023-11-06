namespace CompanyApi
{
    public class CompanyRequest
    {
        public CompanyRequest(string name)
        {
            Name = name;
        }

        public string Name { get; set; }


    }
}
