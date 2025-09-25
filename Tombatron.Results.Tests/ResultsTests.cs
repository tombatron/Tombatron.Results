using Tombatron.Results;

namespace Tombatron.Results.Tests;

public class ResultsTests
{
    public class GenericResultTests
    {
        [Fact]
        public void Ok_WithValue_ShouldReturnOkResult()
        {
            var result = Result<string>.Ok("test");
            
            Assert.IsType<Ok<string>>(result);
        }

        [Fact]
        public void Error_WithSingleMessage_ShouldReturnErrorResult()
        {
            var result = Result<string>.Error("error message");
            
            Assert.IsType<Error<string>>(result);
            var error = (Error<string>)result;
            Assert.Single(error.Messages);
            Assert.Equal("error message", error.Messages[0]);
        }

        [Fact]
        public void Error_WithMultipleMessages_ShouldReturnErrorResult()
        {
            var messages = new[] { "error 1", "error 2", "error 3" };
            var result = Result<string>.Error(messages);
            
            Assert.IsType<Error<string>>(result);
            var error = (Error<string>)result;
            Assert.Equal(3, error.Messages.Length);
            Assert.Equal(messages, error.Messages);
        }

        [Fact]
        public void Unwrap_WhenOk_ShouldReturnValue()
        {
            var result = Result<int>.Ok(42);
            
            var value = result.Unwrap();
            
            Assert.Equal(42, value);
        }

        [Fact]
        public void Unwrap_WhenErrorWithSingleMessage_ShouldThrowResultUnwrapException()
        {
            var result = Result<string>.Error("test error");
            
            var exception = Assert.Throws<ResultUnwrapException>(() => result.Unwrap());
            Assert.Contains("test error", exception.Message);
        }

        [Fact]
        public void Unwrap_WhenErrorWithMultipleMessages_ShouldThrowResultUnwrapAggregateException()
        {
            var messages = new[] { "error 1", "error 2", "error 3" };
            var result = Result<string>.Error(messages);
            
            var exception = Assert.Throws<ResultUnwrapAggregateException>(() => result.Unwrap());
            Assert.Equal(3, exception.InnerExceptions.Count);
            
            var innerMessages = exception.InnerExceptions.Select(e => e.Message).ToArray();
            
            Assert.Contains("error 1", innerMessages);
            Assert.Contains("error 2", innerMessages);
            Assert.Contains("error 3", innerMessages);
        }

        [Fact]
        public void UnwrapOr_WhenOk_ShouldReturnValue()
        {
            var result = Result<int>.Ok(42);
            
            var value = result.UnwrapOr(0);
            
            Assert.Equal(42, value);
        }

        [Fact]
        public void UnwrapOr_WhenError_ShouldReturnDefaultValue()
        {
            var result = Result<int>.Error("error");
            
            var value = result.UnwrapOr(99);
            
            Assert.Equal(99, value);
        }
    }

    public class NonGenericResultTests
    {
        [Fact]
        public void Ok_ShouldReturnOkResult()
        {
            var result = Result.Ok;
            
            Assert.IsType<Ok>(result);
        }

        [Fact]
        public void Error_WithSingleMessage_ShouldReturnErrorResult()
        {
            var result = Result.Error("error message");
            
            Assert.IsType<Error>(result);
            var error = (Error)result;
            Assert.Single(error.Messages);
            Assert.Equal("error message", error.Messages[0]);
        }

        [Fact]
        public void Error_WithMultipleMessages_ShouldReturnErrorResult()
        {
            var messages = new[] { "error 1", "error 2", "error 3" };
            var result = Result.Error(messages);
            
            Assert.IsType<Error>(result);
            var error = (Error)result;
            Assert.Equal(3, error.Messages.Length);
            Assert.Equal(messages, error.Messages);
        }
    }

    public class OkGenericTests
    {
        [Fact]
        public void Constructor_WithValue_ShouldStoreValue()
        {
            var ok = new Ok<string>("test value");
            
            Assert.Equal("test value", ok.Value);
        }

