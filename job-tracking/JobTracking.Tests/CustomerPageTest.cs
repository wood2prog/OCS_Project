using Bunit;
using JobTracking.App.Models;
using JobTracking.App.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace JobTracking.Tests;

public class CustomerPageTest : BunitContext
{
    private static List<Customer> SampleCustomers() =>
    [
        new() { Id = 1, Name = "Alpha Co", Email = "alpha@test.com", Phone = "555-0101", Notes = "First customer", JobCount = 3 },
        new() { Id = 2, Name = "Beta Inc", Email = null, Phone = "555-0102", Notes = null, JobCount = 1 },
    ];

    private sealed class MockCustomerService : ICustomerService
    {
        private readonly List<Customer> _customers;

        public MockCustomerService(List<Customer>? customers = null)
        {
            _customers = customers ?? SampleCustomers();
        }

        public Task<List<Customer>> GetCustomersAsync() => Task.FromResult(_customers.ToList());
        public Task<Customer> CreateCustomerAsync(CreateCustomerRequest dto) =>
            Task.FromResult(new Customer { Id = 99, Name = dto.Name, Email = dto.Email, Phone = dto.Phone, Notes = dto.Notes });

        public Task<Customer> UpdateCustomerAsync(int id, UpdateCustomerRequest dto) =>
            Task.FromResult(new Customer { Id = id, Name = dto.Name ?? "Test", Email = dto.Email, Phone = dto.Phone, Notes = dto.Notes });

        public Task DeleteCustomerAsync(int id) => Task.CompletedTask;
    }

    [Fact]
    public void Shows_empty_state_when_no_customer_selected()
    {
        Services.AddSingleton<ICustomerService>(new MockCustomerService());
        var cut = Render<App.Pages.Customers>();

        Assert.Contains("Select a customer to view details", cut.Find(".empty-detail").TextContent);
    }

    [Fact]
    public void Renders_all_customers_in_list()
    {
        Services.AddSingleton<ICustomerService>(new MockCustomerService());
        var cut = Render<App.Pages.Customers>();

        var rows = cut.FindAll(".customer-row");
        Assert.Equal(2, rows.Count);
        Assert.Contains("Alpha Co", rows[0].TextContent);
        Assert.Contains("Beta Inc", rows[1].TextContent);
    }

    [Fact]
    public void Clicking_customer_shows_detail_with_all_fields()
    {
        Services.AddSingleton<ICustomerService>(new MockCustomerService());
        var cut = Render<App.Pages.Customers>();

        cut.FindAll(".customer-row")[0].Click();

        Assert.Contains("Alpha Co", cut.Find(".detail-title").TextContent);
        Assert.Contains("alpha@test.com", cut.Find(".detail-fields").TextContent);
        Assert.Contains("555-0101", cut.Find(".detail-fields").TextContent);
        Assert.Contains("First customer", cut.Find(".detail-fields").TextContent);
        Assert.Contains("3 job(s)", cut.Find(".job-count-badge").TextContent);
    }

    [Fact]
    public void Shows_contact_info_in_list_row()
    {
        Services.AddSingleton<ICustomerService>(new MockCustomerService());
        var cut = Render<App.Pages.Customers>();

        var rows = cut.FindAll(".customer-row");
        Assert.Contains("alpha@test.com", rows[0].TextContent);
        Assert.Contains("555-0102", rows[1].TextContent);
    }

    [Fact]
    public void Selected_customer_is_highlighted()
    {
        Services.AddSingleton<ICustomerService>(new MockCustomerService());
        var cut = Render<App.Pages.Customers>();

        cut.FindAll(".customer-row")[0].Click();

        var rows = cut.FindAll(".customer-row");
        Assert.Contains("selected", rows[0].ClassName);
        Assert.DoesNotContain("selected", rows[1].ClassName);
    }

    [Fact]
    public void Search_filters_customer_list()
    {
        Services.AddSingleton<ICustomerService>(new MockCustomerService());
        var cut = Render<App.Pages.Customers>();

        cut.Find(".search-input").Change("Beta");

        var rows = cut.FindAll(".customer-row");
        Assert.Single(rows);
        Assert.Contains("Beta Inc", rows[0].TextContent);
    }

    [Fact]
    public void Search_shows_empty_when_no_match()
    {
        Services.AddSingleton<ICustomerService>(new MockCustomerService());
        var cut = Render<App.Pages.Customers>();

        cut.Find(".search-input").Change("NonExistent");

        Assert.Contains("No customers found", cut.Find(".empty-list").TextContent);
    }

