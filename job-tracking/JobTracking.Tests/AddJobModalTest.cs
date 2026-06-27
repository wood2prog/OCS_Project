using Bunit;
using JobTracking.App.Components;
using JobTracking.App.Models;
using Microsoft.AspNetCore.Components;

namespace JobTracking.Tests;

public class AddJobModalTest : BunitContext
{
    private static List<Customer> SampleCustomers() =>
    [
        new() { Id = 1, Name = "Alpha Co" },
        new() { Id = 2, Name = "Beta Inc" },
    ];

    [Fact]
    public void Renders_modal_title()
    {
        var cut = Render<AddJobModal>(p =>
        {
            p.Add(c => c.Customers, SampleCustomers());
            p.Add(c => c.OnDismiss, EventCallback.Factory.Create(this, () => { }));
        });

        Assert.Contains("Add Job", cut.Find(".modal-title").TextContent);
    }

    [Fact]
    public void Renders_customer_dropdown()
    {
        var cut = Render<AddJobModal>(p =>
        {
            p.Add(c => c.Customers, SampleCustomers());
            p.Add(c => c.OnDismiss, EventCallback.Factory.Create(this, () => { }));
        });

        var select = cut.Find("select");
        var options = select.QuerySelectorAll("option");
        Assert.Equal(3, options.Length);
        Assert.Contains("Select a customer", options[0].TextContent);
        Assert.True(options[0].HasAttribute("disabled"));
        Assert.Contains("Alpha Co", options[1].TextContent);
        Assert.Contains("Beta Inc", options[2].TextContent);
    }

    [Fact]
    public void Create_button_disabled_when_no_customer_selected()
    {
        var cut = Render<AddJobModal>(p =>
        {
            p.Add(c => c.Customers, SampleCustomers());
            p.Add(c => c.OnDismiss, EventCallback.Factory.Create(this, () => { }));
        });

        var createBtn = cut.Find(".create-button");
        Assert.True(createBtn.HasAttribute("disabled"));
    }

    [Fact]
    public void Create_button_disabled_when_job_name_empty()
    {
        var cut = Render<AddJobModal>(p =>
        {
            p.Add(c => c.Customers, SampleCustomers());
            p.Add(c => c.OnDismiss, EventCallback.Factory.Create(this, () => { }));
        });

        cut.Find("select").Change("1");

        var createBtn = cut.Find(".create-button");
        Assert.True(createBtn.HasAttribute("disabled"));
    }

    [Fact]
    public void Create_button_enabled_when_customer_and_job_name_filled()
    {
        var cut = Render<AddJobModal>(p =>
        {
            p.Add(c => c.Customers, SampleCustomers());
            p.Add(c => c.OnDismiss, EventCallback.Factory.Create(this, () => { }));
        });

        cut.Find("select").Change("1");
        cut.Find("input[type=text]").Change("New Kitchen");

        var createBtn = cut.Find(".create-button");
        Assert.False(createBtn.HasAttribute("disabled"));
    }

    [Fact]
    public void Clicking_create_fires_OnJobCreated_with_form_data()
    {
        CreateJobRequest? captured = null;
        var cut = Render<AddJobModal>(p =>
        {
            p.Add(c => c.Customers, SampleCustomers());
            p.Add(c => c.OnJobCreated, EventCallback.Factory.Create<CreateJobRequest>(this, r => captured = r));
            p.Add(c => c.OnDismiss, EventCallback.Factory.Create(this, () => { }));
        });

        cut.Find("select").Change("2");
        cut.Find("input[type=text]").Change("Beta Office");

        var leadDateInput = cut.FindAll("input[type=date]")[0];
        leadDateInput.Change("2026-06-01");

        cut.Find(".create-button").Click();

        Assert.NotNull(captured);
        Assert.Equal(2, captured!.CustomerId);
        Assert.Equal("Beta Office", captured.JobName);
    }

    [Fact]
    public void Clicking_cancel_fires_OnDismiss()
    {
        var dismissed = false;
        var cut = Render<AddJobModal>(p =>
        {
            p.Add(c => c.Customers, SampleCustomers());
            p.Add(c => c.OnDismiss, EventCallback.Factory.Create(this, () => dismissed = true));
        });

        cut.Find(".cancel-button").Click();

        Assert.True(dismissed);
    }
}
