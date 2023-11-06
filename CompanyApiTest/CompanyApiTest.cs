using CompanyApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
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

        [Fact]
        public async Task Should_return_all_companies_list_when_get_all_companies()
        {
            // Given
            await ClearDataAsync();
            Company companyGiven = new Company("slb");
            await httpClient.PostAsJsonAsync("/api/companies",companyGiven);

            // When
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("/api/companies/all");
            var result = await httpResponseMessage.Content.ReadFromJsonAsync<List<Company>>();

            // Then
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            Assert.Equal(companyGiven.Name, result[0].Name);
        }

        [Fact]
        public async Task Should_return_exist_company_when_get_the_exist_companies_given_company_name()
        {
            // Given
            await ClearDataAsync();
            Company companyGiven = new Company("slb");
            HttpResponseMessage postResponseMessage =  await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            var createdCompany = await postResponseMessage.Content.ReadFromJsonAsync<Company>();

            // When
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync($"/api/companies/{createdCompany.Id}");
            var result = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();

            // Then
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            Assert.Equal(companyGiven.Name, result.Name);
        }

        [Fact]
        public async Task Should_return_bad_resquest_when_get_the_exist_companies_given_not_exist_company_name()
        {
            // Given
            await ClearDataAsync();
            //Company companyGiven = new Company("slb");
            string nonExistentCompanyId = "nonexistent-id";

            // When
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync($"/api/companies/{nonExistentCompanyId}");
            //var result = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();

            // Then
            Assert.Equal(HttpStatusCode.NotFound, httpResponseMessage.StatusCode);
            //Assert.Equal(companyGiven.Name, result.Name);
        }

        [Fact]
        public async Task Should_return_X_companies_starting_from_Y_index_when_give_x_and_y()
        {
            // Given
            await ClearDataAsync();
            Company companyGiven1 = new Company("slb");
            Company companyGiven2 = new Company("slb1");
            Company companyGiven3 = new Company("slb2");
            Company companyGiven4 = new Company("slb3");
            Company companyGiven5 = new Company("slb4");
            List<Company> companies = new List<Company> { companyGiven1, companyGiven2, companyGiven3, companyGiven4, companyGiven5};
            await AddCompaniesAsync(companies);

            int pageSize = 2;
            int pageIndex = 2;
            //When
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync($"/api/companies?pageSize={pageSize}&pageIndex={pageIndex}");
            var result = await httpResponseMessage.Content.ReadFromJsonAsync<List<Company>>();
            //Then
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            Assert.Equal(pageSize, result.Count);
            Assert.Equal(companyGiven3.Name, result[0].Name);
            Assert.Equal(companyGiven4.Name, result[1].Name);
        }

        [Fact]
        public async Task Should_return_bad_request_when_give_x_or_y_wrong()
        {
            // Given
            await ClearDataAsync();
            Company companyGiven1 = new Company("slb");
            Company companyGiven2 = new Company("slb1");
            Company companyGiven3 = new Company("slb2");
            Company companyGiven4 = new Company("slb3");
            Company companyGiven5 = new Company("slb4");
            List<Company> companies = new List<Company> { companyGiven1, companyGiven2, companyGiven3, companyGiven4, companyGiven5 };
            await AddCompaniesAsync(companies);

            int pageSize = -2;
            int pageIndex = -1;
            //When
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync($"/api/companies?pageSize={pageSize}&pageIndex={pageIndex}");
            //Then
            Assert.Equal(HttpStatusCode.BadRequest, httpResponseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_update_basic_information_when_update_company_given_company_id()
        {
            //Given
            await ClearDataAsync();
            Company companyGiven = new Company("slb");
            HttpResponseMessage postResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            var createdCompany = await postResponseMessage.Content.ReadFromJsonAsync<Company>();

            string updatedName = "google";
            companyGiven.Name = updatedName;
            //When
            HttpResponseMessage response = await httpClient.PutAsJsonAsync($"/api/companies/{createdCompany.Id}", companyGiven);
            HttpResponseMessage getResponse = await httpClient.GetAsync($"/api/companies/{createdCompany.Id}");
            var updatedCompany = await getResponse.Content.ReadFromJsonAsync<Company>();
            //Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            Assert.Equal(updatedName, updatedCompany.Name);
        }

        [Fact]
        public async Task Should_add_employee_to_specific_company()
        {
            // Given
            Company companyGiven = new Company("slb");
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies",companyGiven);
            Company companyCreated = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();
            Employee employeeGiven = new Employee("xianke");

            // When
            httpResponseMessage = await httpClient.PostAsJsonAsync($"/api/companies/{companyCreated?.Id}/employees", employeeGiven);
            Employee employeeCreated = await httpResponseMessage.Content.ReadFromJsonAsync<Employee>();

            // Then
            Assert.Equal(HttpStatusCode.Created, httpResponseMessage.StatusCode);
            Assert.Equal(employeeGiven.Name, employeeCreated.Name);
        }

        [Fact]
        public async Task Should_return_bad_reqeust_when_add_to_specific_employee_given_wrong_company()
        {
            // Given
            Employee employeeGiven = new Employee("xianke");

            // When
            var httpResponseMessage = await httpClient.PostAsJsonAsync($"/api/companies/{new Guid()}/employees", employeeGiven);
            Employee? employeeCreated = await httpResponseMessage.Content.ReadFromJsonAsync<Employee>();

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

        //private async Task AddCompanyAsync(Company company)
        //{
        //    HttpResponseMessage postResponse = await httpClient.PostAsJsonAsync("/api/companies", company);
        //    postResponse.EnsureSuccessStatusCode();
        //}

        private async Task AddCompaniesAsync(List<Company> companies)
        {
            foreach (var company in companies)
            {
                HttpResponseMessage postResponse = await httpClient.PostAsJsonAsync("/api/companies", company);
                postResponse.EnsureSuccessStatusCode();
            }
        }
    }
}