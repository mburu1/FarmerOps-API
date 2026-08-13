namespace FarmerOps.Application.Common.Exceptions;

public class ForbiddenAccessException(string message = "Access to this resource is forbidden.") : Exception(message);
