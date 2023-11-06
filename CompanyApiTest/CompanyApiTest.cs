using CompanyApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;

namespace CompanyApiTest
{
    public class CompanyApiTest
    {
        private HttpClient httpClient;

        public CompanyApiTest()
        {
            WebApplicationFactory<Program> webApplicationFactory = new WebApplicationFactory<Program>();
            httpClient = webApplicationFactory.CreateClient();
        }

        [Fact]
        public async Task Should_return_created_company_with_status_201_when_create_cpmoany_given_a_company_name()
        {
            // Given
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("BlueSky Digital Media");
            
            // When
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
                "/api/companies", 
                SerializeObjectToContent(companyGiven)
            );
           
            // Then
            Assert.Equal(HttpStatusCode.Created, httpResponseMessage.StatusCode);
            Company? companyCreated = await DeserializeTo<Company>(httpResponseMessage);
            Assert.NotNull(companyCreated);
            Assert.NotNull(companyCreated.Id);
            Assert.Equal(companyGiven.Name, companyCreated.Name);
        }

        [Fact]
        public async Task Should_return_bad_reqeust_when_create_company_given_a_existed_company_name()
        {
            // Given
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("BlueSky Digital Media");

            // When
            await httpClient.PostAsync("/api/companies", SerializeObjectToContent(companyGiven));
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
                "/api/companies", 
                SerializeObjectToContent(companyGiven)
            );
            // Then
            Assert.Equal(HttpStatusCode.BadRequest, httpResponseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_bad_reqeust_when_create_company_given_a_company_with_unknown_field()
        {
            // Given
            await ClearDataAsync();
            StringContent content = new StringContent("{\"unknownField\": \"BlueSky Digital Media\"}", Encoding.UTF8, "application/json");
          
            // When
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync("/api/companies", content);
           
            // Then
            Assert.Equal(HttpStatusCode.BadRequest, httpResponseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_All_Companies_with_status_200_when_get_company()
        {
            // Given
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("BlueSky Digital Media");
            await httpClient.PostAsJsonAsync("/api/companies", companyGiven);

            // When
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("/api/companies");

            // Then
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            List<Company>? companies = await httpResponseMessage.Content.ReadFromJsonAsync<List<Company>>();
            Assert.NotNull(companies);
            Assert.Equal(companyGiven.Name, companies[0].Name);
        }

        [Fact]
        public async Task Should_return_company_with_status_200_when_get_company_given_a_company_id()
        {
            // Given
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("BlueSky Digital Media");
            HttpResponseMessage createHttpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            Company? companyCreated = await createHttpResponseMessage.Content.ReadFromJsonAsync<Company>();

            // When
            HttpResponseMessage getHttpResponseMessage = await httpClient.GetAsync(
                $"/api/companies/{companyCreated?.Id}");

            // Then
            Assert.Equal(HttpStatusCode.OK, getHttpResponseMessage.StatusCode);
            Company? companyRetrived = await getHttpResponseMessage.Content.ReadFromJsonAsync<Company>();
            Assert.NotNull(companyRetrived);
            Assert.Equal(companyGiven.Name, companyRetrived.Name);
        }

        [Fact]
        public async Task Should_return_404_when_get_company_given_a_non_exist_company_id()
        {
            // Given
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("BlueSky Digital Media");
            HttpResponseMessage createHttpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            Company? companyCreated = await createHttpResponseMessage.Content.ReadFromJsonAsync<Company>();

            // When
            HttpResponseMessage getHttpResponseMessage = await httpClient.GetAsync(
                $"/api/companies/randomId");

            // Then
            Assert.Equal(HttpStatusCode.NotFound, getHttpResponseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_200_when_update_an_exist_company()
        {
            // Given
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("BlueSky Digital Media");
            HttpResponseMessage createHttpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            Company? companyCreated = await createHttpResponseMessage.Content.ReadFromJsonAsync<Company>();

            // When
            companyCreated.Name = "ChangedName";
            HttpResponseMessage putHttpResponseMessage = await httpClient.PutAsJsonAsync(
                $"/api/companies/{companyCreated.Id}",companyCreated);

            // Then
            Assert.Equal(HttpStatusCode.OK, putHttpResponseMessage.StatusCode);
            Company? company = await putHttpResponseMessage.Content.ReadFromJsonAsync<Company>();
            Assert.NotNull(company);
            Assert.Equal(companyCreated.Name, company.Name);
        }

        [Fact]
        public async Task Should_return_404_when_update_an_non_exist_company()
        {
            // Given
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("BlueSky Digital Media");
            HttpResponseMessage createHttpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            Company? companyCreated = await createHttpResponseMessage.Content.ReadFromJsonAsync<Company>();

            // When
            companyCreated.Name = "ChangedName";
            HttpResponseMessage putHttpResponseMessage = await httpClient.PutAsJsonAsync(
                $"/api/companies/wrongId", companyCreated);

            // Then
            Assert.Equal(HttpStatusCode.NotFound, putHttpResponseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_companies_with_status_200_when_get_company_given_page_and_index()
        {
            // Given
            await ClearDataAsync();
            await httpClient.PostAsJsonAsync("/api/companies", new CreateCompanyRequest("1"));
            await httpClient.PostAsJsonAsync("/api/companies", new CreateCompanyRequest("2"));
            await httpClient.PostAsJsonAsync("/api/companies", new CreateCompanyRequest("3"));
            await httpClient.PostAsJsonAsync("/api/companies", new CreateCompanyRequest("4"));
            int pageIndex = 2;
            int pageSize = 2;
            string url = $"/api/companies?pageSize={pageSize}&pageIndex={pageIndex}";

            // When
            HttpResponseMessage getHttpResponseMessage = await httpClient.GetAsync(url);

            // Then
            Assert.Equal(HttpStatusCode.OK, getHttpResponseMessage.StatusCode);
            List<Company>? companies = await getHttpResponseMessage.Content.ReadFromJsonAsync<List<Company>>();
            Assert.NotNull(companies);
            Assert.Equal("3", companies[0].Name);
            Assert.Equal("4", companies[1].Name);
        }

        [Fact]
        public async Task Should_return_status_204_when_get_company_given_page_and_index_out_of_range()
        {
            // Given
            await ClearDataAsync();
            await httpClient.PostAsJsonAsync("/api/companies", new CreateCompanyRequest("1"));
            await httpClient.PostAsJsonAsync("/api/companies", new CreateCompanyRequest("2"));
            await httpClient.PostAsJsonAsync("/api/companies", new CreateCompanyRequest("3"));
            await httpClient.PostAsJsonAsync("/api/companies", new CreateCompanyRequest("4"));
            int pageIndex = 2;
            int pageSize = 3;
            string url = $"/api/companies?pageSize={pageSize}&pageIndex={pageIndex}";

            // When
            HttpResponseMessage getHttpResponseMessage = await httpClient.GetAsync(url);

            // Then
            Assert.Equal(HttpStatusCode.NoContent, getHttpResponseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_created_employee_with_status_201_given_an_employee_and_Company()
        {
            // Given
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("BlueSky Digital Media");
            HttpResponseMessage createHttpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            Company? companyCreated = await createHttpResponseMessage.Content.ReadFromJsonAsync<Company>();

            // When
            string createEmployeeUrl = $"/api/companies/{companyCreated?.Id}/employees";
            CreateEmployeeRequest employeeRequest = new CreateEmployeeRequest("Zhang San", 10000);
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync(createEmployeeUrl, employeeRequest);
            HttpResponseMessage getCompanyHttpResponseMessage = await httpClient.GetAsync($"/api/companies/{companyCreated?.Id}");

            // Then
            Assert.Equal(HttpStatusCode.Created, httpResponseMessage.StatusCode);
            Employee? employee = await httpResponseMessage.Content.ReadFromJsonAsync<Employee>();
            Company? company = await getCompanyHttpResponseMessage.Content.ReadFromJsonAsync<Company>();
            Assert.NotNull(employee);
            Assert.NotNull(employee.Id);
            Assert.Equal(employeeRequest.Name, employee.Name);
            Assert.Equal(employeeRequest.Salary, employee.Salary);
            Assert.Single(company.Employees);
        }

        [Fact]
        public async Task Should_return_created_employee_with_status_404_given_an_employee_with_Company_non_exist()
        {
            // Given
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("BlueSky Digital Media");
            HttpResponseMessage createHttpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            Company? companyCreated = await createHttpResponseMessage.Content.ReadFromJsonAsync<Company>();

            // When
            string createEmployeeUrl = $"/api/companies/wrongCompanyId/employees";
            CreateEmployeeRequest employeeRequest = new CreateEmployeeRequest("Zhang San", 10000);
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync(createEmployeeUrl, employeeRequest);
            //HttpResponseMessage getCompanyHttpResponseMessage = await httpClient.GetAsync($"/api/companies/{companyCreated?.Id}");

            // Then
            Assert.Equal(HttpStatusCode.NotFound, httpResponseMessage.StatusCode);
            //Employee? employee = await httpResponseMessage.Content.ReadFromJsonAsync<Employee>();
            //Company? company = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();
            //Assert.NotNull(employee);
            //Assert.NotNull(employee.Id);
            //Assert.Equal(employeeRequest.Name, employee.Name);
            //Assert.Equal(employeeRequest.Salary, employee.Salary);
            //Assert.Equal(1, company.Employees.Count());
        }

        private async Task<T?> DeserializeTo<T>(HttpResponseMessage httpResponseMessage)
        {
            string response = await httpResponseMessage.Content.ReadAsStringAsync();
            T? deserializedObject = JsonConvert.DeserializeObject<T>(response);
            return deserializedObject;
        }

        private static StringContent SerializeObjectToContent<T>(T objectGiven)
        {
            return new StringContent(JsonConvert.SerializeObject(objectGiven), Encoding.UTF8, "application/json");
        }

        private async Task ClearDataAsync()
        {
            await httpClient.DeleteAsync("/api/companies");
        }
    }
}