    [Fact]
    public void Add_modal_opens_and_saves()
    {
        var captured = new List<CreateCustomerRequest>();

        var mock = new MockCustomerService();
        Services.AddSingleton<ICustomerService>(mock);
        var cut = Render<App.Pages.Customers>();

        cut.Find(".add-button").Click();

        Assert.Contains("Add Customer", cut.Find(".modal-title").TextContent);

        cut.Find("#add-name").Change("New Customer");
        cut.Find("#add-email").Change("new@test.com");
        cut.Find(".create-button").Click();

        Assert.Contains("New Customer", cut.Find(".detail-title").TextContent);
    }

    [Fact]
    public void Add_modal_save_disabled_when_name_empty()
    {
        Services.AddSingleton<ICustomerService>(new MockCustomerService());
        var cut = Render<App.Pages.Customers>();

        cut.Find(".add-button").Click();

        var saveBtn = cut.Find(".create-button");
        Assert.True(saveBtn.HasAttribute("disabled"));
    }

    [Fact]
    public void Add_modal_cancel_dismisses()
    {
        Services.AddSingleton<ICustomerService>(new MockCustomerService());
        var cut = Render<App.Pages.Customers>();

        cut.Find(".add-button").Click();
        cut.Find(".cancel-button").Click();

        Assert.Equal(0, cut.FindAll(".modal-backdrop").Count);
    }

    [Fact]
    public void Edit_modal_pre_populates_fields()
    {
        Services.AddSingleton<ICustomerService>(new MockCustomerService());
        var cut = Render<App.Pages.Customers>();

        cut.FindAll(".customer-row")[0].Click();
        cut.Find(".edit-button").Click();

        Assert.Contains("Edit Customer", cut.Find(".modal-title").TextContent);
        Assert.Equal("Alpha Co", cut.Find("#edit-name").GetAttribute("value"));
        Assert.Equal("alpha@test.com", cut.Find("#edit-email").GetAttribute("value"));
        Assert.Equal("555-0101", cut.Find("#edit-phone").GetAttribute("value"));
    }

    [Fact]
    public void Edit_modal_save_disabled_when_name_cleared()
    {
        Services.AddSingleton<ICustomerService>(new MockCustomerService());
        var cut = Render<App.Pages.Customers>();

        cut.FindAll(".customer-row")[0].Click();
        cut.Find(".edit-button").Click();

        cut.Find("#edit-name").Change("");

        var saveBtn = cut.Find(".save-button");
        Assert.True(saveBtn.HasAttribute("disabled"));
    }

    [Fact]
    public void Delete_confirmation_shows_customer_name()
    {
        Services.AddSingleton<ICustomerService>(new MockCustomerService());
        var cut = Render<App.Pages.Customers>();

        cut.FindAll(".customer-row")[0].Click();
        cut.Find(".delete-button-sm").Click();

        Assert.Contains("Delete Customer", cut.Find(".modal-title").TextContent);
        Assert.Contains("Alpha Co", cut.Find(".confirm-message").TextContent);
    }

    [Fact]
    public void Delete_cancel_dismisses()
    {
        Services.AddSingleton<ICustomerService>(new MockCustomerService());
        var cut = Render<App.Pages.Customers>();

        cut.FindAll(".customer-row")[0].Click();
        cut.Find(".delete-button-sm").Click();
        cut.Find(".cancel-button").Click();

        Assert.Equal(0, cut.FindAll(".modal-backdrop").Count);
    }

    [Fact]
    public void Delete_confirmed_removes_customer()
    {
        Services.AddSingleton<ICustomerService>(new MockCustomerService());
        var cut = Render<App.Pages.Customers>();

        cut.FindAll(".customer-row")[0].Click();
        cut.Find(".delete-button-sm").Click();
        cut.Find(".delete-button").Click();

        var rows = cut.FindAll(".customer-row");
        Assert.Single(rows);
        Assert.Contains("Beta Inc", rows[0].TextContent);
    }

    [Fact]
    public void Shows_add_customer_button()
    {
        Services.AddSingleton<ICustomerService>(new MockCustomerService());
        var cut = Render<App.Pages.Customers>();

        Assert.Contains("Add Customer", cut.Find(".add-button").TextContent);
    }
}
