﻿namespace CompanyApi
{
    public class CreateCompanyRequest
    {
        public CreateCompanyRequest(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }

}
