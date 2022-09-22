# Unit Tests Common Practices

## Naming Conventions

### Test class

The test class should follow the naming convention **`[ClassUnderTest]Tests`**.

Example: The test class for a class named ProductController should be named `ProductControllerTests`:

```csharp
[TestFixture]
public class ProductControllerTests
{
    ...
}
```

### Test method

The test method should follow the naming convention **`[MethodUnderTest]_[BehaviourToTest]_[ExpectedResult]`**.

Example: A method named GetProduct should be tested to see if it returns an existing product.
The name of the test should be `GetProduct_ProductExist_ProductReturned`:

```csharp
[Test]
public async Task GetProduct_ProductExist_ProductReturned()
{
    ...
}
```

## Unit Test Skeleton: Three Steps/Parts

A unit test should be devided into three steps:

1. Arrange: The first part where the input/expected data are defined
2. Act: The second part where the behavior under test is executed
3. Assert: The third and final part where assertions are made

These three parts are visually defined with comments so that unit tests are humanly comprehensible:

```csharp
[Test]
public async Task GetProduct_ProductExist_ProductReturned()
{
    // Arrange
    var productId = Guid.NewGuid().ToString();
    var expectedProduct = new Product
    {
        Id = productId
    };

    // Act
    var product = this.productService.GetProduct(productId);

    // Asset
    _ = product.Should().BeEquivalentTo(expectedProduct);
}
```

!!! Tip
    On the IoT Hub portal, we use the [fluentassertions](https://github.com/fluentassertions/fluentassertions) library for unit tests for natural/human reusable assertions.

## Mock

A unit test should only test its assigned layer. Any lower layer that requires/interacts with external resources should be mocked to ensure sure that the unit tests are idempotent.

!!! note
    Example: We want to implement unit tests for a controller that requires three services. Each service depends on other services/repositories/http clients that need external resources like databases, APIs...
    Any execution of unit tests that depend on these external resources can be altered (not idempotent) because they depend on the uptime and data of these resources.

On the IoT Hub portal, we use the library [Moq](https://github.com/moq/moq4) for mocking within unit tests:

```csharp
[TestFixture]
public class ProductControllerTests
{
    private MockRepository mockRepository;
    private Mock<IProductRepository> mockProductRepository;
    
    private IProductService productService;

    [SetUp]
    public void SetUp()
    {
        // Init MockRepository with strict behaviour
        this.mockRepository = new MockRepository(MockBehavior.Strict);
        // Init the mock of IProductRepository
        this.mockProductRepository = this.mockRepository.Create<IProductRepository>();
        // Init the service ProductService. The object mock ProductRepository is passed the contructor of ProductService
        this.productService = new ProductService(this.mockProductRepository.Object);
    }

    [Test]
    public async Task GetProduct_ProductExist_ProductReturned()
    {
        // Arrange
        var productId = Guid.NewGuid().ToString();
        var expectedProduct = new Product
        {
            Id = productId
        };
        
        // Setup mock of GetByIdAsync of the repository ProductRepository to return the expected product when given the correct product id
        _ = this.mockProductRepository.Setup(repository => repository.GetByIdAsync(productId))
                .ReturnsAsync(expectedProduct);

        // Act
        var product = this.productService.GetProduct(productId);

        // Asset
        _ = product.Should().BeEquivalentTo(expectedProduct);

        // Assert that all mocks setups have been called
        _ = MockRepository.VerifyAll();
    }
}
```
