
## Problem Details

On IoT Hub Portal, we use the library [Hellang.Middleware.ProblemDetails](https://github.com/khellang/Middleware) which implements [RFC7807](https://datatracker.ietf.org/doc/html/rfc7807) to describe issues/problems that occured on backend.

### Handle a new exception using `Problem Details`

* Create a new exception which extends [`BaseException`](https://github.com/CGI-FR/IoT-Hub-Portal/blob/main/src/AzureIoTHub.Portal/Server/Exceptions/BaseException.cs). For example see 👉 [`InternalServerErrorException`](https://github.com/CGI-FR/IoT-Hub-Portal/blob/main/src/AzureIoTHub.Portal/Server/Exceptions/InternalServerErrorException.cs)
* On [Startup](https://github.com/CGI-FR/IoT-Hub-Portal/blob/main/src/AzureIoTHub.Portal/Server/Startup.cs) class, within the instruction `services.AddProblemDetails()`:
    * Your new exception is already catched by the middleware Problem Details because its extends the exception `BaseException`.
    * It you want override the behaviour of the middleware when processing your exception, you have to add a new mapping within it.

> 💡 You can also map exceptions from dotnet framework and third parties.