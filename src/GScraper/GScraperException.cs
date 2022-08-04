using System;

namespace GScraper;

/// <summary>
/// The exception that is thrown during the scraping process.
/// </summary>
public class GScraperException : Exception
{
    /// <summary>
    /// Gets the search engine that caused this exception.
    /// </summary>
    public string Engine { get; } = "Unknown";

    /// <summary>
    /// Initializes a new instance of the <see cref="GScraperException"/> class.
    /// </summary>
    public GScraperException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GScraperException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public GScraperException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GScraperException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="engine">The search engine that caused this exception.</param>
    public GScraperException(string message, string engine) : this(message)
    {
        Engine = engine;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GScraperException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public GScraperException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GScraperException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="engine">The search engine that caused this exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public GScraperException(string message, string engine, Exception innerException) : this(message, innerException)
    {
        Engine = engine;
    }
}