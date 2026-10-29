using FinanceManagerAspNet.Services;
using FinanceManagerAspNet.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagerAspNet.Controllers;

public abstract class FinanceController : Controller
{
    protected OkObjectResult OperationSucceeded<T>(
        MonthlyMoneyOperationResult result,
        string? redirectUrl,
        T? data)
        => Ok(new OperationResponse<T>(true, result.Message, redirectUrl, data));

    protected IActionResult? ValidateWriteRequest()
        => User.Identity?.IsAuthenticated == true ? null : Unauthorized();

    protected IActionResult? ValidateWriteRequest(MonthlyRequestValidation validation)
    {
        var authenticationFailure = ValidateWriteRequest();
        if (authenticationFailure is not null) return authenticationFailure;
        return validation.IsValid ? null : BadRequest(validation.Error);
    }

    protected IActionResult? ValidateWriteRequest(bool isValid, string error)
        => ValidateWriteRequest(new MonthlyRequestValidation(isValid, isValid ? null : error));
}
