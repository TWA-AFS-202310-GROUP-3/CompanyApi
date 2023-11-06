using Microsoft.AspNetCore.Mvc;

namespace CompanyApi.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private static List<Company> companies = new List<Company>();

        [HttpPost]
        public ActionResult<Company> Create([FromBody] CompanyRequest request)
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
        public ActionResult<List<Company>> GetCompanies([FromQuery] int? page_size, int? index)
        {
            if (page_size != null && index != null)
            {
                List<Company> onePageCompanies = new List<Company>();
                int startIdx = (int)((index - 1) * page_size);
                for (int i = 0; i < page_size && (i + startIdx) < companies.Count; i++)
                {
                    onePageCompanies.Add(companies[i + startIdx]);
                }
                return Ok(onePageCompanies);
            }
            return Ok(companies);
        }

        [HttpGet("{id}")]
        public ActionResult<Company> GetByID(string id)
        {
            var existingCompany =  companies.Find(company => company.Id == id);
            if (existingCompany == null)
            {
                return NotFound($"The item with id {id} does not exist.");
            }
            return Ok(existingCompany);
        }

        [HttpPut("{id}")]
        public ActionResult<Company> PutCompany (string id, [FromBody] Company updateCompay)
        {
            var existingCompany = companies.Find(company => company.Id == id);
            if (existingCompany == null)
            {
                return NotFound($"The item with id {id} does not exist.");
            } 
            existingCompany.Name = updateCompay.Name;

            return existingCompany;
        }

        [HttpPost("{company_id}")]
        public ActionResult<Employee> CreateEmployee([FromBody] EmployeeRequest newCommer,string company_id)
        {
            var theCompanyToJoin = companies.Find(company => company.Id == company_id);
            Employee newEmployee = new Employee(newCommer.Name);
            theCompanyToJoin.Employees.Add(newEmployee);

            return theCompanyToJoin == null ? NotFound($"The company with id {company_id} does not exist.") : Created("", newEmployee);
        }

        [HttpDelete("{company_id}/{employee_id}")]
        public ActionResult<Employee> DeleteEmployee(string company_id, string employee_id)
        {

            var theCompanyJoined = companies.Find(company => company.Id == company_id);
            if (theCompanyJoined == null)
            {
                return NotFound($"The company with id {company_id} does not exist.");
            }
            Employee employeeToGo = theCompanyJoined.Employees.Find(e => e.Id == employee_id);
            if (employeeToGo == null)
            {
                return NotFound($"The employee with id {employee_id} does not exist");
            }
            theCompanyJoined.Employees.Remove(employeeToGo);
            return NoContent();
        }

        [HttpDelete]
        public void ClearData()
        { 
            companies.Clear();
        }
    }
}
