using CompanyApi;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
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
            CreateCompanyRequest companyGiven = new CreateCompanyRequest() { Name = "BlueSky Digital Media" };

            // When
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
           
            // Then
            Assert.Equal(HttpStatusCode.Created, httpResponseMessage.StatusCode);
            Company? companyCreated = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();
            Assert.NotNull(companyCreated);
            Assert.NotNull(companyCreated.Id);
            Assert.Equal(companyGiven.Name, companyCreated.Name);
        }

        [Fact]
        public async Task Should_return_bad_reqeust_when_create_company_given_a_existed_company_name()
        {
            // Given
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest() { Name = "BlueSky Digital Media" };
            await httpClient.PostAsJsonAsync("/api/companies", companyGiven);

            // When
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);

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

        private async Task ClearDataAsync()
        {
            await httpClient.DeleteAsync("/api/companies");
        }

        [Fact]
        public async Task Should_return_company_list_with_statuscode_200_when_get_given_without_company_id()
        {
            // Given 
            await ClearDataAsync();
            // When 
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("/api/companies");
            // Then
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
            Assert.Equal(new List<Company>(), await httpResponseMessage.Content.ReadFromJsonAsync<List<Company>>());
        }

        [Fact]
        public async Task Should_return_a_company_with_statuscode_200_when_get_given_company_id()
        {
            // Given
            await ClearDataAsync();
            Company companyGiven = new Company("BlueSky Digital Media");
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            Company company = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();
            string targetId = company.Id;
            // When
            HttpResponseMessage httpResponseMessage2 = await httpClient.GetAsync($"/api/companies/{targetId}" );

            // Then
            Assert.Equal(HttpStatusCode.Created, httpResponseMessage.StatusCode);
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage2.StatusCode);
            //Assert.Equal(companyGiven.Name, await httpResponseMessage2.Content.ReadFromJsonAsync);
        }

        [Fact]
        public async Task Should_return_not_found_when_get_given_not_existing_id()
        {
            // Given
            await ClearDataAsync();
            CreateCompanyRequest companyGiven = new CreateCompanyRequest() { Name = "BlueSky Digital Media" };
            HttpResponseMessage message = await httpClient.PostAsJsonAsync("/api/companies", companyGiven);
            var deletedId = message.Content.ReadFromJsonAsync<Company>().Id.ToString();
            // When 
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync($"/api/companies/{deletedId}");

            //Then
            Assert.Equal(HttpStatusCode.NotFound, httpResponseMessage.StatusCode);

        }

        [Fact]
        public async Task Should_return_company_list_with_statuscode_200_when_get_companies_with_paging()
        {

        }

        [Fact]
        public async Task Should_return_not_found_when_update_given_invalid_id()
        {

        }
    }
}