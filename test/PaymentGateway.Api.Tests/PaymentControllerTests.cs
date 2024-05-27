using Xunit;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Controllers;
using PaymentGateway.Models;

public class PaymentControllerTests
{
    private HttpClient _httpClient;
    private readonly PaymentController _controller;

    public PaymentControllerTests()
    {
        // Initialize the HttpClient with a mock response
        _httpClient = HttpClientMockFactory.CreateMockClient(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new BankSimulatorResponse
            {
                Authorized = true,
                AuthorizationCode = "0bb07405-6d44-4b50-a14f-7ae0beff13ad"
            }))
        });

        _controller = new PaymentController(_httpClient);
    }

    [Fact]
    public async Task ProcessPayment_ShouldReturnBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        _controller.ModelState.AddModelError("CardNumber", "Required");
        var request = new PaymentRequest();

        // Act
        var result = await _controller.ProcessPayment(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ProcessPayment_ShouldReturnBadRequest_WhenBankSimulatorReturnsError()
    {
        // Arrange
        var request = new PaymentRequest
        {
            CardNumber = "1234567812345678",
            ExpiryMonth = 1,
            ExpiryYear = DateTime.Now.Year + 1,
            Currency = "USD",
            Amount = 100,
            CVV = "123"
        };

        var errorResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);
        _httpClient = HttpClientMockFactory.CreateMockClient(errorResponse);
        var controller = new PaymentController(_httpClient);

        // Act
        var result = await controller.ProcessPayment(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ProcessPayment_ShouldReturnOk_WhenBankSimulatorReturnsSuccess()
    {
        // Arrange
        var request = new PaymentRequest
        {
            CardNumber = "1234567812345678",
            ExpiryMonth = 1,
            ExpiryYear = 2025,
            Currency = "USD",
            Amount = 100,
            CVV = "123"
        };

        var successResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonSerializer.Serialize(new BankSimulatorResponse
            {
                Authorized = true,
                AuthorizationCode = "0bb07405-6d44-4b50-a14f-7ae0beff13ad"
            }))
        };
        _httpClient = HttpClientMockFactory.CreateMockClient(successResponse);
        var controller = new PaymentController(_httpClient);

        // Act
        var result = await controller.ProcessPayment(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var paymentResponse = Assert.IsType<PaymentResponse>(okResult.Value);

        Assert.Equal(StatusEnum.Authorized, paymentResponse.Status);
    }

    // Additional tests can be added here...
}

public static class HttpClientMockFactory
{
    public static HttpClient CreateMockClient(HttpResponseMessage response)
    {
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        return new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:8080/")
        };
    }
}