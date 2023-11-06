using Microsoft.AspNetCore.Mvc;
using System.Linq;

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

        [HttpDelete]
        public void ClearData()
        {
            companies.Clear();
        }

        [HttpGet]
        public ActionResult<List<Company>> GetAll()
        {
            return Ok(companies);
        }

        [HttpGet("{id}")]
        public ActionResult<Company> Get(string id)
        {
            foreach (var company in companies.Where(company => company.Id.Equals(id)))
            {
                return Ok(company);
            }

            return NotFound();
        }

        [HttpPut("{id}")]
        public ActionResult<Company> Update(string id, CreateCompanyRequest companyRequest)
        {
            foreach (var company in companies.Where(company => company.Id.Equals(id)))
            {
                company.Name = companyRequest.Name;
                return NoContent();
            }
            return NotFound();
        }
    }
}
