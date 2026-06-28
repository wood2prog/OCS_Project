using Bunit;
using JobTracking.App.Components;
using JobTracking.App.Models;
using Microsoft.AspNetCore.Components;

namespace JobTracking.Tests;

public class EditJobModalTest : BunitContext
{
    private static Job SampleJob() => new()
    {
        Id = 1,
        JobNumber = 1001,
        Customer = new Customer { Id = 1, Name = "Alpha Co" },
        JobName = "Kitchen Remodel",
        LeadDate = new DateTime(2026, 6, 1),
        StartDate = new DateTime(2026, 6, 15),
        DeliveryDate = new DateTime(2026, 7, 15),
        QuoteAmount = 5000m,
    };

    [Fact]
    public void Renders_modal_title()
    {
        var cut = Render<EditJobModal>(p =>
        {
            p.Add(c => c.Job, SampleJob());
            p.Add(c => c.OnDismiss, EventCallback.Factory.Create(this, () => { }));
        });

        Assert.Contains("Edit Job", cut.Find(".modal-title").TextContent);
    }

    [Fact]
    public void Pre_populates_job_name()
    {
        var cut = Render<EditJobModal>(p =>
        {
            p.Add(c => c.Job, SampleJob());
            p.Add(c => c.OnDismiss, EventCallback.Factory.Create(this, () => { }));
        });

        var input = cut.Find("#edit-jobName");
        Assert.NotNull(input);
        Assert.Equal("Kitchen Remodel", input.GetAttribute("value"));
    }

    [Fact]
    public void Pre_populates_dates()
    {
        var cut = Render<EditJobModal>(p =>
        {
            p.Add(c => c.Job, SampleJob());
            p.Add(c => c.OnDismiss, EventCallback.Factory.Create(this, () => { }));
        });

        var dateInputs = cut.FindAll("input[type=date]");
        Assert.Equal(3, dateInputs.Count);
    }

    [Fact]
    public void Pre_populates_quote_amount()
    {
        var cut = Render<EditJobModal>(p =>
        {
            p.Add(c => c.Job, SampleJob());
            p.Add(c => c.OnDismiss, EventCallback.Factory.Create(this, () => { }));
        });

        var amountInput = cut.Find("#edit-quoteAmount");
        Assert.NotNull(amountInput);
    }

    [Fact]
    public void Shows_customer_as_read_only_label()
    {
        var cut = Render<EditJobModal>(p =>
        {
            p.Add(c => c.Job, SampleJob());
            p.Add(c => c.OnDismiss, EventCallback.Factory.Create(this, () => { }));
        });

        var customerLabel = cut.Find(".customer-label");
        Assert.Contains("Alpha Co", customerLabel.TextContent);
    }

    [Fact]
    public void No_customer_dropdown()
    {
        var cut = Render<EditJobModal>(p =>
        {
            p.Add(c => c.Job, SampleJob());
            p.Add(c => c.OnDismiss, EventCallback.Factory.Create(this, () => { }));
        });

        Assert.Equal(0, cut.FindAll("select").Count);
    }

    [Fact]
    public void Save_button_disabled_when_job_name_empty()
    {
        var cut = Render<EditJobModal>(p =>
        {
            p.Add(c => c.Job, SampleJob());
            p.Add(c => c.OnDismiss, EventCallback.Factory.Create(this, () => { }));
        });

        var nameInput = cut.Find("#edit-jobName");
        nameInput.Change("");

        var saveBtn = cut.Find(".save-button");
        Assert.True(saveBtn.HasAttribute("disabled"));
    }

    [Fact]
    public void Save_button_enabled_when_job_name_filled()
    {
        var cut = Render<EditJobModal>(p =>
        {
            p.Add(c => c.Job, SampleJob());
            p.Add(c => c.OnDismiss, EventCallback.Factory.Create(this, () => { }));
        });

        var saveBtn = cut.Find(".save-button");
        Assert.False(saveBtn.HasAttribute("disabled"));
    }

    [Fact]
    public void Clicking_save_fires_OnJobUpdated_with_modified_data()
    {
        UpdateJobRequest? captured = null;
        var cut = Render<EditJobModal>(p =>
        {
            p.Add(c => c.Job, SampleJob());
            p.Add(c => c.OnJobUpdated, EventCallback.Factory.Create<UpdateJobRequest>(this, r => captured = r));
            p.Add(c => c.OnDismiss, EventCallback.Factory.Create(this, () => { }));
        });

        var nameInput = cut.Find("#edit-jobName");
        nameInput.Change("Renovated Kitchen");

        cut.Find(".save-button").Click();

        Assert.NotNull(captured);
        Assert.Equal("Renovated Kitchen", captured!.JobName);
    }

    [Fact]
    public void Clicking_cancel_fires_OnDismiss()
    {
        var dismissed = false;
        var cut = Render<EditJobModal>(p =>
        {
            p.Add(c => c.Job, SampleJob());
            p.Add(c => c.OnDismiss, EventCallback.Factory.Create(this, () => dismissed = true));
        });

        cut.Find(".cancel-button").Click();

        Assert.True(dismissed);
    }
}
