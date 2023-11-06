using Microsoft.AspNetCore.Mvc;

namespace CompanyApi.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private static List<Company> companies = new List<Company>();
        private static Dictionary<string, List<Employee>> employees = new Dictionary<string, List<Employee>>();

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

        [HttpPost("{companyId}/employees")]
        public ActionResult<Company> Create([FromRoute] string companyId, CreateEmployeeRequest request)
        {
            var company = companies.Find(c => c.Id == companyId);
            if (company is null)
            {
                return BadRequest();
            }

            if (!employees.ContainsKey(companyId))
            {
                employees[companyId] = new List<Employee>();
            }

            if (employees[companyId].Find(employee => employee.Name == request.Name) is not null)
            {
                return BadRequest();
            }

            Employee employeeCreated = new Employee(request.Name);
            employees[companyId].Add(employeeCreated);
            return StatusCode(StatusCodes.Status201Created, employeeCreated);
        }

        [HttpDelete("{companyId}/employees/{employeeId}")]
        public ActionResult<Company> DeleteEmployee([FromRoute] string companyId, [FromRoute] string employeeId)
        {
            var company = companies.Find(c => c.Id == companyId);
            if (company is null)
            {
                return NotFound();
            }

            if (!employees.ContainsKey(companyId))
            {
                return NotFound();
            }

            Employee employee = employees[companyId].Find(employee => employee.Id == employeeId);
            if (employee is null)
            {
                return NotFound();
            }
            else
            {
                employees[companyId].Remove(employee);
                return StatusCode(StatusCodes.Status204NoContent, employee);
            }
        }

        [HttpDelete]
        public void ClearData()
        { 
            companies.Clear();
        }
    }
}
