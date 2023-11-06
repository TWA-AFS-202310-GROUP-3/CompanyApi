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