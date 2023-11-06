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
        public async Task Should_return_success_code_when_add_employee_to_the_company_given_companyId_and_new_Employee()
        {
            //given
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("BlueSky Digital Media");
            HttpResponseMessage postResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            var createdCompany = await postResponseMessage.Content.ReadFromJsonAsync<Company>();
            var createdEmployee = new EmployeeRequest("pengyu");

            //when
            HttpResponseMessage postResponseMessage2 = await httpClient.PostAsJsonAsync($"/api/companies/{createdCompany?.Id}/employees", createdEmployee);
            //var createdEmployee2 = await postResponseMessage2.Content.ReadFromJsonAsync<Employee>();
            var newCompany = await (await httpClient.GetAsync($"api/companies/{createdCompany?.Id}")).Content.ReadFromJsonAsync<Company>();

            //then
            Assert.Equal(HttpStatusCode.Created, postResponseMessage2.StatusCode);

        }

        [Fact]
        public async Task Should_return_all_companies_in_one_page_when_given_pageSize_and_pageIndex()
        {
            //given
            await ClearDataAsync();
            for (int i = 0; i < 10; i++)
            {
                CreateCompanyRequest companyGiven = new CreateCompanyRequest($"BlueSky Digital Media{i}");
                await httpClient.PostAsJsonAsync("api/companies", companyGiven);
            }

            //when
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync($"api/companies?pageSize=2&pageIndex=3");
            List<Company>? companies = await httpResponseMessage.Content.ReadFromJsonAsync<List<Company>>();

            //then
            Assert.Equal("BlueSky Digital Media4", companies?[0].Name);
            Assert.Equal(2, companies?.Count);
        }

        [Fact]
        public async Task Should_return_success_code_when_update_successfully_given_existed_company()
        {
            //given
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("BlueSky Digital Media");
            CreateCompanyRequest newCompany = new CreateCompanyRequest("SLB");
            HttpResponseMessage postResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            var createdCompany = await postResponseMessage.Content.ReadFromJsonAsync<Company>();

            //when
            HttpResponseMessage postResponseMessage2 = await httpClient.PutAsJsonAsync($"/api/companies/{createdCompany?.Id}", newCompany);

            //then
            Assert.Equal(HttpStatusCode.NoContent, postResponseMessage2.StatusCode);
        }

        [Fact]
        public async Task Should_return_bad_request_when_update_given_unexisted_company()
        {
            //given
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest("BlueSky Digital Media");

            //when
            HttpResponseMessage httpResponseMessage = await httpClient.PutAsJsonAsync("api/companies/unexistedcompany", companyGiven);

            //then
            Assert.Equal(HttpStatusCode.NotFound, httpResponseMessage.StatusCode);
        }

        [Fact]
         public async Task Should_return_correct_company_when_send_a_company_id()
        {
            //given
            await ClearDataAsync();
            Company companyGiven = new Company("BlueSky Digital Media");
            HttpResponseMessage postResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            var createdCompany = await postResponseMessage.Content.ReadFromJsonAsync<Company>();

            //when
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync($"/api/companies/{createdCompany?.Id}");
            var response = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();

            //then
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            Assert.Equal(companyGiven.Name, response?.Name);
        }

        [Fact]
        public async Task Should_return_bad_reqeust_when_request_company_given_an_unexisted_company_id()
        {
            //given
            await ClearDataAsync();

            // When
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync($"/api/companies/unexistedcompany");

            // Then
            Assert.Equal(HttpStatusCode.NotFound, httpResponseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_all_companies_when_given_no_company_id()
        {
            //given
            await ClearDataAsync();
            Company companyGiven = new Company("BlueSky Digital Media");

            //when
            await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("/api/companies");
            var response = await httpResponseMessage.Content.ReadFromJsonAsync<List<Company>>();

            //then
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            Assert.Equal(companyGiven.Name, response?[0].Name);
        }

        [Fact]
        public async Task Should_return_created_company_with_status_201_when_create_cpmoany_given_a_company_name()
        {
            // Given
            await ClearDataAsync();
            Company companyGiven = new Company("BlueSky Digital Media");
            
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
            Company companyGiven = new Company("BlueSky Digital Media");

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