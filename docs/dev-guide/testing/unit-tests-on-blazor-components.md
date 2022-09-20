# Unit Tests on Blazor components

!!! info
    To test Blazor components on Iot Hob Portal, we use the library [bUnit](https://bunit.dev/)

## How to unit test component

Let's say we have compoment ProductDetail to test.

```csharp title="Example of the content of the component ProductDetail"
@inject IProductService ProductService

@if(product != null)
{
    <p id="product-id">@product.Id</p>
}

@code {
    [Parameter]
    public string ProductId { get; set; }

    private Product product;

    protected override async Task OnInitializedAsync()
    {
        await GetProduct();
    }

    private async Task GetProduct()
    {
        try
        {
            product = await ProductService.GetProduct(ProductId);
        }
        catch (ProblemDetailsException exception)
        {
            Error?.ProcessProblemDetails(exception);
        }
    }
}
```

```csharp title="First you have to a unit test class that extend"
[TestFixture]
public class ProductDetailTests : BlazorUnitTest
{
}
```

!!! info
    The class [`BlazorUnitTest`](https://github.com/CGI-FR/IoT-Hub-Portal/blob/main/src/AzureIoTHub.Portal.Tests.Unit/UnitTests/Bases/BlazorUnitTest.cs)
    provides helpers/test context dedicated for unit testing on blazor component. Also it avoids code duplication unit tests classes.

```csharp title="Override the method Setup"
[TestFixture]
public class ProductDetailTests : BlazorUnitTest
{
    public override void Setup()
    {
        // Don't forget the method base.Setup() to initialize existing helpers
        base.Setup();
    }
}
```

```csharp title="Setup the mockup of the service IProductService"
[TestFixture]
public class ProductDetailTests : BlazorUnitTest
{
    // Declare the mock of IProductService
    private Mock<IProductService> productServiceMock;

    public override void Setup()
    {
        base.Setup();

        // Intialize the mock of IProductService
        this.productServiceMock = MockRepository.Create<IProductService>();

        // Add the mock of IProductService as a singleton for resolution 
        _ = Services.AddSingleton(this.productServiceMock.Object);
    }
}
```

!!! Info
    After the configuration of the setup of the test class, you can start implementing unit tests.

Below an exemple of a a unit test that assert that the method GetProduct of the serivce ProductService
has been called after the component has been initialized:

```csharp
[TestFixture]
public class ProductDetailTests : BlazorUnitTest
{
    ...

    [Test]
    public void OnInitializedAsync_GetProduct_ProductIsRetrieved()
    {
        // Arrange
        var expectedProduct = Fixture.Create<Product>();

        // Setup mock of GetProduct of the service ProductService
        _ = this.productServiceMock.Setup(service => service.GetProduct(expectedProduct.Id))
            .ReturnsAsync(expectedProduct);

        // Act
        // Render the component ProductDetail with the required ProductId parameter
        var cut = RenderComponent<ProductDetail>(ComponentParameter.CreateParameter("ProductId", expectedProduct.Id));
        // You can wait for a specific element to be rendered before assertions using a css selector, for example the DOM element with id product-id
        _ = cut.WaitForElement("#product-id");

        // Assert
        // Assert that all mocks setups have been called
        cut.WaitForAssertion(() => MockRepository.VerifyAll());
    }
}
```

!!! Tip
    `WaitForAssertion` is usefull with Assertion of asynchronous changes: It will block and wait in a
    test method until the provided assert action does not throw an exception, or until the timeout is reached (the default
    timeout is one second) :point_right: [Assertion of asynchronous changes](https://bunit.dev/docs/verification/async-assertion.html)

!!! Tip
    Within unit test on blazor components, you can interact/query rendered HTML DOM for find html elements (buttons, div...) using
    CSS selectors (id, class..) :point_right: Lean more about [CSS selectors](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_Selectors)

## How to unit test a component requiring an external component

Some components proposed by MudBlazor (MudAutocomplete, MudSelect...) use another component `MudPopoverProvider` to render elements.
Withing a unit test that uses these MudBlazor components, if the component `MudPopoverProvider` is not rendered, we will have limited interations with these components

Let's start with the example below:

```csharp title="Example of the content of the component SearchState"
<MudAutocomplete T="string" Label="US States" @bind-Value="selectedState" SearchFunc="@Search" />

@code {
    private string selectedState;
    private string[] states =
    {
        "Alabama", "Colorado", "Missouri", "Wisconsin"
    }

    private async Task<IEnumerable<string>> Search(string value)
    {
        // In real life use an asynchronous function for fetching data from an api.
        await Task.Delay(5);

        // if text is null or empty, show complete list
        if (string.IsNullOrEmpty(value)) 
            return states;
        return states.Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase));
    }
}
```

We want to test the search when a user interact with the component `MudAutocomplete` to search the state `Wisconsin`:

```csharp
[TestFixture]
public class SearchStateTests : BlazorUnitTest
{
    ...

    [Test]
    public void Search_UserSearchAndSelectState_StateIsSelected()
    {
        // Arrange
        var userQuery = "Wis";

        // First render MudPopoverProvider component
        var popoverProvider = RenderComponent<MudPopoverProvider>();
        // Second, rendrer the component SearchState (under unit test)
        var cut = RenderComponent<SearchState>();
        
        // Find the MudAutocomplete component within SearchState component
        var autocompleteComponent = cut.FindComponent<MudAutocomplete<string>>();

        // Fire click event on, 
        autocompleteComponent.Find("input").Click();
        autocompleteComponent.Find("input").Input(userQuery);

        // Wait until the count of element in the list rendred on the component MudPopoverProvider is equals to one
        popoverProvider.WaitForAssertion(() => popoverProvider.FindAll("div.mud-list-item").Count.Should().Be(1));

        // Act
        // Get the only element present on the list
        var stateElement = popoverProvider.Find("div.mud-list-item");
        // Fire click event on the element
        stateElement.Click();

        // Assert
        // Check if the MudAutocomplete compoment has been closed after the click event
        cut.WaitForAssertion(() => autocompleteComponent.Instance.IsOpen.Should().BeFalse());
        ...
    }
}
```
