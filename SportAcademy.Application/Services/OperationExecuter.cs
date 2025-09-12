using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Services
{
    public class OperationExecutor
    {
        private readonly ILogger<OperationExecutor> _logger;

        public OperationExecutor(ILogger<OperationExecutor> logger)
        {
            _logger = logger;
        }

        public async Task<Result<T>> Execute<T>(Func<Task<Result<T>>> operation, OperationType opType)
        {
            Result<T> res;
            try
            {
                _logger.LogInformation("Starting {Operation} operation...", opType);

                res = await operation();

                if (res.IsSuccess)
                {
                    _logger.LogInformation("{Operation} operation completed successfully: {Message}", opType, res.Message);
                    res.StatusCode = opType switch
                    {
                        OperationType.Add => 201,
                        OperationType.Update => 200,
                        OperationType.Delete => 204,
                        _ => 200
                    };
                }
                else
                {
                    _logger.LogWarning("Operation {Operation} failed: {Message}", opType, res.Message);
                    res.StatusCode = opType switch
                    {
                        OperationType.Add => 400,
                        OperationType.Update => 400,
                        OperationType.Delete => 404,
                        _ => 500
                    };
                }
                return res;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                _logger.LogError(ex, "Argument out of range error occurred during {Operation}", opType);
                return Result<T>.Failure(opType.ToString(), ex.Message, 400);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Argument null error occurred during {Operation}", opType);
                return Result<T>.Failure(opType.ToString(), ex.Message, 400);
            }
            catch (NullReferenceException ex)
            {
                _logger.LogError(ex, "Null reference error occurred during {Operation}", opType);
                return Result<T>.Failure(opType.ToString(), ex.Message, 500);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation error occurred during {Operation}", opType);
                return Result<T>.Failure(opType.ToString(), ex.Message, 400);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Key not found error occurred during {Operation}", opType);
                return Result<T>.Failure(opType.ToString(), ex.Message, 404);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error occurred during {Operation}", opType);
                return Result<T>.Failure(opType.ToString(), ex.Message, 500);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, "Validation error occurred during {Operation}", opType);
                return Result<T>.Failure(opType.ToString(), ex.Message, 400);
            }
            catch (InvalidSortChoiceException ex)
            {
                _logger.LogError(ex, "Invalid sort choice error occurred during {Operation}", opType);
                return Result<T>.Failure(opType.ToString(), ex.Message, 400);
            }
            catch (EmailNotFoundException ex)
            {
                _logger.LogError(ex, "Email not found error occurred during {Operation}", opType);
                return Result<T>.Failure(opType.ToString(), ex.Message, 404);
            }
            catch (EmailExistException ex)
            {
                _logger.LogError(ex, "Email already exists error occurred during {Operation}", opType);
                return Result<T>.Failure(opType.ToString(), ex.Message, 409);
            }
            catch (PhoneExistException ex)
            {
                _logger.LogError(ex, "Phone already exist error occurred during {Operation}", opType);
                return Result<T>.Failure(opType.ToString(), ex.Message, 409);
            }
            catch (UserNameExistException ex)
            {
                _logger.LogError(ex, "Username already exist error occurred during {Operation}", opType);
                return Result<T>.Failure(opType.ToString(), ex.Message, 409);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access error occurred during {Operation}", opType);
                return Result<T>.Failure(opType.ToString(), "Unauthorized access: " + ex.Message, 401);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred during {Operation}", opType);
                return Result<T>.Failure(opType.ToString(), "An unexpected error occurred. Please try again later.", 500);
            }

        }
    }
}
