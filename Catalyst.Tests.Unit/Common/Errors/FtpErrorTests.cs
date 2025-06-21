using Catalyst.Common.Errors;
using Catalyst.Common.Errors.Definitions;
using FluentAssertions;

namespace Catalyst.Tests.Unit.Common.Errors;

public class FtpErrorTests : BaseErrorTest<FtpError>
{
    protected override FtpError CreateError(string message, ErrorType errorType) =>
        new(message, errorType, Faker.Random.AlphaNumeric(3));

    protected override FtpError CreateErrorWithDefaultType(string message) => new(message);
    protected override ErrorType GetDefaultErrorType() => CommonErrorTypes.Internal.FtpConnectionFailed;

    [Fact]
    public void Constructor_WithFtpStatusCode_ShouldSetProperty()
    {
        // Arrange
        var message = Faker.Lorem.Sentence();
        var ftpStatusCode = Faker.Random.AlphaNumeric(3);

        // Act
        var error = new FtpError(message, ftpStatusCode);

        // Assert
        error.FtpStatusCode.Should().Be(ftpStatusCode);
        error.ErrorType.Should().Be(CommonErrorTypes.Internal.FtpConnectionFailed);
    }

    [Fact]
    public void Constructor_WithFtpStatusCodeAndErrorType_ShouldSetProperties()
    {
        // Arrange
        var message = Faker.Lorem.Sentence();
        var ftpStatusCode = Faker.Random.AlphaNumeric(3);
        var customErrorType = new ErrorType("FTP.CUSTOM", "Custom FTP issue", 500);

        // Act
        var error = new FtpError(message, customErrorType, ftpStatusCode);

        // Assert
        error.FtpStatusCode.Should().Be(ftpStatusCode);
        error.ErrorType.Should().Be(customErrorType);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString_WithFtpStatusCode()
    {
        // Arrange
        var message = "FTP connection problem";
        var ftpStatusCode = "530";
        var error = new FtpError(message, ftpStatusCode);
        var expectedString =
            $"[FtpError] {message} (Type: {CommonErrorTypes.Internal.FtpConnectionFailed.CustomCode}, FTP Status: {ftpStatusCode})";

        // Act
        var result = error.ToString();

        // Assert
        result.Should().Be(expectedString);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString_WithNullFtpStatusCode()
    {
        // Arrange
        var message = "FTP connection problem";
        var error = new FtpError(message, ftpStatusCode: null); // Explicitly pass null
        var expectedString =
            $"[FtpError] {message} (Type: {CommonErrorTypes.Internal.FtpConnectionFailed.CustomCode}, FTP Status: N/A)";

        // Act
        var result = error.ToString();

        // Assert
        result.Should().Be(expectedString);
    }
}