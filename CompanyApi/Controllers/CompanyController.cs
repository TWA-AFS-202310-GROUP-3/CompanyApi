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
        public ActionResult<List<Company>> GetAllCompanies([FromQuery] int? pageSize, [FromQuery] int? pageIndex)
        {
            if (pageIndex == null || pageSize == null)
            {
                return StatusCode(StatusCodes.Status200OK, companies);
            }

            if ((pageIndex * pageSize) > companies.Count())
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }
            var ressultCompanies = companies.Skip((int)((pageIndex - 1) * pageSize)).Take((int)pageSize).ToList();
            return StatusCode(StatusCodes.Status200OK, ressultCompanies);
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

        [HttpPut("{id}")]
        public ActionResult<Company> UpdateCompany(string id, CreateCompanyRequest request)
        {
            var company = companies.Find(company => company.Id.Equals(id));
            if (company == null)
            {
                return NotFound();
            }
            company.Name = request.Name;
            return StatusCode(StatusCodes.Status200OK, company);
        }

        [HttpPost("{companyId}/employees")]
        public ActionResult<Company> CreateEmployee(string companyId, CreateEmployeeRequest request)
        {
            Employee employeeCreated = new Employee(request.Name, request.Salary);
            Company? company = companies.Find(company => company.Id == companyId);
            if (company == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
            company.Employees.Add(employeeCreated);
            return StatusCode(StatusCodes.Status201Created, employeeCreated);
        }

        [HttpDelete("{companyId}/employees/{employeeId}")]
        public ActionResult DeleteEmployee(string companyId, string employeeId)
        {
            Company? company = companies.Find(company => companyId.Equals(company.Id));
            if (company != null)
            {
                Employee? employee = company.Employees.Find(employee => employeeId.Equals(employee.Id));
                if (employee != null)
                {
                    company.Employees.Remove(employee);
                    return StatusCode(StatusCodes.Status204NoContent);
                }
            }
            return StatusCode(StatusCodes.Status404NotFound);
        }

        [HttpDelete]
        public void ClearData()
        { 
            companies.Clear();
        }
    }
}
