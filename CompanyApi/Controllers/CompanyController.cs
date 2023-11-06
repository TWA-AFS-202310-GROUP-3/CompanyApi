using Microsoft.AspNetCore.Mvc;

namespace CompanyApi.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private static List<Company> companies = new List<Company>();

        [HttpPost]
        public ActionResult<Company> Create(CompanyRequest request)
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
        public ActionResult<List<Company>> GetALLCompanies([FromQuery] int? page_size, int? index)
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
            var result =  companies.Find(company => company.Id == id);
            if (result == null)
            {
                return NotFound($"The item with id {id} does not exist.");
            }
            return Ok(result);
        }


        [HttpDelete]
        public void ClearData()
        { 
            companies.Clear();
        }
    }
}
