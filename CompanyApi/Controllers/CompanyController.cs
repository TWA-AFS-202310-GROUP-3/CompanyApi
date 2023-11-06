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
        public ActionResult<List<Company>> GetAll([FromQuery] int? pageSize, int? pageIndex)
        {
            if (pageSize != null && pageIndex != null)
            {
                var startIndex = (pageIndex - 1) * pageSize;
                var companiesPage = companies.Skip((int)startIndex).Take((int)pageSize).ToList();
                return Ok(companiesPage);
            }

            return companies;
        }

        [HttpGet("{id}")]
        public ActionResult<Company> Get(string id)
        {
            var company = companies.Find(cp => cp.Id == id);

            if (company != null)
            {
                return company;
            }

            return NotFound();
        }

        [HttpPut("{id}")]
        public ActionResult<Company> Update(string id, [FromBody] Company updatedCompany)
        {
            var existingCompany = companies.Find(c => c.Id == id);
            if (existingCompany == null)
            {
                return NotFound();
            }

            existingCompany.Name = updatedCompany.Name;

            return NoContent();
        }

        [HttpPost("{companyId}/employees")]
        public ActionResult<Company> AddEmployee(string companyId, [FromBody] Employee employee)
        {
            var company = companies.Find(c => c.Id == companyId);
            if (company == null)
            {
                return NotFound();
            }

            //employee.Id = Guid.NewGuid().ToString();
            company.Employees.Add(employee);

            return Ok(employee);
        }

        /*[HttpDelete("employees/{employeeId}")]
        public ActionResult<Company> DeleteEmployee(string companyId, string employeeId)
        {
            var company = companies.Find(c => c.Id == companyId);
            if (company == null)
            {
                return NotFound();
            }

            var employee = company.Employees.FirstOrDefault(e => e.Id == employeeId);
            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            company.Employees.Remove(employee);

            return NoContent();
        }*/
    }
}
