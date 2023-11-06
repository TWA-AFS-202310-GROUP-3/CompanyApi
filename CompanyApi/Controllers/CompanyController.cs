using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace CompanyApi.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private static List<Company> companies = new List<Company>();

        [HttpPost]
        public ActionResult<Company> Create(CreateCompanyRequest request)
        {
            if (companies.Exists(company => company.Name.Equals(request.Name)))
            {
                return BadRequest();
            }
            Company companyCreated = new Company(request.Name);
            companies.Add(companyCreated);
            return StatusCode(StatusCodes.Status201Created, companyCreated);
        }

        [HttpGet]
        public ActionResult<List<Company>> GetAllCompanies()
        {
            return StatusCode(StatusCodes.Status200OK, companies);
        }


        [HttpGet("{id}")]
        public ActionResult<Company> GetCompany(string id)
        {
            var company = companies.Find(company => company.Id.Equals(id));
            if (company == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
            return StatusCode(StatusCodes.Status200OK, company);
        }

        //[HttpPut("{id}")]
        //public ActionResult<Company> UpdateCompany(string id, CreateCompanyRequest request)
        //{
        //    var company = companies.Find(company => company.Id.Equals(id));
        //    if (company == null)
        //    {
        //        return NotFound();
        //    }
        //    company.Name = request.Name;
        //    return company;
        //}

        [HttpDelete]
        public void ClearData()
        { 
            companies.Clear();
        }
    }
}
