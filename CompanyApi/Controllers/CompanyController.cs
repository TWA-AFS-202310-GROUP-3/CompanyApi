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

        [HttpGet("all")]
        public ActionResult<List<Company>> GetAll()
        {
            return companies;
        }

        [HttpGet]
        public ActionResult<List<Company>> GetAllByIndex([FromQuery] int pageSize = 0, [FromQuery] int pageIndex = 1)
        {
            if (pageSize <= 0 || pageIndex <= 0)
            {
                return BadRequest();
            }

            var startIndex = (pageIndex - 1) * pageSize;
            var pagedCompanies = companies.Skip(startIndex).Take(pageSize).ToList();

            return Ok(pagedCompanies);
            //return companies;
        }

        [HttpGet("{id}")]
        public ActionResult<Company> GetById(string id) 
        {
            var company = companies.Find(company => company.Id == id);
            if (company == null)
            {
                return NotFound();
            }
            return Ok(company);
        }

        [HttpPut("{id}")]
        public ActionResult<Company> UpdateCompany(string id, [FromBody] Company updatedCompany)
        {
            var company = companies.Find(c => c.Id == id);

            if (company == null)
            {
                return NotFound();
            }

            company.Name = updatedCompany.Name;
            return Ok(company);
        }

        [HttpDelete]
        public void ClearData()
        { 
            companies.Clear();
        }
    }
}
