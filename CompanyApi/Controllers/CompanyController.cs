using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CompanyApi.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private static List<Company> companies = new List<Company>();

        [HttpPost("{companyId}/empoyees")]
        public ActionResult<Employee> AddEmployee(string companyId, EmployeeRequest newEmployee)
        {
            var company = companies.Find(company => company.Id == companyId);
            if (company == null)
            {
                return NotFound();
            }
            Employee newComer = new Employee(newEmployee.Name);
            return Created("", newComer);
        }

        [HttpPut("{id}")]
        public ActionResult<Company> Put(string id, CreateCompanyRequest company)
        {
            var previousCompany = companies.Find(company => company.Id == id);
            if (previousCompany == null)
            {
                return NotFound();
            }
            previousCompany.Name = company.Name;
            return NoContent();
        }

        [HttpGet]
        public ActionResult<List<Company>> Get(int? pageSize, int? pageIndex)
        {
            if (pageSize == null || pageIndex == null)
            {
                return Ok(companies);
            }
            List<Company> companiesInOnePage = new List<Company>();
            int beginIndex = (int)((pageIndex - 1) * pageSize);
            int i = 0;
            while (i < pageSize && (i + beginIndex) < companies.Count)
            {
                companiesInOnePage.Add(companies[beginIndex + i]);
                i++;
            }
            return Ok(companiesInOnePage);
        }

        [HttpGet("{id}")]
        public ActionResult<Company> Get(string id)
        {
            var company = companies.Find(company => company.Id == id);
            if (company == null) 
            {
                return NotFound();
            }
            return Ok(company);
        }
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
    }
}
