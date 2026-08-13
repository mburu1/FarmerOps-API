namespace FarmerOps.Application.Common.Exceptions;

public class AuthenticationFailedException(string message = "Invalid credentials.") : Exception(message);
