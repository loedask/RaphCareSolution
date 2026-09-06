using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using RaphCare.Ui.Components;
using Xunit;

namespace RaphCare.Ops.Tests.Components;

/// <summary>
/// Regression: choosing an option (or a label-triggered ghost click on the trigger) must close the menu.
/// Users previously had to press Esc even after selecting a hospital.
/// </summary>
public sealed class SearchableSelectDismissTests : TestContext
{
    public SearchableSelectDismissTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void SelectingOption_ClosesMenuAndSetsValue()
    {
        string? selected = null;
        var cut = RenderComponent<SearchableSelect<string>>(parameters => parameters
            .Add(p => p.Placeholder, "Select hospital")
            .Add(p => p.Options,
            [
                new SelectOption<string>("demo", "RaphCare Demo Clinic"),
                new SelectOption<string>("direct", "RaphCare Direct")
            ])
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => selected = v)));

        cut.Find(".searchable-select-trigger").Click();
        Assert.Contains("open", cut.Find(".searchable-select").ClassList);

        cut.Find(".searchable-select-option").Click();

        Assert.Equal("demo", selected);
        Assert.DoesNotContain("open", cut.Find(".searchable-select").ClassList);
    }

    [Fact]
    public void GhostTriggerClickAfterSelect_DoesNotReopenMenu()
    {
        var cut = RenderComponent<SearchableSelect<string>>(parameters => parameters
            .Add(p => p.Options, [new SelectOption<string>("demo", "RaphCare Demo Clinic")])
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, _ => { })));

        cut.Find(".searchable-select-trigger").Click();
        cut.Find(".searchable-select-option").Click();
        Assert.DoesNotContain("open", cut.Find(".searchable-select").ClassList);

        // Simulates the synthetic click a wrapping <label> fires on the trigger after an option click.
        cut.Find(".searchable-select-trigger").Click();
        Assert.DoesNotContain("open", cut.Find(".searchable-select").ClassList);
    }

    [Fact]
    public void BackdropClick_ClosesMenuWithoutChangingValue()
    {
        string? selected = "keep";
        var cut = RenderComponent<SearchableSelect<string>>(parameters => parameters
            .Add(p => p.Value, selected)
            .Add(p => p.Options, [new SelectOption<string>("keep", "Keep me")])
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, v => selected = v)));

        cut.Find(".searchable-select-trigger").Click();
        Assert.Contains("open", cut.Find(".searchable-select").ClassList);

        cut.Find(".searchable-select-backdrop").Click();

        Assert.Equal("keep", selected);
        Assert.DoesNotContain("open", cut.Find(".searchable-select").ClassList);
    }
}
