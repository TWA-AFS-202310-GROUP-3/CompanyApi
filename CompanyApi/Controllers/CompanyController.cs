using Microsoft.AspNetCore.Mvc;

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
            return companies;

        }

        [HttpGet("{id}")]
        public ActionResult<Company> Get(string id)
        {
            var company = companies.FirstOrDefault(cp => cp.Id == id);

            if (company != null)
            {
                return company;
            }

            return NotFound();
        }

        [HttpGet]
        public ActionResult<List<Company>> GetCompanies(int pageSize, int pageIndex)
        {
            var startIndex = (pageIndex - 1) * pageSize;
            var companiesPage = companies.Skip(startIndex).Take(pageSize).ToList();
            return Ok(companiesPage);
        }

        [HttpPut("{id}")]
        public ActionResult<Company> Update(string id, [FromBody] Company updatedCompany)
        {
            var existingCompany = companies.FirstOrDefault(c => c.Id == id);
            if (existingCompany == null)
            {
                return NotFound();
            }

            existingCompany.Name = updatedCompany.Name;

            return NoContent();
        }
    }
}
