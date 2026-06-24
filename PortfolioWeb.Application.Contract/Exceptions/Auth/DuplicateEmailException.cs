namespace PortfolioWeb.Application.Contract.Exceptions.Auth;

public class DuplicateEmailException(string email) : Exception($"The email '{email}' is already registered.");