        [Fact]
        public void Value_ShouldReturnStoredValue()
        {
            var testObject = new { Name = "Test", Value = 42 };
            var ok = new Ok<object>(testObject);
            
            Assert.Same(testObject, ok.Value);
        }
    }

    public class ErrorGenericTests
    {
        [Fact]
        public void Constructor_WithSingleMessage_ShouldCreateArrayWithOneMessage()
        {
            var error = new Error<string>("single error");
            
            Assert.Single(error.Messages);
            Assert.Equal("single error", error.Messages[0]);
        }

        [Fact]
        public void Constructor_WithMessageArray_ShouldStoreMessages()
        {
            var messages = new[] { "error 1", "error 2", "error 3" };
            var error = new Error<string>(messages);
            
            Assert.Equal(messages, error.Messages);
        }

        [Fact]
        public void Constructor_WithEmptyArray_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Error<string>([]));
        }
        

        [Fact]
        public void Constructor_WithChildError_ShouldTrackChildError()
        {
            var child = new Error<string>("child error");
            var parent = new Error<string>("parent error", child);

            Assert.NotNull(parent.ChildError);
            Assert.Equal("child error", parent.ChildError.Message);
        }

        [Fact]
        public void Constructor_ShouldCaptureCallerInfo()
        {
            var error = CreateErrorWithCallerInfo();

            Assert.Contains("ResultsTests.cs", error.CallerFilePath); // Adjust filename if needed
            Assert.True(error.CallerLineNumber > 0);
        }

        private Error<string> CreateErrorWithCallerInfo()
        {
            return new Error<string>("caller info test");
        }

        [Fact]
        public void ToErrorString_ShouldIncludeMessagesAndCallerInfo()
        {
            var error = new Error<string>("formatted error");

            var errorString = error.ToErrorString();

            Assert.Contains("formatted error", errorString);
            Assert.Contains("ResultsTests.cs", errorString); // Adjust filename if needed
            Assert.Contains(error.CallerLineNumber.ToString(), errorString);
        }

        [Fact]
        public void ToErrorString_ShouldIncludeChildErrorDetails()
        {
            var child = new Error<string>("child error");
            var parent = new Error<string>("parent error", child);

            var errorString = parent.ToErrorString();

            Assert.Contains("parent error", errorString);
            Assert.Contains("child error", errorString);
        }
    }

    public class ErrorNonGenericTests
    {
        [Fact]
        public void Constructor_WithSingleMessage_ShouldCreateArrayWithOneMessage()
        {
            var error = new Error("single error");
            
            Assert.Single(error.Messages);
            Assert.Equal("single error", error.Messages[0]);
        }

        [Fact]
        public void Constructor_WithMessageArray_ShouldStoreMessages()
        {
            var messages = new[] { "error 1", "error 2", "error 3" };
            var error = new Error(messages);
            
            Assert.Equal(messages, error.Messages);
        }

        [Fact]
        public void Constructor_WithEmptyArray_ShouldThrowArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Error([]));
        }
        

        [Fact]
        public void Constructor_WithChildError_ShouldTrackChildError()
        {
            var child = new Error("child error");
            var parent = new Error("parent error", child);

            Assert.NotNull(parent.ChildError);
            Assert.Equal("child error", parent.ChildError.Message);
        }

        [Fact]
        public void Constructor_ShouldCaptureCallerInfo()
        {
            var error = CreateErrorWithCallerInfo();

            Assert.Contains("ResultsTests.cs", error.CallerFilePath); // Adjust filename if needed
            Assert.True(error.CallerLineNumber > 0);
        }

        private Error CreateErrorWithCallerInfo()
        {
            return new Error("caller info test");
        }

        [Fact]
        public void ToErrorString_ShouldIncludeMessagesAndCallerInfo()
        {
            var error = new Error("formatted error");

            var errorString = error.ToErrorString();

            Assert.Contains("formatted error", errorString);
            Assert.Contains("ResultsTests.cs", errorString); // Adjust filename if needed
            Assert.Contains(error.CallerLineNumber.ToString(), errorString);
        }

        [Fact]
        public void ToErrorString_ShouldIncludeChildErrorDetails()
        {
            var child = new Error("child error");
            var parent = new Error("parent error", child);

            var errorString = parent.ToErrorString();

            Assert.Contains("parent error", errorString);
            Assert.Contains("child error", errorString);
        }
        
    }

    public class ResultUnwrapExceptionTests
    {
        [Fact]
        public void Constructor_WithMessage_ShouldSetMessage()
        {
            var exception = new ResultUnwrapException("test message");
            
            Assert.Equal("test message", exception.Message);
        }

        [Fact]
        public void ShouldInheritFromException()
        {
            var exception = new ResultUnwrapException("test");
            
            Assert.IsAssignableFrom<Exception>(exception);
        }
    }

    public class ResultUnwrapAggregateExceptionTests
    {
        [Fact]
        public void Constructor_WithMessageAndInnerExceptions_ShouldSetProperties()
        {
            var innerExceptions = new[]
            {
                new ResultUnwrapException("error 1"),
                new ResultUnwrapException("error 2")
            };
            
            var exception = new ResultUnwrapAggregateException("aggregate message", innerExceptions);
            
            Assert.Contains("aggregate message", exception.Message);
            Assert.Equal(2, exception.InnerExceptions.Count);
            Assert.Contains(innerExceptions[0], exception.InnerExceptions);
            Assert.Contains(innerExceptions[1], exception.InnerExceptions);
        }

        [Fact]
        public void ShouldInheritFromAggregateException()
        {
            var exception = new ResultUnwrapAggregateException("test", []);
            
            Assert.IsAssignableFrom<AggregateException>(exception);
        }
    }

    public class EdgeCaseTests
    {
        [Fact]
        public void Result_WithNonNullConstraint_ShouldWorkWithReferenceTypes()
        {
            var result = Result<string>.Ok("test");
            
            Assert.Equal("test", result.Unwrap());
        }

        [Fact]
        public void Result_WithNonNullConstraint_ShouldWorkWithValueTypes()
        {
            var result = Result<int>.Ok(42);
            
            Assert.Equal(42, result.Unwrap());
        }

        [Fact]
        public void Result_WithNonNullConstraint_ShouldWorkWithCustomTypes()
        {
            var testObj = new { Name = "Test", Id = 1 };
            var result = Result<object>.Ok(testObj);
            
            Assert.Same(testObj, result.Unwrap());
        }

        [Fact]
        public void UnwrapOr_WithComplexTypes_ShouldWork()
        {
            var defaultList = new List<int> { 1, 2, 3 };
            var errorResult = Result<List<int>>.Error("failed to get list");
            
            var result = errorResult.UnwrapOr(defaultList);
            
            Assert.Same(defaultList, result);
        }

        [Fact]
        public void Error_Messages_ShouldBeReadOnly()
        {
            var error = new Error<string>("test");
            var messages = error.Messages;
            
            Assert.IsType<string[]>(messages);
            Assert.Single(messages);
        }

        [Fact]
        public void Multiple_Unwrap_Calls_ShouldBehaveConsistently()
        {
            var result = Result<int>.Ok(42);
            
            Assert.Equal(42, result.Unwrap());
            Assert.Equal(42, result.Unwrap());
            Assert.Equal(42, result.Unwrap());
        }

        [Fact]
        public void Error_With_LargeNumberOfMessages_ShouldWork()
        {
            var messages = Enumerable.Range(1, 1000)
                .Select(i => $"Error {i}")
                .ToArray();
            
            var result = Result<string>.Error(messages);
            var error = (Error<string>)result;
            
            Assert.Equal(1000, error.Messages.Length);
            Assert.Equal("Error 1", error.Messages[0]);
            Assert.Equal("Error 1000", error.Messages[999]);
        }
    }
}