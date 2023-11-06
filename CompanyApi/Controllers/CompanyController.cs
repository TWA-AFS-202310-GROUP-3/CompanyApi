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

        [HttpGet("{id}")]
        public ActionResult<Company> Get(string id)
        {
            foreach (var company in companies.Where(company => company.Id.Equals(id)))
            {
                return Ok(company);
            }

            return NotFound();
        }

        [HttpGet]
        public ActionResult<List<Company>> GetInPage([FromQuery] int? pageSize, [FromQuery] int? pageIndex)
        {
            if (pageIndex == null||pageSize == null)
            {
                return Ok(companies);
            }
            List<Company> companiesInPage = companies.Skip(((int)pageIndex -1)* (int)pageSize).Take((int)pageSize).ToList();
            return Ok(companiesInPage);
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

        [HttpPost("{id}/employees")]
        public ActionResult<Employee> AddEmployee(string id, CreateEmployeeRequest request)
        {
            Employee employeeCreated = new Employee(request.Name);
            foreach (var company in companies.Where(company => company.Id.Equals(id)))
            {
                if (company.Employees.Exists(employee => employee.Name.Equals(request.Name)))
                {
                    return BadRequest();
                }
                company.Employees.Add(employeeCreated);
                return StatusCode(StatusCodes.Status201Created, employeeCreated);
            }
            return NotFound();
        }

        [HttpDelete("{companyId}/employees/{employeeId}")]
        public ActionResult DeleteEmployeeFromSpecificCompany(string companyId, string employeeId)
        {

            foreach (var (company, employee) in from company in companies
                                                where company.Id.Equals(companyId)
                                                from employee in company.Employees.Where(employee => employee.Id.Equals(employeeId))
                                                select (company, employee))
            {
                company.Employees.Remove(employee);
                return NoContent() ;
            }
            return NotFound() ;
        }
    }
}
