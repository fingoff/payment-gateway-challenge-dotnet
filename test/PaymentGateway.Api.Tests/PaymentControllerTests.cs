using Xunit;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Controllers;
using PaymentGateway.Models;

namespace PaymentGatewayTests
{
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

        // PROCESS PAYMENT TESTS
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

        // RETRIEVE PAYMENT DETAILS TESTS
        [Fact]
        public async Task RetrievePaymentDetails_ShouldReturnOk_WhenPaymentExists()
        {
            // Arrange
            Guid expectedId = Guid.Parse("f32bd16f-e103-425e-9fa8-faa4accc3b93");
            StatusEnum expectedStatus = StatusEnum.Authorized;
            string expectedLast4CardDigits = "1234";
            int expectedExpiryMonth = 12;
            int expectedExpiryYear = 2027;
            CurrencyCodes expectedCurrency = CurrencyCodes.USD;
            int expectedAmount = 1000;

            // Act
            var result = await _controller.RetrievePaymentDetails(expectedId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var paymentResponse = Assert.IsType<PaymentResponse>(okResult.Value);
            Assert.Equal(expectedId, paymentResponse.Id);
            Assert.Equal(expectedStatus, paymentResponse.Status);
            Assert.Equal(expectedLast4CardDigits, paymentResponse.Last4CardDigits);
            Assert.Equal(expectedExpiryMonth, paymentResponse.ExpiryMonth);
            Assert.Equal(expectedExpiryYear, paymentResponse.ExpiryYear);
            Assert.Equal(expectedCurrency, paymentResponse.Currency);
            Assert.Equal(expectedAmount, paymentResponse.Amount);
        }

        [Fact]
        public async Task RetrievePaymentDetails_ShouldReturnNotFound_WhenPaymentDoesNotExist()
        {
            // Arrange
            var nonExistentPaymentId = Guid.NewGuid();

            // Act
            var result = await _controller.RetrievePaymentDetails(nonExistentPaymentId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
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
}