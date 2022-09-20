# Unit Tests Common Practices

## Naming Conventions

### Test class

Test class should follow the naming convention **`[ClassUnderTest]Tests`**.

Example: The test class for a class named ProductController should be `ProductControllerTests`:

```csharp
[TestFixture]
public class ProductControllerTests
{
    ...
}
```

### Test method

Test method should follow the naming convention **`[MethodUnderTest]_[BehaviourToTest]_[ExpectedResult]`**.

Example: For a method called GetProduct, the behaviour that we want to test is that it should return an exsiting project.
The name of the test should be `GetProduct_ProductExist_ProductReturned`:

```csharp
[Test]
public async Task GetProduct_ProductExist_ProductReturned()
{
    ...
}
```

## Unit Test Skeleton: Three Steps/Parts

A unit test should be designed/cut into three steps:

1. Arrange: The first part where input/expected data are defined
2. Act: The second part where the behavior under test is executed
3. Assert: The third and last part where assertions are done

These three parts are visually defined with comments so that unit tests can be humain readeable:

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
    On IoT Hub Portal, we use the library [fluentassertions](https://github.com/fluentassertions/fluentassertions) on unit tests for natural/humain readeable assertions.

## Mock

A unit test should only test its dedicated layer. Any lower layer that requires/interact with external resources should be mocked to make sure that unit tests are idempotents.

!!! note
    For example, we want to implement unit tests on a controller that requires three services. Each service depends on others services/repositories/http clients that require external resources like databases, APIs...
    Any execution of unit tests that require on these external resources can be altered (not idempotent) because they depend on the uptime, the data of these resources.

On IoT Hub Portal, we use the library [Moq](https://github.com/moq/moq4) for mocking within unit tests:

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
