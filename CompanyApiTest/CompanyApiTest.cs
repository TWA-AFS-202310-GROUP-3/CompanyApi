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

        [Fact]
        public async Task Should_return_all_companies_with_status_200_when_GetAll_given_without_company_name()
        {
            // Given
            await ClearDataAsync();
            var companyGiven = new CreateCompanyRequest("BlueSky Digital Media");
            await httpClient.PostAsJsonAsync("api/companies", companyGiven);

            // When
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("api/companies");

            // Then
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode); //ok = 200
            //Assert.Equal(companyGiven,await httpResponseMessage.Content.ReadFromJsonAsync<ListCompany>>());

        }

        [Fact]
        public async Task Should_return_company_name_with_status_200_when_Get_given_company_name_and_company_exist()
        {
            await ClearDataAsync();
            var companyGiven = new CreateCompanyRequest("BlueSky Digital Media");
            var createResponse = await httpClient.PostAsync(
                "/api/companies",
                SerializeObjectToContent(companyGiven));
            var company = await createResponse.Content.ReadFromJsonAsync<Company>();
            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("api/companies" + $"/{company?.Id}");
            var companyGet = await httpResponseMessage.Content.ReadFromJsonAsync<Company>();

            // Then
            Assert.Equal(company?.Id, companyGet?.Id);
            Assert.Equal(HttpStatusCode.OK, httpResponseMessage.StatusCode);
        }

        [Fact]
        public async Task Should_return_status_404_when_Get_given_company_name_and_company_do_not_exist()
        {
            await ClearDataAsync();

            HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("api/companies/blueSky Digital Media");

            Assert.Equal(HttpStatusCode.NotFound, httpResponseMessage.StatusCode); //NotFound = 404
        }
    }
}